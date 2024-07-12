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

            var fetchRequest = new FetchRequest(generatedChainTail.GetBlockFromTail(0)!, DateTime.UtcNow);
            var firstNodeDownloader = new FetchRequestHandler(fetchRequest);
            var secondNodeListener = new ProviderSessionListener(connectionPair.second_node);
            secondNodeListener.StartListening();
            secondNodeListener.SourceChain = generatedChainTail;
            firstNodeDownloader.StartHandling(connectionPair.first_node, firstNodeChain).Wait();
        }

        [Fact]
        public void DownloadFromMiddleBlock_Test()
        {
            var connectionPair = new NodeNetConnection();
            var chainGenerator = new MicrocoinTestChainsGenerator();
            var generatedChainTail = chainGenerator.CreateChain(10, 10, 100);

            var firstNodeChain = new MutableChain();
            firstNodeChain.AddTailBlock(generatedChainTail.GetBlockFromHead(0)!);
            var secondNodeChain = new MutableChain();
            secondNodeChain = generatedChainTail;
        }
    }
}
