﻿PeerBuilder peerBuilder = new PeerBuilder();
peerBuilder.AddDefaultMiner();
peerBuilder.AddDefaultAcceptancePools();
peerBuilder.AddDefaultRules();
peerBuilder.AddChainsStorage("chains");
peerBuilder.AddChainsFetcher();
peerBuilder.AddWalletKeys();
var peer = peerBuilder.Build();
peer.PeerMining.StartMining();
peer.SendCoins("sadasd", 0.002);