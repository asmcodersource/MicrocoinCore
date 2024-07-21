using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.PipelineHandling;

namespace Microcoin.Microcoin.Blockchain.BlocksPool
{
    public class VerifyBlockFields : IPipelineHandler<Block.Block>
    {
        public async Task<bool> Handle(Block.Block block)
        {
            if (block.Transactions == null || block.Transactions.Count == 0)
                return false;
            if (block.MiningBlockInfo.CreateTime > DateTime.UtcNow)
                return false;
            if (block.MiningBlockInfo == null)
                return false;
            return true;
        }
    }
}
