using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class CompanyInvoiceDetailsRequestDC
    {
        [DataMember(Order = 1)]
        public string InvoiceNo  { get; set; }
        [DataMember(Order = 2)]
        public string RoleName { get; set;}
    }
}
