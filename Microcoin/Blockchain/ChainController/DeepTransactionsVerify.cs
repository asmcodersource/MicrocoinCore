using Microcoin.Blockchain.Chain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.ChainController
{
    internal class DeepTransactionsVerify: IDeepTransactionsVerify
    {
        public async Task<bool> Verify(IChain chain, List<Transaction.Transaction> transaction, CancellationToken cancellationToken)
        { 

            return true;
        }
    }
}
