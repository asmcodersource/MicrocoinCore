using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Network.NodeNet.Communication
{
    public interface INodeListener
    {
        public event Action<INodeConnection> ConnectionOpened;
        public string GetConnectionAddress();
        public void StartListening();
        public void StopListening();
    }
}
