using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.Interfaces
{
    [ServiceContract]
    public interface IIdentityGrpcService
    {
        [OperationContract]
        Task<CreateUserResponse> CreateUser(CreateUserRequest request,
            CallContext context = default);
        [OperationContract]
        Task<CreateUserResponse> CreateUserWithToken(CreateUserRequest request,
            ProtoBuf.Grpc.CallContext context = default);
        [OperationContract]
        Task<CreateUserResponse> GetCustomerToken(string userName,
            ProtoBuf.Grpc.CallContext context = default);
        [OperationContract]
        Task<UserListDetailsResponse> GetUserById(UserListDetailsRequest request,
            CallContext context = default);
        [OperationContract]
        Task<GRPCReply<ChangeUserPasswordResponse>> ResetUserPassword(string userId,
            CallContext context = default);
        [OperationContract]
        Task<GRPCReply<ChangeUserPasswordResponse>> ForgotUserPassword(GRPCRequest<string> UserName,
            CallContext context = default);
        [OperationContract]
        Task<GRPCReply<CreateClientResponse>> CreateClient(CallContext context = default);

        [OperationContract]
        Task<GRPCReply<string>> UpdateUserByUserId(UserUpdateByIdRequestDTO req, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> GetUserByEmailMobile(GRPCRequest<GetUserRequest> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> CheckCompanyAdminExist(GRPCRequest<List<string>> request, CallContext context = default);
        [OperationContract]
        Task<GRPCReply<bool>> UpdateUserRole(GRPCRequest<UpdateUserRoleRequest> request, CallContext context = default);
    }
}
