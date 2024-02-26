﻿using Microcoin.Blockchain.Block;
using Microcoin.Blockchain.Chain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.ChainController
{
    public class DeepTransactionsVerify: IDeepTransactionsVerify
    {
        //  Since this is a long operation, I use a cancel token in case it is no longer needed between complex operations.
        //  cancellationToken.ThrowIfCancellationRequested();

        public async Task<bool> Verify(IChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken)
        {
            var verifyTransferAmountsTask = VerifyTransferAmmounts(chain, transactions, cancellationToken);
            var verifyTransactionsDuplication = VerifyTransationUniqueness(chain, transactions, cancellationToken);
            await Task.WhenAll(verifyTransactionsDuplication, verifyTransferAmountsTask);
            if (verifyTransferAmountsTask.Result is true && verifyTransactionsDuplication.Result is true)
                return true;
            return false;
        }

        public async Task<bool> VerifyTransferAmmounts(IChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken)
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

        public async Task<bool> VerifyTransationUniqueness(IChain chain, List<Transaction.Transaction> transactions, CancellationToken cancellationToken)
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
            } while(currentChain != null);
            return true;
        }
    }
}
