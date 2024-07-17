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

        public async Task<bool> StartUploadingProcess(CancellationToken generalCancellationToken)
        {
            CancellationTokenSource initialCommunicationCTS = new CancellationTokenSource(60_000);
            var initialCt = CancellationTokenSource.CreateLinkedTokenSource(generalCancellationToken, initialCommunicationCTS.Token);
            var blockPresentRequestHandler = new ChainBlockPresentHandler(this);
            var isBlockPresented = await blockPresentRequestHandler.CreateHandleTask(initialCt.Token);
            if (isBlockPresented is not true)
                return false;
            var closestBlockRequestHandler = new ClosestBlockHandler(this);
            var closestBlock = await closestBlockRequestHandler.CreateHandleTask(initialCt.Token);
            var chainDownloadingHandler = new ChainDownloadingHandler(this, SourceChain, closestBlock, blockPresentRequestHandler.TargetBlock);
            await chainDownloadingHandler.CreateHandleTask(generalCancellationToken);
            return true;
        }
    }
}
