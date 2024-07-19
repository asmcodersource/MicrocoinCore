using Microcoin.Microcoin;
using Microcoin.Microcoin.Logging;
using Microcoin.Microcoin.ChainStorage;
using Xunit;
using Xunit.Abstractions;
using Tests.Generators;
using Microcoin.Microcoin.Blockchain.Block;
using NodeNet.NodeNet;
using System.Xml.Linq;

namespace Tests
{
    public class MicrocoinTests
    {
        private readonly ITestOutputHelper output;
        public static ChainStorage ChainStorage { get; private set; }
        public static string InitialPeerWalletKeys { get; private set; } = "initial-peer-wallet.keys";
        protected Dictionary<Peer, double> peersCoins = new Dictionary<Peer, double>();

        public MicrocoinTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        static MicrocoinTests()
        {
            Logging.InitializeLogger();
            CleanChainsDirectory();
            PeerBuilder peerBuilder = new PeerBuilder();
            peerBuilder.AddDefaultMiner();
            peerBuilder.AddDefaultAcceptancePools();
            peerBuilder.AddDefaultRules();
            peerBuilder.AddNetworkNode(0);
            peerBuilder.AddChainsStorage("chains");
            peerBuilder.AddChainsFetcher();
            peerBuilder.AddWalletKeysFromFileOrCreate(InitialPeerWalletKeys);
            peerBuilder.Build();
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
            // Create and initialize peer
            var peerBuilder = new PeerBuilder();
            peerBuilder.AddDebugServices();
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
            NodeNetTestNetworksGenerator nodeNetNetworkConnections = new NodeNetTestNetworksGenerator();
            nodeNetNetworkConnections.CreateNetworkPeers(5);
            nodeNetNetworkConnections.CreateNetworkTree(5);
            nodeNetNetworkConnections.PerformRandomConnections(0);
            // Create many peers on test network nodes
            List<Peer> peers = new List<Peer>();
            foreach(var node in nodeNetNetworkConnections.Nodes.Skip(1))
            {
                PeerBuilder peerBuilder = new PeerBuilder();
                peerBuilder.AddDefaultMiner();
                peerBuilder.AddDefaultAcceptancePools();
                peerBuilder.AddDefaultRules();
                peerBuilder.AddNetworkNode(node);
                peerBuilder.AddChainsStorage("chains");
                peerBuilder.AddChainsFetcher();
                peerBuilder.AddWalletKeys();
                var peer = peerBuilder.Build();
                peer.PeerMining.StartMining();
                peers.Add(peer);

                //peer.TransactionsPool.OnTransactionReceived += (transaction) => output.WriteLine($"Peer [{peer.GetHashCode()}] accepted transaction [{transaction.GetHashCode()}]");
                //peer.BlocksPool.OnBlockReceived += (blockspool, block) => output.WriteLine($"Peer [{peer.GetHashCode()}] accepted block [{block.GetMiningBlockHash()}]");
                //peer.PeerMining.BlockMined += (block) => output.WriteLine($"Peer [{peer.GetHashCode()}] mined block [{block.GetMiningBlockHash()}]");
                //peer.PeerNetworking.TransactionReceived += (transaction) => output.WriteLine($"Peer [{peer.GetHashCode()}] receive transaction [{transaction.GetHashCode()}] from network");
                //peer.PeerNetworking.BlockReceived += (block) => output.WriteLine($"Peer [{peer.GetHashCode()}] receive block [{block.GetMiningBlockHash()}] from network");
            }
            // Set first peers as initial peer
            PeerBuilder zeroTransactionPeerBuilder = new PeerBuilder();
            zeroTransactionPeerBuilder.AddDefaultMiner();
            zeroTransactionPeerBuilder.AddDefaultAcceptancePools();
            zeroTransactionPeerBuilder.AddDefaultRules();
            zeroTransactionPeerBuilder.AddNetworkNode(nodeNetNetworkConnections.Nodes.First());
            zeroTransactionPeerBuilder.AddChainsStorage("chains");
            zeroTransactionPeerBuilder.AddChainsFetcher();
            zeroTransactionPeerBuilder.AddWalletKeysFromFile(InitialPeerWalletKeys);
            var zeroTransactionPeer = zeroTransactionPeerBuilder.Build();
            zeroTransactionPeer.PeerMining.StartMining();
            peers.Add(zeroTransactionPeer);
            DoPeerCoinsCount(peers.Last(), 0.1);
            output.WriteLine("Network and peers created");

            // Sending coins by peers to another peers
            for( int i = 0; i < 32*32; i++)
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
            Thread.Sleep(30*60*1000);
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
