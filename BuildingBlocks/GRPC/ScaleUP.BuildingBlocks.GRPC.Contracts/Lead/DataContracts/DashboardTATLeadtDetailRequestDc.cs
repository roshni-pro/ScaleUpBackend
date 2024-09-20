using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class DashboardTATLeadtDetailRequestDc
    {
        [DataMember(Order = 1)]
        public string? ProductType { get; set; }
        [DataMember(Order = 2)]
        public long? ProductId { get; set; }
        [DataMember(Order = 3)]
        public DateTime FromDate { get; set; }
        [DataMember(Order = 4)]
        public DateTime ToDate { get; set; }

        [DataMember(Order = 5)]
        public List<int> AnchorId { get; set; }
        //[DataMember(Order = 5)]
        //public List<int> NbfcCompanyId { get; set; }
        [DataMember(Order = 6)]
        public List<string> CityName { get; set; }
        [DataMember(Order = 7)]
        public List<int> CityId { get; set; }
    }
    [DataContract]
    public class DashBoardCohortData
    {
        [DataMember(Order = 1)]
        public string InitiateSubmittedTime { get; set; }
        [DataMember(Order = 2)]
        public string SubmittedToAllApprovedTime { get; set; }
        [DataMember(Order = 3)]
        public string AllApprovedToSendToLosTime { get; set; }
        [DataMember(Order = 4)]
        public string SendToLosToOfferAcceptTime { get; set; }
        [DataMember(Order = 5)]
        public string OfferAcceptToAgreementTime { get; set; }
        [DataMember(Order = 6)]
        public string AgreementToAgreementAcceptTime { get; set; }

        [DataMember(Order = 7)]
        public double InitiateSubmittedTimeInHours { get; set; }
        [DataMember(Order = 8)]
        public double SubmittedToAllApprovedTimeInHours { get; set; }
        [DataMember(Order = 9)]
        public double AllApprovedToSendToLosTimeInHours { get; set; }
        [DataMember(Order = 10)]
        public double SendToLosToOfferAcceptTimeInHours { get; set; }
        [DataMember(Order = 11)]
        public double OfferAcceptToAgreementTimeInHours { get; set; }
        [DataMember(Order = 12)]
        public double AgreementToAgreementAcceptTimeInHours { get; set; }
    }

    [DataContract]
    public class DSAMISLeadResponseDC
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string StateName { get; set; }
        [DataMember(Order = 3)]
        public string CityName { get; set; }
        [DataMember(Order = 4)]
        public string PANNo { get; set; }
        [DataMember(Order = 5)]
        public string LeadCode { get; set; }
        [DataMember(Order = 6)]
        public string SKCode { get; set; }
        [DataMember(Order = 7)]
        public DateTime CreatedDate { get; set; }
    }
}
