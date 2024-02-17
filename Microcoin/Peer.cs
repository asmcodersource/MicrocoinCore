using Microcoin.Blockchain.TransactionsPool;
using Microcoin.Network.NodeNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin
{
    internal class Peer
    {
        public TransactionsPool TransactionsPool { get; protected set; } = new TransactionsPool();
        public PeerNetworking PeerNetworking { get; protected set; } 
        public PeerWalletKeys PeerWalletKeys { get; protected set; }

        public void InitializeNetworking()
        {
            PeerNetworking = new PeerNetworking();
            PeerNetworking.CreateDefaultNode();
            PeerNetworking.CreateDefaultRouting();
            PeerNetworking.TransactionReceived += (transaction) => TransactionsPool.HandleTransaction(transaction);
            // PeerNetworking.BlockReceived += 
        }

        public void LoadOrCreateWalletKeys(string filePath = "wallet.keys")
        {
            PeerWalletKeys = new PeerWalletKeys();
            if (File.Exists(filePath))
            {
                PeerWalletKeys.LoadKeys(filePath);
            }
            else
            {
                PeerWalletKeys.CreateKeys();
                PeerWalletKeys.SaveKeys(filePath);
            }
        }
    }
}
