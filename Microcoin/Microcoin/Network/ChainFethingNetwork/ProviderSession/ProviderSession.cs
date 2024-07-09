using NodeNet.NodeNetSession.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    internal class ProviderSession
    {
        public event Action<ProviderSession>? SessionFinishedSuccesful;
        public event Action<ProviderSession>? SessionFinishedFaulty;
        private readonly Session wrappedSession;

        public ProviderSession(Session session)
        {
            wrappedSession = session;
        }
    }
}
