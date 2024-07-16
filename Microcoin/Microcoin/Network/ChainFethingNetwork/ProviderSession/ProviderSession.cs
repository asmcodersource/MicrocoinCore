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
            var blockPresentRequestHandler = new ChainBlockPresentHandler(this);
            var isBlockPresented = await blockPresentRequestHandler.CreateHandleTask(initialCommunicationCTS.Token);
            if (isBlockPresented is not true)
                return false;
            var closestBlockRequestHandler = new ClosestBlockHandler(this);
            var closestBlock = await closestBlockRequestHandler.CreateHandleTask(initialCommunicationCTS.Token);
            // var chainDownloadingHandler = new ChainDownloadingHandler(this, SourceChain, closestBlock, );
            // await chainDownloadingHandler.CreateHandleTask(CancellationToken.None);
            return true;
        }
    }
}
