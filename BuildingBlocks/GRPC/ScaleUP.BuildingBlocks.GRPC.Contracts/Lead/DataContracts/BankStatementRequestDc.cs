﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class BankStatementRequestDc
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }
        [DataMember(Order = 2)]
        public string ProductCode { get; set; }
    }
}