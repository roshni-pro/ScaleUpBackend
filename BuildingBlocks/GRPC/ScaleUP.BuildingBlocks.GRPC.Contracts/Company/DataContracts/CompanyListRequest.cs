using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class CompanyListRequest
    {
        [DataMember(Order = 1)]
        public string CompanyType { get; set; }
        [DataMember(Order = 2)]
        public string? keyword { get; set; }

        [DataMember(Order = 3)]
        public int Skip { get; set; }
        [DataMember(Order = 4)]
        public int Take { get; set; }
    }
    [DataContract]
    public class AddCompanyUserMappingRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public string UserId { get; set; }
    }
    [DataContract]
    public class GetEntityCodeRequest
    {
        [DataMember(Order = 1)]
        public string EntityName { get; set; }
        [DataMember(Order = 2)]
        public string CompanyType { get; set; }
    }
}
