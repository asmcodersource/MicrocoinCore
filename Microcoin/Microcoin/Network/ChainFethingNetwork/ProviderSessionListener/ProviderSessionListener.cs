using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.NodeNet;
using NodeNet.NodeNetSession.Session;
using NodeNet.NodeNetSession.SessionListener;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using Microcoin.Microcoin.Blockchain.Chain;
using SimpleInjector;
using NodeNet.NodeNet.Communication;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSessionListener
{
    public class ProviderSessionListener
    {
        public event Action<ProviderSession.ProviderSession> NewProviderSessionCreated = null!;
        public AbstractChain? SourceChain { get; set; }
        private readonly Node communicationNode;
        private readonly SessionListener sessionListener;

        public ProviderSessionListener(Container servicesContainer)
        {
            this.communicationNode = servicesContainer.GetInstance<Node>();
            sessionListener = new SessionListener(communicationNode, "chain-fetching");
            sessionListener.NewSessionCreated += NewSessionHandler;
        }

        public void StartListening()
        {
            sessionListener.StartListening();
        }

        public void StopListening()
        {
            sessionListener.StopListening();
        }

        protected void NewSessionHandler(Session newSession)
        {
            var providerSession = new ProviderSession.ProviderSession(newSession);
            providerSession.SourceChain = SourceChain;
            providerSession.StartUploadingProcess(CancellationToken.None);
            NewProviderSessionCreated?.Invoke(providerSession);
        }
    }
}
