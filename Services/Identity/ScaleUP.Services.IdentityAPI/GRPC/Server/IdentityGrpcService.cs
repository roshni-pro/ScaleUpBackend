using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nito.AsyncEx;
using ProtoBuf.Grpc;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.Interfaces;
using ScaleUP.Global.Infrastructure.Common.Models.Identity;
using ScaleUP.Services.IdentityAPI.Constants;
using ScaleUP.Services.IdentityAPI.Manager;
using ScaleUP.Services.IdentityAPI.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ScaleUP.Services.IdentityAPI.GRPC.Server
{
    public class IdentityGrpcService : IIdentityGrpcService
    {

        private readonly ApplicationDBContext _context;
        private readonly IUserCreateManager _usercreatemanager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISigningCredentialStore _signingCredentialStore;
        public IdentityGrpcService(ApplicationDBContext context, IUserCreateManager userCreateManager
            , UserManager<ApplicationUser> userManager, ISigningCredentialStore signingCredentialStore)
        {
            _context = context;
            _usercreatemanager = userCreateManager;
            _userManager = userManager;
            _signingCredentialStore = signingCredentialStore;
        }
        public Task<CreateUserResponse> CreateUser(CreateUserRequest request, ProtoBuf.Grpc.CallContext context = default)
        {
            CreateUserResponse response = new CreateUserResponse();

            var userResponse = AsyncContext.Run(() => _usercreatemanager.CreateUser(new IdentityDTO.Master.UserDTO
            {
                Email = request.EmailId,
                MobileNo = request.MobileNo,
                Password = request.Password,
                UserName = request.UserName,
                UserType = request.UserType,
                Claims = request.Claims,
                UserRoles = request.UserRoles
            }));

            response.Status = userResponse.Status;
            response.Message = userResponse.Message;
            response.UserId = userResponse.Status ? userResponse.ReturnObject.Id : "";

            return Task.FromResult(response);
        }

        public Task<UserListDetailsResponse> GetUserById(UserListDetailsRequest request, ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.GetUserById(request));
            return Task.FromResult(userResponse);
        }

        public Task<CreateUserResponse> CreateUserWithToken(CreateUserRequest request, ProtoBuf.Grpc.CallContext context = default)
        {
            CreateUserResponse response = new CreateUserResponse();

            var userResponse = AsyncContext.Run(() => _usercreatemanager.CreateUserForLead(new IdentityDTO.Master.UserDTO
            {
                Email = request.EmailId,
                MobileNo = request.MobileNo,
                Password = request.Password,
                UserName = request.UserName,
                UserType = request.UserType,
                Claims = request.Claims,
                UserRoles = request.UserRoles
            }));

            response.Status = userResponse.Status;
            response.Message = userResponse.Message;
            response.UserId = userResponse.Status ? userResponse.ReturnObject.Id : "";

            if (userResponse.Status)
            {
                response.UserToken = GetApplicationUserToken(request.UserName);
            }

            return Task.FromResult(response);
        }

        public Task<CreateUserResponse> GetCustomerToken(string userName, ProtoBuf.Grpc.CallContext context = default)
        {
            CreateUserResponse response = new CreateUserResponse();
            response.UserToken = GetApplicationUserToken(userName);
            response.Status = string.IsNullOrEmpty(response.UserToken) ? false : true;
            return Task.FromResult(response);
        }

        private string GetApplicationUserToken(string username)
        {
            UserTokenReply userTokenReply = new UserTokenReply();
            var user = AsyncContext.Run(() => _userManager.FindByNameAsync(username));
            var signingCredentials = AsyncContext.Run(() => _signingCredentialStore.GetSigningCredentialsAsync());
            var tokenHandler = new JwtSecurityTokenHandler();


            IList<string> userRoles = AsyncContext.Run(() => _userManager.GetRolesAsync(user));
            var userClaims = AsyncContext.Run(() => _userManager.GetClaimsAsync(user));

            List<Claim> claims = new List<Claim> {  new Claim("userId", user.Id),
                    new Claim("username", user.UserName??""),
                    new Claim("loggedon", DateTime.Now.ToString())
                   , new Claim("scope", "crmApi" )
                   , new Claim("usertype", user.UserType??"" )
                   , new Claim("mobile", user.PhoneNumber??"" )
                   , new Claim("email", user.Email ?? "" )
                   , new Claim("roles",  string.Join(",",userRoles) )};

            if (userClaims != null && userClaims.Any())
                claims.AddRange(userClaims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = EnvironmentConstants.IdentityUrl,
                IssuedAt = DateTime.UtcNow,
                Audience = "crmApi",
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = signingCredentials,
                TokenType = "at+jwt",
                //,Claims = new Dictionary<string, object> { { "TokenScopes", "crmApi" } }
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public Task<GRPCReply<ChangeUserPasswordResponse>> ResetUserPassword(string request, ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.ResetUserPassword(request));
            return Task.FromResult(userResponse);
        }

        public Task<GRPCReply<ChangeUserPasswordResponse>> ForgotUserPassword(GRPCRequest<string> UserName, ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.ForgotUserPassword(UserName));
            return Task.FromResult(userResponse);
        }

        public Task<GRPCReply<CreateClientResponse>> CreateClient(ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.CreateClient());
            return Task.FromResult(userResponse);
        }

        public Task<GRPCReply<string>> UpdateUserByUserId(UserUpdateByIdRequestDTO req, ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.UpdateUserByUserId(req));
            return Task.FromResult(userResponse);
        }

        public Task<GRPCReply<bool>> GetUserByEmailMobile(GRPCRequest<GetUserRequest> request, ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.GetUserByEmailMobile(request));
            return Task.FromResult(userResponse);
        }

        public Task<GRPCReply<bool>> CheckCompanyAdminExist(GRPCRequest<List<string>> request, ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.CheckCompanyAdminExist(request));
            return Task.FromResult(userResponse);
        }

        public Task<GRPCReply<bool>> UpdateUserRole(GRPCRequest<UpdateUserRoleRequest> request, ProtoBuf.Grpc.CallContext context = default)
        {
            var userResponse = AsyncContext.Run(() => _usercreatemanager.UpdateUserRole(request));
            return Task.FromResult(userResponse);
        }
    }
}
