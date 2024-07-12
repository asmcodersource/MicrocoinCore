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
            CancellationTokenSource initialCTS = new CancellationTokenSource();
            //initialCTS.CancelAfter(10_000);
            var isBlockPresented = await ChainBlockPresentHandler.CreateHandleTask(this, initialCTS.Token);
            if (isBlockPresented is not true)
                throw new OperationCanceledException();
            var closestBlock = await ClosestBlockHandler.CreateHandleTask(this, initialCTS.Token);
            throw new NotImplementedException();
        }
    }
}
