namespace Microcoin.Microcoin.Blockchain.Chain
{
    [Serializable]
    public class ImmutableChain : AbstractChain
    {
        public ImmutableChain(AbstractChain chain)
        {
            EntireChainLength = chain.EntireChainLength;
            TransactionsSet = chain.TransactionsSet;
            WalletsCoins = chain.WalletsCoins;
            PreviousChain = chain.PreviousChain;
            BlocksList = chain.BlocksList;
            BlocksDictionary = chain.BlocksDictionary;
        }
    }
}
