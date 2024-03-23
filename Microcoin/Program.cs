using Microcoin;
using Microcoin.Microcoin;
using Microcoin.Microcoin.Logging;
using Serilog.Core;

InitializeDepencyInjections();
// CreateInitialChain(); // Uncomment this line, if you wan't to create initial chain

void InitializeDepencyInjections()
{
    Logging.InitializeLogger();
    DepencyInjection.CreateContainer();
    DepencyInjection.AddChainsStorage(Path.Combine(Directory.GetCurrentDirectory(), "chains-storage"));
}

void CreateInitialChain()
{
    var initialChainCreator = new InitialChainCreator();
    initialChainCreator.CreateInitialialChain();
    initialChainCreator.StoreInitialChainToFile(); // use created keys file to access first coins in network
}