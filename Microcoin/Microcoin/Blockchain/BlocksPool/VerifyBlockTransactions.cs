using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.PipelineHandling;


namespace Microcoin.Microcoin.Blockchain.BlocksPool
{
    public class VerifyBlockTransactions : IPipelineHandler<Block.Block>
    {
        protected readonly IHandlePipeline<Transaction.Transaction> transactionVerifyPipeline;

        public VerifyBlockTransactions(IHandlePipeline<Transaction.Transaction> transactionVerifyPipeline)
        {
            this.transactionVerifyPipeline = transactionVerifyPipeline;
        }

        public bool Handle(Block.Block block)
        {
            foreach (var transaction in block.Transactions)
            {
                var handleResult = transactionVerifyPipeline.Handle(transaction);
                if (handleResult.IsHandleSuccesful is not true)
                    return false;
            }
            return true;
        }
    }
}
