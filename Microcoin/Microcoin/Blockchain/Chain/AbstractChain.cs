using Microcoin.Microcoin.Blockchain.Block;

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
        public Dictionary<string, double> WalletsCoins { get; protected set; }
        public AbstractChain? PreviousChain { get; protected set; }
        public HashSet<Transaction.Transaction> TransactionsSet { get; protected set; }
        public Dictionary<string, Microcoin.Blockchain.Block.Block> BlocksDictionary { get; protected set; }
        public int ChainLength { get; protected set; } = 0;
        protected List<Microcoin.Blockchain.Block.Block> blocksList { get; set; }


        public void LinkPreviousChain(AbstractChain previousChain)
        { 
            PreviousChain = previousChain;
            ChainLength = blocksList.Count() + PreviousChain.ChainLength;
        }

        public List<Microcoin.Blockchain.Block.Block> GetBlocksList()
            => blocksList;

        /// <summary> Use it careful, because it don't update initiale state of object </summary>
        public void SetBlockList(List<Microcoin.Blockchain.Block.Block> blockList)
        { 
            blocksList = blockList;
            ChainLength = blocksList.Count() + PreviousChain.ChainLength;
        }

        public bool IsChainHasTransaction(Transaction.Transaction transaction)
            => TransactionsSet.Contains(transaction);

        public double GetWalletCoins(string walletPublicKey)
            => WalletsCoins.ContainsKey(walletPublicKey) ? WalletsCoins[walletPublicKey] : 0;

        public Microcoin.Blockchain.Block.Block? GetLastBlock()
            => GetBlockFromTail(0);

        public Microcoin.Blockchain.Block.Block? GetBlockFromTail(int blockIdFromTail)
        {
            if (blockIdFromTail < blocksList.Count)
                return blocksList[(blocksList.Count - blockIdFromTail) - 1];
            else if (PreviousChain is not null)
                return PreviousChain.GetBlockFromTail(blockIdFromTail - blocksList.Count);
            return null;
        }

        public Microcoin.Blockchain.Block.Block? GetBlockFromHead(int blockIdFromHead)
        {
            if (ChainLength - blocksList.Count() <= blockIdFromHead && ChainLength > blockIdFromHead)
                return blocksList[ChainLength - (blockIdFromHead + 1)];
            else if (PreviousChain is not null)
                return GetBlockFromHead(blockIdFromHead);
            else
                return null;
        }
    }
}
