using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    public class ProviderSession
    {
        public event Action<ProviderSession>? SessionFinishedSuccesful;
        public event Action<ProviderSession>? SessionFinishedFaulty;
        public readonly ISessionConnection WrappedSession;
        public AbstractChain SourceChain { get; set; }

        public ProviderSession(ISessionConnection session, AbstractChain sourceChain)
        {
            WrappedSession = session;
            SourceChain = sourceChain;
        }

        public async Task<bool> StartUploadingProcess(CancellationToken generalCancellationToken)
        {
            CancellationTokenSource initialCommunicationCTS = new CancellationTokenSource(600_000);
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
