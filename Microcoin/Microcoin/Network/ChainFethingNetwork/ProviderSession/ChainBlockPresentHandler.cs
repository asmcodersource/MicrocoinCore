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
        public readonly ProviderSession ProviderSession;
        public Block TargetBlock { get; private set; } = null!;

        public ChainBlockPresentHandler( ProviderSession providerSession )
        {
            ProviderSession = providerSession;
        }

        public async Task<bool> CreateHandleTask(CancellationToken cancellationToken)
        {
            var requestMessage = await ProviderSession.WrappedSession.WaitForMessage();
            var request = MessageContextHelper.Parse<ChainBlockPresentRequestDTO>(requestMessage);
            if (request is null)
                throw new OperationCanceledException();
            var storedBlock = ProviderSession.SourceChain.GetBlockFromHead(request.RequestedBlockId);
            var response = new ChainBlockPresentResponseDTO()
            {
                RequestedBlockId = request.RequestedBlockId,
                RequestedBlockHash = request.RequestedBlockHash,
                IsPresented = false
            };
            if (storedBlock is not null && storedBlock.Hash == request.RequestedBlockHash)
            {
                TargetBlock = storedBlock;
                response.IsPresented = true;
            }
            ProviderSession.WrappedSession.SendMessage(JsonSerializer.Serialize(response));
            return response.IsPresented;
        }
    }
}
