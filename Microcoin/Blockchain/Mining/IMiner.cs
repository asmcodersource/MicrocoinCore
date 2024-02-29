using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    public interface IMiner
    {
        public event Action<Block.Block> BlockMined;
        public MiningRules MiningRules { get; }
       
        public Task StartBlockMining(IChain chain, Block.Block block, CancellationToken cancellationToken);
        public Task<bool> VerifyBlockMining(IChain chain, Block.Block block);
    }
}
