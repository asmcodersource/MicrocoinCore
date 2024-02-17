using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    internal interface IRewardRule
    {
        public decimal CalculateReward(List<Blockchain.Block.Block> blocks, int blockId);
    }
}
