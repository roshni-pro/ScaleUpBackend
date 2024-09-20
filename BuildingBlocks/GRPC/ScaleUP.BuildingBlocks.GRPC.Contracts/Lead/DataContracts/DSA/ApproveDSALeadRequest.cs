using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA
{
    [DataContract]
    public class ApproveDSALeadRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string UserId { get; set; }
        [DataMember(Order = 3)]
        public List<long> ProductIds { get; set; }
        [DataMember(Order = 4)]
        public string Comment { get; set; }
    }
    [DataContract]
    public class DSADashboardRequest
    {
        [DataMember(Order = 1)]
        public string AgentUserId { get; set; }
        [DataMember(Order = 2)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 3)]
        public DateTime EndDate { get; set; }
    }

    [DataContract]
    public class DSADashboardLeadRequest
    {
        [DataMember(Order = 1)]
        public List<string> AgentUserIds { get; set; }
        [DataMember(Order = 2)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 3)]
        public DateTime EndDate { get; set; }
        [DataMember(Order = 4)]
        public int Skip { get; set; }
        [DataMember(Order = 5)]
        public int Take { get; set; }
        [DataMember(Order = 6)]
        public bool IsPagination { get; set; }
        [DataMember(Order = 7)]
        public long ProductId { get; set; }
    }

    [DataContract]
    public class DSADashboardLeadListRequest
    {
        [DataMember(Order = 1)]
        public string AgentUserId { get; set; }
        [DataMember(Order = 2)]
        public DateTime StartDate { get; set; }
        [DataMember(Order = 3)]
        public DateTime EndDate { get; set; }
        [DataMember(Order = 4)]
        public int Skip { get; set; }
        [DataMember(Order = 5)]
        public int Take { get; set; }
    }

    [DataContract]
    public class UpdateLeadStatusRequest
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public string Status { get; set; }
    }
    [DataContract]
    public class CheckLeadCreatePermissionRequest
    {
        [DataMember(Order = 1)]
        public string MobileNo { get; set; }
        [DataMember(Order = 2)]
        public List<string> AgentUserIds { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
        [DataMember(Order = 4)]
        public string UserId { get; set; }
        [DataMember(Order = 5)]
        public string UserName { get; set; }
    }
}
