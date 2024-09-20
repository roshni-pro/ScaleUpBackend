using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts
{
    [DataContract]
    public class UserListDetailsResponse
    {
        [DataMember(Order = 1)]
        public bool Status { get; set; }
        [DataMember(Order = 2)]
        public string Message { get; set; }
        [DataMember(Order = 3)]
        public List<UserListDetails> UserListDetails { get; set; }

    }
    [DataContract]
    public class UserListDetails
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }
        [DataMember(Order = 2)]
        public string UserType { get; set; }
        [DataMember(Order = 3)]
        public string? UserName { get; set; }
        [DataMember(Order = 4)]
        public string? NormalizedUserName { get; set; }
        [DataMember(Order = 5)]
        public string? Email { get; set; }
        [DataMember(Order = 6)]
        public string? NormalizedEmail { get; set; }
        [DataMember(Order = 7)]
        public string? PhoneNumber { get; set; }
        [DataMember(Order = 8)]
        public List<string>? UserRoles { get; set; }
        [DataMember(Order = 9)]
        public int TotalRecords { get; set; }
    }

    [DataContract]
    public class ChangeUserPasswordResponse
    {
        [DataMember(Order = 1)]
        public string? UserName { get; set; }
        [DataMember(Order = 2)]
        public string Password { get; set; }
        [DataMember(Order = 3)]
        public string? Email { get; set; }
    }
    [DataContract]
    public class GetUserRequest
    {
        [DataMember(Order = 1)]
        public string Email { get; set; }
        [DataMember(Order = 2)]
        public string MobileNumber { get; set; }
    }
    [DataContract]
    public class UpdateUserRoleRequest
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }
        [DataMember(Order = 2)]
        public List<string> RoleNames { get; set; }
    }
}
