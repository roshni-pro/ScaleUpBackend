using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class FintechCompanyResponse
    {
        [DataMember(Order = 1)]
        public string? CompanyCode { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
    }
    [DataContract]
    public class GetCompanyDetailListResponse
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string BusinessName { get; set; }
        [DataMember(Order = 3)]
        public string? LendingName { get; set; }
        [DataMember(Order = 4)]
        public string? EmailAddress { get; set; }
        [DataMember(Order = 5)]
        public string BeneficiaryName { get; set; }
        [DataMember(Order = 6)]
        public string BeneficiaryAccountNumber { get; set; }

    }
}
