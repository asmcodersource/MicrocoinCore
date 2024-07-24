using Microcoin.Microcoin.Blockchain.Chain;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microcoin.Microcoin.ChainStorage
{
    public class ChainIdentifier
    {
        public int TailChainBlockCount { get; protected set; }
        public int TailBlockId { get; protected set; }
        public string TailBlockHash { get; protected set; }
        public int ChainComplexity { get; protected set; }

        // Deserialize constructor
        [JsonConstructor]
        public ChainIdentifier(int tailChainBlockCount, int tailBlockId, string tailBlockHash, int chainComplexity)
        {
            TailChainBlockCount = tailChainBlockCount;
            TailBlockId = tailBlockId;
            TailBlockHash = tailBlockHash;
            ChainComplexity = chainComplexity;
        }

        public ChainIdentifier(AbstractChain chain)
        {
            var tailBlock = chain.GetLastBlock();
            if (tailBlock == null)
                throw new Exception("Empty chain can't be used to create ChainIdentifier");
            TailChainBlockCount = chain.GetBlocksList().Count();
            TailBlockId = tailBlock.MiningBlockInfo.BlockId;
            TailBlockHash = tailBlock.Hash;
            ChainComplexity = tailBlock.MiningBlockInfo.ChainComplexity;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ChainIdentifier another)
                return (TailBlockId, TailBlockHash, ChainComplexity).Equals((another.TailBlockId, another.TailBlockHash, another.ChainComplexity));
            else
                return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TailBlockId, TailBlockHash, ChainComplexity);
        }

        public string GetSHA256()
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                string json = JsonSerializer.Serialize(this);
                var hash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(json));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
