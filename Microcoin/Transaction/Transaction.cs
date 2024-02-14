using System.Text.Json;

namespace Microcoin.Transaction
{
    internal class Transaction
    {
        public decimal TransferAmount { get; set; }
        public string SenderPublicKey { get; set; }
        public string ReceiverPublicKey { get; set; }
        public string Sign { get; set; }
        public DateTime DateTime { get; set; }

        public static Transaction? ParseTransactionFromJson(string transactionJson)
            => JsonSerializer.Deserialize<Transaction>(transactionJson);
    }
}
