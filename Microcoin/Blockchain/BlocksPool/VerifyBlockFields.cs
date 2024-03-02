using Microcoin.PipelineHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Block
{
    public class VerifyBlockFields : IPipelineHandler<Block>
    {
        public async Task<bool> Handle(Block block)
        {
            if (block.Transactions == null || block.Transactions.Count == 0)
                return false;
            if ( block.MiningBlockInfo.CreateTime < new DateTime(2024, 1, 1) && block.MiningBlockInfo.CreateTime > DateTime.UtcNow.AddMinutes(1) )
                return false;
            if (block.MiningBlockInfo == null)
                return false;
            return true;
        }
    }
}
