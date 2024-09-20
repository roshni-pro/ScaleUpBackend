using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class AcceptOfferByLeadDc
    {
        [DataMember(Order =1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public int Tenure { get; set; }
        [DataMember(Order = 3)]
        public double Amount { get; set; }
        [DataMember(Order = 4)]
        public string otp { get; set; }
        [DataMember(Order = 5)]
        public string requestId { get; set;}
        [DataMember(Order = 6)]
        public string UserId { get; set; }
        [DataMember(Order = 7)]
        public long ActivityId { get; set; }
        [DataMember(Order = 8)]
        public long? SubActivityId { get; set; }
        [DataMember(Order = 9)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 10)]
        public List<ProductSlabConfigResponse>? ProductSlabConfigResponse { get; set; }
        [DataMember(Order = 11)]
        public double GST { get; set; }
    }

}
