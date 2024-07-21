using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Mining;
using Microcoin.Microcoin.Blockchain.Block;
using Microcoin.Microcoin.ChainFetcher;
using SimpleInjector;
using Microcoin.Microcoin.ChainStorage;

namespace Microcoin.Microcoin
{
    // Represent current chain, and operations with it
    public class PeerChain
    {
        public Action<MutableChain> ChainTailPartChanged;
        public Action<MutableChain, Block>? ChainReceiveNextBlock;
        protected ChainStorage.ChainStorage ChainsStorage { get; set; }
        public AbstractChain? ChainTail { get; protected set; }
        public ChainController? ChainController { get; protected set; }
        public ChainFetcher.ChainFetcher ChainFetcher { get; protected set; }
        public Container ServicesContainer { get; protected set; }

        public PeerChain(Container servicesContainer, Peer? parentPeer = null)
        {
            ServicesContainer = servicesContainer;
            ChainsStorage = servicesContainer.GetInstance<ChainStorage.ChainStorage>();
            ChainFetcher = servicesContainer.GetInstance<ChainFetcher.ChainFetcher>();
            Initialize(parentPeer);
        }

        public void Initialize(Peer? initiatorPeer = null)
        {
            ChainsStorage.FetchChains();
            if (ChainsStorage.CountOfChainsHeaders() == 0 && initiatorPeer is not null)
                InitByInitialChain(initiatorPeer);
            else if (ChainsStorage.CountOfChainsHeaders() > 0)
                InitByMostComprehensive();
            else
                throw new Exception("It is not possible to create a new chain or load an existing one from the repository");
        }

        public void SetSpecificChain(MutableChain newChain)
        {
            CreateChainContext(newChain);
        }
       
        public void InitByMostComprehensive()
        {
            var mostComprehensiveChain = ChainsStorage.LoadMostComprehensiveChain();
            if (mostComprehensiveChain is null || mostComprehensiveChain?.Chain is null )
                throw new Exception("Chain storage return null chain context or null chain");
            CreateChainContext(mostComprehensiveChain.Chain);
        }

        public void InitByInitialChain(Peer initiatorPeer)
        {
            var initialChainCreator = new InitialChainCreator(initiatorPeer);
            var initialChain = initialChainCreator.CreateInitialialChain();
            ChainsStorage.AddNewChainToStorage(initialChain);
            CreateChainContext(initialChain);
        }

        public async Task<bool> TryAcceptBlock(Block block)
        {
            if (ChainController is null)
                throw new Exception("Chain controlled dont initialized");
            return await ChainController.AcceptBlock(block);
        }

        public AbstractChain GetChainTail()
        {
            if (ChainController is null || ChainController.ChainTail is null)
                throw new Exception("Chain controller or chain controller tail is null");
            return ChainController.ChainTail;
        }

        private void CreateChainContext(MutableChain chain)
        {
            ChainController = new ChainController(chain, ServicesContainer);
            ChainController.DefaultInitialize();
            ChainController.ChainReceivedNextBlock += (chain, block) => ChainReceiveNextBlock?.Invoke(chain, block);
            ChainController.ChainHasNewTailPart += ChainTailPartChanged;
            ChainController.IsAllowChainFetchingRequests = true;
            ChainTailPartChanged?.Invoke(chain);
        }
    }
}
