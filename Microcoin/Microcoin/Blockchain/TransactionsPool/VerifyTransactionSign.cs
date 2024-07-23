using Microcoin.PipelineHandling;
using Microcoin.RSAEncryptions;
using Microcoin.Microcoin.Blockchain.Transaction;

namespace Microcoin.Microcoin.Blockchain.TransactionsPool
{
    public class VerifyTransactionSign : IPipelineHandler<Transaction.Transaction>
    {
        public bool Handle(Transaction.Transaction transaction)
        {
            IReceiverSignOptions receiverSignOptions = TransactionValidator.GetReceiverValidateOptions(transaction);
            ITransactionValidator transactionValidator = new TransactionValidator();
            transactionValidator.SetValidateOptions(receiverSignOptions);
            return transactionValidator.Validate(transaction);
        }
    }
}
