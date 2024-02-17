using Microcoin;

Peer peer = new Peer();
peer.LoadOrCreateWalletKeys("wallet.keys");
peer.InitializeNetworking();