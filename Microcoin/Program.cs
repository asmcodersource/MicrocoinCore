using Microcoin;
using Microcoin.Microcoin;
using Microcoin.Microcoin.Logging;
using Serilog.Core;


PeerBuilder peerBuilder = new PeerBuilder();
peerBuilder.AddDefaultMiner();
peerBuilder.AddDefaultAcceptancePools();
peerBuilder.AddDefaultRules();
peerBuilder.AddNetworkNode(8000);
peerBuilder.AddChainsStorage("chains");
peerBuilder.AddChainsFetcher();
peerBuilder.AddWalletKeys();
var peer = peerBuilder.Build();
peer.PeerMining.StartMining();
peer.SendCoins("sadasd", 0.002);