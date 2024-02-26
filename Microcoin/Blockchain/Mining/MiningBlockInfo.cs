using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    public class MiningBlockInfo
    {
        public int BlockId { get; set; } = -1;
        public string PreviousBlockHash { get; set; } = "";
        public string MinerPublicKey { get; set; } = "";
        public decimal MinerReward { get; set; } = 0;
        public double Complexity { get; set; } = 0;
    }
}
