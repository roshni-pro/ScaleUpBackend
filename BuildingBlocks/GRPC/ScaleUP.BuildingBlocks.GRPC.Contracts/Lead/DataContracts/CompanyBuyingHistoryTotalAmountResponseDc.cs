﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class CompanyBuyingHistoryTotalAmountResponseDc
    {
        [DataMember(Order = 1)]
        public int MonthTotalAmount { get; set; }
        [DataMember(Order = 2)]
        public long LeadId { get; set; }
        [DataMember(Order = 3)]
        public long CompanyLeadId { get; set; }
    }
}