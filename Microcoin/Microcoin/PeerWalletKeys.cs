using Microcoin.Microcoin.Blockchain.Transaction;
using System.Xml.Serialization;

namespace Microcoin.Microcoin
{
    public class PeerWalletKeys
    {
        public TransactionSigner? TransactionSigner { get; protected set; }

        protected PeerWalletKeys() { }

        public static PeerWalletKeys LoadOrCreateWalletKeys(string filePath = "wallet.keys")
        {
            if (File.Exists(filePath))
            {
                return LoadKeys(filePath);
            }
            else
            {
                var walletKeys = CreateKeys();
                walletKeys.SaveKeys(filePath);
                return walletKeys;
            }
        }

        public static PeerWalletKeys LoadKeys(string filePath)
        {
            var peerWalletKeys = new PeerWalletKeys();
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(TransactionSigner));
                peerWalletKeys.TransactionSigner = xSer.Deserialize(fs) as TransactionSigner;
            }
            return peerWalletKeys;
        }

        public static PeerWalletKeys CreateKeys()
        {
            var peerWalletKeys = new PeerWalletKeys();
            var signOptions = RSAEncryptions.RSAEncryption.CreateSignOptions();
            peerWalletKeys.TransactionSigner = new TransactionSigner();
            peerWalletKeys.TransactionSigner.SetSignOptions(signOptions);
            return peerWalletKeys;
        }

        public void SaveKeys(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    XmlSerializer xSer = new XmlSerializer(typeof(TransactionSigner));
                    xSer.Serialize(fs, TransactionSigner);
                }
            }
            catch (Exception ex)
            {
                // well, at least, I should remove broken file
                File.Delete(filePath);
                throw ex;
            }
        }

        public void SignTransaction(Transaction transaction)
        {
            if (TransactionSigner == null)
                throw new NullReferenceException("Keys is not initialized");
            TransactionSigner.Sign(transaction);
        }
    }
}
