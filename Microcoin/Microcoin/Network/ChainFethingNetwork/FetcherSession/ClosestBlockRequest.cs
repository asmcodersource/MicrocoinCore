using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using NodeNet.NodeNetSession.Session;
using NodeNet.NodeNetSession.SessionMessage;
using Microcoin.Json;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using System.Text.Json;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public record ClaimBlockAsDownloadRootDTO
    {
        public int ClaimBlockId { get; set; }
    }

    public class ClosestBlockRequest
    {
        public readonly FetcherSession FetcherSession;

        public ClosestBlockRequest(FetcherSession fetcherSession)
        {
            FetcherSession = fetcherSession;
        }

        public async Task<Block> CreateRequestTask(CancellationToken cancellationToken)
        {
            var lastBlock = FetcherSession.SourceChain.GetLastBlock();
            if (await BlockPresentRequest(lastBlock, cancellationToken))
            {
                await ClaimBlockAsClosest(lastBlock);
                return lastBlock;
            }

            int leftBlockId = 0;
            int rightBlockId = lastBlock.MiningBlockInfo.BlockId;
            Block? centralBlock = null;

            while (leftBlockId <= rightBlockId)
            {
                int centralBlockId = (rightBlockId + leftBlockId) / 2;
                centralBlock = FetcherSession.SourceChain.GetBlockFromHead(centralBlockId);

                if (centralBlock == null)
                {
                    rightBlockId = centralBlockId - 1;
                    continue;
                }

                if (await BlockPresentRequest(centralBlock, cancellationToken))
                {
                    leftBlockId = centralBlockId + 1;
                }
                else
                {
                    rightBlockId = centralBlockId - 1;
                }
            }

            if (centralBlock == null)
            {
                throw new ChainDownloadingException("Central block not found.");
            }

            await ClaimBlockAsClosest(centralBlock);
            return centralBlock;
        }

        private async Task<bool> BlockPresentRequest(Block block, CancellationToken cancellationToken)
        {
            var request = new ChainBlockPresentRequestDTO
            {
                RequestedBlockId = block.MiningBlockInfo.BlockId,
                RequestedBlockHash = block.Hash
            };

            await FetcherSession.WrappedSession.SendMessageAsync(JsonTypedWrapper.Serialize(request));
            var responseMsgContext = await FetcherSession.WrappedSession.WaitForMessage(cancellationToken);
            var responseObject = JsonSerializer.Deserialize<ChainBlockPresentResponseDTO>(responseMsgContext.SessionMessage.Data);

            if (responseObject == null)
            {
                throw new ChainDownloadingException("Bad response in block present request");
            }
            return responseObject.IsPresented;
        }

        private async Task ClaimBlockAsClosest(Block block)
        {
            var claimRequest = new ClaimBlockAsDownloadRootDTO { ClaimBlockId = block.MiningBlockInfo.BlockId };
            await FetcherSession.WrappedSession.SendMessageAsync(JsonTypedWrapper.Serialize(claimRequest));
        }
    }
}
