using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.PipelineHandling;
using Microcoin.Transaction;

namespace Microcoin.TransactionPool
{
    internal class VerifyTransactionFields : IPipelineHandler<ITransaction>
    {
        public Task<bool> Handle(ITransaction handleObject)
        {
            throw new NotImplementedException();
        }
    }
}
