using Microcoin.Microcoin;
using Microcoin.Microcoin.Logging;
using Microcoin.Microcoin.ChainStorage;

namespace Tests
{
    public class MicrocoinTests
    {
        public static ChainStorage ChainStorage { get; private set; }
        public static string InitialPeerWalletKeys { get; private set; } = "initial-peer-wallet.keys";

        static MicrocoinTests()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "test-peers-wallet-keys"));
            foreach (var file in dir.GetFiles())
                file.Delete();

            Logging.InitializeLogger();
            DepencyInjection.CreateContainer();
            DepencyInjection.AddChainsStorage(Path.Combine(Directory.GetCurrentDirectory(), "test-peers-wallet-keys"));
            var initialChainCreator = new InitialChainCreator();
            initialChainCreator.CreateInitialialChain();
            initialChainCreator.StoreInitialChainToFile();
            initialChainCreator.InitialPeer.PeerWalletKeys.SaveKeys(InitialPeerWalletKeys);
        }

        // This shit works bad, but anyway it works!
        [Fact]
        public void SinglePeerIntegrationTest()
        {
            // Create and initialize peer
            Peer peer = new Peer();
            peer.LoadOrCreateWalletKeys(InitialPeerWalletKeys);
            peer.InitializeAcceptancePools();
            peer.InitializeMining();
            peer.InitializeChain();
            peer.InitializeNetworking();
            // I create transactions and mine them with the same peer
            for (int i = 0; i < 40; i++)
            {
                peer.SendCoins(peer.WalletPublicKey, 0.000000001);
            }
            Thread.Sleep(1000);
            // We just ensure, that no exceptions throws, and no dead-locks there..
        }
    }
}
