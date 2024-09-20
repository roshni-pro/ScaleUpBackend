using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;

namespace ScaleUP.ApiGateways.Aggregator.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<CreateUserResponse> CreateUser(CreateUserRequest request);
        Task<UserListDetailsResponse> GetUserById(UserListDetailsRequest userIds);
        Task<CreateUserResponse> CreateUserWithToken(CreateUserRequest request);
        Task<CreateUserResponse> GetCustomerToken(string userName);
        Task<GRPCReply<ChangeUserPasswordResponse>> ResetUserPassword(string UserId);
        Task<GRPCReply<ChangeUserPasswordResponse>> ForgotUserPassword(GRPCRequest<string> UserName);
        Task<GRPCReply<CreateClientResponse>> CreateClient();
        Task<GRPCReply<string>> UpdateUserByUserId(UserUpdateByIdRequestDTO req);
        Task<GRPCReply<bool>> GetUserByEmailMobile(GRPCRequest<GetUserRequest> request);
        Task<GRPCReply<bool>> CheckCompanyAdminExist(GRPCRequest<List<string>> request);
        Task<GRPCReply<bool>> UpdateUserRole(GRPCRequest<UpdateUserRoleRequest> request);
    }
}
