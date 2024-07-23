using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microcoin.Json;
using NodeNet.NodeNetSession.SessionMessage;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;

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
            ICollection<Block>? receivedBlocks;
            var currentChain = StartingChain;

            while (!cancellationToken.IsCancellationRequested)
            {
                receivedBlocks = await ReceiveBlocks(cancellationToken);
                if (receivedBlocks == null)
                {
                    throw new ChainDownloadingException("Bad response while blocks downloading");
                }
                if (receivedBlocks.Count == 0)
                {
                    break;
                }
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
            }
            return currentChain;
        }

        private async Task RequestDownloading(CancellationToken cancellationToken)
        {
            var request = new ChainDownloadRequestDTO
            {
                StartingBlockId = StartingChain.GetLastBlock().MiningBlockInfo.BlockId,
                TargetBlockId = TargetBlock.MiningBlockInfo.BlockId,
                DownloadingTillChainEnd = true
            };

            try
            {
                using var acceptTimeoutCTS = new CancellationTokenSource(10000);
                using var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, acceptTimeoutCTS.Token);

                await FetcherSession.WrappedSession.SendMessageAsync(JsonTypedWrapper.Serialize(request));
                var responseMsg = await FetcherSession.WrappedSession.WaitForMessage(linkedCTS.Token);
                var response = JsonTypedWrapper.Deserialize<ChainDownloadResponseDTO>(responseMsg.SessionMessage.Data);

                if (response == null || !response.IsAccepted)
                {
                    throw new ChainDownloadingException("Chain downloading rejected or bad response received.");
                }

                Serilog.Log.Debug($"Downloading accepted {FetcherSession.GetHashCode()}");
            }
            catch (OperationCanceledException ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Serilog.Log.Debug($"Downloading accept error due to timeout for transceive {FetcherSession.GetHashCode()}: {ex.Message}");
                }
                throw;
            }
        }

        private async Task<ICollection<Block>?> ReceiveBlocks(CancellationToken cancellationToken)
        {
            try
            {
                using var waitForBlocksTimeoutCTS = new CancellationTokenSource(180000);
                using var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, waitForBlocksTimeoutCTS.Token);
                var request = new RequestNextPartOfBlocks();
                await FetcherSession.WrappedSession.SendMessageAsync(JsonTypedWrapper.Serialize(request));
                var responseMsg = await FetcherSession.WrappedSession.WaitForMessage(linkedCTS.Token);
                return JsonTypedWrapper.Deserialize<List<Block>>(responseMsg.SessionMessage.Data);
            }
            catch (OperationCanceledException ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Serilog.Log.Debug($"Blocks receive error due to timeout for blocks part transceive {FetcherSession.GetHashCode()}: {ex.Message}");
                }
                throw;
            }
        }
    }
}
