using Microcoin.Microcoin.Network;
using MockNetwork.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MockNetwork.Logic
{
    public class MockSessionConnection : ISessionConnection
    {
        private readonly Queue<TaskCompletionSource<ISessionMessage>> _receiveBroadcastMessagesTasks = new Queue<TaskCompletionSource<ISessionMessage>>();
        private readonly Queue<MockSessionMessage> _receivedBroadcastMessages = new Queue<MockSessionMessage>();
        
        public MockSessionConnection OppositeSideConnection { get; set; }

        public ICommunicationEndPoint EndPoint => throw new NotImplementedException();

        public ISessionMessage ReceiveMessage()
        {
            return ReceiveMessageAsync(CancellationToken.None).Result;
        }

        public Task<ISessionMessage> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (_receivedBroadcastMessages.Any())
                    return Task.FromResult(_receivedBroadcastMessages.Dequeue() as ISessionMessage);

                var tcs = new TaskCompletionSource<ISessionMessage>();
                _receiveBroadcastMessagesTasks.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public bool SendMessage(object message)
        {
            return SendMessageAsync(message, CancellationToken.None).Result;
        }

        public async Task<bool> SendMessageAsync(object message, CancellationToken cancellationToken)
        {
            lock (this)
            {
                var sessionMessage = new MockSessionMessage()
                {
                    Payload = message is string ? message as string : JsonSerializer.Serialize(message),
                    PayloadType = "" // I don`t remember why this field exists, and never using...
                };
                Task.Run(() => OppositeSideConnection.HandleIncommingMessage(sessionMessage));
            }
            return true;
        }

        private void HandleIncommingMessage(MockSessionMessage message)
        {
            lock (this)
            {
                if (!_receiveBroadcastMessagesTasks.Any())
                {
                    _receivedBroadcastMessages.Enqueue(message);
                }
                else
                {
                    var tcs = _receiveBroadcastMessagesTasks.Dequeue();
                    tcs.SetResult(message);
                }
            }
        }
    }
}
