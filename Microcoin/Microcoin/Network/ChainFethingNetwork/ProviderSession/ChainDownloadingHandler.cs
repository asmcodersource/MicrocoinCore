using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Json;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    public enum ChainDownloadRejectReason
    {
        Unknown,
        RequestedChainToLong,
    };

    public record ChainDownloadResponseDTO
    {
        public bool IsAccepted { get; set; }
        public ChainDownloadRejectReason? RejectReason { get; set; }
    }

    public class ChainDownloadingHandler
    {
        public int MaxBlocksPerMessage { get; private set; }
        public readonly ProviderSession ProviderSession;
        public readonly AbstractChain SourceChain;
        public readonly Block StartingBlock;
        public readonly Block TargetBlock;
        private IEnumerator<Block>? downloadingBlocks;

        public ChainDownloadingHandler(ProviderSession providerSession, AbstractChain sourceChain, Block startingBlock, Block targetBlock, int maxBlocksPerMessage = 50)
        {
            ProviderSession = providerSession;
            SourceChain = sourceChain;
            StartingBlock = startingBlock;
            TargetBlock = targetBlock;
            SetMaxBlocksPerMessage(maxBlocksPerMessage);
        }

        public void SetMaxBlocksPerMessage(int maxBlocksPerMessage)
        {
            if (maxBlocksPerMessage <= 0)
                throw new Exception("Count of blocks per message must be more than zero");
            MaxBlocksPerMessage = maxBlocksPerMessage;
        }

        public async Task CreateHandleTask(CancellationToken cancellationToken)
        {
            var isAccepted = await AcceptDownloadingRequest(cancellationToken);
            if (isAccepted is not true)
                throw new OperationCanceledException();
            while (await AcceptBlocksDownloadingRequest(cancellationToken)) ;
        }

        private async Task<bool> AcceptDownloadingRequest(CancellationToken cancellationToken)
        {
            var requestMsg = await ProviderSession.WrappedSession.ReceiveMessageAsync(cancellationToken);
            var request = JsonTypedWrapper.Deserialize<ChainDownloadRequestDTO>(requestMsg.Payload);
            var response = new ChainDownloadResponseDTO()
            {
                IsAccepted = true,
                RejectReason = null,
            };
            downloadingBlocks = SourceChain.GetEnumerable(StartingBlock).GetEnumerator();
            ProviderSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(response));
            return true;
        }

        private async Task<bool> AcceptBlocksDownloadingRequest(CancellationToken cancellationToken)
        {
            var requestMsg = await ProviderSession.WrappedSession.ReceiveMessageAsync(cancellationToken);
            var request = JsonTypedWrapper.Deserialize<RequestNextPartOfBlocks>(requestMsg.Payload);
            var blocksToSend = new List<Block>();
            for (int i = 0; i < MaxBlocksPerMessage && downloadingBlocks.MoveNext(); i++)
            {
                if (downloadingBlocks.Current == null)
                    break;
                blocksToSend.Add(downloadingBlocks.Current);
            }
            ProviderSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize<ICollection<Block>>(blocksToSend));
            return blocksToSend.Count > 0;
        }
    }
}
