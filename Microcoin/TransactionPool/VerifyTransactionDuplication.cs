﻿using Microcoin.PipelineHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.TransactionPool
{
    internal class VerifyTransactionDuplication : IPipelineHandler<ITransaction>
    {
        public Task<bool> Handle(ITransaction handleObject)
        {
            throw new NotImplementedException();
        }
    }
}
