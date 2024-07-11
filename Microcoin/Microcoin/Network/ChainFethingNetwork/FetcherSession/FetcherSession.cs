using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainStorage;
using NodeNet.NodeNetSession.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public class FetcherSession
    {
        public event Action<AbstractChain>? ChainFetched;
        public event Action<FetcherSession>? SessionFinishedSuccesful;
        public event Action<FetcherSession>? SessionFinishedFaulty;
        public Session WrappedSession { get; set; }

        public FetcherSession(Session session)
        {
            WrappedSession = session;
        }
    }
}
