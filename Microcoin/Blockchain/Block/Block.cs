using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text.Json;
using Microcoin.Blockchain.Mining;
using System.Numerics;

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

        public static int GetHashComplexity(string hash)
        {
            byte[] hashBytes = Convert.FromBase64String(hash);
            for( int i = 0; i < hashBytes.Length; i++)
            {
                byte b = hashBytes[i];
                for (int j = 0; j < 8; j++)
                    if (((b >> 1) & 1) == 1)
                        return i * 8 + b;
            }
            return hashBytes.Length * 8;
        }

        public static Block? ParseBlockFromJson(string blockJson)
            => JsonSerializer.Deserialize<Block>(blockJson);
    }
}
