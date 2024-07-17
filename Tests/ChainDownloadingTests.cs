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

namespace Tests
{
    public class ChainDownloadingTests
    {
        [Fact]
        public void DownloadFromZeroBlock_Test()
        {
            var connectionPair = new NodeNetConnection();
            var chainGenerator = new MicrocoinTestChainsGenerator();
            var generatedChainTail = chainGenerator.CreateChain(5, 5, 10);

            var firstNodeChain = new MutableChain();
            firstNodeChain.AddTailBlock(generatedChainTail.GetBlockFromHead(0)!);
            var secondNodeChain = new MutableChain();
            secondNodeChain = generatedChainTail;

            var fetchRequest = new FetchRequest(generatedChainTail.GetBlockFromTail(0)!, DateTime.UtcNow, 1);
            var firstNodeDownloader = new FetchRequestHandler(fetchRequest, 50);
            var secondNodeListener = new ProviderSessionListener(connectionPair.second_node);
            secondNodeListener.StartListening();
            secondNodeListener.SourceChain = generatedChainTail;
            var chainDownloadSucess = firstNodeDownloader.StartHandling(connectionPair.first_node, firstNodeChain, CancellationToken.None).Result;
            Assert.True(chainDownloadSucess, "Chain download isn't sucessful");
            var isChainEquals = IsChainsEqual(firstNodeDownloader.DownloadedChain, generatedChainTail);
            Assert.True(chainDownloadSucess, "Download chain isn't equal to source chain");
        }

        [Fact]
        public void DownloadFromMiddleBlock_Test()
        {
            var connectionPair = new NodeNetConnection();
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
            var firstNodeDownloader = new FetchRequestHandler(fetchRequest, 50);
            var secondNodeListener = new ProviderSessionListener(connectionPair.second_node);
            secondNodeListener.StartListening();
            secondNodeListener.SourceChain = generatedChainTail;
            var chainDownloadSucess = firstNodeDownloader.StartHandling(connectionPair.first_node, firstNodeChain, CancellationToken.None).Result;
            Assert.True(chainDownloadSucess, "Chain download isn't sucessful");
            var isChainEquals = IsChainsEqual(firstNodeDownloader.DownloadedChain, generatedChainTail);
            Assert.True(chainDownloadSucess, "Download chain isn't equal to source chain");
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
    }
}
