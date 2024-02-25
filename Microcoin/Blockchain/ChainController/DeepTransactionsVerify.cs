using Microcoin.Blockchain.Block;
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
        public async Task<bool> Verify(IChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken)
        {
            // Count summary coins difference after this block
            cancellationToken.ThrowIfCancellationRequested();
            Dictionary<string, decimal> tempCoinsCount = new Dictionary<string, decimal>();
            foreach (Transaction.Transaction transaction in transactions)
            {
                if (tempCoinsCount.TryAdd(transaction.SenderPublicKey, -transaction.TransferAmount) is not true)
                    tempCoinsCount[transaction.SenderPublicKey] -= transaction.TransferAmount;
                if (tempCoinsCount.TryAdd(transaction.ReceiverPublicKey, transaction.TransferAmount) is not true)
                    tempCoinsCount[transaction.ReceiverPublicKey] += transaction.TransferAmount;
            }
            cancellationToken.ThrowIfCancellationRequested();
            // Any wallet can't have less than zero coint after transaction
            foreach (var walletCoinsPair in tempCoinsCount)
                if (chain.GetWalletCoins(walletCoinsPair.Key) + tempCoinsCount[walletCoinsPair.Key] < 0)
                    return false;
            // anything is okay
            return true;
        }
    }
}
