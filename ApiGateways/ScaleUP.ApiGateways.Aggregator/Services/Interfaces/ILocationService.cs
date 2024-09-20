
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.Global.Infrastructure.Helper;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface ILocationService
    {
        Task<LocationReply> CreateLocation(CreateLocationRequest createCompanyRequest);

        Task<CompanyAddressReply> GetCompanyAddress(List<long> companyAddressRequests);

        Task<StateReply> GetStateByName(GSTverifiedRequest gSTverifiedRequestDTO);

        Task<GRPCReply<CityReply>> GetCityByName(GSTverifiedRequest gSTverifiedRequestDTO);

        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request);
        Task<GRPCReply<CityReply>> GetCityById(GRPCRequest<long> cityID);
        Task<GRPCReply<UpdateCompanyAddressRequest>> UpdateAddress(UpdateCompanyAddressRequest request);
        Task<GRPCReply<bool>> AddCity(GRPCRequest<AddCityRequest> request);
        Task<GRPCReply<List<CityMasterListReply>>> GetAllCities();
        Task<GRPCReply<List<CityReply>>> GetCityListByIds(GRPCRequest<List<long>> req);
        Task<GRPCReply<long>> AddAddress(UpdateCompanyAddressRequest request);
        Task<GRPCReply<List<CityMasterListReply>>> GetAllLeadCities(LeadCityIds req);
        Task<GRPCReply<CityMasterListReply>> GetCityDetails(GRPCRequest<long> req);
        //Task<GRPCReply<List<KarzaElectricityServiceProviderReply>>> GetKarzaElectricityServiceProviderList();
        //Task<GRPCReply<List<KarzaElectricityStateReply>>> GetKarzaElectricityState(string state);
    }
}
