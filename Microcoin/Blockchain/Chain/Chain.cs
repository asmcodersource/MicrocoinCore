using Microcoin.Blockchain.Block;

namespace Microcoin.Blockchain.Chain
{
    internal class Chain : IChain
    {
        public ImmutableChain? PreviousChain { get; protected set; } = null;
        public List<Block.Block> BlocksList { get; protected set; } = new List<Block.Block>();
        public Dictionary<string, Block.Block> BlocksDictionary { get; protected set; } = new Dictionary<string, Block.Block>();

        public void AddTailBlock(Block.Block block)
        {
            BlocksList.Add(block);
            BlocksDictionary.Add(block.Hash, block);
        }
    }
}
