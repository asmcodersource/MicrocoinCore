using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Mining
{
    public interface IMiner
    {
        public event Action<Blockchain.Block.Block, string> BlockMined;
        public MiningRules MiningRules { get; }

        public void LinkBlockToChain(AbstractChain chain, Blockchain.Block.Block block)
        {
            var tailBlock = chain.GetLastBlock();
            if (tailBlock is not null)
            {
                block.MiningBlockInfo.BlockId = tailBlock.MiningBlockInfo.BlockId + 1;
                block.MiningBlockInfo.PreviousBlockHash = tailBlock.Hash;
            }
            else throw new Exception("Can't link to empty chain");
        }

        public Task<string> StartBlockMining(AbstractChain chain, Blockchain.Block.Block block, string minerWallet, CancellationToken cancellationToken);
        public bool VerifyBlockMining(AbstractChain chain, Blockchain.Block.Block block);
    }
}
