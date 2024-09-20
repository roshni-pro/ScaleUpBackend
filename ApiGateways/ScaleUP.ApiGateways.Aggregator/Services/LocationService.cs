using Grpc.Net.Client;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.Extensions;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class LocationService : ILocationService
    {
        private IConfiguration Configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocationGrpcService _client;
        public LocationService(IConfiguration _configuration
             ,ILocationGrpcService client)
        {
            Configuration = _configuration;
            _client = client;
        }
        public async Task<LocationReply> CreateLocation(CreateLocationRequest request)
        {            
            var locationReply =await _client.CreateLocation(request);
            return locationReply;
        }

        public async Task<CompanyAddressReply> GetCompanyAddress(List<long> locationDc)
        {           
            var locationReply = await _client.GetCompanyAddress(locationDc);
            return locationReply;
        }

        public async Task<StateReply> GetStateByName(GSTverifiedRequest gSTverifiedRequestDTO)
        {
            var stateReply = await _client.GetStateByName(gSTverifiedRequestDTO);
            return stateReply;
        }

        public async Task<GRPCReply<CityReply>> GetCityByName(GSTverifiedRequest gSTverifiedRequestDTO)
        {
            var cityReply = await _client.GetCityByName(gSTverifiedRequestDTO);
            return cityReply;
        }

        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            var reply = await _client.GetAuditLogs(request);
            return reply;
        }

        public async Task<GRPCReply<CityReply>> GetCityById(GRPCRequest<long> cityID)
        {
            var cityReply = await _client.GetCityById(cityID);
            return cityReply;
        }

        public async Task<GRPCReply<UpdateCompanyAddressRequest>> UpdateAddress(UpdateCompanyAddressRequest request)
        {
            var reply = await _client.UpdateAddress(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> AddCity(GRPCRequest<AddCityRequest> request)
        {
            var reply = await _client.AddCity(request);
            return reply;
        }

        public async Task<GRPCReply<List<CityMasterListReply>>> GetAllCities()
        {
            var reply = await _client.GetAllCities();
            return reply;
        }

        public async Task<GRPCReply<List<CityReply>>> GetCityListByIds(GRPCRequest<List<long>> req)
        {
            var reply = await _client.GetCityListByIds(req);
            return reply;
        }

        public async Task<GRPCReply<long>> AddAddress(UpdateCompanyAddressRequest request)
        {
            var reply = await _client.AddAddress(request);
            return reply;
        }

        public async Task<GRPCReply<List<CityMasterListReply>>> GetAllLeadCities(LeadCityIds req)
        {
            var reply = await _client.GetAllLeadCities(req);
            return reply;
        }


        public async Task<GRPCReply<CityMasterListReply>> GetCityDetails(GRPCRequest<long> req)
        {
            var reply = await _client.GetCityDetails(req);
            return reply;
        }

        //public async Task<GRPCReply<List<KarzaElectricityServiceProviderReply>>> GetKarzaElectricityServiceProviderList()
        //{
        //    var reply = await _client.GetKarzaElectricityServiceProviderList();
        //    return reply;
        //}
        //public async Task<GRPCReply<List<KarzaElectricityStateReply>>> GetKarzaElectricityState(string state)
        //{
        //    var reply = await _client.GetKarzaElectricityState(state);
        //    return reply;
        //}
    }
}
