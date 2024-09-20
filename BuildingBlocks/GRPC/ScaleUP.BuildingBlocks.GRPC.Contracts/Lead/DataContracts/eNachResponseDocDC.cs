using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]

    public class eNachResponseDocDC
    {
        [DataMember(Order = 1)]
        public string Status { get; set; }
        [DataMember(Order = 2)]
        public string MsgId { get; set; }
        [DataMember(Order = 3)]
        public string RefId { get; set; }
        [DataMember(Order =4)]
        public List<Error> Errors { get; set; }
        [DataMember(Order = 5)]
        public string Filler1 { get; set; }
        [DataMember(Order = 6)]
        public string Filler2 { get; set; }
        [DataMember(Order = 7)]
        public string Filler3 { get; set; }
        [DataMember(Order = 8)]
        public string Filler4 { get; set; }
        [DataMember(Order = 9)]
        public string Filler5 { get; set; }
        [DataMember(Order = 10)]
        public string Filler6 { get; set; }
        [DataMember(Order = 11)]
        public string Filler7 { get; set; }
        [DataMember(Order = 12)]
        public string Filler8 { get; set; }
        [DataMember(Order = 13)]
        public string Filler9 { get; set; }
        [DataMember(Order = 14)]
        public string Filler10 { get; set; }
    }

    [DataContract]
    public class Error
    {
        [DataMember(Order = 1)]
        public string Error_Code { get; set; }
        [DataMember(Order = 2)]
        public string Error_Message { get; set; }
    }
}
