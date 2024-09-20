using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts
{
    [DataContract]
    public class leadDashboardDetailDc
    {
        [DataMember(Order = 1)]
        public string ProductType { get; set; }
        [DataMember(Order = 2)]
        public int ProductId { get; set; }
        [DataMember(Order = 3)]
        public DateTime FromDate { get; set; }
        [DataMember(Order = 4)]
        public DateTime ToDate { get; set; }
        [DataMember(Order = 5)]
        public List<int> AnchorId { get; set; }
        [DataMember(Order = 6)]
        public List<string> CityName { get; set; }
        [DataMember(Order = 7)]
        public List<int> CityId { get; set; }

    }


    [DataContract]
    public class leadDashboardResponseDc
    {
        [DataMember(Order = 1)]
        public int Approved { get; set; }
        [DataMember(Order = 2)]
        public int Pending { get; set; }
        [DataMember(Order = 3)]
        public int Rejected { get; set; }
        [DataMember(Order = 4)]
        public int NotContactable { get; set; }
        [DataMember(Order = 5)]
        public int NotIntrested { get; set; }
        [DataMember(Order = 6)]
        public double ApprovalPercentage { get;  set; }
        [DataMember(Order = 7)]
        public int TotalLeads { get; set; }
        [DataMember(Order = 8)]
        public int WholeDays { get; set; }
        [DataMember(Order = 9)]
        public int RemainingHours { get; set; }
        [DataMember(Order = 10)]
        public List<AccountResponseDc> AccountRowDataDC { get; set; }
        [DataMember(Order = 11)]
        public int DisbursementApprove { get; set; }
        [DataMember(Order = 12)]
        public int totalDisbursementByUMRN { get; set; }
        [DataMember(Order = 13)]
        public int DisbursementRejected { get; set; }

    }


    [DataContract]
    public class leadDashboardData
    {
        [DataMember(Order = 1)]
        public int Approved { get; set; }
        [DataMember(Order = 2)]
        public int Pending { get; set; }
        [DataMember(Order = 3)]
        public int Rejected { get; set; }
        [DataMember(Order = 4)]
        public int NotContactable { get; set; }
        [DataMember(Order = 5)]
        public int NotIntrested { get; set; }
        [DataMember(Order = 6)]
        public double ApprovalPercentage { get; set; }
        [DataMember(Order = 7)]
        public int TotalLeads { get; set; }
        [DataMember(Order = 8)]
        public int WholeDays { get; set; }
        [DataMember(Order = 9)]
        public int RemainingHours { get; set; }
    }

    [DataContract]
    public class AccountResponseDc
    {
        [DataMember(Order = 1)]
        public long LeadId { get; set; }
        [DataMember(Order = 2)]
        public DateTime Created { get; set; }
        [DataMember(Order = 3)]
        public long ProductId { get; set; }
        [DataMember(Order = 4)]
        public string Status { get; set; }
    }

    [DataContract]
    public class leadExportDc
    {
        [DataMember(Order = 1)]
        public string LeadCode { get; set; }
        [DataMember(Order = 2)]
        public string? ApplicantName { get; set; }
        [DataMember(Order = 3)]
        public string? MobileNo { get; set; }
        [DataMember(Order = 4)]
        public string? Activity{ get; set; }
        [DataMember(Order = 5)]
        public string? SubActivityMasterName { get; set; }
        [DataMember(Order = 6)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 7)]
        public string? Status { get; set; }
        [DataMember(Order = 8)]
        public long CityId { get; set; }
        [DataMember(Order = 9)]
        public DateTime Created { get; set; }
        [DataMember(Order = 10)]
        public string? UserUniqueCode { get; set; }
        [DataMember(Order = 11)]
        public long LeadID { get; set; }
        [DataMember(Order = 12)]
        public double CreditLimit { get; set; }
        [DataMember(Order = 13)]
        public DateTime DisbursedDate { get; set; }

    }
    [DataContract]
    public class leadExportRes
    {
        [DataMember(Order = 1)]
        public string LeadCode { get; set; }
        [DataMember(Order = 2)]
        public string? ApplicantName { get; set; }
        [DataMember(Order = 3)]
        public string? MobileNo { get; set; }
        [DataMember(Order = 4)]
        public string? Activity { get; set; }
        [DataMember(Order = 5)]
        public string? SubActivityMasterName { get; set; }
        [DataMember(Order = 6)]
        public string? AnchorName { get; set; }
        [DataMember(Order = 7)]
        public string? Status { get; set; }
        [DataMember(Order = 8)]
        public long CityId { get; set; }
        [DataMember(Order = 9)]
        public DateTime Created { get; set; }
        [DataMember(Order = 10)]
        public string? UserUniqueCode { get; set; }
        [DataMember(Order = 11)]
        public long LeadID { get; set; }
        [DataMember(Order = 12)]
        public double CreditLimit { get; set; }
        [DataMember(Order = 13)]
        public DateTime DisbursedDate { get; set; }
        [DataMember(Order = 14)]
        public string cityName { get; set; }

    }

}
