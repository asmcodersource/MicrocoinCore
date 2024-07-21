using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.NodeNetSession.Session;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System.Text.Json;
using NodeNet.NodeNetSession.SessionMessage;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using Microcoin.Json;
using static Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession.FetcherSession;
namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public record ClaimBlockAsDownloadRootDTO
    {
        public int ClaimBlockId { get; set; }
    }

    /// <summary>
    /// Tasks performed by instances of this class search for the nearest common point from which to begin loading an unknown part of the chain. 
    /// The block does not always have to be the last one for the requester, since the requester may have a long chain that it discards.
    /// </summary>
    public class ClosestBlockRequest
    {
        public readonly FetcherSession FetcherSession;

        public ClosestBlockRequest(FetcherSession fetcherSession)
        {
            FetcherSession = fetcherSession;
        }

        public async Task<Block> CreateRequestTask(CancellationToken cancellationToken)
        {
            // If the last block of the current chain matches a block in the provider's chain, then starts downloading from this block
            var lastBlock = FetcherSession.SourceChain.GetLastBlock();
            var lastBlockIsPresented = await BlockPresentRequest(lastBlock, cancellationToken);
            if( lastBlockIsPresented is true)
            {
                ClaimBlockAsClosest(lastBlock, cancellationToken);
                return lastBlock;
            }
            // In another case, we look for a common block using binary search, knowing that the block presence function is monotonic
            int leftBlockId = 0;
            int rightBlockId = lastBlock.MiningBlockInfo.BlockId;
            Block? centralBlock = null;
            while( leftBlockId < rightBlockId)
            {
                var centralBlockId = (rightBlockId + leftBlockId) / 2; // hi, overflow
                centralBlock = FetcherSession.SourceChain.GetBlockFromHead(centralBlockId);
                var isBlockPresented = await BlockPresentRequest(centralBlock, cancellationToken);
                if (isBlockPresented)
                    leftBlockId = centralBlockId + 1;
                else
                    rightBlockId = centralBlockId - 1;
            }
            if (centralBlock is null)
                throw new ChainDownloadingException("Something wen't wrong");
/*            if ( await BlockPresentRequest(centralBlock, cancellationToken) is not true )
                throw new ChainDownloadingException("Block not found by binary algorithm");*/
            ClaimBlockAsClosest(centralBlock, cancellationToken);
            return centralBlock;
        }

        private async Task<bool> BlockPresentRequest(Block block, CancellationToken cancellationToken)
        {
            var request = new ChainBlockPresentRequestDTO()
            {
                RequestedBlockId = block.MiningBlockInfo.BlockId,
                RequestedBlockHash = block.Hash
            };
            FetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(request));
            var responseMsgContext = await FetcherSession.WrappedSession.WaitForMessage(cancellationToken);
            if (responseMsgContext is null)
                throw new ChainDownloadingException("Bad response in block present request");
            var responseObject = MessageContextHelper.Parse<ChainBlockPresentResponseDTO>(responseMsgContext);
            if( responseObject is null )
                throw new ChainDownloadingException("Bad response in block present request");
            return responseObject.IsPresented;
        }

        private void ClaimBlockAsClosest(Block block, CancellationToken cancellationToken)
        {
            var claimRequest = new ClaimBlockAsDownloadRootDTO() { ClaimBlockId = block.MiningBlockInfo.BlockId };
            FetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(claimRequest));
        }
    }
}
