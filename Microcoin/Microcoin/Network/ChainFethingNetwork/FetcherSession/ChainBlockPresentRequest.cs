using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using System.Text.Json;

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
            FetcherSession.WrappedSession.SendMessage(requestDTO);
            var response = await FetcherSession.WrappedSession.ReceiveMessageAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<ChainBlockPresentResponseDTO>(response.Payload);
            if (result is null)
                throw new ChainDownloadingException("Bad response in block present request");
            return result.IsPresented;
        }
    }
}
