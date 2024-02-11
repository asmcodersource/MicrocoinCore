﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microcoin.Transaction
{
    internal interface ITransaction
    {
        decimal TransferAmount { get; set; }
        public string SenderPublicKey { get; set; }
        public string ReceiverPublicKey { get; set; }
        public string Sign { get; set; }
    }
}
