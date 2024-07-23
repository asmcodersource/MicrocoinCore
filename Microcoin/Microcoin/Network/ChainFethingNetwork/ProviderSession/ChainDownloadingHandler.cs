using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using NodeNet.NodeNetSession.SessionMessage;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microcoin.Json;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession
{
    public enum ChainDownloadRejectReason
    {
        Unknown,
        RequestedChainTooLong
    }

    public record ChainDownloadResponseDTO
    {
        public bool IsAccepted { get; set; }
        public ChainDownloadRejectReason? RejectReason { get; set; }
    }

    public class ChainDownloadingHandler
    {
        public int MaxBlocksPerMessage { get; private set; }
        public readonly ProviderSession ProviderSession;
        public readonly AbstractChain SourceChain;
        public readonly Block StartingBlock;
        public readonly Block TargetBlock;
        private IEnumerator<Block>? downloadingBlocks;

        public ChainDownloadingHandler(ProviderSession providerSession, AbstractChain sourceChain, Block startingBlock, Block targetBlock, int maxBlocksPerMessage = 50)
        {
            ProviderSession = providerSession;
            SourceChain = sourceChain;
            StartingBlock = startingBlock;
            TargetBlock = targetBlock;
            SetMaxBlocksPerMessage(maxBlocksPerMessage);
        }

        public void SetMaxBlocksPerMessage(int maxBlocksPerMessage)
        {
            if (maxBlocksPerMessage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBlocksPerMessage), "Count of blocks per message must be more than zero");
            }
            MaxBlocksPerMessage = maxBlocksPerMessage;
        }

        public async Task CreateHandleTask(CancellationToken cancellationToken)
        {
            if (!await AcceptDownloadingRequest(cancellationToken))
            {
                throw new OperationCanceledException("Downloading request not accepted.");
            }

            while (await AcceptBlocksDownloadingRequest(cancellationToken)) { }
        }

        private async Task<bool> AcceptDownloadingRequest(CancellationToken cancellationToken)
        {
            var requestMsg = await ProviderSession.WrappedSession.WaitForMessage(cancellationToken);
            var request = JsonTypedWrapper.Deserialize<ChainDownloadRequestDTO>(requestMsg.SessionMessage.Data);

            if (request == null)
            {
                return false;
            }

            var response = new ChainDownloadResponseDTO
            {
                IsAccepted = true,
                RejectReason = null
            };

            downloadingBlocks = SourceChain.GetEnumerable(StartingBlock).GetEnumerator();
            await ProviderSession.WrappedSession.SendMessageAsync(JsonTypedWrapper.Serialize(response));
            return true;
        }

        private async Task<bool> AcceptBlocksDownloadingRequest(CancellationToken cancellationToken)
        {
            try
            {
                var requestMsg = await ProviderSession.WrappedSession.WaitForMessage(cancellationToken);
                var request = JsonTypedWrapper.Deserialize<RequestNextPartOfBlocks>(requestMsg.SessionMessage.Data);

                if (request == null)
                {
                    return false;
                }

                var blocksToSend = new List<Block>();
                for (int i = 0; i < MaxBlocksPerMessage && downloadingBlocks.MoveNext(); i++)
                    blocksToSend.Add(downloadingBlocks.Current);

                await ProviderSession.WrappedSession.SendMessageAsync(JsonTypedWrapper.Serialize(blocksToSend));
                return blocksToSend.Count > 0;
            } catch ( Exception ex )
            {
                Serilog.Log.Error($"Accept blocks downloading request error {ex.Message}");
                throw;
            }
        }
    }
}
