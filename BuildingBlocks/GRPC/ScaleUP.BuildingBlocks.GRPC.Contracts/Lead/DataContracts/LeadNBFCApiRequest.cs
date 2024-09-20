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
    public class LeadNBFCSubActivityRequestDc
    {
        [DataMember(Order = 1)]
        public long NBFCCompanyId { get; set; } 

        [DataMember(Order = 2)]
        public long ActivityMasterId { get; set; }

        [DataMember(Order = 3)]
        public long? SubActivityMasterId { get; set; }

        [DataMember(Order = 4)]
        public string Code { get; set; }
        [DataMember(Order = 5)]
        public string Status { get; set; }
        [DataMember(Order = 6)]
        public long LeadId { get; set; }
        [DataMember(Order = 7)]
        public int SubActivitySequence { get; set; }
        [DataMember(Order = 8)]
        public string ActivityName { get; set; }
        [DataMember(Order = 9)]
        public string IdentificationCode { get; set; }
        [DataMember(Order = 10)]
        public List<LeadNBFCApiDc> NBFCCompanyApiList { get; set; }

        [DataMember(Order = 11)]
        public long ProductCompanyActivityMasterId { get; set; }

        [DataMember(Order = 12)]
        public long ProductId{ get; set; }
    }

    [DataContract]
    public class LeadNBFCApiDc
    {

        [DataMember(Order = 1)]
        public string APIUrl { get; set; }
        [DataMember(Order = 2)]
        public string Code { get; set; }
        [DataMember(Order = 3)]
        public string TAPIKey { get; set; }
        [DataMember(Order = 4)]
        public string TAPISecretKey { get; set; }
        [DataMember(Order = 5)]
        public int Sequence { get; set; }
        [DataMember(Order = 6)]
        public string Status { get; set; }
        [DataMember(Order = 7)]
        public long CompanyApiId { get; set; }

        [DataMember(Order = 8)]
        public string? TReferralCode { get; set; }
        [DataMember(Order = 9)]
        public long ProductCompanyActivityMasterId { get; set; }
    }
}
