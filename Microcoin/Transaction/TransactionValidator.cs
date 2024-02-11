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
    internal class MessageValidator : ITransactionValidator
    {
        public ReceiverSignOptions ValidateOptions { get; protected set; }


        public void SetValidateOptions(IReceiverSignOptions options)
        {
            ValidateOptions = options as ReceiverSignOptions;
            if (ValidateOptions == null)
                throw new ArgumentException(nameof(options));
        }

        public bool Validate(ITransaction transaction)
        {
            if (ValidateOptions == null)
                throw new NullReferenceException(nameof(ValidateOptions));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // TODO: Think about sign field impact on transaction sign
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, transaction);
                return RSAEncryption.VerifySign(memoryStream.ToArray(), transaction.Sign, ValidateOptions);
            }
        }

        public static IReceiverSignOptions GetReceiverValidateOptions(Transaction transaction)
        {
            return new ReceiverSignOptions(transaction.SenderPublicKey);
        }
    }
}
