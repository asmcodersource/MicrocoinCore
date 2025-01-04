using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Tests.NetworkTesting.TestNetworks;
using Microcoin.Microcoin.Network;
using Tests.MockNetworkTesting;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;

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
                        var sendingNumber = Random.Shared.Next(1);
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
                        var sendingNumber = Random.Shared.Next(1);
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

        [Fact]
        public async Task Session_PingPong_Communication_Test()
        {
            string resource = "ping-pong";
            string pingMessage = "ping";
            string pongMessage = "pong";
            var network = TwoNodesNetwork.Create();

            // Start listening
            var listener = network.FirstNode.CreateListener();
            var listeningTask = listener.StartListeningAsync(SessionServerSideHandler, "ping-pong", CancellationToken.None);

            // Connect to the server
            var endpointToConnect = network.SecondNode.GetEndPoints().First();
            var session = await network.SecondNode.Connect(endpointToConnect, resource);

            // Start client side handler manually
            var clientHandlerTask = SessionClientSideHandler(session);

            // Wait for results
            var awaitResult = await Task.WhenAny(clientHandlerTask, Task.Delay(Consts.TimeoutForCommunication));
            Assert.True(awaitResult == clientHandlerTask, "Communication timeout");

            async Task SessionClientSideHandler(ISessionConnection sessionConnection)
            {
                await session.SendMessageAsync(pingMessage, CancellationToken.None);
                var messageFromServer = await session.ReceiveMessageAsync(CancellationToken.None);
                Assert.Equal(pongMessage, messageFromServer.Payload);
            }

            async Task SessionServerSideHandler(ISessionConnection sessionConnection)
            {
                var messageFromClient = await sessionConnection.ReceiveMessageAsync(CancellationToken.None);
                Assert.Equal(pingMessage, messageFromClient.Payload);
                await sessionConnection.SendMessageAsync(pongMessage, CancellationToken.None);
            }
        }

        [Fact]
        public async Task Session_ReceivedSum_Communication_Test()
        {
            var network = TwoNodesNetwork.Create();
            int clientNodeReceiveSum = 0;
            int serverNodeReceiveSum = 0;
            int clientNodeExpectedReceiveSum = 0;
            int serverNodeExpectedReceiveSum = 0;

            var communicationTestAction = async () =>
            {
                // Start listening
                var listener = network.FirstNode.CreateListener();
                var listeningTask = listener.StartListeningAsync(SessionServerSideHandler, "", CancellationToken.None);

                // Connect to the server
                var endpointToConnect = network.SecondNode.GetEndPoints().First();
                var session = await network.SecondNode.Connect(endpointToConnect, "");

                // Start client side handler manually
                Task serverHandlerTask = null!;
                var clientHandlerTask = SessionClientSideHandler(session);

                async Task SessionClientSideHandler(ISessionConnection sessionConnection)
                {
                    var sendingTask = SessionClientSendingHandler();
                    var receivingTask = SessionClientReceivingHandler();
                    await Task.WhenAll(sendingTask, receivingTask);

                    async Task SessionClientSendingHandler()
                    {
                        for (int i = 0; i < 512; i++)
                        {
                            var sendingNumber = 1;
                            Volatile.Write(ref serverNodeExpectedReceiveSum, serverNodeExpectedReceiveSum + sendingNumber);
                            await sessionConnection.SendMessageAsync(sendingNumber.ToString(), CancellationToken.None);
                        }
                    }
                    async Task SessionClientReceivingHandler()
                    {
                        for (int i = 0; i < 512; i++)
                        {
                            var message = await sessionConnection.ReceiveMessageAsync(CancellationToken.None);
                            Volatile.Write(ref clientNodeReceiveSum, clientNodeReceiveSum + Convert.ToInt32(message.Payload));
                        }
                    }
                }

                async Task SessionServerSideHandler(ISessionConnection sessionConnection)
                {
                    
                    var sendingTask = SessionServerSendingHandler();
                    var receivingTask = SessionServerReceivingHandler();
                    serverHandlerTask = Task.WhenAll(sendingTask, receivingTask);

                    async Task SessionServerSendingHandler()
                    {
                        for (int i = 0; i < 512; i++)
                        {
                            var sendingNumber = 1;
                            Volatile.Write(ref clientNodeExpectedReceiveSum, clientNodeExpectedReceiveSum + sendingNumber);
                            await sessionConnection.SendMessageAsync(sendingNumber.ToString(), CancellationToken.None);
                        }
                    }
                    async Task SessionServerReceivingHandler()
                    {
                        for (int i = 0; i < 512; i++)
                        {
                            var message = await sessionConnection.ReceiveMessageAsync(CancellationToken.None);
                            Volatile.Write(ref serverNodeReceiveSum, serverNodeReceiveSum + Convert.ToInt32(message.Payload));
                        }
                    }
                }

                while (serverHandlerTask == null) await Task.Yield();
                await Task.WhenAll(clientHandlerTask, serverHandlerTask);
            };
            await Helpers.RunWithTimeout(communicationTestAction, Consts.TimeoutForCommunication, "Communication timeout");

            Assert.Equal(Volatile.Read(ref clientNodeReceiveSum), Volatile.Read(ref clientNodeExpectedReceiveSum));
            Assert.Equal(Volatile.Read(ref serverNodeReceiveSum), Volatile.Read(ref serverNodeExpectedReceiveSum));
        }
    }
}
