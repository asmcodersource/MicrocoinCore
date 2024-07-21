using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Mining;
using System;
using System.Collections.Generic;
using SimpleInjector;
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
        public Container ServicesContainer { get; set; }
        private readonly BlocksPool.BlocksPool blocksVerificationPool;
 
        public ChainVerificator(Container servicesContainer) 
        {
            ServicesContainer = servicesContainer;
            blocksVerificationPool = servicesContainer.GetInstance<BlocksPool.BlocksPool>(); 
        }


        public async Task<bool> VerifyChain(AbstractChain chain, Block.Block startingBlock)
        {
            /*
             * I'm too lazy to do a normal verification, so I'll just run the blocks through the entire processing pipeline, 
             * as if they came from the network, and build a new chain. If it can’t complete the chain, 
             * then there are errors in it. On the plus side, 
             * such a check will display the same behavior as for regular blocks from the network
             */
            var blocksToVerifyEnumerator = chain.GetEnumerable(startingBlock).GetEnumerator();
            var trancatedChain = chain.CreateTrunkedChain(startingBlock);
            var chainController = new ChainController.ChainController(trancatedChain, ServicesContainer);
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
