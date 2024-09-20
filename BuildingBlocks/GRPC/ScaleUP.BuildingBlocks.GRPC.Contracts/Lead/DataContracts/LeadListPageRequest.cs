using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadListPageRequest
    {
        [DataMember(Order = 1)]
        public List<int> CompanyId { get; set; }
        [DataMember(Order = 2)]
        public List<long> ProductId { get; set; }
        [DataMember(Order = 3)]
        public string Keyword { get; set; }
        [DataMember(Order = 4)]
        public DateTime? FromDate { get; set; }
        [DataMember(Order = 5)]
        public DateTime? ToDate { get; set; }
        [DataMember(Order = 6)]
        public int? CityId { get; set; }
        [DataMember(Order = 7)]
        public String? Status { get; set; }
        [DataMember(Order = 8)]
        public int Skip { get; set; }
        [DataMember(Order = 9)]
        public int Take { get; set; }
        [DataMember(Order = 10)]
        public string? ProductType { get; set; }
        [DataMember(Order = 11)]
        public List<string>? UserIds { get; set; }
        [DataMember(Order = 12)]
        public bool IsDSA { get; set; }
        [DataMember(Order = 13)]
        public string? Role { get; set; }
        [DataMember(Order = 14)]
        public bool isDelete { get; set; }
        [DataMember(Order = 15)]
        public bool isForNBFC { get; set; }
        [DataMember(Order = 16)]
        public long? NbfcCompanyId { get; set; }
        [DataMember(Order = 17)]
        public string? UserType { get; set; }

    }

    [DataContract]
    public class DSALeadListPageRequest
    {
        [DataMember(Order = 1)]
        public string Keyword { get; set; }
        [DataMember(Order = 2)]
        public DateTime? FromDate { get; set; }
        [DataMember(Order = 3)]
        public DateTime? ToDate { get; set; }
        [DataMember(Order = 4)]
        public int? CityId { get; set; }
        [DataMember(Order = 5)]
        public int Skip { get; set; }
        [DataMember(Order = 6)]
        public int Take { get; set; }
        [DataMember(Order = 7)]
        public string? ProductType { get; set; }
        [DataMember(Order = 8)]
        public string? Status { get; set; }
        [DataMember(Order = 9)]
        public string? CityName { get; set; }
        [DataMember(Order = 10)]
        public bool isDelete { get; set; }
        //[DataMember(Order = 9)]
        //public List<long?> ProductId { get; set; }
    }

    [DataContract]
    public class InitiateLeadOfferRequestDC
    {
        [DataMember(Order = 1)]
        public List<long> CompanyIds { get; set; }
        [DataMember(Order = 2)]
        public long LeadId { get; set; }
    }
}
