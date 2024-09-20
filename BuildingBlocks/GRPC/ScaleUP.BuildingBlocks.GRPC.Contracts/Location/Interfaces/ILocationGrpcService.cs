using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using System.ServiceModel;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Location.Interfaces
{
    [ServiceContract]
    public interface ILocationGrpcService
    {
        [OperationContract]
        Task<LocationReply> CreateLocation(CreateLocationRequest request,
            CallContext context = default);

        [OperationContract]
        Task<CompanyAddressReply> GetCompanyAddress(List<long> request,
            CallContext context = default);
        [OperationContract]
        Task<StateReply> GetStateByName(GSTverifiedRequest gSTverifiedRequestDTO,
            CallContext context = default);


        [OperationContract]
        Task<GRPCReply<CityReply>> GetCityByName(GSTverifiedRequest gSTverifiedRequestDTO, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<CityReply>> GetCityById(GRPCRequest<long> cityID, CallContext context = default);


        [OperationContract]
        Task<GRPCReply<UpdateCompanyAddressRequest>> UpdateAddress(UpdateCompanyAddressRequest request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<bool>> AddCity(GRPCRequest<AddCityRequest> request, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<CityMasterListReply>>> GetAllCities(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<CityReply>>> GetCityListByIds(GRPCRequest<List<long>> req,CallContext context = default);

        [OperationContract]
        Task<GRPCReply<long>> AddAddress(UpdateCompanyAddressRequest request, CallContext context = default);
        //[OperationContract]
        //Task<GRPCReply<long>> GetKarzaElectricityServiceProviderList(CallContext context = default);
        //[OperationContract]
        //Task<GRPCReply<List<KarzaElectricityStateReply>>> GetKarzaElectricityState(string state, CallContext context = default);

        [OperationContract]
        Task<GRPCReply<List<CityMasterListReply>>> GetAllLeadCities(LeadCityIds req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<CityMasterListReply>> GetCityDetails(GRPCRequest<long> req, CallContext context = default);
    }
}
