using System.Runtime.Serialization;

namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class CreateUserDTO
    {
        public string UserType { get; set; } // CompanyUser, AdminUser
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public List<long> CompanyIds { get; set; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
        public List<string> UserRoles { get; set; }
    }
    public class CheckCompanyAdminDTO
    {
        public string UserId { get; set; }
        public long CompanyId { get; set; }
    }
}
