using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class CompanyInvoiceRequestDc
    {
        [DataMember(Order = 1)]
        public long NBFCId { get; set; }
        [DataMember(Order = 2)]
        public List<long> AnchorId { get; set; }
        [DataMember(Order = 3)]
        public DateTime? FromDate { get; set; }
        [DataMember(Order = 4)]
        public DateTime? ToDate { get; set; }
        [DataMember(Order = 5)]
        public int? Skip { get; set;}
        [DataMember(Order = 6)]
        public int? Take { get; set; }
        [DataMember(Order = 7)]
        public int Status { get; set; }
    }
}
