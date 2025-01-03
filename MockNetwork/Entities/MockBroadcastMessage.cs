﻿using Microcoin.Microcoin.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockNetwork.Entities
{
    public class MockBroadcastMessage : IBroadcastMessage
    {
        public string? Payload { get; set; }
        public string PayloadType { get; set; }
    }
}
