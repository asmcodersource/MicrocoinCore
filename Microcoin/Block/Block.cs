using Microcoin.Mining;
using System.Text.Json;

namespace Microcoin.Block
{
    internal class Block
    {
        public List<Transaction.Transaction> Transactions { get; set; } = new List<Transaction.Transaction>();
        public MiningBlockInfo MiningBlockInfo { get; set; } = new MiningBlockInfo();
        public String Hash { get; set; } = "";
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        public static Block? ParseBlockFromJson(string blockJson)
            => JsonSerializer.Deserialize<Block>(blockJson);
    }
}
