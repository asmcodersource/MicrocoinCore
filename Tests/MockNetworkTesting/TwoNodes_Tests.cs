using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Tests.NetworkTesting.TestNetworks;
using Microcoin.Microcoin.Network;
using Tests.MockNetworkTesting;

namespace Tests.NetworkTesting
{
    public class TwoNodes_Tests
    {
        [Fact]
        public async Task PingPong_Communication_Test()
        {
            var network = TwoNodesNetwork.Create();
            IBroadcastMessage pingMessage = null!, pongMessage = null!;
            const string ping = "Ping";
            const string pong = "Pong";

            var communicationTestAction = async () =>
            {
                await network.FirstNode.SendBroadcastMessageAsync(ping, "Message", 128, CancellationToken.None);
                pingMessage = await network.SecondNode.ReceiveBroadcastMessageAsync(CancellationToken.None);

                await network.SecondNode.SendBroadcastMessageAsync(pong, "Message", 128, CancellationToken.None);
                pongMessage = await network.FirstNode.ReceiveBroadcastMessageAsync(CancellationToken.None);
            };

            await Helpers.RunWithTimeout(communicationTestAction, Consts.TimeoutForCommunication, "Communication timeout");
            if (pingMessage.Payload != ping)
                Assert.True(pingMessage.Payload == ping, "Received messaged is different from sent");
            if (pongMessage.Payload != pong)
                Assert.True(pongMessage.Payload == pong, "Received messaged is different from sent");
        }

        [Fact]
        public async Task ReceivedSum_Communication_Test()
        {
            var network = TwoNodesNetwork.Create();
            int firstNodeReceiveSum = 0;
            int secondNodeReceiveSum = 0;
            int firstNodeExpectedReceiveSum = 0;
            int secondNodeExpectedReceiveSum = 0;

            var communicationTestAction = async () =>
            {
                var firstNodeSendingTask = Task.Run(async () =>
                {
                    for (int i = 0; i < 512; i++)
                    {
                        var sendingNumber = 1;
                        Volatile.Write(ref secondNodeExpectedReceiveSum, secondNodeExpectedReceiveSum + sendingNumber);
                        await network.FirstNode.SendBroadcastMessageAsync(
                            sendingNumber.ToString(),
                            "Message",
                            128,
                            CancellationToken.None
                        );
                    }
                });
                var secondNodeSendingTask = Task.Run(async () =>
                {
                    for (int i = 0; i < 512; i++)
                    {
                        var sendingNumber = 1;
                        Volatile.Write(ref firstNodeExpectedReceiveSum, firstNodeExpectedReceiveSum + sendingNumber);
                        await network.SecondNode.SendBroadcastMessageAsync(
                            sendingNumber.ToString(),
                            "Message",
                            128,
                            CancellationToken.None
                        );
                    }
                });
                var firstNodeReceivingTask = Task.Run(async () =>
                {
                    for (int i = 0; i < 512; i++)
                    {
                        var message = await network.FirstNode.ReceiveBroadcastMessageAsync(CancellationToken.None);
                        Volatile.Write(ref firstNodeReceiveSum, firstNodeReceiveSum + Convert.ToInt32(message.Payload));
                    }
                });
                var secondNodeReceivingTask = Task.Run(async () =>
                {
                    for (int i = 0; i < 512; i++)
                    {
                        var message = await network.SecondNode.ReceiveBroadcastMessageAsync(CancellationToken.None);
                        Volatile.Write(ref secondNodeReceiveSum, secondNodeReceiveSum + Convert.ToInt32(message.Payload));
                    }
                });

                await Task.WhenAll(firstNodeReceivingTask, secondNodeReceivingTask, firstNodeSendingTask, secondNodeSendingTask);
            };
            await Helpers.RunWithTimeout(communicationTestAction, Consts.TimeoutForCommunication, "Communication timeout");

            Assert.Equal(Volatile.Read(ref firstNodeReceiveSum), Volatile.Read(ref firstNodeExpectedReceiveSum));
            Assert.Equal(Volatile.Read(ref secondNodeReceiveSum), Volatile.Read(ref secondNodeExpectedReceiveSum));
        }
    }
}
