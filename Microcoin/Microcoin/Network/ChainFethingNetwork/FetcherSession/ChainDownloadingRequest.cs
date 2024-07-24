using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Json;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using System.Text.Json;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public record ChainDownloadRequestDTO
    {
        public int StartingBlockId { get; set; }
        public int TargetBlockId { get; set; }
        public bool DownloadingTillChainEnd { get; set; }
    }

    public record RequestNextPartOfBlocks { }

    public class ChainDownloadingRequest
    {
        public readonly FetcherSession FetcherSession;
        public readonly MutableChain StartingChain;
        public readonly Block TargetBlock;
        public readonly int ChainBranchBlocksCount;

        public ChainDownloadingRequest(FetcherSession fetcherSession, MutableChain startingChain, Block targetBlock, int chainBranchBlocksCount)
        {
            FetcherSession = fetcherSession;
            StartingChain = startingChain;
            TargetBlock = targetBlock;
            ChainBranchBlocksCount = chainBranchBlocksCount;
        }

        public async Task<MutableChain> CreateRequestTask(CancellationToken cancellationToken)
        {
            await RequestDownloading(cancellationToken);
            ICollection<Block>? receivedBlocks = null;
            var currentChain = StartingChain;
            do
            {
                receivedBlocks = await ReceiveBlocks(cancellationToken);
                if (receivedBlocks is null)
                    throw new ChainDownloadingException("Bad response while blocks downloading");
                if (receivedBlocks.Count == 0)
                    break;
                foreach (var block in receivedBlocks)
                {
                    if (currentChain.BlocksList.Count >= ChainBranchBlocksCount)
                    {
                        var newTailChain = new MutableChain();
                        newTailChain.LinkPreviousChain(currentChain);
                        currentChain = newTailChain;
                    }
                    currentChain.AddTailBlock(block);
                }
            } while (cancellationToken.IsCancellationRequested is not true);
            return currentChain;
        }


        private async Task RequestDownloading(CancellationToken cancellationToken)
        {
            ChainDownloadRequestDTO request = new ChainDownloadRequestDTO
            {
                StartingBlockId = StartingChain.GetLastBlock().MiningBlockInfo.BlockId,
                TargetBlockId = TargetBlock.MiningBlockInfo.BlockId,
                DownloadingTillChainEnd = true,
            };
            try
            {
                CancellationTokenSource acceptTimeoutCTS = new CancellationTokenSource(10000);
                var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, acceptTimeoutCTS.Token);
                FetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(request));
                var response = await FetcherSession.WrappedSession.ReceiveMessageAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<ChainDownloadResponseDTO>(response.Payload);
                if (result.IsAccepted is not true)
                    throw new ChainDownloadingException("Chain downloading rejected");
                Serilog.Log.Debug($"Downloading accepted {this.FetcherSession.GetHashCode()}");
            }
            catch (OperationCanceledException ex)
            {
                if (cancellationToken.IsCancellationRequested is not true)
                    Serilog.Log.Debug($"Downloading accept error because of timeout for  transceive {this.FetcherSession.GetHashCode()}: {ex.Message}");
                throw;
            }
        }

        private async Task<ICollection<Block>?> ReceiveBlocks(CancellationToken cancellationToken)
        {
            try
            {
                CancellationTokenSource waitForBlocksTimeoutCTS = new CancellationTokenSource(30000);
                var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, waitForBlocksTimeoutCTS.Token);
                var request = new RequestNextPartOfBlocks();
                FetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(request));
                var response = await FetcherSession.WrappedSession.ReceiveMessageAsync(cancellationToken);
                return JsonTypedWrapper.Deserialize<ICollection<Block>>(response.Payload);
            }
            catch (OperationCanceledException ex)
            {
                if (cancellationToken.IsCancellationRequested is not true)
                    Serilog.Log.Debug($"Blocks receive error because of timeout for blocks part transceive {this.FetcherSession.GetHashCode()}: {ex.Message}");
                throw;
            }
        }
    }
}
