using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Json;
using System.Xml.Linq;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSession;
using NodeNet.NodeNetSession.SessionMessage;
using System.Text.Json;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public record ChainDownloadRequestDTO
    {
        public int StartingBlockId { get; set; }
        public int TargetBlockId {  get; set; }
        public bool DownloadingTillChainEnd { get; set; }
    }

    public record RequestNextPartOfBlocks{}

    public class ChainDownloadingRequest
    {
        public readonly FetcherSession FetcherSession;
        public readonly MutableChain StartingChain;
        public readonly Block TargetBlock;

        public ChainDownloadingRequest(FetcherSession fetcherSession, MutableChain startingChain, Block targetBlock)
        {
            FetcherSession = fetcherSession;
            StartingChain = startingChain;
            TargetBlock = targetBlock;
        }

        public async Task<AbstractChain> CreateRequestTask(CancellationToken cancellationToken)
        {
            await RequestDownloading(cancellationToken);
            ICollection<Block>? receivedBlocks = null;
            do
            {
                receivedBlocks = await ReceiveBlocks(cancellationToken);
                if (receivedBlocks is null)
                    throw new Exception("Bad response while blocks downloading");
                if (receivedBlocks.Count == 0)
                    break;
                foreach( var block in receivedBlocks )
                    StartingChain.AddTailBlock(block);
            } while (cancellationToken.IsCancellationRequested is not true);
            return StartingChain;
        }


        private async Task RequestDownloading(CancellationToken cancellationToken)
        {
            ChainDownloadRequestDTO request = new ChainDownloadRequestDTO
            {
                StartingBlockId = StartingChain.GetLastBlock().MiningBlockInfo.BlockId,
                TargetBlockId = TargetBlock.MiningBlockInfo.BlockId,
                DownloadingTillChainEnd = true,
            };
            FetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(request));
            var responseMsg = await FetcherSession.WrappedSession.WaitForMessage();
            var responseSessionMsg = MessageContextHelper.GetSessionMessageData(responseMsg);
            var response = JsonTypedWrapper.Deserialize<ChainDownloadResponseDTO>(responseSessionMsg);
            if (response.IsAccepted is not true)
                throw new Exception("Chain downloading rejected");
        }

        private async Task<ICollection<Block>?> ReceiveBlocks(CancellationToken cancellationToken)
        {
            var request = new RequestNextPartOfBlocks();
            FetcherSession.WrappedSession.SendMessage(JsonTypedWrapper.Serialize(request));
            var responseMsg = await FetcherSession.WrappedSession.WaitForMessage(cancellationToken);
            var responseSessionMsg = MessageContextHelper.GetSessionMessageData(responseMsg);
            return JsonSerializer.Deserialize<ICollection<Block>>(responseSessionMsg);
        }
    }
}
