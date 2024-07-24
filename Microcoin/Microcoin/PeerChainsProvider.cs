using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Network;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using SimpleInjector;

namespace Microcoin.Microcoin
{
    public class PeerChainsProvider
    {
        private AbstractChain? sourceChain;
        private readonly ISessionManager SessionManager;

        private ISessionListener sessionListener;
        private Task? sessionListeningTask;
        private CancellationTokenSource? sessionListeningCTS;
        private readonly string listeningResource = "chains-providing";

        public PeerChainsProvider(Container container)
        {
            SessionManager = container.GetInstance<ISessionManager>();
            sessionListener = SessionManager.CreateListener();
        }

        public void ChangeSourceChain(AbstractChain sourceChain)
        {
            this.sourceChain = new MutableChain(sourceChain);
        }

        public void StartListening()
        {
            lock (this)
            {
                if (sessionListeningTask is not null)
                    throw new InvalidOperationException("Session listening is already listening");
                sessionListeningCTS = new CancellationTokenSource();
                sessionListeningTask = sessionListener.StartListeningAsync(AcceptedSessionsHandler, listeningResource, sessionListeningCTS.Token);
            }
        }

        public void StopListening()
        {
            lock (this)
            {
                if (sessionListeningTask is not null && sessionListeningCTS is not null)
                {
                    sessionListeningCTS.Cancel();
                    sessionListeningTask.Wait();
                    sessionListeningCTS = null;
                    sessionListeningTask = null;
                }
            }
        }

        public bool IsListening()
        {
            return sessionListeningTask is not null;
        }

        private async Task AcceptedSessionsHandler(ISessionConnection sessionConnection)
        {
            if (sourceChain is null)
                throw new InvalidOperationException("Provider don't have initialized source chain to share");
            var providerSession = new ProviderSession(sessionConnection, sourceChain);
            await providerSession.StartUploadingProcess(CancellationToken.None);
        }
    }
}
