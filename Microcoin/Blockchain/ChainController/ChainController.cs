using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Blockchain.Chain;

namespace Microcoin.Blockchain.ChainController
{
    internal class ChainController
    {
        protected CancellationTokenSource currentChainOperationsCTS;
        public ChainLoader ChainLoader { get; protected set; }
        public Chain.Chain ChainTail { get; protected set; }
        public INextBlockRule NextBlockRule { get; protected set; }
        public IFetchableChainRule FetchableChainRule { get; protected set; }

        public ChainController(Chain.Chain chainTail, ChainLoader chainLoader = null)
        {
            // ChainControllers without chainLoader can't load chain from network
            // This is done to avoid loading threads inside loading threads.
            ChainTail = chainTail;
            ChainLoader = chainLoader;
            currentChainOperationsCTS = new CancellationTokenSource();
        }

        public async Task AcceptBlock(Block.Block block)
        {
            CancellationToken cancellationToken = GetChainOperationCancellationToken();
            if( NextBlockRule.IsBlockNextToChain(block, ChainTail) )
            {
                
            } 
            else if ( FetchableChainRule.IsPossibleChainUpgrade(block) && ChainLoader != null )
            {
                
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


        // Any actions that start for some Chain valid only for this chain
        // If some operation change current chain, all operations have to be cancelled
        public CancellationToken GetChainOperationCancellationToken()
        {
            // I used lock to prevent returning past CTS
            lock (ChainTail)
                return currentChainOperationsCTS.Token;
        }
    }
}
