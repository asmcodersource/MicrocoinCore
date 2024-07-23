using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using NodeNet.NodeNetSession.SessionMessage;
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
            await FetcherSession.WrappedSession.SendMessageAsync(JsonSerializer.Serialize(requestDTO));
            var response = await FetcherSession.WrappedSession.WaitForMessage(cancellationToken);
            var result = JsonSerializer.Deserialize<ChainBlockPresentResponseDTO>(response.SessionMessage.Data);
            if (result is null)
                throw new ChainDownloadingException("Bad response in block present request");
            return result.IsPresented;
        }
    }
}
