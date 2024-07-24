using Microcoin.Microcoin.RSAEncryptions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microcoin.Microcoin.Blockchain.Transaction
{
    public class TransactionValidator : ITransactionValidator
    {
        public ReceiverSignOptions ValidateOptions { get; protected set; }


        public void SetValidateOptions(IReceiverSignOptions options)
        {
            ValidateOptions = options as ReceiverSignOptions;
            if (ValidateOptions == null)
                throw new ArgumentException(nameof(options));
        }

        public bool Validate(Transaction transaction)
        {
            if (ValidateOptions == null)
                throw new NullReferenceException(nameof(ValidateOptions));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, transaction.ReceiverPublicKey);
                formatter.Serialize(memoryStream, transaction.SenderPublicKey);
                formatter.Serialize(memoryStream, transaction.DateTime);
                formatter.Serialize(memoryStream, transaction.TransferAmount);
                return RSAEncryption.VerifySign(memoryStream.ToArray(), transaction.Sign, ValidateOptions);
            }
        }

        public static IReceiverSignOptions GetReceiverValidateOptions(Transaction transaction)
        {
            return new ReceiverSignOptions(transaction.SenderPublicKey);
        }
    }
}
