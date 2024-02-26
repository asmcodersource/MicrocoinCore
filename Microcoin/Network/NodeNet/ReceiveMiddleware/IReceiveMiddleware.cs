using Microcoin.Network.NodeNet.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Network.NodeNet.ReceiveMiddleware
{
    // Every middleware have to perform some actions on received message
    // Its separate logic of different actions, and relations
    // Last middleware returns true if message have to be accepted
    public interface IReceiveMiddleware
    {
        public bool Invoke(MessageContext messageContext);
        public void SetNext(IReceiveMiddleware next);
    }
}
