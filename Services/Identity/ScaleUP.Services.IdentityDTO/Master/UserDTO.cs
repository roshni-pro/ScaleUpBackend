using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.Services.IdentityDTO.Master
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public string UserName { get; set; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
        public List<string> UserRoles { get; set; }
    }

    public class UpdateUserReqDTO
    {
        public string Id { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<long> CompanyIds { get; set; }
        public string UserType { get; set; }

    }

    public class UpdateUserResponseDTO
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public List<string> Roles { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string UserType { get; set; }
        public List<long> CompanyIds { get; set; }

    }
}
