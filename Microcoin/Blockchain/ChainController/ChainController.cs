using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Blockchain.Chain;
using Microcoin.Blockchain.Mining;

namespace Microcoin.Blockchain.ChainController
{
    internal class ChainController
    {   
        protected CancellationTokenSource currentChainOperationsCTS;
        public ChainLoader ChainLoader { get; protected set; }
        public Chain.Chain ChainTail { get; protected set; }
        public MiningRules MiningRules { get; protected set; }
        public INextBlockRule NextBlockRule { get; protected set; }
        public IFetchableChainRule FetchableChainRule { get; protected set; }
        public IDeepTransactionsVerify DeepTransactionVerify { get; protected set; }

        public ChainController(Chain.Chain chainTail, ChainLoader chainLoader = null)
        {
            // ChainControllers without chainLoader can't load chain from network
            // This is done to avoid loading threads inside loading threads.
            ChainTail = chainTail;
            ChainLoader = chainLoader;
            currentChainOperationsCTS = new CancellationTokenSource();
            DeepTransactionVerify = new DeepTransactionsVerify();
        }

        public async Task AcceptBlock(Block.Block block)
        {
            CancellationToken cancellationToken = GetChainOperationCancellationToken();
            if (NextBlockRule.IsBlockNextToChain(block, ChainTail))
            {
                var isBlockValid = await DeepBlockVerify(block, ChainTail, cancellationToken);
                if (isBlockValid is not true)
                    return; 
                AddBlockToTail(block);  // this block valid as new tail of current blockchain, try to set it as tail
            }
            else if (ChainLoader != null && FetchableChainRule.IsPossibleChainUpgrade(block))
            {
                // Essentially, with this call we only request that the chain be loaded, but not necessarily that it will be loaded.
                // We don't care about everything else inside the chain.
                var isBlockValid = await DeepBlockVerify(block, ChainTail, cancellationToken);
                if (isBlockValid is not true)
                    return;
                ChainLoader.RequestChainFetch(block);
            }
        }

        public void AddBlockToTail(Block.Block block)
        {
            // lock used to avoid thread running
            // Only one block can be added as tail of chain
            lock (ChainTail)
            {
                // is block still continues to this chain?
                if (NextBlockRule.IsBlockNextToChain(block, ChainTail) is not true)
                    return;
                ChainTail.AddTailBlock(block);
                currentChainOperationsCTS.Cancel();
                currentChainOperationsCTS = new CancellationTokenSource();
            }
        }

        protected async Task<bool> DeepBlockVerify(Block.Block block, IChain chainTail, CancellationToken cancellationToken)
        {
            var isBlockRewardCorrect = await MiningRules.RewardRule.Verify(ChainTail, block);
            if (isBlockRewardCorrect is not true)
                return false;
            var isBlockComplexityCorrect = await MiningRules.ComplexityRule.Verify(ChainTail, block);
            if (isBlockComplexityCorrect is not true)
                return false;
            var isTransactionsCorrect = await DeepTransactionVerify.Verify(ChainTail, block.Transactions, cancellationToken); // as it the most long operations, I put it on end of execution
            if (isTransactionsCorrect is not true)
                return false;
            return true;
        } 

        // Any actions that start for some Chain valid only for this chain
        // If some operation change current chain, all operations have to be cancelled
        protected CancellationToken GetChainOperationCancellationToken()
        {
            // I used lock to prevent returning past CTS
            lock (ChainTail)
                return currentChainOperationsCTS.Token;
        }
    }
}
