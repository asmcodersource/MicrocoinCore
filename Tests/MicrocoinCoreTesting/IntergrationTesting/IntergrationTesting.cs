using Microcoin.Microcoin.ChainStorage;
using Microcoin.Microcoin.Logging;
using Microcoin.Microcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Tests.NetworkTesting.TestNetworks;
using Microcoin.Microcoin.ChainFetcher;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using MockNetwork.Logic;

namespace Tests.MicrocoinCoreTesting.IntergrationTesting
{
    public class IntergrationTesting
    {
        private readonly ITestOutputHelper output;
        public static ChainStorage ChainStorage { get; private set; }
        public static string InitialPeerWalletKeys { get; private set; } = "initial-peer-wallet.keys";
        protected Dictionary<Peer, double> peersCoins = new Dictionary<Peer, double>();

        public IntergrationTesting(ITestOutputHelper output)
        {
            this.output = output;
        }

        static IntergrationTesting()
        {
            Logging.InitializeLogger();
            CleanChainsDirectory();
        }

        static void CleanChainsDirectory()
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "chains"));
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "chains"));
            foreach (var file in dir.GetFiles())
                file.Delete();
        }

        // This shit works bad, but anyway it works!
        [Fact]
        public void SinglePeerIntegrationTest()
        {
            var node = new MockBroadcastNode();
            // Create and initialize peer
            var peerBuilder = new PeerBuilder();
            peerBuilder.AddDefaultMiner();
            peerBuilder.AddDefaultAcceptancePools();
            peerBuilder.AddDefaultRules();
            peerBuilder.AddBroadcastSessionManager(node);
            peerBuilder.AddBroadcastTransceiver(node);
            peerBuilder.AddEndPointCollectionProvider(node);
            peerBuilder.AddChainsStorage("SinglePeerIntegrationTest_ChainStorage");
            peerBuilder.AddChainsFetcher();
            peerBuilder.AddChainBranchBlocksCount(25);
            peerBuilder.AddWalletKeys();
            var peer = peerBuilder.Build();
            peer.PeerMining.StartMining();

            // I create transactions and mine them with the same peer
            for (int i = 0; i < 40; i++)
            {
                peer.SendCoins(peer.WalletPublicKey, 0.000000000001);
            }
            Thread.Sleep(10000);
            // We just ensure, that no exceptions throws, and no dead-locks there..
        }

        [Fact]
        public void MultiplePeersIntegrationTest()
        {
            // Create test network nodes with connections
            var network = MultipleNodesNetwork.Create(Consts.NetworkLevels);

            // Create many peers on test network nodes
            List<Peer> peers = new List<Peer>();

            var zeroPeerNode = network.NetworkNodes.First();
            PeerBuilder zeroTransactionPeerBuilder = new PeerBuilder();
            zeroTransactionPeerBuilder.AddDefaultMiner();
            zeroTransactionPeerBuilder.AddDefaultAcceptancePools();
            zeroTransactionPeerBuilder.AddDefaultRules();
            zeroTransactionPeerBuilder.AddBroadcastSessionManager(zeroPeerNode);
            zeroTransactionPeerBuilder.AddBroadcastTransceiver(zeroPeerNode);
            zeroTransactionPeerBuilder.AddEndPointCollectionProvider(zeroPeerNode);
            zeroTransactionPeerBuilder.AddChainBranchBlocksCount(25);
            zeroTransactionPeerBuilder.AddChainProvidersRating(new ChainProvidersRating());
            zeroTransactionPeerBuilder.AddChainsStorage("MultiplePeersIntegrationTest_ChainStorage");
            zeroTransactionPeerBuilder.AddChainsFetcher();
            zeroTransactionPeerBuilder.AddWalletKeysFromFileOrCreate(InitialPeerWalletKeys);
            var zeroTransactionPeer = zeroTransactionPeerBuilder.Build();
            zeroTransactionPeer.PeerMining.StartMining();
            zeroTransactionPeer.PeerChainsProvider.StartListening();
            peers.Add(zeroTransactionPeer);
            DoPeerCoinsCount(peers.Last(), 0.1);


            foreach (var node in network.NetworkNodes.Skip(1))
            {
                PeerBuilder peerBuilder = new PeerBuilder();
                peerBuilder.AddDefaultMiner();
                peerBuilder.AddDefaultAcceptancePools();
                peerBuilder.AddDefaultRules();
                peerBuilder.AddBroadcastSessionManager(node);
                peerBuilder.AddBroadcastTransceiver(node);
                peerBuilder.AddEndPointCollectionProvider(node);
                peerBuilder.AddChainBranchBlocksCount(25);
                peerBuilder.AddChainProvidersRating(new ChainProvidersRating());
                peerBuilder.AddChainsStorage("MultiplePeersIntegrationTest_ChainStorage");
                peerBuilder.AddChainsFetcher();
                peerBuilder.AddWalletKeys();
                var peer = peerBuilder.Build();
                peer.PeerMining.StartMining();
                peer.PeerChainsProvider.StartListening();
                peers.Add(peer);

                //peer.TransactionsPool.OnTransactionReceived += (transaction) => output.WriteLine($"Peer [{peer.GetHashCode()}] accepted transaction [{transaction.GetHashCode()}]");
                //peer.BlocksPool.OnBlockReceived += (blockspool, block) => output.WriteLine($"Peer [{peer.GetHashCode()}] accepted block [{block.GetMiningBlockHash()}]");
                //peer.PeerMining.BlockMined += (block) => output.WriteLine($"Peer [{peer.GetHashCode()}] mined block [{block.GetMiningBlockHash()}]");
                //peer.PeerNetworking.TransactionReceived += (transaction) => output.WriteLine($"Peer [{peer.GetHashCode()}] receive transaction [{transaction.GetHashCode()}] from network");
                //peer.PeerNetworking.BlockReceived += (block) => output.WriteLine($"Peer [{peer.GetHashCode()}] receive block [{block.GetMiningBlockHash()}] from network");
            }
            // Set first peers as initial peer
            output.WriteLine("Network and peers created");

            // Sending coins by peers to another peers
            for (int i = 0; i < 512 * 512; i++)
            {
                Peer peerSender, peerReceiver;
                peerSender = peersCoins.ElementAt(Random.Shared.Next(peersCoins.Count)).Key;
                peerReceiver = peers.ElementAt(Random.Shared.Next(peers.Count));
                if (peerSender == peerReceiver)
                {
                    i--;
                    continue;
                }
                double coinsToSend = peersCoins[peerSender] / 1000;
                var transaction = peerSender.SendCoins(peerReceiver.WalletPublicKey, coinsToSend);
                DoPeerCoinsCount(peerSender, -coinsToSend);
                DoPeerCoinsCount(peerReceiver, +coinsToSend);
                //output.WriteLine($"Peer [{peerSender.GetHashCode()}] send transaction [{transaction.GetHashCode()}]");
            }

            // If there is no exceptions, we can thing that it works, at least not faults, self-locks, etc.
            // Additional, we have logs, to read, what really happens behind
            // -- Log inside xUnit output ( less information )
            // -- Log file from run directory ( more information )
            Thread.Sleep(Timeout.Infinite);
        }

        protected void DoPeerCoinsCount(Peer peer, double coinsChange)
        {
            peersCoins.TryAdd(peer, 0);
            var peerCoins = peersCoins[peer];
            var newPeerCoins = peerCoins + coinsChange;
            Assert.True(newPeerCoins >= 0);
            if (newPeerCoins == 0)
                peersCoins.Remove(peer);
            else
                peersCoins[peer] = newPeerCoins;
        }
    }
}
