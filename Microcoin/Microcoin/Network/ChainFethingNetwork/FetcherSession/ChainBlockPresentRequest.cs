using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using NodeNet.NodeNetSession.SessionMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public record ChainBlockPresentRequestDTO
    {
        public int RequestedBlockId { get; set; }
        public string RequestedBlockHash { get; set; }
    }

    public class ChainBlockPresentRequest
    {
        public static async Task<bool> CreateRequestTask(FetcherSession fetcherSession, CancellationToken cancellationToken)
        {
            var requestBlock = fetcherSession.FetchRequest.RequestedBlock;
            var requestDTO = new ChainBlockPresentRequestDTO
            {
                RequestedBlockId = requestBlock.MiningBlockInfo.BlockId,
                RequestedBlockHash = requestBlock.Hash
            };
            fetcherSession.WrappedSession.SendMessage(JsonSerializer.Serialize(requestDTO));
            var response = await fetcherSession.WrappedSession.WaitForMessage();
            var result = MessageContextHelper.Parse<ChainBlockPresentResponseDTO>(response);
            if (result is null)
                throw new OperationCanceledException();
            return result.IsPresented;
        }
    }
}
