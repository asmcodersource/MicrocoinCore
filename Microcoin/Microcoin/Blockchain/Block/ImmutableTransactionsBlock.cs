using Microcoin.Microcoin.Mining;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Microcoin.Microcoin.Blockchain.Block
{
    public class ImmutableTransactionsBlock
    {
        public Block InnerBlock { get; protected set; }
        public byte[] TransactionsHash { get; protected set; }
        public byte[] MiningBlockInfoBytes { get; protected set; }


        public ImmutableTransactionsBlock(Block block)
        {
            InnerBlock = block;
            GetTransactionsHash(InnerBlock.Transactions);
            ChangeMiningBlockInfo(InnerBlock.MiningBlockInfo);
        }

        public void ChangeMiningBlockInfo(MiningBlockInfo miningBlockInfo)
        {
            InnerBlock.MiningBlockInfo = miningBlockInfo;
            MiningBlockInfoBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(InnerBlock.MiningBlockInfo));
        }

        public string CalculateMiningBlockHash()
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                sha256Hash.ComputeHash(TransactionsHash);
                sha256Hash.ComputeHash(MiningBlockInfoBytes);
                InnerBlock.Hash = Convert.ToBase64String(sha256Hash.Hash);
                return InnerBlock.Hash;
            }
        }

        protected void GetTransactionsHash(List<Transaction.Transaction> transactions)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var transactionsBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transactions));
                TransactionsHash = sha256Hash.ComputeHash(transactionsBytes);
            }
        }
    }
}
