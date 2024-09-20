using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts
{
    [DataContract]
    public class UpdateLeadDocumentDetailRequest
    {
        [DataMember(Order = 1)]
        public required long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string? DocumentNumber { get; set; }
        [DataMember(Order = 3)]
        public required string DocumentName { get; set; }
        [DataMember(Order = 4)]
        public string? FileUrl { get; set; }
        [DataMember(Order = 5)]
        public string? PdfPassword { get; set; }
        [DataMember(Order = 6)]
        public long LeadDocDetailId { get; set; }
        [DataMember(Order = 7)]
        public int? Sequence { get; set; }
        [DataMember(Order = 8)]
        public string? UserId { get; set;}
        [DataMember(Order = 9)]
        public string? ProductCode { get; set;}
        [DataMember(Order = 10)]
        public string? DocId { get; set;}
    }

    [DataContract]
    public class UpdateLeadDocumentDetailListRequest
    {
        [DataMember(Order = 1)]
        public required long LeadId { get; set; }
        //[DataMember(Order = 2)]
        //public string? DocumentNumber { get; set; }
        [DataMember(Order = 2)]
        public required string DocumentName { get; set; }
        [DataMember(Order = 3)]
        public string? ProductCode { get; set; }
        [DataMember(Order = 4)]
        public string? PdfPassword { get; set; }
        [DataMember(Order = 5)]
        public long LeadDocDetailId { get; set; }
        [DataMember(Order = 6)]
        public int? Sequence { get; set; }
        [DataMember(Order = 7)]
        public string? UserId { get; set; }
        [DataMember(Order = 8)]
        public List<DocDc> DocList { get; set; }
    }
    [DataContract]
    public class DocDc
    {
        [DataMember(Order = 1)]
        public string? DocId { get; set; }
        [DataMember(Order = 2)]
        public string FilePath { get; set; }
    }

    [DataContract]
    public class MASFinanceAgreementDc
    {
        [DataMember(Order = 1)]
        public string? DocId { get; set; }
        [DataMember(Order = 2)]
        public string DocUrl { get; set; }
        [DataMember(Order = 3)]
        public long LeadId { get; set;}
        [DataMember(Order = 4)]
        public long? NbfcCompanyId { get; set; }
    }
}
