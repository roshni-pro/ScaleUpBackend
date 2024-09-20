using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts
{
    [DataContract]
    public class CompanyInvoicesChargesResponseDc
    {
        [DataMember(Order = 1)]
        public double ProcessingFee { get; set; }
        [DataMember(Order = 2)]
        public double InterestCharges { get; set; }
        [DataMember(Order = 3)]
        public double OverDueInterest { get; set; }
        [DataMember(Order = 4)]
        public double PenalCharges { get; set; }
        [DataMember(Order = 5)]
        public double BounceCharges { get; set; }
        [DataMember(Order = 6)]
        public double TotalTaxableAmount { get;set; }
        [DataMember(Order = 7)]
        public double TotalGstAmount { get; set;}
        [DataMember(Order = 8)]
        public double TotalInvoiceAmount { get; set; }
    }
}
