using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts.DSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class VerifyLeadDocumentRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public long ActivityMasterId { get; set; }
        [DataMember(Order = 3)]
        public long SubActivityMasterId { get; set; }
        [DataMember(Order = 4)]
        public int IsApprove { get; set; }
        [DataMember(Order = 5)]
        public string Comment { get; set; }
    }
    [DataContract]
    public class ArthmateAgreementRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string AgreementURL { get; set; }
        [DataMember(Order = 3)]
        public bool IsSubmit { get; set;}
        [DataMember(Order = 4)]
        public ProductCompanyConfigDc ProductCompanyConfig { get; set; }
    }
    [DataContract]
    public class EsignAgreementRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string AgreementURL { get; set; }
        [DataMember(Order = 3)]
        public bool IsSubmit { get; set; }
        [DataMember(Order = 4)]
        public ConnectorPersonalDetailDc ConnectorPersonalDetail { get; set; }
        [DataMember(Order = 5)]
        public DSAPersonalDetailDc DSAPersonalDetail { get; set; }
        [DataMember(Order = 6)]
        public List<SalesAgentCommissionList> SalesAgentCommissions { get; set; }
    }




    [DataContract]
    public class UpdateLeadOfferResponse
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string UserId { get; set; }
    }
    [DataContract]
    public class UpdateLeadOfferRequest
    {
        [DataMember(Order = 1)]
        public long LeadOfferId { get; set; }
        [DataMember(Order = 2)]
        public double? InterestRate { get; set; }
        [DataMember(Order = 3)]
        public double? newOfferAmout { get; set; }
    }
}
