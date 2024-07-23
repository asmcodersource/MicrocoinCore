using Microcoin.Microcoin.Blockchain.Chain;

namespace Microcoin.Microcoin.Blockchain.ChainController
{
    public class DeepTransactionsVerify : IDeepTransactionsVerify
    {
        //  Since this is a long operation, I use a cancel token in case it is no longer needed between complex operations.
        //  cancellationToken.ThrowIfCancellationRequested();

        public bool Verify(AbstractChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken)
        {
            var verifyTransferAmountsTask = VerifyTransferAmmounts(chain, transactions, cancellationToken);
            var verifyTransactionsDuplication = VerifyTransationUniqueness(chain, transactions, cancellationToken);
            if (verifyTransferAmountsTask is true && verifyTransactionsDuplication is true)
                return true;
            return false;
        }

        public bool VerifyTransferAmmounts(AbstractChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken)
        {
            // Count summary coins difference after this block
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, double> tempCoinsCount = new Dictionary<string, double>();
            foreach (Transaction.Transaction transaction in transactions)
            {
                if (tempCoinsCount.TryAdd(transaction.SenderPublicKey, -transaction.TransferAmount) is not true)
                    tempCoinsCount[transaction.SenderPublicKey] -= transaction.TransferAmount;
                if (tempCoinsCount.TryAdd(transaction.ReceiverPublicKey, transaction.TransferAmount) is not true)
                    tempCoinsCount[transaction.ReceiverPublicKey] += transaction.TransferAmount;
            }
            cancellationToken.ThrowIfCancellationRequested();
            // Any wallet can't have less than zero coins after transaction
            foreach (var walletCoinsPair in tempCoinsCount)
                if (chain.GetWalletCoins(walletCoinsPair.Key) + tempCoinsCount[walletCoinsPair.Key] < 0)
                    return false;
            // anything is okay
            return true;
        }

        public bool VerifyTransationUniqueness(AbstractChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken)
        {
            // TODO:
            // I think I can do something better here.
            // Because currently it looks like the need to pull large HashSets for each branch of the blockchain,
            // and check them for each transaction in the block.
            //      but right now i will implement it as I can right now
            var currentChain = chain;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (Transaction.Transaction transaction in transactions)
                    if (currentChain.IsChainHasTransaction(transaction) is true)
                        return false;
                currentChain = currentChain.PreviousChain;
            } while (currentChain != null);
            return true;
        }
    }
}
