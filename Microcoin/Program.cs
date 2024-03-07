using Microcoin;


Peer peer = new Peer();
Console.Write("Read or creating wallet keys...");
peer.LoadOrCreateWalletKeys("wallet.keys");
Console.WriteLine("Ok");
Console.Write("Initialize inner pipeline of handling network messages...");
peer.InitializeAcceptancePools();
Console.WriteLine("Ok");
Console.Write("Initialize network system, and execute NetworkExplore procedure...");
peer.InitializeNetworking();
Console.WriteLine("Ok");
