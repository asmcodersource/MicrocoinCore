using System.Text.Json;

namespace Microcoin.Blockchain.Transaction
{
    [Serializable]
    public class Transaction
    {
        public decimal TransferAmount { get; set; }
        public string SenderPublicKey { get; set; }
        public string ReceiverPublicKey { get; set; }
        public string Sign { get; set; }
        public DateTime DateTime { get; set; }

        public override string ToString()
        {
            return String.Join("\n", new object[]{ SenderPublicKey, ReceiverPublicKey, TransferAmount, Sign});
        }

        public static Transaction? ParseTransactionFromJson(string transactionJson)
            => JsonSerializer.Deserialize<Transaction>(transactionJson);
    }
}
