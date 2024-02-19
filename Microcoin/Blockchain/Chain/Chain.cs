using Microcoin.Blockchain.Block;

namespace Microcoin.Blockchain.Chain
{
    internal class Chain : IChain
    {
        public Chain? PreviousChain { get; protected set; } = null;
        public List<Block.Block> BlocksList { get; protected set; } = new List<Block.Block>();
        public Dictionary<string, Block.Block> BlocksDictionary { get; protected set; } = new Dictionary<string, Block.Block>();


    }
}
