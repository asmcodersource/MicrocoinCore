using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.PipelineHandling;
using Microcoin.Microcoin.RSAEncryptions;

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
