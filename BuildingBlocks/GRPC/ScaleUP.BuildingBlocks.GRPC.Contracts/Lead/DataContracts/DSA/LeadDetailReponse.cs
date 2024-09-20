using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA
{
    [DataContract]

    public class LeadDetailReponse
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string MobileNo { get; set; }
    }
    [DataContract]

    public class LeadAggrementDetailReponse
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string? Status { get; set; } //Signed, Pending,Failed, Expired
        [DataMember(Order = 3)]
        public DateTime? StartedOn { get; set; }
        [DataMember(Order = 4)]
        public DateTime? ExpiredOn { get; set; }
        [DataMember(Order = 5)]
        public string? DocSignedUrl { get; set; }
        [DataMember(Order = 6)]
        public string? DocUnSignedUrl { get; set; }
        [DataMember(Order = 7)]
        public string? eSignedUrl { get; set; }
        [DataMember(Order = 8)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        [DataMember(Order = 9)]
        public string LeadCode { get; set; }
        [DataMember(Order = 10)]
        public string? UserName { get; set; }
        [DataMember(Order = 11)]
        public bool isActivation { get; set; }
    }

    [DataContract]
    public class LeadDataDC
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string? Status { get; set; } 
        [DataMember(Order = 3)]
        public string? UserId { get; set; }
        [DataMember(Order = 4)]
        public DateTime? Created { get; set; }
        [DataMember(Order = 5)]
        public string MobileNo { get; set; }
        [DataMember(Order = 6)]
        public long? ProductId { get; set; }
        [DataMember(Order = 7)]
        public string LeadCode { get; set; }
        [DataMember(Order = 8)]
        public bool IsActive { get; set; }
        [DataMember(Order = 9)]
        public bool IsDeleted { get; set; }
        [DataMember(Order = 10)]
        public string? ApplicantName { get; set; }
        [DataMember(Order = 11)]
        public string? ProductCode { get; set; }
        [DataMember(Order = 12)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
        [DataMember(Order = 13)]
        public string? CreatedBy { get; set; }
        [DataMember(Order = 14)]
        public string? DSALeadCode { get; set; }
    }

    [DataContract]
    public class LeadRequestDataDC
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string Status { get; set; }
    }
 }
