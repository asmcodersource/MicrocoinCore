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
        public readonly FetcherSession FetcherSession;

        public ChainBlockPresentRequest(FetcherSession fetcherSession)
        {
            FetcherSession = fetcherSession;
        }

        public async Task<bool> CreateRequestTask(CancellationToken cancellationToken)
        {
            var requestBlock = FetcherSession.FetchRequest.RequestedBlock;
            var requestDTO = new ChainBlockPresentRequestDTO
            {
                RequestedBlockId = requestBlock.MiningBlockInfo.BlockId,
                RequestedBlockHash = requestBlock.Hash
            };
            FetcherSession.WrappedSession.SendMessage(JsonSerializer.Serialize(requestDTO));
            var response = await FetcherSession.WrappedSession.WaitForMessage(cancellationToken);
            var result = MessageContextHelper.Parse<ChainBlockPresentResponseDTO>(response);
            if (result is null)
                throw new ChainDownloadingException("Bad response in block present request");
            return result.IsPresented;
        }
    }
}
