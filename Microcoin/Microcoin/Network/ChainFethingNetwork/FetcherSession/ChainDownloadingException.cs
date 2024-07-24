namespace Microcoin.Microcoin.Network.ChainFethingNetwork.FetcherSession
{
    public class ChainDownloadingException : Exception
    {
        public ChainDownloadingException(string? reason = null) : base(reason) { }
    }
}
