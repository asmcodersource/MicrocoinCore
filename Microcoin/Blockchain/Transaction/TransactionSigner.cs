using Microcoin.RSAEncryptions;
using Microcoin.Network.NodeNet.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Transaction
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
                transaction.Sign = "";
                transaction.DateTime = DateTime.UtcNow;
                transaction.SenderPublicKey = SignOptions.PublicKey;
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, transaction);
                string sign = RSAEncryption.Sign(memoryStream.ToArray(), SignOptions);
                transaction.Sign = sign;
            }
        }
    }
}
