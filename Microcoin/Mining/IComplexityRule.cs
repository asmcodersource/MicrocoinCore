using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Block;

namespace Microcoin.Mining
{
    internal interface IComplexityRule
    {
        public decimal CalculateComplexity(List<Block.Block> blocks, int blockId);
    }
}
