﻿namespace Microcoin.Microcoin.RSAEncryptions
{
    public interface ISenderSignOptions
    {
        public string PublicKey { get; protected set; }
        public string PrivateKey { get; protected set; }
    }
}
