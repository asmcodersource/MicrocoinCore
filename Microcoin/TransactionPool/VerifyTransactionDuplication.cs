﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microcoin.PipelineHandling;
using Microcoin.Transaction;

namespace Microcoin.TransactionPool
{
    // Remember: I don't need this verify when receiving trasaction from network
    // because I have condition check before adding new value to list
    // but I need this because something can change from receiving one or multiple block from network
    internal class VerifyTransactionDuplication : IPipelineHandler<ITransaction>
    {
        public Task<bool> Handle(ITransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
