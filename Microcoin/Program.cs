using Microcoin.Microcoin;
using Serilog;
using Serilog.Core;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("log.info")
    .CreateLogger();


Log.Logger.Information($"Microcoin started");

Peer peer = new Peer();
Console.Write("Initialize wallet keys...");
peer.LoadOrCreateWalletKeys("wallet.keys");
Console.WriteLine("Ok");
Console.Write("Initialize inner pipeline of handling network messages...");
peer.InitializeAcceptancePools();
Console.WriteLine("Ok");
Console.Write("Initialize mining...");
peer.InitializeMining();
Console.WriteLine("Ok");
Console.Write("Initialize network system, and execute NetworkExplore procedure...");
peer.InitializeNetworking();
Console.WriteLine("Ok");
