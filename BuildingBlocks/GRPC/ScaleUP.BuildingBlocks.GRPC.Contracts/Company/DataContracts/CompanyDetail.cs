using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class CompanyDetail
    {
        [DataMember(Order = 1)]
        public string LogoURL { get; set; }
        [DataMember(Order = 2)]
        public double GstRate { get; set; }

    }
    [DataContract]
    public class CompanyIdentificationCodeDc
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string IdentificationCode { get; set; }

    }
    [DataContract]
    public class CompanyDataDc
    {
        [DataMember(Order = 1)]
        public string CompanyName { get; set; }

        [DataMember(Order = 2)]
        public string IdentificationCode { get; set; }
        [DataMember(Order = 3)]
        public string? LogoURL { get; set; }

        [DataMember(Order = 4)]
        public long CompanyId { get; set; }

    }

    [DataContract]
    public class CompanyDetailDc
    {
        [DataMember(Order = 1)]
        public string CompanyName { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public string IdentificationCode { get; set; }

    }
    [DataContract]
    public class CompanyBankDetailsDc
    {
        [DataMember(Order = 1)]
        public string bankName { get; set; }

        [DataMember(Order = 2)]
        public string bankAccountNumber { get; set; }
        [DataMember(Order = 3)]
        public long CompanyId { get; set; }
    }

    [DataContract]

    public class GstList
    {
        [DataMember(Order = 1)]
        public string Code { get; set; }
        [DataMember(Order = 2)]
        public double Value { get; set; }

    }
    [DataContract]
    public class GetAllCompanyDetailDc
    {
        [DataMember(Order = 1)]
        public long Id { get; set; }
        [DataMember(Order = 2)]
        public string BusinessName { get; set; }
        [DataMember(Order = 3)]
        public string IdentificationCode { get; set; }
        [DataMember(Order = 4)]
        public string CompanyType { get; set; }
        [DataMember(Order = 5)]
        public bool IsDefault { get; set; }
        [DataMember(Order = 6)]
        public string LendinName { get; set; }
        [DataMember(Order = 7)]
        public bool IsDSA { get; set; }

    }
}
