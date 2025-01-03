using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrocoinCore.Microcoin.Network;
using MockNetwork.Logic;

namespace Tests.NetworkTesting.TestNetworks
{
    internal class TwoNodesNetwork
    {
        public IBroadcastNode FirstNode { get; init; }
        public IBroadcastNode SecondNode { get; init; }

        private TwoNodesNetwork(MockBroadcastNode firstNode, MockBroadcastNode secondNode)
        {
            this.FirstNode = firstNode;
            this.SecondNode = secondNode;
        }

        public static TwoNodesNetwork Create()
        {
            var firstNode = new MockBroadcastNode();
            var secondNode = new MockBroadcastNode();
            var network = new TwoNodesNetwork(firstNode, secondNode);
            firstNode.Connect(secondNode);
            return network;
        }
    }
}
