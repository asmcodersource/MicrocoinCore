
using Microcoin.Microcoin.Blockchain.BlocksPool;
using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using Microcoin.Microcoin.ChainFetcher;
using Microcoin.Microcoin.Mining;
using Microcoin.Microcoin.Network;
using SimpleInjector;

namespace Microcoin.Microcoin
{
    public class PeerBuilder
    {
        public Container ServicesContainer { get; protected set; } = new Container();

        // -------------------------------------------------------------
        // Rules registrations
        public void AddDefaultRules()
        {
            ServicesContainer.Register<IDeepTransactionsVerify, DeepTransactionsVerify>(Lifestyle.Transient);
            ServicesContainer.Register<IFetchableChainRule, FetchableChainRule>(Lifestyle.Transient);
            ServicesContainer.Register<INextBlockRule, NextBlockRule>(Lifestyle.Transient);
        }

        // -------------------------------------------------------------
        // Pools registrations
        public void AddDefaultAcceptancePools()
        {
            ServicesContainer.Register<TransactionsPool>(Lifestyle.Transient);
            ServicesContainer.Register<BlocksPool>(Lifestyle.Transient);
        }

        // -------------------------------------------------------------
        // PeerWallet builder functions
        public void AddWalletKeys()
            => ServicesContainer.RegisterInstance(PeerWalletKeys.CreateKeys());

        public void AddWalletKeysFromFile(string file)
            => ServicesContainer.RegisterInstance(PeerWalletKeys.LoadKeys(file));

        public void AddWalletKeysFromFileOrCreate(string file)
            => ServicesContainer.RegisterInstance(PeerWalletKeys.LoadOrCreateWalletKeys(file));


        // Consts registration
        public void AddChainBranchBlocksCount(int branchValue)
        {
            ServicesContainer.RegisterInstance<ChainBranchBlocksCount>(new ChainBranchBlocksCount(branchValue));
        }


        // -------------------------------------------------------------
        // Network services registration
        public void AddBroadcastTransceiver(IBroadcastTransceiver broadcastTransceiver)
        {
            ServicesContainer.RegisterInstance<IBroadcastTransceiver>(broadcastTransceiver);
        }

        public void AddBroadcastSessionManager(ISessionManager sessionManager)
        {
            ServicesContainer.RegisterInstance<ISessionManager>(sessionManager);
        }

        public void AddEndPointCollectionProvider(IEndPointCollectionProvider endPointCollectionProvider)
        {
            ServicesContainer.RegisterInstance<IEndPointCollectionProvider>(endPointCollectionProvider);
        }

        // -------------------------------------------------------------
        // Miner builder functions
        public void AddDefaultMiner()
        {
            Miner miner = new Miner();
            miner.SetRules(
                new MiningRules(
                    new ComplexityRule(),
                    new RewardRule()
                )
            );
            ServicesContainer.RegisterInstance<IMiner>(miner);
        }

        public void AddDebugMiner()
        {
            Miner miner = new Miner();
            miner.SetRules(
                new MiningRules(
                    new DebugComplexityRule(),
                    new RewardRule()
                )
            );
            ServicesContainer.RegisterInstance<IMiner>(miner);
        }

        public void AddMiner(IComplexityRule complexityRule, IRewardRule rewardRule)
        {
            Miner miner = new Miner();
            miner.SetRules(
                new MiningRules(
                    complexityRule,
                    rewardRule
                )
            );
            ServicesContainer.RegisterInstance<IMiner>(miner);
        }

        // -------------------------------------------------------------
        // ChainsStorage builder functions
        public void AddChainsStorage(string workingDirectory)
        {
            Directory.CreateDirectory(workingDirectory);
            var chainStorage = new ChainStorage.ChainStorage();
            chainStorage.WorkingDirectory = workingDirectory;
            chainStorage.FetchChains();
            ServicesContainer.RegisterInstance(chainStorage);
        }

        public void AddChainsFetcher()
        {
            ServicesContainer.Register<ChainFetcher.ChainFetcher>(Lifestyle.Singleton);
        }

        // -------------------------------------------------------------
        // ChainsStorage builder functions
        public void AddChainProvidersRating(IChainProvidersRating chainProvidersRating)
        {
            ServicesContainer.RegisterInstance<IChainProvidersRating>(chainProvidersRating);
        }

        // -------------------------------------------------------------
        // Miner builder functions

        public Peer Build()
        {
            ServicesContainer.Verify();
            var peer = new Peer(ServicesContainer);
            return peer;
        }

        public void AddDefaultServices()
        {
            AddChainBranchBlocksCount(10);
            AddDefaultMiner();
            AddDefaultAcceptancePools();
            AddDefaultRules();
            AddChainsStorage("chains");
            AddChainsFetcher();
            AddWalletKeys();
        }

        public void AddDebugServices()
        {
            AddChainBranchBlocksCount(10);
            AddDebugMiner();
            AddDefaultAcceptancePools();
            AddDefaultRules();
            AddChainsStorage("chains");
            AddChainsFetcher();
            AddWalletKeys();
        }
    }
}
