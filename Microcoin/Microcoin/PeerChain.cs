using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Mining;
using Microcoin.Microcoin.Blockchain.Block;

namespace Microcoin.Microcoin
{
    // Represent current chain, and operations with it
    public class PeerChain
    {
        public Action<Block> ChainReceiveNextBlock;
        protected ChainStorage.ChainStorage ChainStorage { get; set; }
        public ChainController? ChainController { get; protected set; }
        public ChainFetcher.ChainFetcher ChainFetcher { get; protected set; }
        public INextBlockRule NextBlockRule { get; protected set; }
        public IMiner Miner { get; set; }

        public PeerChain(IMiner miner, ChainFetcher.ChainFetcher chainFetcher)
        {
            Miner = miner;
            ChainFetcher = chainFetcher;
            ChainStorage = DepencyInjection.Container.GetInstance<ChainStorage.ChainStorage>();
        }

        public AbstractChain GetChainTail()
        {
            return ChainController.ChainTail;
        }
       
        public void InitByMostComprehensive()
        {
            var mostComprehensiveChain = ChainStorage.LoadMostComprehensiveChain();
            ChainController = new ChainController(mostComprehensiveChain.Chain, Miner, ChainFetcher);
            ChainController.DefaultInitialize();
            ChainController.ChainReceivedNextBlock += (block) => ChainReceiveNextBlock?.Invoke(block);
        }

        public void InitByInitialChain()
        {
            var initialChainCreator = new InitialChainCreator();
            initialChainCreator.CreateInitialialChain();
            var initialChain = initialChainCreator.InitialChain;
            ChainController = new ChainController(initialChain, Miner, ChainFetcher);
            ChainController.DefaultInitialize();
            ChainController.ChainReceivedNextBlock += (block) => ChainReceiveNextBlock?.Invoke(block);
        }
    }
}
