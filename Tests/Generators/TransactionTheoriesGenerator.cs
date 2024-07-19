﻿using Microcoin.Microcoin;
using Microcoin.Microcoin.Blockchain.Transaction;
using System.Text;

namespace Tests.Generators
{
    public enum InvalidTheoryType
    {
        WrongTransferAmount,
        WrongPublicKey,
        WrongSignature,
        WrongTime,
    }

    public class TransactionTheory
    {
        public string WrongType { get; set; } = "None";
        public readonly Transaction Transaction;
        public readonly bool IsTransactionValid;

        public TransactionTheory(Transaction transaction, bool isTransactionValid)
        {
            Transaction = transaction;
            IsTransactionValid = isTransactionValid;
        }
    }

    public static class TransactionTheoriesGenerator
    {
        public static List<TransactionTheory> GetValidTransactionsTheories(List<Peer> peers, int count)
        {
            var transactionTheories = new List<TransactionTheory>();
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                Peer first_peer = null;
                Peer second_peer = null;
                do
                {
                    first_peer = peers[random.Next(peers.Count)];
                    second_peer = peers[random.Next(peers.Count)];
                } while (first_peer == second_peer);
                var transaction = first_peer.CreateTransaction(second_peer.PeerWalletKeys.TransactionSigner.SignOptions.PublicKey, Random.Shared.Next());
                var theory = new TransactionTheory(transaction, true);
                transactionTheories.Add(theory);
            }
            return transactionTheories;
        }

        public static List<TransactionTheory> GetInvalidTransactionsTheories(List<Peer> peers, int count)
        {
            var transactionTheories = new List<TransactionTheory>();
            var random = new Random();
            Array values = Enum.GetValues(typeof(InvalidTheoryType));
            char[] chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();
            for (int i = 0; i < count; i++)
            {
                Peer first_peer = null;
                Peer second_peer = null;
                do
                {
                    first_peer = peers[random.Next(peers.Count)];
                    second_peer = peers[random.Next(peers.Count)];
                } while (first_peer == second_peer);
                StringBuilder stringBuilder = null;
                var transaction = first_peer.CreateTransaction(second_peer.PeerWalletKeys.TransactionSigner.SignOptions.PublicKey, 10);
                var wrongType = values.GetValue(random.Next(values.Length));
                switch (wrongType)
                {
                    case InvalidTheoryType.WrongSignature:
                        stringBuilder = new StringBuilder(transaction.Sign);
                        do
                        {
                            stringBuilder[random.Next(transaction.Sign.Length)] = chars[random.Next(chars.Length)];
                        } while (stringBuilder.ToString() == transaction.Sign);
                        transaction.Sign = stringBuilder.ToString();
                        break;
                    case InvalidTheoryType.WrongTime:
                        transaction.DateTime = DateTime.UtcNow + new TimeSpan(1, random.Next(10), random.Next(50) + 50);
                        break;
                    case InvalidTheoryType.WrongPublicKey:
                        stringBuilder = new StringBuilder(transaction.SenderPublicKey);
                        do
                        {
                            stringBuilder[random.Next(transaction.SenderPublicKey.Length)] = chars[random.Next(chars.Length)];
                        } while (stringBuilder.ToString() == transaction.SenderPublicKey);
                        transaction.SenderPublicKey = stringBuilder.ToString();
                        break;
                    case InvalidTheoryType.WrongTransferAmount:
                        transaction.TransferAmount = -random.Next();
                        break;
                    default:
                        throw new Exception("Invalid \"Invalid theory type\"");
                };
                var theory = new TransactionTheory(transaction, false);
                theory.WrongType = wrongType.ToString();
                transactionTheories.Add(theory);
            }
            return transactionTheories;
        }

        public static List<Peer> CreateTestPeers(int peersListLength)
        {
            var peers = new List<Peer>();
            for (int i = 0; i < peersListLength; i++)
            {
                var peerBuilder = new PeerBuilder();
                peerBuilder.AddDebugServices();
                var peer = peerBuilder.Build();
                peers.Add(peer);
            }
            return peers;
        }
    }
}
