using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Mining;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Blockchain.Chain
{
    /// <summary>
    /// This class provides functionality for checking chains for compliance with all blockchain rules
    /// </summary>
    public class ChainVerificator
    {
        private readonly TransactionsPool.TransactionsPool transactionsVerificationPool = new TransactionsPool.TransactionsPool();
        private readonly BlocksPool.BlocksPool blocksVerificationPool = new BlocksPool.BlocksPool();
        public readonly IMiner Miner;
 
        public ChainVerificator(IMiner miner) 
        {
            Miner = miner;
            transactionsVerificationPool.InitializeHandlerPipeline();
            blocksVerificationPool.InitializeHandlerPipeline(transactionsVerificationPool.HandlePipeline); 
        }

        
        public async Task<bool> VerifyChain(AbstractChain chain, Block.Block startingBlock)
        {
            if (startingBlock.MiningBlockInfo.BlockId == 0)
                throw new ArgumentException("Zero block cannot be verified in chain verificator");
            /*
             * I'm too lazy to do a normal verification, so I'll just run the blocks through the entire processing pipeline, 
             * as if they came from the network, and build a new chain. If it can’t complete the chain, 
             * then there are errors in it. On the plus side, 
             * such a check will display the same behavior as for regular blocks from the network
             */
            var blocksToVerifyEnumerator = chain.GetEnumerable(startingBlock).GetEnumerator();
            var trancatedChain = chain.CreateTrunkedChain(startingBlock);
            var chainController = new ChainController.ChainController(trancatedChain, Miner);
            chainController.DefaultInitialize();
            chainController.ChainBranchBlocksCount = int.MaxValue;
            while (blocksToVerifyEnumerator.MoveNext())
            {
                var block = blocksToVerifyEnumerator.Current;
                bool isBlockValidItSelf = await blocksVerificationPool.HandleBlock(block);
                if (isBlockValidItSelf is not true)
                    return false;
                bool isBlockSuccesfulAddedToChain = await chainController.AcceptBlock(block);
                if( isBlockSuccesfulAddedToChain is not true )
                    return false;
            }
            return true;
        }
    }
}
