using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Blockchain.ChainController
{
    public interface IDeepTransactionsVerify
    {
        public Task<bool> Verify(AbstractChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken);
    }
}
