using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Generators;
using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSessionListener;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin;
using NodeNet.NodeNet;

namespace Tests
{
    public class ChainDownloadingTests
    {
        [Fact]
        public void DownloadFromZeroBlock_Test()
        {
           /* var connectionPair = new NodeNetConnection();
            var chainGenerator = new MicrocoinTestChainsGenerator();
            var generatedChainTail = chainGenerator.CreateChain(5, 5, 10);

            var firstNodeChain = new MutableChain();
            firstNodeChain.AddTailBlock(generatedChainTail.GetBlockFromHead(0)!);
            var secondNodeChain = new MutableChain();
            secondNodeChain = generatedChainTail;

            var fetchRequest = new FetchRequest(generatedChainTail.GetBlockFromTail(0)!, DateTime.UtcNow, 1);
            var firstNodeDownloader = new HandlingFetchRequest(fetchRequest, 50);
            var secondNodeListener = new ProviderSessionListener(connectionPair.second_node);
            secondNodeListener.StartListening();
            secondNodeListener.SourceChain = generatedChainTail;
            firstNodeDownloader.ChainFetched += (result) =>
            {
                var isChainEquals = IsChainsEqual(result.DownloadedChain, generatedChainTail);
                Assert.True(isChainEquals, "Download chain isn't equal to source chain");
            };
            var chainDownloadSucess = firstNodeDownloader.StartHandling(connectionPair.first_node, firstNodeChain, CancellationToken.None).Result;
            Assert.True(chainDownloadSucess, "Chain download isn't sucessful");*/

        }

        [Fact]
        public void DownloadFromMiddleBlock_Test()
        {
/*          var connectionPair = new NodeNetConnection();
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

            var fetchRequest = new FetchRequest(generatedChainTail.GetBlockFromTail(0)!, DateTime.UtcNow, 1);
            var firstNodeDownloader = new HandlingFetchRequest(fetchRequest, 50);
            var secondNodeListener = new ProviderSessionListener(connectionPair.second_node);
            secondNodeListener.StartListening();
            secondNodeListener.SourceChain = generatedChainTail;
            firstNodeDownloader.ChainFetched += (result) =>
            {
                var isChainEquals = IsChainsEqual(result.DownloadedChain, generatedChainTail);
                Assert.True(isChainEquals, "Download chain isn't equal to source chain");
            };
            var chainDownloadSucess = firstNodeDownloader.StartHandling(connectionPair.first_node, firstNodeChain, CancellationToken.None).Result;
            Assert.True(chainDownloadSucess, "Chain download isn't sucessful");*/
        }

        [Fact]
        public void DownloadThrought_ChainFetherSystem()
        {
            NodeNetConnection nodeNetConnection = new NodeNetConnection();
            var peer1 = CreatePeer(nodeNetConnection.first_node);
            var peer2 = CreatePeer(nodeNetConnection.second_node);

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

            bool isChainFetched = false;
            AbstractChain fetchedChain = null;
            peer1.ChainFetcher.ChainFetchCompleted += (chain) =>
            {
                isChainFetched = true;
                fetchedChain = chain;
            };
            peer1.ChainFetcher.RequestChainFetch(secondNodeChain.GetLastBlock()!);
            
            Thread.Sleep(10000);
            Assert.True(isChainFetched, "Chain isn't fetched");
            Assert.True(IsChainsEqual(fetchedChain, secondNodeChain), "Chains isn't equals");
        }

        private bool IsChainsEqual(AbstractChain first, AbstractChain second)
        {
            var firstEnumerator = first.GetEnumerable().GetEnumerator();
            var secondEnumerator = second.GetEnumerable().GetEnumerator();
            while( firstEnumerator.MoveNext() | secondEnumerator.MoveNext() )
                if( firstEnumerator.Current.Hash != secondEnumerator.Current.Hash )
                    return false;
            return true;
        }

        private Peer CreatePeer(Node node)
        {
            PeerBuilder peerBuilder = new PeerBuilder();
            peerBuilder.AddDebugMiner();
            peerBuilder.AddDefaultAcceptancePools();
            peerBuilder.AddDefaultRules();
            peerBuilder.AddNetworkNode(node);
            peerBuilder.AddChainsStorage("chains");
            peerBuilder.AddChainsFetcher();
            peerBuilder.AddWalletKeys();
            var peer = peerBuilder.Build();
            peer.PeerMining.StartMining();
            peer.ProviderSessionListener.StartListening();
            return peer;
        }
    }
}
