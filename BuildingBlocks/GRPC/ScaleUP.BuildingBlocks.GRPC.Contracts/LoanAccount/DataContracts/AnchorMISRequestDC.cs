using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class AnchorMISRequestDC
    {
        //[DataMember(Order = 1)]
        //public string AnchorName { get; set; }
        [DataMember(Order = 1)]
        public long AnchorId { get; set; }
        [DataMember(Order = 2)]
        public string Status { get; set; } //Invoice status - All , Disbursed, Canceled
        [DataMember(Order = 3)]
        public DateTime FromDate { get; set; }
        [DataMember(Order = 4)]
        public DateTime ToDate { get; set; }

        //public long AnchorId { get; set; }
        //public long ProductType { get; set; }
    }

    [DataContract]
    public class DSAMISRequestDC
    {
        [DataMember(Order = 1)]
        public long DSACompanyId { get; set; }
        [DataMember(Order = 2)]
        public DateTime FromDate { get; set; }
        [DataMember(Order = 3)]
        public DateTime ToDate { get; set; }
    }
}
