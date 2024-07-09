using Microcoin;
using Microcoin.Microcoin;
using Microcoin.Microcoin.Logging;
using Serilog.Core;

InitializeDepencyInjections();
//CreateInitialChain(); // Uncomment this line, if you wan't to create initial chain

Peer peer = new Peer();
peer.LoadOrCreateWalletKeys("wallet.keys");
peer.InitializeAcceptancePools();
peer.InitializeMining();
peer.InitializeChain();
peer.InitializeNetworking(0);

void InitializeDepencyInjections()
{
    Logging.InitializeLogger();
    DepencyInjection.CreateContainer();
    DepencyInjection.AddChainsStorage(Path.Combine(Directory.GetCurrentDirectory(), "chains-storage"));
}

void CreateInitialChain()
{
    Serilog.Log.Verbose("Creating new initial chain");
    var initialChainCreator = new InitialChainCreator();
    initialChainCreator.CreateInitialialChain();
    initialChainCreator.StoreInitialChainToFile(); // use created keys file to access first coins in network
    Serilog.Log.Verbose("Creating new initial chain finished");
}