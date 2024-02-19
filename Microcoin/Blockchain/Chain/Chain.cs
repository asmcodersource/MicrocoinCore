using Microcoin.Blockchain.Block;

namespace Microcoin.Blockchain.Chain
{
    internal class Chain : IChain
    { 
        public Chain PreviousChain { get; protected set; }
        public List<Block.Block> ChainList { get; protected set; } = new List<Block.Block>();
        public Dictionary<string, Block.Block> ChainDictionary { get; protected set; } = new Dictionary<string, Block.Block>();


    }
}
