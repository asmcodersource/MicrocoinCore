using Microcoin.Blockchain.Transaction;
using System.IO;
using System.Xml.Serialization;

namespace Microcoin
{
    internal class PeerWalletKeys
    {
        protected TransactionSigner? transactionSigner;

        public void CreateKeys()
        {
            var signOptions = RSAEncryptions.RSAEncryption.CreateSignOptions();
            transactionSigner = new TransactionSigner();
            transactionSigner.SetSignOptions(signOptions);
        }

        public void SaveKeys(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(TransactionSigner));
                xSer.Serialize(fs, transactionSigner);
            }
        }

        public void LoadKeys(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer xSer = new XmlSerializer(typeof(TransactionSigner));
                transactionSigner = xSer.Deserialize(fs) as TransactionSigner;
            }
        }
        
        public void SignTransaction(Transaction transaction)
        {
            if (transactionSigner == null)
                throw new NullReferenceException("Keys is not initialized");
            transactionSigner.Sign(transaction);
        }
    }
}
