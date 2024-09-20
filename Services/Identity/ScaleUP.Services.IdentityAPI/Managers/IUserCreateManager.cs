using Microsoft.IdentityModel.Tokens;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.Global.Infrastructure.Common.Models.Identity;
using ScaleUP.Services.IdentityDTO.Master;
using System.ServiceModel;

namespace ScaleUP.Services.IdentityAPI.Manager
{
    public interface IUserCreateManager
    {
       
        Task<UserResponse> CreateUser(UserDTO request);
        Task<UserResponse> CreateUserForLead(UserDTO request);
        Task<UserResponse> GetUserByUserName(string userName);
        Task<RoleResponse> CreateRoleByName(string RoleName);
        Task<RoleResponse> CreateRoleWithPermissions(RoleWithPermissionsDTO request);
        Task<IdentityResponse> GetRoleList(string? Keyword);
        Task<IdentityResponse> UpdateRole(string RoleId, string NewRoleName);
        Task<IdentityResponse> DeleteRole(string RoleId);
        Task<UserListDetailsResponse> GetUserById(UserListDetailsRequest request);
        //Task<IdentityResponse> UpdateUserByUserId(UpdateUserReqDTO request);
        Task<GRPCReply<ChangeUserPasswordResponse>> ResetUserPassword(string UserId);
        Task<GRPCReply<ChangeUserPasswordResponse>> ForgotUserPassword(GRPCRequest<string> UserName);
        Task<IdentityResponse> ChangeUserPassword(string UserId, string OldPassword, string NewPassword);
        Task<UpdateUserResponseDTO> GetUser(string request);
        Task<GRPCReply<CreateClientResponse>> CreateClient();
        Task<List<ApplicationUser>> GetUserList(string Keyword);
        Task<GRPCReply<string>> UpdateUserByUserId(UserUpdateByIdRequestDTO req);
        Task<GRPCReply<bool>> GetUserByEmailMobile(GRPCRequest<GetUserRequest> request);
        Task<GRPCReply<bool>> CheckCompanyAdminExist(GRPCRequest<List<string>> request);
        Task<GRPCReply<bool>> UpdateUserRole(GRPCRequest<UpdateUserRoleRequest> request);
    }
}
