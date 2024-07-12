using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using NodeNet.NodeNetSession.SessionMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    public record ChainBlockPresentResponseDTO
    {
        public int RequestedBlockId { get; set; }
        public string RequestedBlockHash { get; set; }
        public bool IsPresented { get; set; }
    }

    public class ChainBlockPresentHandler
    {
        public static async Task<bool> CreateHandleTask(ProviderSession providerSession, CancellationToken cancellationToken)
        {
            var requestMessage = await providerSession.WrappedSession.WaitForMessage();
            var request = MessageContextHelper<ChainBlockPresentRequestDTO>.Parse(requestMessage);
            if (request is null)
                throw new OperationCanceledException();
            var storedBlock = providerSession.SourceChain.GetBlockFromHead(request.RequestedBlockId);
            var response = new ChainBlockPresentResponseDTO()
            {
                RequestedBlockId = request.RequestedBlockId,
                RequestedBlockHash = request.RequestedBlockHash,
                IsPresented = false
            };
            if( storedBlock is not null && storedBlock.Hash == request.RequestedBlockHash )
                response.IsPresented = true;
            providerSession.WrappedSession.SendMessage(JsonSerializer.Serialize(response));
            return response.IsPresented;
        }
    }
}
