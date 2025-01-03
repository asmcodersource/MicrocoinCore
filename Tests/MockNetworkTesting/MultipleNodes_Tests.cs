using Microcoin.Microcoin.Network;
using MicrocoinCore.Microcoin.Network;
using MockNetwork.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Tests.MockNetworkTesting;
using Tests.NetworkTesting.TestNetworks;
using Xunit;

namespace Tests.NetworkTesting
{
    public class MultipleNodes_Tests
    {

        [Fact]
        public async Task Broadcast_Neighbor_DiffLevel_Communication_Test()
        {
            int levels = 4;
            var network = MultipleNodesNetwork.Create(levels);
            await Broadcast_Test_Communication_Between_Nodes(network.NetworkLevels[0][0], network.NetworkLevels[1][0]);
        }

        [Fact]
        public async Task Broadcast_Neighbor_SameLevel_Communication_Test()
        {
            int levels = 4;
            var network = MultipleNodesNetwork.Create(levels);
            await Broadcast_Test_Communication_Between_Nodes(network.NetworkLevels[1][0], network.NetworkLevels[1][1]);
        }

        [Fact]
        public async Task Broadcast_RootToLeaf_Communication_Test()
        {
            int levels = 4;
            var network = MultipleNodesNetwork.Create(levels);
            await Broadcast_Test_Communication_Between_Nodes(network.NetworkLevels[0][0], network.NetworkLevels[levels - 1][0]);
        }

        [Fact]
        public async Task Broadcast_LeafToLeaf_Communication_Test()
        {
            int levels = 4;
            var network = MultipleNodesNetwork.Create(levels);
            await Broadcast_Test_Communication_Between_Nodes(network.NetworkLevels[levels - 1][0], network.NetworkLevels[levels - 1][(int)Math.Pow(2, levels - 1) - 1]);
        }

        private async Task Broadcast_Test_Communication_Between_Nodes(IBroadcastNode node1, IBroadcastNode node2)
        {
            IBroadcastMessage pingMessage = null!, pongMessage = null!;
            string pingSignature = Guid.NewGuid().ToString();
            string pongSignature = Guid.NewGuid().ToString();

            var communicationTestAction = async () =>
            {
                await node1.SendBroadcastMessageAsync(pingSignature, "Message", 128, CancellationToken.None);
                do {
                    pingMessage = await node2.ReceiveBroadcastMessageAsync(CancellationToken.None);
                } while (pingMessage.Payload != pingSignature);

                await node2.SendBroadcastMessageAsync(pongSignature, "Message", 128, CancellationToken.None);
                do
                {
                    pongMessage = await node1.ReceiveBroadcastMessageAsync(CancellationToken.None);
                } while (pongMessage.Payload != pongSignature);
            };

            await Helpers.RunWithTimeout(communicationTestAction, Consts.TimeoutForCommunication, "Communication timeout");

        }
    }
}
