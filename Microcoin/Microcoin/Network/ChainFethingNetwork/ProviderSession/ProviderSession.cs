using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
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
        public readonly Session WrappedSession;
        public AbstractChain SourceChain { get; set; }

        public ProviderSession(Session session)
        {
            WrappedSession = session;
        }

        public async Task<bool> StartUploadingProcess()
        {
            CancellationTokenSource initialCommunicationCTS = new CancellationTokenSource();
            initialCommunicationCTS.CancelAfter(20_000);
            var isBlockPresented = await ChainBlockPresentHandler.CreateHandleTask(this, initialCommunicationCTS.Token);
            if (isBlockPresented is not true)
                return false;
            var closestBlock = await ClosestBlockHandler.CreateHandleTask(this, initialCommunicationCTS.Token);
            await ChainDownloadingHandler.CreateHandleTask(this, SourceChain, closestBlock, CancellationToken.None);
            return true;
        }
    }
}
