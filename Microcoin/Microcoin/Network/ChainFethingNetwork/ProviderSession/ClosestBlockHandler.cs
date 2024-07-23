using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using NodeNet.NodeNetSession.SessionMessage;
using Microcoin.Json;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    public class ClosestBlockHandler
    {
        public readonly ProviderSession ProviderSession;

        public ClosestBlockHandler(ProviderSession providerSession)
        {
            ProviderSession = providerSession;
        }

        public async Task<Block> CreateHandleTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var msgContext = await ProviderSession.WrappedSession.WaitForMessage(cancellationToken);
                var msgType = JsonTypedWrapper.GetWrappedTypeName(msgContext.SessionMessage.Data);

                switch (msgType)
                {
                    case nameof(ChainBlockPresentRequestDTO):
                        var blockPresentRequest = JsonTypedWrapper.Deserialize<ChainBlockPresentRequestDTO>(msgContext.SessionMessage.Data);
                        if (blockPresentRequest == null)
                        {
                            throw new Exception("Invalid message received");
                        }
                        await BlockPresentRequestHandler(blockPresentRequest);
                        break;

                    case nameof(ClaimBlockAsDownloadRootDTO):
                        var claimBlockRequest = JsonTypedWrapper.Deserialize<ClaimBlockAsDownloadRootDTO>(msgContext.SessionMessage.Data);
                        if (claimBlockRequest == null)
                        {
                            throw new Exception("Invalid message received");
                        }
                        return BlockClaimRequestHandler(claimBlockRequest);

                    default:
                        throw new Exception("Unexpected message in closest block communications");
                }
            }

            throw new OperationCanceledException();
        }

        private async Task BlockPresentRequestHandler(ChainBlockPresentRequestDTO blockPresentRequest)
        {
            var blockFromChain = ProviderSession.SourceChain.GetBlockFromHead(blockPresentRequest.RequestedBlockId);
            var presentRequestResponse = new ChainBlockPresentResponseDTO
            {
                RequestedBlockHash = blockPresentRequest.RequestedBlockHash,
                RequestedBlockId = blockPresentRequest.RequestedBlockId,
                IsPresented = blockFromChain?.Hash == blockPresentRequest.RequestedBlockHash
            };

            await ProviderSession.WrappedSession.SendMessageAsync(JsonSerializer.Serialize(presentRequestResponse));
        }

        private Block BlockClaimRequestHandler(ClaimBlockAsDownloadRootDTO claimBlockRequest)
        {
            var blockFromChain = ProviderSession.SourceChain.GetBlockFromHead(claimBlockRequest.ClaimBlockId);

            if (blockFromChain == null)
            {
                throw new OperationCanceledException("Block not found in chain.");
            }

            return blockFromChain;
        }
    }
}
