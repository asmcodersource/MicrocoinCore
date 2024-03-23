using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainStorage;
using Tests.Generators;

namespace Tests
{
    public class ChainStorageTests
    {
        protected static string workingDirectory = null;

        static ChainStorageTests()
        {
            // Get path to test chains storage dir, and clear it 
            workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestChainsStorage");
            Directory.CreateDirectory(workingDirectory);
            DirectoryInfo dir = new DirectoryInfo(workingDirectory);
            foreach (var file in dir.GetFiles())
                file.Delete();
        }

        [Fact]
        public void CreateChainTest()
        {
            var chainsGenerator = new MicrocoinTestChainsGenerator();
            var tailOfTestChain = chainsGenerator.CreateChain(10, 10, 10);
        }

        [Fact]
        public void StoreLoadChainTest()
        {
            // Create test chain
            var chainsGenerator = new MicrocoinTestChainsGenerator();
            var tailOfTestChain = chainsGenerator.CreateChain(10, 10, 10);
            // Try store chain to disk
            ChainStorage chainStorage = new ChainStorage();
            chainStorage.WorkingDirectory = workingDirectory;
            chainStorage.AddNewChainToStorage(tailOfTestChain);
            // Try to load chain back
            ChainContext? loadedChain = null;
            // -- On same chain storage
            loadedChain = chainStorage.LoadChainByIdentifier(new ChainIdentifier(tailOfTestChain));
            Assert.NotNull(loadedChain);
            // -- On new chain storage
            ChainStorage anotherChainStorage = new ChainStorage();
            anotherChainStorage.WorkingDirectory = workingDirectory;
            anotherChainStorage.FetchChains();
            loadedChain = chainStorage.LoadChainByIdentifier(new ChainIdentifier(tailOfTestChain));
            Assert.NotNull(loadedChain);
        }
    }
}
