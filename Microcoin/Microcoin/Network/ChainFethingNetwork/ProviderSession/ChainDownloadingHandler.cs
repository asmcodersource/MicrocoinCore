using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using NodeNet.NodeNetSession.SessionMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Json;
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
        private IEnumerator<Block> downloadingBlocks;

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
            if( isAccepted is not true )
                throw new OperationCanceledException();
            while (await AcceptBlocksDownloadingRequest(cancellationToken));
        }

        private async Task<bool> AcceptDownloadingRequest( CancellationToken cancellationToken)
        {
            var requestMsg = await ProviderSession.WrappedSession.WaitForMessage(cancellationToken);
            var requestSessionMsg = MessageContextHelper.GetSessionMessageData(requestMsg);
            var request = JsonTypedWrapper.Deserialize<ChainDownloadRequestDTO>(requestSessionMsg);
            var response = new ChainDownloadResponseDTO()
            {
                IsAccepted = true,
                RejectReason = null,
            };
            ProviderSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(response));
            downloadingBlocks = TakeBlocksOfChain(true).GetEnumerator();
            return true;
        }

        private async Task<bool> AcceptBlocksDownloadingRequest(CancellationToken cancellationToken)
        {
            var requestMsg = await ProviderSession.WrappedSession.WaitForMessage(cancellationToken);
            var requestSessionMsg = MessageContextHelper.GetSessionMessageData(requestMsg);
            var request = JsonTypedWrapper.Deserialize<RequestNextPartOfBlocks>(requestSessionMsg);
            if (request is null)
                throw new Exception("Wrong message received due chain downloading communication");
            var blocksToSend = new List<Block>();
            for( int i = 0; i < MaxBlocksPerMessage && downloadingBlocks.MoveNext(); i++)
                blocksToSend.Add(downloadingBlocks.Current);
            ProviderSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize<ICollection<Block>>(blocksToSend));
            return blocksToSend.Count > 0;
        }

        private IEnumerable<Block> TakeBlocksOfChain(bool takeUntilChainEnding)
        {
            // Find starting point of downloading
            // find last part of chain, that need to be taken from source
            var currentChain = SourceChain;
            Stack<AbstractChain> chainQueue = new Stack<AbstractChain>();
            do
            {
                if (currentChain is null)
                    throw new Exception("Something wen't wrong with finding last chain");
                chainQueue.Push(currentChain);
                currentChain = currentChain.PreviousChain;
            } while ((currentChain.EntireChainLength - 1) > StartingBlock.MiningBlockInfo.BlockId);

            // We return blocks through the yield generator, either to the end of the chain,
            // or to the requested block, depending on the parameter
            while (chainQueue.Count > 0)
            {
                var chain = chainQueue.Pop();
                foreach (var block in chain.BlocksList)
                {
                    if (block.MiningBlockInfo.BlockId <= StartingBlock.MiningBlockInfo.BlockId)
                        continue;
                    yield return block;
                    if (takeUntilChainEnding is not true && block.MiningBlockInfo.BlockId == TargetBlock.MiningBlockInfo.BlockId)
                        yield break;
                }
            }
            yield break;
        }
    }
}
