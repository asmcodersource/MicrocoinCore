using Microcoin.Microcoin.RSAEncryptions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microcoin.Microcoin.Blockchain.Transaction
{
    public class TransactionSigner : ITransactionSigner
    {
        public SenderSignOptions SignOptions { get; set; }


        public void SetSignOptions(ISenderSignOptions options)
        {
            SignOptions = options as SenderSignOptions;
            if (SignOptions == null)
                throw new ArgumentException(nameof(options));
        }

        public void Sign(Transaction transaction)
        {
            if (SignOptions == null)
                throw new NullReferenceException(nameof(SignOptions));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                transaction.DateTime = DateTime.UtcNow;
                transaction.SenderPublicKey = SignOptions.PublicKey;
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, transaction.ReceiverPublicKey);
                formatter.Serialize(memoryStream, transaction.SenderPublicKey);
                formatter.Serialize(memoryStream, transaction.DateTime);
                formatter.Serialize(memoryStream, transaction.TransferAmount);
                transaction.Sign = RSAEncryption.Sign(memoryStream.ToArray(), SignOptions);
            }
        }
    }
}
