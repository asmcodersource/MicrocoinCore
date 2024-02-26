using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Network.NodeNet.Communication
{
    public interface INodeSender
    {
        public void SendMessage(Message.Message message);
    }
}
