﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Blockchain.Chain
{
    [Serializable]
    public class ImmutableChain : AbstractChain
    {

        public ImmutableChain(AbstractChain chain)
        {
            TransactionsSet = new HashSet<Transaction.Transaction>(chain.TransactionsSet);
            WalletsCoins = new Dictionary<string, decimal>(chain.WalletsCoins);
            PreviousChain = chain.PreviousChain;
            BlocksList = new List<Block.Block>(chain.BlocksList);
            BlocksDictionary = new Dictionary<string, Block.Block>(chain.BlocksDictionary);
        }
    }
}
