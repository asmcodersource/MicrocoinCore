﻿using Microcoin.Microcoin.Blockchain.Chain;
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
    public class FetcherSession
    {
        public readonly Session WrappedSession;
        public readonly int ChainBranchBlocksCount;
        public readonly AbstractChain SourceChain;
        public readonly FetchRequest FetchRequest;

        public FetcherSession(Session session, AbstractChain sourceChain, FetchRequest fetchRequest, int chainBranchBlocksCount)
        {
            WrappedSession = session;
            SourceChain = new MutableChain(sourceChain);
            FetchRequest = fetchRequest;
            ChainBranchBlocksCount = chainBranchBlocksCount;
        }

        public async Task<ChainDownloadingResult> StartDonwloadingProccess(CancellationToken generalCancellationToken)
        {
            try
            {
                CancellationTokenSource initialCommunicationCTS = new CancellationTokenSource(600_000);
                var initialCt = CancellationTokenSource.CreateLinkedTokenSource(generalCancellationToken, initialCommunicationCTS.Token);
                var blockPresentRequest = new ChainBlockPresentRequest(this);
                var isBlockPresented = await blockPresentRequest.CreateRequestTask(initialCt.Token);
                if (isBlockPresented is not true)
                    throw new ChainDownloadingException("Block is not presented");
                var closestBlockRequest = new ClosestBlockRequest(this);
                var closestBlock = await closestBlockRequest.CreateRequestTask(initialCt.Token);
                var truncatedChain = SourceChain.CreateTrunkedChain(closestBlock);
                var downloadingChainRequest = new ChainDownloadingRequest(this, truncatedChain, FetchRequest.RequestedBlock, ChainBranchBlocksCount);
                var downloadedChain = await downloadingChainRequest.CreateRequestTask(generalCancellationToken);
                return new ChainDownloadingResult(downloadedChain, closestBlock);
            } 
            catch( OperationCanceledException ex)
            {
                throw new ChainDownloadingException("Operations cancelled exception");
            }
            catch( Exception ex)
            {
                Serilog.Log.Error($"Some error happend during fetch request {this.GetHashCode()}: {ex.Message}");
                throw ex;
            }
        }
    }
}
