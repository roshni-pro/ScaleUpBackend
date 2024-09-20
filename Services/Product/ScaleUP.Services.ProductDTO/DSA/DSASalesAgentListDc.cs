using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.ProductDTO.DSA
{

    public class GetDSAAgentUsersListResponseDc
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string MobileNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? EmailId { get; set; }
        public double? PayoutPercenatge { get; set; }
        public bool Status { get; set; }
        public long AnchorCompanyId { get; set; }
        public string Type { get; set; }
        public string? Role { get; set; }
        public DateTime? AgreementStatDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public int TotalRecords { get; set; }

    }

    public class DSAUserActivationDc
    {
        public string UserId { get; set; }
        public bool Status { get; set; }
    }

    public class SavePayoutDc
    {
        public string UserId { get; set;}
        public double PayoutPercentage { get; set;}
    }

    public class GetSalesAgentDataDc
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Type { get; set; }

    }
}
