using Microcoin.Microcoin.Network;
using MicrocoinCore.Microcoin.Network;
using MockNetwork.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MockNetwork.Logic
{
    public class MockBroadcastNode : IBroadcastNode
    {
        public event Action<IBroadcastMessage> OnMessageReceived;
        private event Action<MockBroadcastMessage>? _onMessageSend;
        
        private readonly List<MockBroadcastNodeEndpoint> _connectedNodesEndpoints = new List<MockBroadcastNodeEndpoint>();
        private readonly Queue<TaskCompletionSource<IBroadcastMessage>> _receiveBroadcastMessagesTasks = new Queue<TaskCompletionSource<IBroadcastMessage>>();
        private readonly Queue<MockBroadcastMessage> _receivedBroadcastMessages = new Queue<MockBroadcastMessage>();
        private readonly HashSet<MockBroadcastMessage> _receivedMessages = new HashSet<MockBroadcastMessage>();

        public IEnumerable<CommunicationEndPoint> GetEndPoints()
        {
            return _connectedNodesEndpoints.ToList();
        }

        public IBroadcastMessage ReceiveBroadcastMessage()
        {
            return ReceiveBroadcastMessageAsync(CancellationToken.None).Result;
        }

        public Task<IBroadcastMessage> ReceiveBroadcastMessageAsync(CancellationToken cancellationToken = default)
        {
            lock (this)
            {
                if (_receivedBroadcastMessages.Any())
                    return Task.FromResult(_receivedBroadcastMessages.Dequeue() as IBroadcastMessage);
                
                var tcs = new TaskCompletionSource<IBroadcastMessage>();
                _receiveBroadcastMessagesTasks.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void SendBroadcastMessage(object message, string? type, int ttl)
        {
            SendBroadcastMessageAsync(message, type, ttl, CancellationToken.None).Wait();
        }


        public Task SendBroadcastMessageAsync(object message, string? type, int ttl, CancellationToken cancellationToken)
        {
            var broadcastMessage = new MockBroadcastMessage()
            {
                Payload = message is string ? message as string : JsonSerializer.Serialize(message),
                PayloadType = type ?? ""
            };
            lock (this)
            {
                _receivedMessages.Add(broadcastMessage);
                Task.Run(() => _onMessageSend?.Invoke(broadcastMessage));
            }
            return Task.CompletedTask;
        }

        public async Task BeaconBroadcastMessageAsync(MockBroadcastMessage message)
        {
            Task.Run(() => _onMessageSend?.Invoke(message));
        }

        public void Connect(MockBroadcastNode serverNode)
        {
            _connectedNodesEndpoints.Add(new MockBroadcastNodeEndpoint(serverNode));
            serverNode._onMessageSend += HandleIncommingMessage;
            serverNode.HandleConnection(this);
        }

        private void HandleConnection(MockBroadcastNode clientNode)
        {
            _connectedNodesEndpoints.Add(new MockBroadcastNodeEndpoint(clientNode));
            clientNode._onMessageSend += HandleIncommingMessage;
        }

        private void HandleIncommingMessage(MockBroadcastMessage message)
        {
            lock (this)
            {
                if (_receivedMessages.Contains(message))
                    return;
                _receivedMessages.Add(message);
                OnMessageReceived?.Invoke(message);

                if (!_receiveBroadcastMessagesTasks.Any())
                {
                    _receivedBroadcastMessages.Enqueue(message);
                }
                else
                {
                    var tcs = _receiveBroadcastMessagesTasks.Dequeue();
                    tcs.SetResult(message);
                }
                Task.Run(async () => await BeaconBroadcastMessageAsync(message));
            }
        }
    }
}
