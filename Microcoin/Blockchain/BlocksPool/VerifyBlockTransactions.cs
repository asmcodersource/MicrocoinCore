using Microcoin.PipelineHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Block
{
    internal class VerifyBlockTransactions : IPipelineHandler<Block>
    {
        protected readonly IHandlePipeline<Transaction.Transaction> transactionVerifyPipeline;

        public VerifyBlockTransactions(IHandlePipeline<Transaction.Transaction> transactionVerifyPipeline)
        {
            this.transactionVerifyPipeline = transactionVerifyPipeline;
        }

        public async Task<bool> Handle(Block block)
        {
            foreach(var transaction in block.Transactions)
            {
                if (transaction.DateTime > block.CreateTime)
                    return false;
                var handleResult = await transactionVerifyPipeline.Handle(transaction);
                if (handleResult.IsHandleSuccesful is not true)
                    return false;
            }
            return true;
        }
    }
}
