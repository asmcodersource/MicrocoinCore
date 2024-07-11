using NodeNet.NodeNetSession.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    public class ProviderSession
    {
        public event Action<ProviderSession>? SessionFinishedSuccesful;
        public event Action<ProviderSession>? SessionFinishedFaulty;
        public Session WrappedSession { get; set; }

        public ProviderSession(Session session)
        {
            WrappedSession = session;
        }
    }
}
