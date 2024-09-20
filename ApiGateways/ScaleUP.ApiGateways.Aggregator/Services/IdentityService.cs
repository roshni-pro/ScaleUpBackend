using Grpc.Net.Client;
using Microsoft.Net.Http.Headers;
using ProtoBuf.Grpc.Client;
using ScaleUP.ApiGateways.Aggregator.Constants;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Extensions;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces;

namespace ScaleUP.ApiGateways.Aggregator.Services
{
    public class IdentityService : IIdentityService
    {
        private IConfiguration Configuration;
        private readonly IIdentityGrpcService _client;

        public IdentityService(IConfiguration _configuration
            , IIdentityGrpcService client)
        {
            Configuration = _configuration;
            _client = client;
        }


        public async Task<CreateUserResponse> CreateUser(CreateUserRequest request)
        {           
           
            var reply = await _client.CreateUser(request);

            return reply;
        }
        public async Task<UserListDetailsResponse> GetUserById(UserListDetailsRequest userIds)
        {           
          
            var reply = await _client.GetUserById(userIds);

            return reply;
        }

        public async Task<CreateUserResponse> CreateUserWithToken(CreateUserRequest request)
        {           
          
            var reply = await _client.CreateUserWithToken(request);

            return reply;
        }

        public async Task<CreateUserResponse> GetCustomerToken(string userName)
        {

            var reply = await _client.GetCustomerToken(userName);

            return reply;
        }

        public async Task<GRPCReply<ChangeUserPasswordResponse>> ResetUserPassword(string userId)
        {  
            var reply = await _client.ResetUserPassword(userId);

            return reply;
        }

        public async Task<GRPCReply<ChangeUserPasswordResponse>> ForgotUserPassword(GRPCRequest<string> UserName)
        {
            var reply = await _client.ForgotUserPassword(UserName);

            return reply;
        }

        public async Task<GRPCReply<CreateClientResponse>> CreateClient()
        {
            var reply = await _client.CreateClient();

            return reply;
        }

        public async Task<GRPCReply<string>> UpdateUserByUserId(UserUpdateByIdRequestDTO req)
        {
            var reply = await _client.UpdateUserByUserId(req);
            return reply;
        }

        public async Task<GRPCReply<bool>> GetUserByEmailMobile(GRPCRequest<GetUserRequest> request)
        {
            var reply = await _client.GetUserByEmailMobile(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> CheckCompanyAdminExist(GRPCRequest<List<string>> request)
        {
            var reply = await _client.CheckCompanyAdminExist(request);
            return reply;
        }

        public async Task<GRPCReply<bool>> UpdateUserRole(GRPCRequest<UpdateUserRoleRequest> request)
        {
            var reply = await _client.UpdateUserRole(request);
            return reply;
        }
    }
}
