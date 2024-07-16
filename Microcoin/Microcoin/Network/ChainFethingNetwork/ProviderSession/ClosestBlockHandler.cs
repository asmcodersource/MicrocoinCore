using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Microcoin.Blockchain.Block;
using NodeNet.NodeNetSession.SessionMessage;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using System.Text.Json;
using static Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession.ClosestBlockRequest;
using Microcoin.Json;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    /// <summary>
    /// Creates a task that will communicate through a session,
    /// processing a request to find the nearest block serving as a common point from which to start loading the continuation of the chain.
    /// </summary>
    public class ClosestBlockHandler
    {
        public readonly ProviderSession ProviderSession;

        public ClosestBlockHandler(ProviderSession providerSession) 
        {
            ProviderSession = providerSession;
        }

        public async Task<Block> CreateHandleTask(CancellationToken cancellationToken)
        { 
            do
            {
                var requestWaitingCTS = new CancellationTokenSource();
                //requestWaitingCTS.CancelAfter(5000);
                var msgContext = await ProviderSession.WrappedSession.WaitForMessage(requestWaitingCTS.Token);
                var sessionMsgData = MessageContextHelper.GetSessionMessageData(msgContext);
                var msgType = JsonTypedWrapper.GetWrappedTypeName(sessionMsgData);
                switch (msgType)
                {
                    case nameof(ChainBlockPresentRequestDTO):
                        var chainBlockPresentRequestDTO = JsonTypedWrapper.Deserialize<ChainBlockPresentRequestDTO>(sessionMsgData);
                        if (chainBlockPresentRequestDTO is null)
                            throw new Exception("Invalid message received");
                        BlockPresentRequestHandler(chainBlockPresentRequestDTO, cancellationToken);
                        break;
                    case nameof(ClaimBlockAsDownloadRootDTO):
                        var chainBlockClaimRequestDTO = JsonTypedWrapper.Deserialize<ClaimBlockAsDownloadRootDTO>(sessionMsgData);
                        if (chainBlockClaimRequestDTO is null)
                            throw new Exception("Invalid message received");
                        return BlockClaimRequestHandler(chainBlockClaimRequestDTO, cancellationToken);
                    default:
                        throw new Exception("Unexpected message in closest block communications");
                }
            } while (cancellationToken.IsCancellationRequested is not true);
            throw new OperationCanceledException();
        }

        private bool BlockPresentRequestHandler(ChainBlockPresentRequestDTO chainBlockPresentRequest, CancellationToken cancellationToken)
        {
            var blockFromChain = ProviderSession.SourceChain.GetBlockFromHead(chainBlockPresentRequest.RequestedBlockId);
            var presentRequestResponse = new ChainBlockPresentResponseDTO()
            {
                RequestedBlockHash = chainBlockPresentRequest.RequestedBlockHash,
                RequestedBlockId = chainBlockPresentRequest.RequestedBlockId,
            };
            if (blockFromChain is not null && blockFromChain.Hash == chainBlockPresentRequest.RequestedBlockHash)
                presentRequestResponse.IsPresented = true;
            else
                presentRequestResponse.IsPresented = false;
            ProviderSession.WrappedSession.SendMessage(JsonSerializer.Serialize(presentRequestResponse));
            return presentRequestResponse.IsPresented;
        }

        private Block BlockClaimRequestHandler(ClaimBlockAsDownloadRootDTO claimBlockAsDownloadingRoot, CancellationToken cancellationToken)
        {
            var blockFromChain = ProviderSession.SourceChain.GetBlockFromHead(claimBlockAsDownloadingRoot.ClaimBlockId);
            if (blockFromChain is not null)
                return blockFromChain;
            else
                throw new OperationCanceledException();
        }
    }
}
