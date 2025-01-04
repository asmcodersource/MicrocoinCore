using Microcoin.Microcoin.Blockchain.Transaction;
using Microcoin.Microcoin.RSAEncryptions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.MicrocoinCoreTesting.EncryptionTesting
{
    public class RSAEncryptionsTests
    {
        [Fact]
        public void RSAEncryptions_TransactionEncryption_Test()
        {
            // Create any keys
            var signOptions = RSAEncryption.CreateSignOptions();
            // Sign transaction
            var transaction = new Transaction();
            transaction.ReceiverPublicKey = "";
            TransactionSigner transactionSigner = new TransactionSigner();
            transactionSigner.SetSignOptions(signOptions);
            transactionSigner.Sign(transaction);
            // Validate transaction sign
            TransactionValidator trasnactionValidator = new TransactionValidator();
            trasnactionValidator.SetValidateOptions(TransactionValidator.GetReceiverValidateOptions(transaction));
            var success = trasnactionValidator.Validate(transaction);
            // Is sign validation works fine?
            Assert.True(success);
        }
    }
}
