using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Block;

namespace Microcoin.Mining
{
    internal interface IRewardRule
    {
        public decimal CalculateReward(List<Block.Block> blocks, int blockId);
    }
}
