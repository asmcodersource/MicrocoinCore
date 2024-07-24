using Microcoin.Microcoin.Mining;

namespace Microcoin.Microcoin.Blockchain.Block
{
    [Serializable]
    public class Block
    {
        public List<Transaction.Transaction> Transactions { get; set; } = new List<Transaction.Transaction>();
        public MiningBlockInfo MiningBlockInfo { get; set; } = new MiningBlockInfo();
        public string Hash { get; set; } = "";

        public string GetMiningBlockHash()
        {
            if (Hash == "")
            {
                // hash is not initialized, lets create it for first time...
                var immutalbeTransactionBlock = new ImmutableTransactionsBlock(this);
                immutalbeTransactionBlock.CalculateMiningBlockHash();
            }
            return Hash;
        }

        public static int GetHashComplexity(string hash)
        {
            byte[] hashBytes = Convert.FromBase64String(hash);
            for (int i = 0; i < hashBytes.Length; i++)
            {
                byte b = hashBytes[i];
                for (int j = 0; j < 8; j++)
                    if ((b >> j & 1) == 1)
                        return i * 8 + j;
            }
            return hashBytes.Length * 8;
        }
    }
}
