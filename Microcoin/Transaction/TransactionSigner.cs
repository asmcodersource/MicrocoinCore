using Microcoin.RSAEncryptions;
using NodeNet.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Transaction
{
    internal class TransactionSigner : ITransactionSigner
    {
        public SenderSignOptions SignOptions { get; protected set; }


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
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, transaction);
                string sign = RSAEncryption.Sign(memoryStream.ToArray(), SignOptions);
                transaction.Sign = sign;
            }
        }
    }
}
