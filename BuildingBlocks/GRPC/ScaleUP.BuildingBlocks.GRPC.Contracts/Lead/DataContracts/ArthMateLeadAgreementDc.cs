using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class ArthMateLeadAgreementDc
    {
        [DataMember(Order = 1)]
        public string AgreementPdfUrl { get; set; }
        [DataMember(Order = 2)]
        public long LeadId { get; set; }
        [DataMember(Order = 3)]
        public ProductCompanyConfigDc ProductCompanyConfig { get; set;}
    }
    [DataContract]
    public class EsignLeadAgreementDc
    {
        [DataMember(Order = 1)]
        public string AgreementPdfUrl { get; set; }
        [DataMember(Order = 2)]
        public long LeadId { get; set; }
    }




    [DataContract]
    public class AddLoanConfigurationDc
    {
        [DataMember(Order = 1)]
        public double? PF { get; set; }
        [DataMember(Order = 2)]
        public double? GST { get; set; }
        [DataMember(Order = 3)]
        public double? ODCharges { get; set; }
        [DataMember(Order = 4)]
        public int? ODdays { get; set; }
        [DataMember(Order = 5)]
        public double? InterestRate { get; set; }
        [DataMember(Order = 6)]
        public double? BounceCharge { get; set; }
        [DataMember(Order = 7)]
        public double? PenalPercent { get; set; }
    }
}
