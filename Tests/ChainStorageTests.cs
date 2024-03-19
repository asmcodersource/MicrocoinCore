using Microcoin.Microcoin.Blockchain.Chain;
using Tests.Generators;

namespace Tests
{
    public class ChainStorageTests
    {
        protected Chain tailOfTestChain = null; 

        [Fact]
        public void CreateChainTest()
        {
            var chainsGenerator = new MicrocoinTestChainsGenerator();
            tailOfTestChain = chainsGenerator.CreateChain(10, 10, 10);

        }
    }
}
