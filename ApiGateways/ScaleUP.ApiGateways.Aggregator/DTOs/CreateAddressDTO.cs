namespace ScaleUP.ApiGateways.Aggregator.DTOs
{
    public class CreateAddressDTO
    {
        public long CompanyId { get; set; }
        public long AddressTypeId { get; set; }
        public string AddressLineOne { get; set; }
        public string? AddressLineTwo { get; set; }
        public string? AddressLineThree { get; set; }
        public int ZipCode { get; set; }
        public long CityId { get; set; }
    }
}
