using Microcoin.Blockchain.TransactionsPool;
using Microcoin.Blockchain.Transaction;
using Newtonsoft.Json;

namespace Microcoin
{
    internal class Peer
    {
        public TransactionsPool TransactionsPool { get; protected set; } = new TransactionsPool();
        public PeerNetworking PeerNetworking { get; protected set; } 
        public PeerWalletKeys PeerWalletKeys { get; protected set; }


        public void SendCoins(String receiverPublicKey, decimal coinsCount )
        {
            if (PeerNetworking == null || PeerWalletKeys == null)
                throw new NullReferenceException("Peer is not initialized");

            Transaction transaction = new Transaction();
            transaction.ReceiverPublicKey = receiverPublicKey;
            transaction.TransferAmount = coinsCount;
            transaction.DateTime = DateTime.UtcNow;
            PeerWalletKeys.SignTransaction( transaction );

            var transactionBroadcast = JsonConvert.SerializeObject( transaction );
            PeerNetworking.NetworkNode.SendMessage(transactionBroadcast);
        }

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
