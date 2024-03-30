﻿using Microcoin.Microcoin.Blockchain.Transaction;
using NodeNet.NodeNet;
using NodeNet.NodeNet.RSAEncryptions;
using NodeNet.NodeNet.TcpCommunication;
using Microcoin.Microcoin.Network.MessageAcceptors;

namespace Microcoin.Microcoin
{
    public class PeerNetworking
    {
        public Node NetworkNode { get; protected set; }
        public IAcceptor EntryAcceptor { get; protected set; }
        public BlocksAcceptor BlocksAcceptor { get; protected set; }
        public TransactionsAcceptor TransactionsAcceptor { get; protected set; }

        public event Action<Microcoin.Blockchain.Block.Block> BlockReceived;
        public event Action<Transaction> TransactionReceived;

        public PeerNetworking() { }

        public PeerNetworking(Node nodeNet)
        {
            NetworkNode = nodeNet;
            NetworkNode.NetworkExplorer.LoadRecentConnectionsFromFile("knownPeers.json");
        }

        public virtual void CreateDefaultRouting()
        {
            BlocksAcceptor = new BlocksAcceptor();
            TransactionsAcceptor = new TransactionsAcceptor();
            EntryAcceptor = new EntryAcceptor(TransactionsAcceptor, BlocksAcceptor);
            BlocksAcceptor.BlockReceived += (block) => BlockReceived?.Invoke(block);
            TransactionsAcceptor.TransactionReceived += (transaction) => TransactionReceived?.Invoke(transaction);
            NetworkNode.MessageReceived += (messageContext) => EntryAcceptor.Handle(messageContext);
        }

        public virtual void CreateDefaultNode(int port = 0)
        {
            var senderEncryptionOptions = RSAEncryption.CreateSignOptions();
            var senderTcpOptions = new TcpListenerOptions(port);
            NetworkNode = Node.CreateRSAHttpNode(senderEncryptionOptions, senderTcpOptions);
            NetworkNode.NetworkExplorer.LoadRecentConnectionsFromFile("knownPeers.json");
        }

        public void PostInitialize()
        {
            NetworkNode.NetworkExplorer.SendExploreEcho();
            // Wait until node speaks with network for first time
            Thread.Sleep(5000);
            NetworkNode.NetworkExplorer.SaveRecentConnectionsToFile("knownPeers.json");
        }

        public void SendTransactionToNetwork(Transaction transaction)
        {
            var messageDTO = new
            {
                transaction,
                application = "Microcoin",
                type = "WalletTransaction"
            };
            var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(messageDTO);
            lock (this)
                NetworkNode.SendMessage(messageJson);
        }

        public void SendBlockToNetwork(Microcoin.Blockchain.Block.Block block)
        {
            var messageDTO = new
            {
                block,
                application = "Microcoin",
                type = "ChainBlock"
            };
            var messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(messageDTO);
            lock (this)
                NetworkNode.SendMessage(messageJson);
        }
    }
}
