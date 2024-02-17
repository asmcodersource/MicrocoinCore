using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.RSAEncryptions
{
    public interface ISenderSignOptions
    {
        public string PublicKey { get; protected set; }
        public string PrivateKey { get; protected set; }
    }
}
