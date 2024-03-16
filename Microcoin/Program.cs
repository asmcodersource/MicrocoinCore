using Microcoin;
using Microcoin.Network.NodeNet;
using System.Text;
using Tests.NodeNetNetworkConnections;

/*Peer peer = new Peer();
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
Console.WriteLine("Ok");*/


string? data;
Node first_node, second_node;
NodeNetNetworkConnections nodeNetNetworkConnections = new NodeNetNetworkConnections();
nodeNetNetworkConnections.CreateNetworkPeers(50);
nodeNetNetworkConnections.CreateNetworkTree(4);
nodeNetNetworkConnections.PerformRandomConnections(0);

do
{
    first_node = nodeNetNetworkConnections.GetRandomNode();
    second_node = nodeNetNetworkConnections.GetRandomNode();
} while (first_node == second_node);

StringBuilder stringBuilder = new StringBuilder();
for (int i = 0; i < 512; i++)
    stringBuilder.Append((char)(Random.Shared.Next()));
data = stringBuilder.ToString();

Console.WriteLine("begin...");

void AsynchronousSending()
{
    List<Task> tasks = new List<Task>();
    for (int i = 0; i < 8192; i++)
        tasks.Add(first_node.SendMessage(data));
    Task.WhenAll(tasks).Wait();
}