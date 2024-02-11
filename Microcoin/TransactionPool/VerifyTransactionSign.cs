using Microcoin.PipelineHandling;
using Microcoin.Transaction;


namespace Microcoin.TransactionPool
{
    internal class VerifyTransactionSign : IPipelineHandler<ITransaction>
    {
        public Task<bool> Handle(ITransaction handleObject)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            //IReceiverSignOptions receiverSignOptions = new ReceiverSignOptions();
            return taskCompletionSource.Task;
        }
    }
}
