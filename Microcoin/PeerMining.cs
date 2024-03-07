using Microcoin.Blockchain.Block;
using Microcoin.Blockchain.Chain;
using Microcoin.Blockchain.ChainController;
using Microcoin.Blockchain.Mining;
using Microcoin.Blockchain.TransactionsPool;
using System.Threading;

namespace Microcoin
{
    public class PeerMining
    {
        public event Action<Block> BlockMined;
        
        public int MaxTransactionsPerBlock { get; set; } = 512;
        public IMiner Miner { get; protected set; } 
        public TransactionsPool LinkedTransactionsPool { get; protected set; }
        public bool IsMiningEnabled { get; protected set; } = false;
        public bool IsMiningRunning { get; protected set; } = false;
        public string MinerWaller { get; set; }

        public void InizializeMiner(IMiner miner, string minerWallet, TransactionsPool transactionsPool)
        {
            Miner = miner;
            MinerWaller = minerWallet;
            LinkedTransactionsPool = transactionsPool;
        }

        public void StartMining()
        {
            IsMiningEnabled = true;
        }

        public void StopMining()
        {
            IsMiningEnabled = false;
        }

        public void TryStartMineBlock(IChain tailChain, IDeepTransactionsVerify deepTransactionsVerify, CancellationToken cancellationToken)
        {
            if (IsMiningEnabled && IsMiningRunning )
                StartMineBlock(tailChain, deepTransactionsVerify, cancellationToken);
        }
        
        public void StartMineBlock(IChain tailChain, IDeepTransactionsVerify deepTransactionsVerify, CancellationToken cancellationToken)
        {
            var transactions = LinkedTransactionsPool.ClaimTailTransactions(tailChain, deepTransactionsVerify, MaxTransactionsPerBlock);
            if (transactions.Count == 0)
                return;
            Block block = new Block();
            block.MiningBlockInfo = new MiningBlockInfo();
            block.Transactions = transactions;
            Miner.LinkBlockToChain(tailChain, block);
            // TODO: verify this line
            Task.Run(() => MineBlock(tailChain, block, deepTransactionsVerify, cancellationToken));
        }
 

        protected async Task MineBlock(IChain tailChain, Block block, IDeepTransactionsVerify deepTransactionsVerify, CancellationToken cancellationToken)
        {
            try
            {
                IsMiningRunning = true;
                block.Hash = await Miner.StartBlockMining(tailChain, block, MinerWaller, cancellationToken);
                BlockMined.Invoke(block);
            }
            catch (OperationCanceledException) { }
            finally { IsMiningRunning = false; }
        }
    }
}
