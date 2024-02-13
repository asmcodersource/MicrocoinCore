using Microcoin.PipelineHandling;
using Microcoin.RSAEncryptions;
using Microcoin.Transaction;


namespace Microcoin.TransactionPool
{
    internal class VerifyTransactionSign : IPipelineHandler<ITransaction>
    {
        public async Task<bool> Handle(ITransaction transaction)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            IReceiverSignOptions receiverSignOptions = TransactionValidator.GetReceiverValidateOptions(transaction);
            ITransactionValidator transactionValidator = new TransactionValidator();
            transactionValidator.SetValidateOptions(receiverSignOptions);
            bool isSignValid = transactionValidator.Validate(transaction);
            return isSignValid;
        }
    }
}
