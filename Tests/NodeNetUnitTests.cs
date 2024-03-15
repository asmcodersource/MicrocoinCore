using Microcoin.Network.NodeNet;
using Microcoin.RSAEncryptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tests
{
    public record ConnectionPair
    {
        Node node1;
        Node node2;
        string info;

        public ConnectionPair(Node first, Node second, string info)
        {
            node1 = first;
            node2 = second;
            this.info = info;
        }
    }

    /// <summary>
    /// Utility for creating network of connections between NodeNet peers
    /// for testing main features of NodeNet system
    /// </summary>
    public class NodeNetNetworkConnections: IDisposable
    {
        static public NodeNetNetworkConnections Shared { get; protected set; }
        static protected int portIterator = 6000; // used to give uniq socket port for every nodenet peer
        public List<Node>? Nodes { get; protected set; } = null;
        public List<ConnectionPair> ConnectionsList = new List<ConnectionPair>();


        static NodeNetNetworkConnections()
        {
            // Create for test performing
            NodeNetNetworkConnections nodeNetNetworkConnections = new NodeNetNetworkConnections();
            nodeNetNetworkConnections.CreateNetworkPeers(100);
            nodeNetNetworkConnections.CreateNetworkTree(4);
            nodeNetNetworkConnections.PerformRandomConnections(0);
            Shared = nodeNetNetworkConnections;
        }

        public void CreateNetworkPeers(int peersCount)
        {
            if (Nodes is not null)
                throw new Exception("Network already initialized");
            if (peersCount == 0)
                throw new Exception("Are you sure about that? peersCount is 0");

            Nodes = new List<Node>();
            for (int i = 0; i < peersCount; i++)
            {
                var peer = Node.CreateRSAHttpNode(
                    RSAEncryption.CreateSignOptions(),
                    new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(portIterator)
                );
                Nodes.Add(peer);
                portIterator++;
            }
        }

        /// <summary>
        /// Creates tree of network connections between NodeNet peers
        /// </summary>
        public void CreateNetworkTree( int maxConnectionsPerNode )
        {
            if (Nodes is null)
                throw new Exception("Network is not initialized");

            int firstPeerOffset = 0;
            for ( int  layer_i = 0; firstPeerOffset < Nodes.Count(); layer_i++)
            {
                int peersOnLayer = (int)Math.Pow(maxConnectionsPerNode, layer_i);
                for( int firstPeerId = firstPeerOffset; firstPeerId < peersOnLayer + firstPeerOffset; firstPeerId++)
                {
                    if (firstPeerId >= Nodes.Count())
                        break;
                    var firstPeer = Nodes[firstPeerId];
                    var layerLocalId = firstPeerId - firstPeerOffset;
                    var secondPeerOffset = (firstPeerOffset + peersOnLayer) + (layerLocalId * maxConnectionsPerNode); 
                    for( int secondPeerId = secondPeerOffset; secondPeerId < maxConnectionsPerNode + secondPeerOffset; secondPeerId++)
                    {
                        if (secondPeerId >= Nodes.Count())
                            break;
                        var secondPeer = Nodes[secondPeerId];
                        var success = firstPeer.Connect($"127.0.0.1:{secondPeer.GetNodeTcpPort()}");
                        Assert.True(success);
                        ConnectionsList.Add(new ConnectionPair(firstPeer, secondPeer, $"{firstPeerId} - {secondPeerId}"));
                    }
                }
                firstPeerOffset = firstPeerOffset + peersOnLayer;
            }
        }

        public void PerformRandomConnections( int randomConnectionsCount )
        {
            for ( int i = 0; i < randomConnectionsCount; i++)
            {
                var firstPeer = Nodes[Random.Shared.Next(Nodes.Count())];
                var secondPeer = Nodes[Random.Shared.Next(Nodes.Count())];
                if( secondPeer == firstPeer)
                {
                    i--;
                    continue;
                }
                firstPeer.Connect($"127.0.0.1:{secondPeer.GetNodeTcpPort()}");
            }
        }

        public Node GetRandomNode()
        {
            return Nodes[Random.Shared.Next(Nodes.Count())];
        }
        
        public void Dispose() 
        {
            foreach (Node node in Nodes)
                node.Close();
        }
    }


    public class NodeNetConnection : IDisposable
    {
        public Node first_node { get; protected set; }
        public Node second_node { get; protected set; }

        public bool IsConnectionSuccess { get; protected set; }

        public NodeNetConnection()
        {
            first_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1333)
            );
            second_node = Node.CreateRSAHttpNode(
                RSAEncryption.CreateSignOptions(),
                new Microcoin.Network.NodeNet.TcpCommunication.TcpListenerOptions(1334)
            );
            IsConnectionSuccess = first_node.Connect("127.0.0.1:1334");
        }

        public void Dispose()
        {
            first_node.Close();
            second_node.Close();
        }
    }


    public class NodeNetUnitTests
    {
        static object wallLock = new object();

        [Fact]
        public void NodeNet_Communications_Messaging_SingleMessage_Test()
        {
            lock (wallLock)
            {
                using (var connections = new NodeNetConnection())
                {
                    Node first_node = connections.first_node;
                    Node second_node = connections.second_node;

                    string message = "Example message for testing";
                    int receivedMessagesCount = 0;
                    second_node.MessageReceived += (msgContext) =>
                    {
                        receivedMessagesCount++;
                        Assert.True(msgContext.Message.Data == message);
                    };
                    first_node.MessageReceived += (msgContext) =>
                    {
                        receivedMessagesCount++;
                        Assert.True(msgContext.Message.Data == message);
                    };

                    first_node.SendMessage(message);
                    second_node.SendMessage(message);

                    Thread.Sleep(50);

                    Assert.Equal(2, receivedMessagesCount);
                }
            }
        }


        [Fact]
        public void NodeNet_Communications_Messaging_MultipleMessage_Test()
        {
            lock(wallLock) 
            {
                using (var connections = new NodeNetConnection())
                {
                    Node first_node = connections.first_node;
                    Node second_node = connections.second_node;
                    int first_received_summary = 0;
                    int second_received_summary = 0;
                    int sending_summary = 0;
                    first_node.MessageReceived += (msgcontext) => { first_received_summary += Convert.ToInt32(msgcontext.Message.Data); };
                    second_node.MessageReceived += (msgcontext) => { second_received_summary += Convert.ToInt32(msgcontext.Message.Data); };
                    for (int i = 0; i < 1024; i++)
                    {
                        sending_summary += i;
                        first_node.SendMessage(i.ToString());
                        second_node.SendMessage((1023-i).ToString());
                    }

                    Thread.Sleep(100);

                    Assert.Equal(sending_summary, first_received_summary);
                    Assert.Equal(sending_summary, second_received_summary);
                }
            }
        }


        [Fact]
        public void NodeNet_Communcations_Messaging_NetworkTest()
        {
            // Create for test performing
            NodeNetNetworkConnections nodeNetNetworkConnections = NodeNetNetworkConnections.Shared;

            // Verifies that data passes through the network from sender to recipient
            List<Task> tasks = new List<Task>();
            for( int i = 0; i < 50; i++)
            {
                var firstPeer = nodeNetNetworkConnections.GetRandomNode();
                var secondPeer = nodeNetNetworkConnections.GetRandomNode();
                if( firstPeer == secondPeer)
                {
                    i--;
                    continue;
                }
                var task = Task.Run(() => TestBroadcastConnectionBetweenNodes(firstPeer, secondPeer));
                tasks.Add(task);
            }
            Task.WhenAll(tasks).Wait();
        }

        protected void TestBroadcastConnectionBetweenNodes(Node first_node, Node second_node)
        {
            object atomicLock = new object();
            string message = Random.Shared.Next().ToString();
            int receivedMessagesCount = 0;
            second_node.PersonalMessageReceived += (msgContext) =>
            {
                if (msgContext.Message.Data == message)
                    lock (atomicLock)
                        receivedMessagesCount |= 1;
            };
            first_node.PersonalMessageReceived += (msgContext) =>
            {
                if (msgContext.Message.Data == message)
                    lock (atomicLock)
                        receivedMessagesCount |= 2;
            };

            first_node.SendMessage(message, second_node.SignOptions.PublicKey);
            second_node.SendMessage(message, first_node.SignOptions.PublicKey);
            Thread.Sleep(50);
            Assert.Equal(3, receivedMessagesCount);
        }
    }
}
