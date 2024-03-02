using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Json;
using Microcoin.Blockchain.Mining;

namespace Microcoin.Blockchain.Block
{
    public class Block
    {
        public List<Transaction.Transaction> Transactions { get; set; } = new List<Transaction.Transaction>();
        public MiningBlockInfo MiningBlockInfo { get; set; } = new MiningBlockInfo();
        public string Hash { get; set; } = "";


        /// <returns>Special hash of block, it dosn't include all field of object</returns>
        public string GetMiningBlockHash()
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var objectToHashStream = new MemoryStream();
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectToHashStream, MiningBlockInfo);
                formatter.Serialize(objectToHashStream, Transactions);
                byte[] hash = sha256Hash.ComputeHash(objectToHashStream);
                return Convert.ToBase64String(hash);
            }
        }

        public static Block? ParseBlockFromJson(string blockJson)
            => JsonSerializer.Deserialize<Block>(blockJson);
    }
}
