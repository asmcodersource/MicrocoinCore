using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin.Blockchain.Chain
{
    public class NextBlockRule : INextBlockRule
    {
        public bool IsBlockNextToChain(Block.Block block, AbstractChain chain)
        {
            var currentTailBlock = chain.GetLastBlock();
            return IsBlockNextToBlock(block, currentTailBlock);
        }

        public bool IsBlockNextToBlock(Block.Block nextBlock, Block.Block currentTailBlock)
        {
            if (currentTailBlock.MiningBlockInfo.BlockId + 1 != nextBlock.MiningBlockInfo.BlockId)
                return false;
            if (currentTailBlock.Hash != nextBlock.MiningBlockInfo.PreviousBlockHash)
                return false;
            return true;
        }
    }
}
