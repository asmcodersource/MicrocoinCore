using Microcoin.RSAEncryptions;

namespace Microcoin.RSAEncryptions
{
    internal class SenderSignOptions : ISenderSignOptions
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public SenderSignOptions(string publicKey, string privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }
    }
}
