using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class InitiateLeadOfferRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public List<LeadNbfcResponse> Companys { get; set; }
        [DataMember(Order = 3)]
        public UserDetailsReply kycdetail { get; set; }
        [DataMember(Order = 4)]
        public List<LeadNBFCSubActivityRequestDc> LeadNBFCSubActivityRequest { get; set; }
        [DataMember(Order = 6)]
        public double? CreditLimit { get; set; }


    }
}
