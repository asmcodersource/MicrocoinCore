using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Microcoin.Transaction
{
    internal record Transaction : ITransaction
    {
        public decimal TransferAmount { get; set; }
        public string SenderPublicKey { get; set; }
        public string ReceiverPublicKey { get; set; }
        public string Sign { get; set; }

        public static ITransaction? ParseTransactionFromJson(string transactionJson)
            => JsonSerializer.Deserialize<Transaction>(transactionJson);
    }
}
