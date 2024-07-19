using Microcoin.Microcoin.Blockchain.Chain;
using Microcoin.Microcoin.Blockchain.ChainController;
using Microcoin.Microcoin.Blockchain.TransactionsPool;
using Microcoin.Microcoin.Mining;

namespace Microcoin.Microcoin
{
    public class PeerMining
    {
        public event Action<Microcoin.Blockchain.Block.Block> BlockMined;
        protected Thread? MiningThread;
        protected CancellationTokenSource? MiningCancellationTokenSource;
        public int MaxTransactionsPerBlock { get; set; } = 512;
        public IMiner Miner { get; protected set; }
        public TransactionsPool LinkedTransactionsPool { get; protected set; }
        public bool IsMiningEnabled { get; protected set; } = false;
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
            CancelCurrentMiningProcess();
        }

        /// <returns>true if some mining process was cancelled</returns>
        public bool CancelCurrentMiningProcess()
        {
            lock (this)
            {
                // I use temporary variable, to prevent some concurrency error
                var temp = MiningThread;
                if (temp is not null)
                {
                    MiningCancellationTokenSource?.Cancel();
                    temp.Join();
                    return true;
                }
                return false;
            }
        }

        public void TryStartMineBlock(AbstractChain tailChain, IDeepTransactionsVerify deepTransactionsVerify)
        {
            Serilog.Log.Verbose($"Microcoin peer | Trying to start mine new block");
            lock (this)
            {
                if (MiningThread is not null || IsMiningEnabled is not true )
                    return;
                Serilog.Log.Debug($"Microcoin peer | Starting to mine new block");
                MiningCancellationTokenSource = new CancellationTokenSource();
                StartMineBlock(tailChain, deepTransactionsVerify, MiningCancellationTokenSource.Token);
            }
        }

        public void StartMineBlock(AbstractChain tailChain, IDeepTransactionsVerify deepTransactionsVerify, CancellationToken cancellationToken)
        {
            var transactions = LinkedTransactionsPool.ClaimTailTransactions(tailChain, deepTransactionsVerify, MaxTransactionsPerBlock);
            if (transactions.Count == 0)
                return;
            Microcoin.Blockchain.Block.Block block = new Microcoin.Blockchain.Block.Block();
            block.MiningBlockInfo = new MiningBlockInfo();
            block.Transactions = transactions;
            // TODO: verify this line
            MiningThread = new Thread(() => MineBlock(tailChain, block, deepTransactionsVerify, cancellationToken));
            MiningThread.Start();
        }


        protected void MineBlock(AbstractChain tailChain, Microcoin.Blockchain.Block.Block block, IDeepTransactionsVerify deepTransactionsVerify, CancellationToken cancellationToken)
        {
            try
            {
                (Miner as IMiner).LinkBlockToChain(tailChain, block);
                block.Hash = Miner.StartBlockMining(tailChain, block, MinerWaller, cancellationToken).Result;
                if (cancellationToken.IsCancellationRequested is not true )
                    Task.Run(() => BlockMined.Invoke(block));
            }
            catch { }
            finally {
                MiningThread = null;
            }
        }
    }
}
