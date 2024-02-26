using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Network.NodeNet.Communication
{
    public interface INodeConnections
    {
        public List<INodeConnection> Connections();
        public void AddConnection(INodeConnection connection);
        public void RemoveConnection(INodeConnection connection);
    }
}
