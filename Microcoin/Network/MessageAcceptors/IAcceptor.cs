using Microcoin.Network.NodeNet.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Network.MessageAcceptors
{
    /// <summary>
    /// Acceptor it is entity that handle any type of NodeNet messages in context of Microcoin
    /// It is entry point to begin handle transactions, blocks, or other messages of network
    /// </summary>
    internal interface IAcceptor
    {
        public Task Handle(MessageContext messageContext);
    }
}
