using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public class ChainDownloadingException : Exception
    {
        public ChainDownloadingException(string? reason = null) : base(reason) { }
    }
}
