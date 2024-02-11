using Microcoin.RSAEncryptions;

namespace Microcoin.RSAEncryptions
{
    internal class ReceiverSignOptions : IReceiverSignOptions
    {
        public string PublicKey { get; set; }

        public ReceiverSignOptions(string publicKey)
        {
            PublicKey = publicKey;
        }
    }
}
