using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tests.MicrocoinCoreTesting.Generators;
using Xunit;
using Tests.NetworkTesting.TestNetworks;
using MicrocoinCore.Microcoin.Network;

namespace Tests.MicrocoinCoreTesting.ChainFetcherTesting
{
    public class ChainFetcherTesting
    {
        [Fact]
        public void DownloadFromZeroBlock_Test()
        {
            var network = TwoNodesNetwork.Create();
            var peer1 = CreatePeer(network.FirstNode);
            var peer2 = CreatePeer(network.SecondNode);

            var chainGenerator = new MicrocoinTestChainsGenerator();
            var generatedChainTail = chainGenerator.CreateChain(5, 5, 10);

            var firstNodeChain = new MutableChain();
            firstNodeChain.AddTailBlock(generatedChainTail.GetBlockFromHead(0)!);
            var secondNodeChain = new MutableChain();
            secondNodeChain = generatedChainTail;

            peer1.PeerChain.SetSpecificChain(firstNodeChain);
            peer2.PeerChain.SetSpecificChain(secondNodeChain);
            ArrangeAndActSection(firstNodeChain, peer1, secondNodeChain, peer2);
        }

        [Fact]
        public void DownloadFromMiddleBlock_Test()
        {
            var network = TwoNodesNetwork.Create();
            var peer1 = CreatePeer(network.FirstNode);
            var peer2 = CreatePeer(network.SecondNode);

            var chainGenerator = new MicrocoinTestChainsGenerator();
            var generatedChainTail = chainGenerator.CreateChain(5, 5, 10);

            var firstNodeChain = new MutableChain();
            var countOfAvailableBlocks = Random.Shared.Next(1, generatedChainTail.EntireChainLength);
            var blockTakenFromGeneratedChain = generatedChainTail
                .GetEnumerable()
                .Take(countOfAvailableBlocks);
            foreach (var block in blockTakenFromGeneratedChain)
                firstNodeChain.AddTailBlock(block);
            var secondNodeChain = new MutableChain();
            secondNodeChain = generatedChainTail;

            peer1.PeerChain.SetSpecificChain(firstNodeChain);
            peer2.PeerChain.SetSpecificChain(secondNodeChain);
            ArrangeAndActSection(firstNodeChain, peer1, secondNodeChain, peer2);
        }

        private void ArrangeAndActSection(MutableChain firstChain, Peer firstPeer, MutableChain secondChain, Peer secondPeer)
        {
            var fetchRequest = new FetchRequest(secondChain.GetBlockFromTail(0)!, DateTime.UtcNow, 1);
            var firstNodeDownloader = new HandlingFetchRequest(fetchRequest, firstPeer.ServicesContainer);
            var secondNodeListener = secondPeer.PeerChainsProvider;
            secondNodeListener.ChangeSourceChain(secondChain);
            firstNodeDownloader.ChainFetched += (result) =>
            {
                var isChainEquals = IsChainsEqual(result.DownloadedChain, secondChain);
                Assert.True(isChainEquals, "Download chain isn't equal to source chain");
            };
            var chainDownloadSucess = firstNodeDownloader.StartHandling(firstChain, CancellationToken.None).Result;
            Assert.True(chainDownloadSucess, "Chain download isn't sucessful");
        }

        [Fact]
        public void DownloadThrought_ChainFetherSystem()
        {
            var network = TwoNodesNetwork.Create();
            var peer1 = CreatePeer(network.FirstNode);
            var peer2 = CreatePeer(network.SecondNode);

            var chainGenerator = new MicrocoinTestChainsGenerator();
            var generatedChainTail = chainGenerator.CreateChain(4, 50, 1);
            var firstNodeChain = new MutableChain();
            var countOfAvailableBlocks = Random.Shared.Next(1, generatedChainTail.EntireChainLength);
            var blockTakenFromGeneratedChain = generatedChainTail
                .GetEnumerable()
                .Take(countOfAvailableBlocks);
            foreach (var block in blockTakenFromGeneratedChain)
                firstNodeChain.AddTailBlock(block);
            var secondNodeChain = new MutableChain();
            secondNodeChain = generatedChainTail;

            peer1.PeerChain.SetSpecificChain(firstNodeChain);
            peer2.PeerChain.SetSpecificChain(secondNodeChain);

            bool isChainFetched = false;
            AbstractChain fetchedChain = null;
            peer1.ChainFetcher.ChainFetchCompleted += (chain) =>
            {
                isChainFetched = true;
                fetchedChain = chain;
            };
            peer1.ChainFetcher.RequestChainFetch(secondNodeChain.GetBlockFromHead(Random.Shared.Next(firstNodeChain.EntireChainLength, generatedChainTail.EntireChainLength))!);

            Thread.Sleep(3000);
            Assert.True(isChainFetched, "Chain isn't fetched");
            Assert.True(IsChainsEqual(fetchedChain, secondNodeChain), "Chains isn't equals");
        }

        private bool IsChainsEqual(AbstractChain first, AbstractChain second)
        {
            var firstEnumerator = first.GetEnumerable().GetEnumerator();
            var secondEnumerator = second.GetEnumerable().GetEnumerator();
            while (firstEnumerator.MoveNext() | secondEnumerator.MoveNext())
                if (firstEnumerator.Current.Hash != secondEnumerator.Current.Hash)
                    return false;
            return true;
        }

        private Peer CreatePeer(IBroadcastNode node)
        {
            PeerBuilder peerBuilder = new PeerBuilder();
            peerBuilder.AddDebugMiner();
            peerBuilder.AddDefaultAcceptancePools();
            peerBuilder.AddDefaultRules();
            peerBuilder.AddBroadcastSessionManager(node);
            peerBuilder.AddBroadcastTransceiver(node);
            peerBuilder.AddEndPointCollectionProvider(node);
            peerBuilder.AddChainProvidersRating(new ChainProvidersRating());
            peerBuilder.AddChainBranchBlocksCount(10);
            peerBuilder.AddChainsStorage("chains");
            peerBuilder.AddChainsFetcher();
            peerBuilder.AddWalletKeys();
            var peer = peerBuilder.Build();
            peer.PeerMining.StartMining();
            peer.PeerChainsProvider.StartListening();
            return peer;
        }
    }
}
