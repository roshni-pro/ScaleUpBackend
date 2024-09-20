using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class CompanyListReply
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public List<companyList> CompanyList { get; set; }

    }
    [DataContract]
    public class companyList
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public string landingName { get; set; }
        [DataMember(Order = 3)]
        public string businessName { get; set; }
        [DataMember(Order = 4)]
        public string gst { get; set; }
        [DataMember(Order = 5)]
        public List<long> locationId { get; set; }
        [DataMember(Order = 6)]
        public int totalRecords { get; set; }
        [DataMember(Order = 7)]
        public string CompanyType { get; set; }
        [DataMember(Order = 8)]
        public bool IsActive { get; set; }
        [DataMember(Order = 9)]
        public string? CompanyCode { get; set; }

        [DataMember(Order = 11)]
        public string MobileNumber { get; set; }
        [DataMember(Order = 12)]
        public string Email { get; set; }
        [DataMember(Order = 13)]
        public string AgreementUrl { get; set; }
        [DataMember(Order = 14)]
        public DateTime? AgreementStartDate { get; set; }
        [DataMember(Order = 15)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 16)]
        public string Pan { get; set; }
        [DataMember(Order = 17)]
        public List<PartnerDetailsList> PartnerDetailsList { get; set; }
        [DataMember(Order = 18)]
        public bool IsDefault { get; set; }
        [DataMember(Order = 19)]
        public double GstRate { get; set; }
        [DataMember(Order = 20)]
        public long? AgreementDocId { get; set; }
        [DataMember(Order = 21)]
        public long? CustomerAgreementDocId { get; set; }
        [DataMember(Order = 22)]
        public long? BusinessPanDocId { get; set; }
        [DataMember(Order = 23)]
        public long? CancelChequeDocId { get; set; }
        [DataMember(Order = 24)]
        public required string CancelChequeURL { get; set; }
        [DataMember(Order = 25)]
        public string CustomerAgreementURL { get; set; }
        [DataMember(Order = 26)]
        public string BusinessPanURL { get; set; }
        [DataMember(Order = 27)]
        public int BusinessTypeId { get; set; }


    }
    [DataContract]
    public class PartnerDetailsList
    {
        [DataMember(Order = 1)]
        public string PartnerName { get; set; }
        [DataMember(Order = 2)]
        public string PartnerMobileNo { get; set; }
        [DataMember(Order = 3)]
        public long PartnerCompanyId { get; set; }

    }


}
