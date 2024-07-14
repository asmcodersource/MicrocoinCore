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
    public class ClosestBlockHandler
    {
        /// <summary>
        /// Creates a task that will communicate through a session, processing a request to find the nearest block serving as a common point from which to start loading the continuation of the chain.
        /// </summary>
        public static async Task<Block> CreateHandleTask(ProviderSession providerSession, CancellationToken cancellationToken)
        { 
            do
            {
                var requestWaitingCTS = new CancellationTokenSource();
                //requestWaitingCTS.CancelAfter(5000);
                var msgContext = await providerSession.WrappedSession.WaitForMessage(requestWaitingCTS.Token);
                var sessionMsgData = MessageContextHelper.GetSessionMessageData(msgContext);
                var temp = JsonTypedWrapper.GetWrappedTypeName(sessionMsgData);
                try
                {
                    var presentRequest = JsonTypedWrapper.Deserialize<ChainBlockPresentRequestDTO>(sessionMsgData);
                    var blockFromChain = providerSession.SourceChain.GetBlockFromHead(presentRequest.RequestedBlockId);
                    var presentRequestResponse = new ChainBlockPresentResponseDTO()
                    {
                        RequestedBlockHash = presentRequest.RequestedBlockHash,
                        RequestedBlockId = presentRequest.RequestedBlockId,
                    };
                    if ( blockFromChain.Hash == presentRequest.RequestedBlockHash)
                    {
                        presentRequestResponse.IsPresented = true;
                        providerSession.WrappedSession.SendMessage(JsonSerializer.Serialize(presentRequestResponse));
                    } else
                    {
                        presentRequestResponse.IsPresented = false;
                        providerSession.WrappedSession.SendMessage(JsonSerializer.Serialize(presentRequestResponse));
                    }
                } catch (JsonTypedWrapper.JsonTypedException)
                {
                    // Maybe its claim request?
                    var claimRequest = JsonTypedWrapper.Deserialize<ClaimBlockAsDownloadBeginning>(sessionMsgData);
                    var blockFromChain = providerSession.SourceChain.GetBlockFromHead(claimRequest.ClaimBlockId);
                    if (blockFromChain is not null)
                        return blockFromChain;
                    else
                        throw new OperationCanceledException();
                }
            } while (cancellationToken.IsCancellationRequested is not true);
            throw new OperationCanceledException();
        }
    }
}
