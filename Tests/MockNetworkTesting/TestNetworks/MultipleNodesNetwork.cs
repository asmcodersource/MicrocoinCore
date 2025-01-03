using Microcoin.Microcoin.Network;
using MicrocoinCore.Microcoin.Network;
using MockNetwork.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.NetworkTesting.TestNetworks
{
    internal class MultipleNodesNetwork
    {
        public readonly IReadOnlyList<IReadOnlyList<IBroadcastNode>> NetworkLevels = null; 

        private MultipleNodesNetwork(List<List<IBroadcastNode>> networkLevels)
        {
            NetworkLevels = networkLevels;
        }

        public static MultipleNodesNetwork Create(int levelsCount)
        {
            var networkLevels = new List<List<IBroadcastNode>>();
            var firstLevel = new List<IBroadcastNode>() { new MockBroadcastNode() };

            networkLevels.Add(firstLevel);
            for(int level = 1; level < levelsCount; level++)
            {
                var networkLevel = new List<IBroadcastNode>();
                int nodesOnCurrentLevel = (int)Math.Pow(2, level);
                for(int nodeI = 0; nodeI < nodesOnCurrentLevel; nodeI++)
                {
                    var broadcastNode = new MockBroadcastNode();
                    networkLevel.Add(broadcastNode);
                    broadcastNode.Connect((MockBroadcastNode)networkLevels.Last()[nodeI / 2]);
                }
                networkLevels.Add(networkLevel);
            }

            return new MultipleNodesNetwork(networkLevels);
        }
    }
}
