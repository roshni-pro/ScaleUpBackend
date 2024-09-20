namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class UserListResponse
    {
        public List<Users> Users { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }

    }

    public class Users
    {
        public string UserId { get; set; }
        public string UserType { get; set; }
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string>? UserRoles { get; set; }
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
        public int TotalRecords { get; set; }

    }
}
