namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class UserUpdateByIdDTO
    {
        public string Id { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public List<long> CompanyIds { get; set; }
        public string UserType { get; set; }

    }
}
