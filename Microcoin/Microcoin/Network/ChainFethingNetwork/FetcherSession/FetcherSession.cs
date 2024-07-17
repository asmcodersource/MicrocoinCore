using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin.Blockchain.Block;
using NodeNet.NodeNetSession.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Microcoin.Blockchain.ChainController;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public class ChainDownloadingException : Exception
    {
        public ChainDownloadingException(string? reason = null) : base(reason) { }
    }


    public class FetcherSession
    {
        public readonly Session WrappedSession;
        public readonly int ChainBranchBlocksCount;
        public readonly AbstractChain SourceChain;
        public readonly FetchRequest FetchRequest;

        public FetcherSession(Session session, AbstractChain sourceChain, FetchRequest fetchRequest, int chainBranchBlocksCount)
        {
            WrappedSession = session;
            SourceChain = sourceChain;
            FetchRequest = fetchRequest;
            ChainBranchBlocksCount = chainBranchBlocksCount;
        }

        public async Task<MutableChain> StartDonwloadingProccess(CancellationToken generalCancellationToken)
        {
            CancellationTokenSource initialCommunicationCTS = new CancellationTokenSource(60_000);
            var initialCt = CancellationTokenSource.CreateLinkedTokenSource(generalCancellationToken, initialCommunicationCTS.Token);
            var blockPresentRequest = new ChainBlockPresentRequest(this);
            var isBlockPresented = await blockPresentRequest.CreateRequestTask(initialCt.Token);
            if (isBlockPresented is not true)
                throw new OperationCanceledException();
            var closestBlockRequest = new ClosestBlockRequest(this);
            var closestBlock = await closestBlockRequest.CreateRequestTask(initialCt.Token);
            var truncatedChain = CreateTrunkedChain(closestBlock);
            var downloadingChainRequest = new ChainDownloadingRequest(this, truncatedChain, FetchRequest.RequestedBlock, ChainBranchBlocksCount);
            var downloadedChain = await downloadingChainRequest.CreateRequestTask(generalCancellationToken);
            return downloadedChain;
        }

        private MutableChain CreateTrunkedChain(Block lastBlock)
        {
            // find last part of chain, that need to be taken from source
            var endingChain = SourceChain;
            while ((endingChain.EntireChainLength-1) > lastBlock.MiningBlockInfo.BlockId)
            {
                endingChain = endingChain.PreviousChain;
                if (endingChain is null)
                    throw new ChainDownloadingException("Something wen't wrong with finding last chain");
            }
            var forkedEndChain = new MutableChain();
            if(endingChain.PreviousChain is not null)
                forkedEndChain.LinkPreviousChain(endingChain.PreviousChain);
            var endingChainBlocks = endingChain.GetBlocksList();
            var firstBlock = endingChainBlocks.First();
            var numberOfBlocksToAppend = lastBlock.MiningBlockInfo.BlockId - firstBlock.MiningBlockInfo.BlockId;
            for (int i = 0; i <= numberOfBlocksToAppend; i++)
                forkedEndChain.AddTailBlock(endingChainBlocks[i]);
            return forkedEndChain;
        }
    }
}
