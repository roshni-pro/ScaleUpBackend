using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA
{
    [DataContract]
    public class CreateDSAUser
    {
        [StringLength(10)]
        [DataMember(Order = 1)]
        public string MobileNumber { get; set; }
        [DataMember(Order = 2)]
        public string FullName { get; set; }
        [DataMember(Order = 3)]
        public string EmailId { get; set; }
        [DataMember(Order = 4)]
        public double? PayoutPercenatge { get; set; }

        [DataMember(Order = 5)]
        public string UserId { get; set; }

        [DataMember(Order = 6)]
        public string CreateBy { get; set; }

        [DataMember(Order = 7)]
        public long ProductId { get; set; }

        [DataMember(Order = 8)]
        public long CompanyId { get; set; }
    }
    [DataContract]
    public class GetDSASalesAgentListResponseDc
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }

        [DataMember(Order = 2)]
        public string MobileNo { get; set; }

        [DataMember(Order = 3)]
        public long AnchorCompanyId { get; set; }

        [DataMember(Order = 4)]
        public string Type { get; set; }

        [DataMember(Order = 5)]
        public string FullName { get; set; }

        [DataMember(Order = 6)]
        public double? PayoutPercenatge { get; set; }

        [DataMember(Order = 7)]
        public string? Role { get; set; }

        [DataMember(Order = 8)]
        public DateTime? AgreementStartDate { get; set; }

        [DataMember(Order = 9)]
        public DateTime? AgreementEndDate { get; set; }
        [DataMember(Order = 10)]
        public bool IsActive { get; set; }
        [DataMember(Order = 11)]
        public bool IsDeleted { get; set; }
        [DataMember(Order = 12)]
        public string DSACode { get; set; }
        [DataMember(Order = 13)]
        public DateTime CreatedDate { get; set; }
    }

    [DataContract]
    public class GetproductCommissionConfigDC
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }
        [DataMember(Order = 2)]
        public double? CommissionValue { get; set; }
    }

}
