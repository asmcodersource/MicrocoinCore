using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Mining
{
    public class MiningRules
    {
        public IComplexityRule ComplexityRule { get; set; }
        public IRewardRule RewardRule { get; set; }

        public MiningRules(IComplexityRule complexityRule, IRewardRule rewardRule) 
        {
            ComplexityRule = complexityRule;
            RewardRule = rewardRule;
        }
    }
}
