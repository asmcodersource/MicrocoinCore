using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Network.ChainFethingNetwork.ProviderSessionListener;
using SimpleInjector;

namespace Microcoin.Microcoin
{
    public class PeerChainsProvider
    {
        public ProviderSessionListener ProviderSessionListener { get; protected set; }

        public PeerChainsProvider(Container container) 
        {
            ProviderSessionListener = new ProviderSessionListener(container);
        }

        public void StartListening(AbstractChain sourceChain)
        {
            ProviderSessionListener.SourceChain = sourceChain;
            ProviderSessionListener.StartListening();
        }

        public void StartListening()
        {
            if (ProviderSessionListener.SourceChain is null)
                throw new Exception("Provider dont have source chain initialized");
            ProviderSessionListener.StartListening();
        }

        public void StopListening()
        {
            ProviderSessionListener.StopListening();
        }

        public void ChangeSourceChain(AbstractChain sourceChain)
        {
            ProviderSessionListener.SourceChain = sourceChain;
        }
    }
}
