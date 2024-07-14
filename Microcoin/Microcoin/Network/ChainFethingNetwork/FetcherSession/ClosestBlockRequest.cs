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
namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    /// <summary>
    /// Tasks performed by instances of this class search for the nearest common point from which to begin loading an unknown part of the chain. 
    /// The block does not always have to be the last one for the requester, since the requester may have a long chain that it discards.
    /// </summary>
    public class ClosestBlockRequest
    {
        public record ClaimBlockAsDownloadBeginning
        {
            public int ClaimBlockId { get; set; }
        }

        /// <summary>
        /// Creates a task that will communicate through the session to find the closest parent block for the requested chain
        /// </summary>  
        /// <returns>The block that will act as the parent for the loaded blockchain.</returns>
        public static async Task<Block> CreateRequestTask(FetcherSession fetcherSession, CancellationToken cancellationToken)
        {
            // If the last block of the current chain matches a block in the provider's chain, then starts downloading from this block
            var lastBlock = fetcherSession.SourceChain.GetLastBlock();
            var lastBlockIsPresented = await BlockPresentRequest(fetcherSession, lastBlock);
            if( lastBlockIsPresented is true)
            {
                ClaimBlockAsClosest(fetcherSession, lastBlock);
                return lastBlock;
            }
            // In another case, we look for a common block using binary search, knowing that the block presence function is monotonic
            int leftBlockId = 0;
            int rightBlockId = lastBlock.MiningBlockInfo.BlockId;
            Block? centralBlock = null;
            while( leftBlockId < rightBlockId)
            {
                var centralBlockId = (rightBlockId + leftBlockId) / 2; // hi, overflow
                centralBlock = fetcherSession.SourceChain.GetBlockFromHead(centralBlockId);
                var isBlockPresented = await BlockPresentRequest(fetcherSession, centralBlock);
                if (isBlockPresented)
                    leftBlockId = centralBlockId + 1;
                else
                    rightBlockId = centralBlockId - 1;
            }
            if (centralBlock is null)
                throw new Exception("Something wen't wrong");
            if ( await BlockPresentRequest(fetcherSession, centralBlock) )
                throw new OperationCanceledException();
            ClaimBlockAsClosest(fetcherSession, centralBlock);
            return centralBlock;
        }

        private static async Task<bool> BlockPresentRequest(FetcherSession fetcherSession, Block block)
        {
            var request = new ChainBlockPresentRequestDTO()
            {
                RequestedBlockId = block.MiningBlockInfo.BlockId,
                RequestedBlockHash = block.Hash
            };
            fetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(request));
            var responseMsgContext = await fetcherSession.WrappedSession.WaitForMessage();
            if (responseMsgContext is null)
                throw new OperationCanceledException();
            var responseObject = MessageContextHelper.Parse<ChainBlockPresentResponseDTO>(responseMsgContext);
            if( responseObject is null )
                throw new OperationCanceledException();
            return responseObject.IsPresented;
        }

        private static void ClaimBlockAsClosest(FetcherSession fetcherSession, Block block)
        {
            var claimRequest = new ClaimBlockAsDownloadBeginning() { ClaimBlockId = block.MiningBlockInfo.BlockId };
            fetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(claimRequest));
        }
    }
}
