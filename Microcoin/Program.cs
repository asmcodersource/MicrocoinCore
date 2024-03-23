using Microcoin.Microcoin;
using Microcoin.Microcoin.Logging;
using Serilog.Core;

Logging.InitializeLogger();

DepencyInjection.CreateContainer();
DepencyInjection.AddChainsStorage(Path.Combine(Directory.GetCurrentDirectory(), "chains-storage"));

Peer peer1 = new Peer();
peer1.LoadOrCreateWalletKeys("wallet1.keys");
peer1.InitializeAcceptancePools();
peer1.InitializeMining();
peer1.InitializeChain();
peer1.InitializeNetworking(1300);

Peer peer2 = new Peer();
peer2.LoadOrCreateWalletKeys("wallet2.keys");
peer2.InitializeAcceptancePools();
peer2.InitializeMining();
peer2.InitializeChain();
peer2.InitializeNetworking(0);
