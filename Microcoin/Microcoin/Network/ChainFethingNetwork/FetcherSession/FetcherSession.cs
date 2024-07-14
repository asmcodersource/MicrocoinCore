using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin.Blockchain.Block;
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
        public Session WrappedSession { get; set; }

        public event Action<AbstractChain>? ChainFetched;
        public event Action<FetcherSession>? SessionFinishedSuccesful;
        public event Action<FetcherSession>? SessionFinishedFaulty;

        public readonly AbstractChain SourceChain;
        public readonly FetchRequest FetchRequest;

        public FetcherSession(Session session, AbstractChain sourceChain, FetchRequest fetchRequest)
        {
            WrappedSession = session;
            this.SourceChain = sourceChain;
            this.FetchRequest = fetchRequest;
        }

        public async Task<AbstractChain> StartDonwloadingProccess(CancellationToken generalCancellationToken)
        {
            CancellationTokenSource initialCommunicationCTS = new CancellationTokenSource();
            initialCommunicationCTS.CancelAfter(25_000);
            var isBlockPresented = await ChainBlockPresentRequest.CreateRequestTask(this, initialCommunicationCTS.Token);
            if (isBlockPresented is not true)
                throw new OperationCanceledException();
            var closestBlock = await ClosestBlockRequest.CreateRequestTask(this, initialCommunicationCTS.Token);
            var truncatedChain = CreateTrunkedChain(closestBlock);
            var downloadedChain = await ChainDownloadingRequest.CreateRequestTask(this, truncatedChain, generalCancellationToken);
            return downloadedChain;
        }

        private MutableChain CreateTrunkedChain(Block lastBlock)
        {
            throw new NotImplementedException();
        }
    }
}
