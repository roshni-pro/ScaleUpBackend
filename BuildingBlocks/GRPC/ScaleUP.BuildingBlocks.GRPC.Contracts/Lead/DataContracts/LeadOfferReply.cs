using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadOfferReply
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 3)]
        public long LeadId { get; set; }

        [DataMember(Order = 4)]
        public int VintageDays { get; set; }
        [DataMember(Order = 5)]
        public string CustomerType { get; set; }
        [DataMember(Order = 6)]
        public int CibilScore { get; set; }
        [DataMember(Order = 7)]
        public int AvgMonthlyBuying { get; set; }
    }
}
