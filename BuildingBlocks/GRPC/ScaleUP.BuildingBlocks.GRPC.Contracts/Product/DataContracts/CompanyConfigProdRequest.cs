using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts
{
    [DataContract]
    public class CompanyConfigProdRequest
    {
        [DataMember(Order = 1)]
        public long AnchorCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
    }

    [DataContract]
    public class GetesignRequest
    {
        [DataMember(Order = 1)]
        public long ProductId { get; set; }

        [DataMember(Order = 2)]
        public long LeadId { get; set; }
        [DataMember(Order = 3)]
        public string AgreementType { get; set; }

    }
    [DataContract]
    public class CreditDaysRequest
    {
        [DataMember(Order = 1)]
        public long CompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
    }
    [DataContract]
    public class NBFCConfigProdRequest
    {
        [DataMember(Order = 1)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 2)]
        public long ProductId { get; set; }
    }
    [DataContract]
    public class NBFCIdentificationCodes
    {
        [DataMember(Order = 1)]
        public long NBFCCompanyId { get; set; }
        [DataMember(Order = 2)]
        public string IdentificationCodes { get; set; }
    }
}
