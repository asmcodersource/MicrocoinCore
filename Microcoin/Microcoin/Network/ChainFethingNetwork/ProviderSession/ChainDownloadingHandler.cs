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
        public readonly ProviderSession ProviderSession;
        public readonly AbstractChain SourceChain;
        public readonly Block StartingBlock;
        public readonly Block TargetBlock;

        public ChainDownloadingHandler(ProviderSession providerSession, AbstractChain sourceChain, Block startingBlock, Block targetBlock)
        {
            ProviderSession = providerSession;
            SourceChain = sourceChain;
            StartingBlock = startingBlock;
            TargetBlock = targetBlock;
        }

        public async Task CreateHandleTask(CancellationToken cancellationToken)
        {
            var isAccepted = await AcceptDownloadingRequest(cancellationToken);
            if( isAccepted is not true )
                throw new OperationCanceledException();
            //while (await AcceptBlocksDownloadingRequest(cancellationToken)) ;
        }

        private async Task<bool> AcceptDownloadingRequest( CancellationToken cancellationToken)
        {
            var requestMsg = await ProviderSession.WrappedSession.WaitForMessage(cancellationToken);
            var requestSessionMsg = MessageContextHelper.GetSessionMessageData(requestMsg);
            var request = JsonTypedWrapper.Deserialize<ChainDownloadRequestDTO>(requestSessionMsg);
            
            return true;
        }

        /*private async Task<bool> AcceptBlocksDownloadingRequest(CancellationToken cancellationToken)
        {

        }*/

        private IEnumerable<Block> TakeBlocksOfChain(bool takeUntilChainEnding)
        {
            // Find starting point of downloading
            // find last part of chain, that need to be taken from source
            var currentChain = SourceChain;
            Queue<AbstractChain> chainQueue = new Queue<AbstractChain>();
            while ((currentChain.EntireChainLength - 1) > StartingBlock.MiningBlockInfo.BlockId)
            {
                currentChain = currentChain.PreviousChain;
                chainQueue.Enqueue(currentChain);
                if (currentChain is null)
                    throw new Exception("Something wen't wrong with finding last chain");
            }

            // We return blocks through the yield generator, either to the end of the chain,
            // or to the requested block, depending on the parameter
            while (chainQueue.Count >= 0)
            {
                var chain = chainQueue.Dequeue();
                foreach (var block in chain.BlocksList)
                {
                    if (block.MiningBlockInfo.BlockId <= StartingBlock.MiningBlockInfo.BlockId)
                        continue;
                    yield return block;
                    if (takeUntilChainEnding && block.MiningBlockInfo.BlockId == TargetBlock.MiningBlockInfo.BlockId)
                        yield break;
                }
            }
            yield break;
        }
    }
}
