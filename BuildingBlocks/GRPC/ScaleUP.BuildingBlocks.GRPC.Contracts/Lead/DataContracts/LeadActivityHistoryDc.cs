using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class LeadActivityHistoryDc
    {
        [DataMember(Order = 1)]
        public string? ActivityMasterName { get; set; }
        [DataMember(Order = 2)]
        public string? SubActivityMasterName { get; set; }
        [DataMember(Order = 3)]
        public string? UserId { get; set; }
        [DataMember(Order = 4)]
        public DateTime? TimeStamp { get; set; }
        [DataMember(Order = 5)]
        public string? Changes { get; set; }
        [DataMember(Order = 6)]
        public string? Action { get; set; }
        [DataMember(Order = 7)]
        public string? UserName { get; set; }
    }
}
