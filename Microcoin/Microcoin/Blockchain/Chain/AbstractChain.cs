using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;

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
        public IReadOnlyDictionary<string, double> WalletsCoins { get; protected set; }
        public IReadOnlyCollection<Transaction.Transaction> TransactionsSet { get; protected set; }
        public IReadOnlyDictionary<string, Microcoin.Blockchain.Block.Block> BlocksDictionary { get; protected set; }
        public IReadOnlyList<Microcoin.Blockchain.Block.Block> BlocksList { get; protected set; }
        public int EntireChainLength { get; protected set; } = 0;


        public IReadOnlyList<Microcoin.Blockchain.Block.Block> GetBlocksList()
            => BlocksList;

        public bool IsChainHasTransaction(Transaction.Transaction transaction)
            => TransactionsSet.Contains(transaction);

        public double GetWalletCoins(string walletPublicKey)
        {
            AbstractChain? currentChain = this;
            double walletsCoins = 0;
            while (currentChain is not null )
            {
                walletsCoins += currentChain.WalletsCoins.ContainsKey(walletPublicKey) ? currentChain.WalletsCoins[walletPublicKey] : 0;
                currentChain = currentChain.PreviousChain;
            }
            return walletsCoins;
        }

        public Microcoin.Blockchain.Block.Block? GetLastBlock()
            => GetBlockFromTail(0);

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
            if( blockIdFromHead < EntireChainLength && blockIdFromHead >= EntireChainLength - BlocksList.Count())
                return BlocksList[blockIdFromHead - (EntireChainLength - BlocksList.Count())];
            else if ( PreviousChain is not null )
                return PreviousChain.GetBlockFromHead(blockIdFromHead);
            else
                return null;
        }

        public IEnumerable<Block.Block> GetEnumerable(Block.Block? startingBlock = null, Block.Block? endingBlock = null)
        {
            // Find starting point of downloading
            // find last part of chain, that need to be taken from source
            var currentChain = this;
            Stack<AbstractChain> chainQueue = new Stack<AbstractChain>();
            if (startingBlock is not null)
            {
                do
                {
                    if (currentChain is null)
                        throw new Exception("Something wen't wrong with finding last chain");
                    chainQueue.Push(currentChain);
                    currentChain = currentChain.PreviousChain;
                } while ((currentChain.EntireChainLength - 1) > startingBlock.MiningBlockInfo.BlockId);
            }
            else
            {
                do
                {
                    chainQueue.Push(currentChain);
                    currentChain = currentChain.PreviousChain;
                } while (currentChain is not null);
            }

            // We return blocks through the yield generator, either to the end of the chain,
            // or to the requested block, depending on the parameter
            while (chainQueue.Count > 0)
            {
                var chain = chainQueue.Pop();
                foreach (var block in chain.BlocksList)
                {
                    if (startingBlock is not null && block.MiningBlockInfo.BlockId <= startingBlock.MiningBlockInfo.BlockId)
                        continue;
                    yield return block;
                    if (endingBlock is not null && block.MiningBlockInfo.BlockId == endingBlock.MiningBlockInfo.BlockId)
                        yield break;
                }
            }
            yield break;
        }

        public MutableChain CreateTrunkedChain(Block.Block lastBlock)
        {
            // find last part of chain, that need to be taken from source
            AbstractChain endingChain = this;
            while (endingChain is not null )
            {
                if (endingChain.BlocksList.Contains(lastBlock))
                    break;
                endingChain = endingChain.PreviousChain;
            }
            var forkedEndChain = new MutableChain();
            if (endingChain.PreviousChain is not null)
                forkedEndChain.LinkPreviousChain(endingChain.PreviousChain);
            var endingChainBlocks = endingChain.GetBlocksList();
            var firstBlock = endingChainBlocks.First();
            var numberOfBlocksToAppend = lastBlock.MiningBlockInfo.BlockId - firstBlock.MiningBlockInfo.BlockId;
            for (int i = 0; i <= numberOfBlocksToAppend; i++)
                forkedEndChain.AddTailBlock(endingChainBlocks[i]);
            return forkedEndChain;
        }
    }
}
