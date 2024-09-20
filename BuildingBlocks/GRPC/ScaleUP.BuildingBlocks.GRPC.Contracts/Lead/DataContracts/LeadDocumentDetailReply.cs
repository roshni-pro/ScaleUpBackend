using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadDocumentDetailReply
    {
        [DataMember(Order = 1)]
        public required long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string? DocumentNumber { get; set; }
        [DataMember(Order = 3)]
        public required string DocumentName { get; set; }
        [DataMember(Order = 4)]
        public List<string> FileUrl { get; set; }
        [DataMember(Order = 5)]
        public string? PdfPassword { get; set; }
        [DataMember(Order = 6)]
        public long LeadDocDetailId { get; set; }
        [DataMember(Order = 7)]
        public int? Sequence { get; set; }
        [DataMember(Order = 8)]
        public string? Label { get; set;}

    }
    //[DataContract]
    //public class MultiLeadDocumentDetailReply
    //{
    //    [DataMember(Order = 1)]
    //    public required long LeadId { get; set; }
    //    [DataMember(Order = 2)]
    //    public string? DocumentNumber { get; set; }
    //    [DataMember(Order = 3)]
    //    public required string DocumentName { get; set; }
    //    [DataMember(Order = 4)]
    //    public List<string>? FileUrl { get; set; }
    //    [DataMember(Order = 5)]
    //    public string? PdfPassword { get; set; }
    //    [DataMember(Order = 6)]
    //    public long LeadDocDetailId { get; set; }
    //    [DataMember(Order = 7)]
    //    public int? Sequence { get; set; }

    //}
}
