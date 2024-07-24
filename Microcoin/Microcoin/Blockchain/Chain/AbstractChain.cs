using System.Collections.Immutable;

namespace Microcoin.Microcoin.Blockchain.Chain
{
    /// <summary>
    /// The chain is a sequence of blocks for which the connection rule is executed. 
    /// A chain can be connected to the end of another chain. 
    /// A chain acting as a parent for another must be immutable
    /// </summary>
    [Serializable]
    public abstract class AbstractChain
    {
        public ImmutableChain? PreviousChain { get; protected set; }
        public IReadOnlyDictionary<string, double> WalletsCoins { get; protected set; } = new Dictionary<string, double>();
        public IReadOnlyCollection<Transaction.Transaction> TransactionsSet { get; protected set; } = new HashSet<Transaction.Transaction>();
        public IReadOnlyDictionary<string, Block.Block> BlocksDictionary { get; protected set; } = new Dictionary<string, Block.Block>();
        public IReadOnlyList<Block.Block> BlocksList { get; protected set; } = new List<Block.Block>();
        public int EntireChainLength { get; protected set; } = 0;

        public IReadOnlyList<Block.Block> GetBlocksList() => BlocksList;

        public bool IsChainHasTransaction(Transaction.Transaction transaction) => TransactionsSet.Contains(transaction);

        public double GetWalletCoins(string walletPublicKey)
        {
            var currentChain = this;
            double walletCoins = 0;

            while (currentChain != null)
            {
                if (currentChain.WalletsCoins.TryGetValue(walletPublicKey, out var coins))
                {
                    walletCoins += coins;
                }
                currentChain = currentChain.PreviousChain;
            }

            return walletCoins;
        }

        public Block.Block? GetLastBlock() => GetBlockFromTail(0);

        public Microcoin.Blockchain.Block.Block? GetBlockFromTail(int blockIdFromTail)
        {
            if (blockIdFromTail < BlocksList.Count)
                return BlocksList[(BlocksList.Count - blockIdFromTail) - 1];
            else if (PreviousChain is not null)
                return PreviousChain.GetBlockFromTail(blockIdFromTail - BlocksList.Count);
            return null;
        }

        public Microcoin.Blockchain.Block.Block? GetBlockFromHead(int blockIdFromHead)
        {
            if (blockIdFromHead < EntireChainLength && blockIdFromHead >= EntireChainLength - BlocksList.Count())
                return BlocksList[blockIdFromHead - (EntireChainLength - BlocksList.Count())];
            else if (PreviousChain is not null)
                return PreviousChain.GetBlockFromHead(blockIdFromHead);
            else
                return null;
        }

        public IEnumerable<Block.Block> GetEnumerable(Block.Block? startingBlock = null, Block.Block? endingBlock = null)
        {
            // Find starting point of downloading
            var currentChain = this;
            var chainQueue = new Stack<AbstractChain>();

            while (currentChain != null)
            {
                chainQueue.Push(currentChain);
                currentChain = currentChain.PreviousChain;
            }

            // Yield blocks from the chains
            while (chainQueue.Count > 0)
            {
                var chain = chainQueue.Pop();

                foreach (var block in chain.BlocksList)
                {
                    if (startingBlock != null && block.MiningBlockInfo.BlockId <= startingBlock.MiningBlockInfo.BlockId)
                        continue;

                    yield return block;

                    if (endingBlock != null && block.MiningBlockInfo.BlockId == endingBlock.MiningBlockInfo.BlockId)
                        yield break;
                }
            }
        }

        public MutableChain CreateTrunkedChain(Block.Block lastBlock)
        {
            if (lastBlock == null)
            {
                throw new ArgumentNullException(nameof(lastBlock));
            }

            var currentChain = this;
            while (currentChain != null && !currentChain.BlocksList.Contains(lastBlock))
            {
                currentChain = currentChain.PreviousChain;
            }

            if (currentChain == null)
            {
                throw new InvalidOperationException("Last block don't found for truncking");
            }

            var forkedEndChain = new MutableChain();
            if (currentChain.PreviousChain != null)
            {
                forkedEndChain.LinkPreviousChain(currentChain.PreviousChain);
            }

            foreach (var block in currentChain.BlocksList)
            {
                forkedEndChain.AddTailBlock(block);
                if (block == lastBlock)
                {
                    break;
                }
            }

            return forkedEndChain;
        }

    }
}
