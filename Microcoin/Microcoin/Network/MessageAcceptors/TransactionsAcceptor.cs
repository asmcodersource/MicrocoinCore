using Microcoin.Microcoin.Blockchain.Transaction;
using System.Text.Json;

namespace Microcoin.Microcoin.Network.MessageAcceptors
{
    public class TransactionsAcceptor : IAcceptor
    {
        public event Action<List<Transaction>>? TransactionReceived;

        public void Handle(IBroadcastMessage message)
        {
            List<Transaction> transaction = JsonSerializer.Deserialize<List<Transaction>>(message.Payload);
            Serilog.Log.Verbose($"Microcoin peer | Transaction({transaction.GetHashCode()}) accepted from network");
            TransactionReceived?.Invoke(transaction);
        }
    }
}
