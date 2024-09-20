using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class InvoiceRequestDC
    {

        [DataMember(Order = 1)]
        public string InvoiceNo { get; set; }
        [DataMember(Order = 2)]
        public DateTime InvoiceDate { get; set; }
        [DataMember(Order = 3)]
        public string InvoicePdfURL { get; set; }

        [DataMember(Order = 4)]
        public string OrderNo { get; set;}
        [DataMember(Order = 5)] 
        public string StatusMsg { get; set; }
        [DataMember(Order = 6)]
        public double InvoiceAmount { get; set; }
        [DataMember(Order = 7)]
        public string? ayeFinNBFCToken { get; set; } = "";

    }

    [DataContract]
    public class InvoiceResponseDC
    {
        [DataMember(Order = 1)]
        public bool status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public string Result { get; set; }
        [DataMember(Order = 4)]
        public string CustomerName { get; set; }        
        [DataMember(Order = 5)]
        public string AnchorName { get; set; }
        [DataMember(Order = 6)]
        public string MobileNo { get; set; }
        [DataMember(Order = 7)]
        public long? AnchorCompanyId { get; set; }

    }


    [DataContract]
    public class RefundRequestDTO
    {
        [DataMember(Order = 1)]
        public double RefundAmount { get; set; }
        [DataMember(Order = 2)]
        public string OrderNo { get; set; }
        [DataMember(Order = 3)]
        public string NBFCToken { get; set; }
    }
}
