using Microcoin.Microcoin.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockNetwork.Logic
{
    public class MockSessionConnection : ISessionConnection
    {
        public CommunicationEndPoint EndPoint => throw new NotImplementedException();

        public IMessage ReceiveMessage()
        {
            throw new NotImplementedException();
        }

        public Task<IMessage> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool SendMessage(object message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendMessageAsync(object message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
