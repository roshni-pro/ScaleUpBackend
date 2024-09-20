using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts
{
    [DataContract]
    public class UserIdsListResponse
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public List<UserCompanyData> UserCompanyDataList { get; set; }
        
    }

    [DataContract]
    public class UserCompanyData
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }
        [DataMember(Order = 2)]
        public long CompanyId { get; set; }
        [DataMember(Order = 3)]
        public string CompanyName { get; set; }
        [DataMember(Order = 4)]
        public bool IsActive { get; set; }

        [DataMember(Order = 5)]
        public DateTime CreatedDate { get; set; }
    }
}
