using Microcoin.Microcoin;
using Microcoin.Microcoin.Logging;


Logging.InitializeLogger();
Peer peer = new Peer();
peer.LoadOrCreateWalletKeys("wallet.keys");
peer.InitializeAcceptancePools();
peer.InitializeMining();
peer.InitializeNetworking();
