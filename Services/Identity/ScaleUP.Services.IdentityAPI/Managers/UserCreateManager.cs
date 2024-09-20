using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Common.Models.Identity;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.IdentityAPI.Persistence;
using ScaleUP.Services.IdentityDTO.Master;
using ScaleUP.Services.IdentityModels.Master;
using System.Data;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ScaleUP.Services.IdentityAPI.Manager
{
    public class UserCreateManager : IUserCreateManager
    {
        public readonly ApplicationDBContext _context;
        public readonly ConfigurationDbContext _configContext;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserCreateManager(ApplicationDBContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ConfigurationDbContext configContext)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configContext = configContext;
        }

        public async Task<UserResponse> CreateUser(UserDTO user)
        {
            UserResponse _userresponse = new UserResponse();
            _userresponse.Message = "Issue in creating user.";

            var allUsers = await _userManager.Users.ToListAsync();
            if (allUsers != null && allUsers.Any())
            {
                var _userexist = allUsers.FirstOrDefault(x => x.UserName == user.UserName || x.Email == user.Email || x.PhoneNumber == user.MobileNo);
                if (_userexist != null)
                {
                    _userresponse.Message = (_userexist.UserName == user.UserName ? "UserName" : _userexist.Email == user.Email ? "Email" : "MobileNo")
                    + " already exists.";
                    _userresponse.Status = false;
                    return _userresponse;
                }
            }
            var _result = await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = user.UserName,
                UserType = user.UserType,
                Email = user.Email,
                PhoneNumber = user.MobileNo,
                NormalizedEmail = user.Email,
                NormalizedUserName = user.UserName,
            }, user.Password);

            if (_result.Succeeded)
            {
                var _user = await _userManager.FindByNameAsync(user.UserName);
                user.Id = _user.Id;

                if (user.UserRoles != null && user.UserRoles.Any())
                {
                    foreach (var UserRole in user.UserRoles)
                    {
                        var roleMaster = await _roleManager.FindByNameAsync(UserRole);
                        if (roleMaster == null)
                        {
                            await _roleManager.CreateAsync(new IdentityRole
                            {
                                Name = UserRole,
                                NormalizedName = UserRole
                            });
                        }
                        await _userManager.AddToRoleAsync(_user, UserRole);
                    }
                }

                if (user.Claims != null && user.Claims.Any())
                {
                    foreach (var claim in user.Claims)
                    {
                        await _userManager.AddClaimAsync(_user, new Claim(claim.Key, claim.Value));
                    }
                }
                _userresponse.Message = "User Created and role assigned successfully.";
                _userresponse.Status = true;
                _userresponse.ReturnObject = user;

            }
            else
            {
                _userresponse.Status = false;
                _userresponse.Message = _result.Errors != null && _result.Errors.Any() ? string.Join(", ", _result.Errors.Select(x => x.Description)) : "Try Using Different Password";
            }

            return _userresponse;
        }
        public async Task<UserResponse> CreateUserForLead(UserDTO user)
        {
            UserResponse _userresponse = new UserResponse();
            _userresponse.Message = "user created.";
            _userresponse.Status = true;
            var _userexist = await _userManager.FindByNameAsync(user.UserName);

            if (_userexist == null)
            {
                var _result = await _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = user.UserName,
                    UserType = user.UserType,
                    Email = user.Email,
                    PhoneNumber = user.MobileNo,
                    NormalizedEmail = user.Email,
                    NormalizedUserName = user.UserName,
                }, user.Password);

                if (_result.Succeeded)
                {
                    _userexist = await _userManager.FindByNameAsync(user.UserName);
                    user.Id = _userexist.Id;

                    if (user.UserRoles != null && user.UserRoles.Any())
                    {
                        foreach (var UserRole in user.UserRoles)
                        {
                            var roleMaster = await _roleManager.FindByNameAsync(UserRole);
                            if (roleMaster == null)
                            {
                                await _roleManager.CreateAsync(new IdentityRole
                                {
                                    Name = UserRole,
                                    NormalizedName = UserRole
                                });
                            }
                            await _userManager.AddToRoleAsync(_userexist, UserRole);
                        }
                    }


                    _userresponse.Message = "User Created and role assigned successfully.";
                    _userresponse.Status = true;
                    _userresponse.ReturnObject = user;

                }
            }
            else
            {
                if (user.UserRoles != null && user.UserRoles.Any())
                {
                    foreach (var UserRole in user.UserRoles)
                    {
                        var roleMaster = await _roleManager.FindByNameAsync(UserRole);
                        if (roleMaster == null)
                        {
                            await _roleManager.CreateAsync(new IdentityRole
                            {
                                Name = UserRole,
                                NormalizedName = UserRole
                            });
                        }
                        await _userManager.AddToRoleAsync(_userexist, UserRole);
                    }
                }
            }



            if (user.Claims != null && user.Claims.Any())
            {
                var existingUserclaim = await _userManager.GetClaimsAsync(_userexist);
                if (existingUserclaim != null && existingUserclaim.Any())
                {
                    await _userManager.RemoveClaimsAsync(_userexist, existingUserclaim);
                }
                var claims = user.Claims.Select(x => new Claim(x.Key, x.Value)).ToList();
                await _userManager.AddClaimsAsync(_userexist, claims);
            }

            _userresponse.ReturnObject = _userexist;
            return _userresponse;
        }

        public async Task<UserResponse> GetUserByUserName(string userName)
        {
            UserResponse _userresponse = new UserResponse();
            _userresponse.Message = "user available.";
            _userresponse.Status = true;
            var _userexist = await _userManager.FindByNameAsync(userName);
            _userresponse.ReturnObject = _userexist;
            return _userresponse;
        }
        public async Task<RoleResponse> CreateRoleByName(string RoleName)
        {
            RoleResponse roleResponse = new RoleResponse();
            if (string.IsNullOrEmpty(RoleName))
            {
                roleResponse.Status = false;
                roleResponse.Message = "Role Name is Invalid";
                return roleResponse;
            }
            var role = await _roleManager.FindByNameAsync(RoleName);
            if (role == null)
            {
                IdentityRole identityRole = new IdentityRole { Name = RoleName };
                var newRole = await _roleManager.CreateAsync(identityRole);
                if (newRole.Succeeded)
                {
                    roleResponse.Status = true;
                    roleResponse.Message = "Role Created.";
                }
                else
                {
                    roleResponse.Status = false;
                    roleResponse.Message = "Failed to Add Role.";
                }
            }
            else
            {
                roleResponse.Status = false;
                roleResponse.Message = "RoleName Already Exists.";
            }
            return roleResponse;
        }

        public async Task<RoleResponse> CreateRoleWithPermissions(RoleWithPermissionsDTO request)
        {
            RoleResponse roleResponse = new RoleResponse();
            if (string.IsNullOrEmpty(request.RoleName))
            {
                roleResponse.Status = false;
                roleResponse.Message = "Role Name is Invalid";
                return roleResponse;
            }
            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
            {
                IdentityRole identityRole = new IdentityRole { Name = request.RoleName };
                var newRole = await _roleManager.CreateAsync(identityRole);
                if (newRole.Succeeded)
                {
                    var roleid = await _roleManager.GetRoleIdAsync(identityRole);
                    var pages = await _context.AspNetPageMasters.Where(x => x.PageName.ToLower().Contains("lead") && x.IsActive && !x.IsDeleted).ToListAsync();
                    if (pages != null && pages.Any())
                    {
                        foreach (var page in pages)
                        {
                            AspNetRolePagePermission aspNetRolePagePermission = new AspNetRolePagePermission
                            {
                                PageMasterId = page.Id,
                                RoleId = roleid,
                                IsView = request.IsView,
                                IsAdd = request.IsAdd,
                                IsEdit = request.IsUpdate,
                                IsDelete = false,
                                IsAll = false
                            };
                            _context.Add(aspNetRolePagePermission);
                        }
                    }
                    await _context.SaveChangesAsync();
                    roleResponse.Status = true;
                    roleResponse.Message = "Role Created.";
                }
                else
                {
                    roleResponse.Status = false;
                    roleResponse.Message = "Failed to Add Role.";
                }
            }
            else
            {
                roleResponse.Status = false;
                roleResponse.Message = "RoleName Already Exists.";
            }
            return roleResponse;
        }

        public async Task<UserListDetailsResponse> GetUserById(UserListDetailsRequest req)
        {
            UserListDetailsResponse response = new UserListDetailsResponse();

            List<ApplicationUser> users = new List<ApplicationUser>();

            foreach (string UserId in req.userIds)
            {
                var user = await _userManager.FindByIdAsync(UserId);
                if (user != null)
                    users.Add(user);
            }
            if (users != null && users.Any())
            {
                response.UserListDetails = new List<UserListDetails>();


                var query = users.Where(x => (string.IsNullOrEmpty(req.keyword) || (x.UserName != null && x.UserName.ToLower().Contains(req.keyword.ToLower()))
                            || (x.Email != null && x.Email.Contains(req.keyword)) || (x.PhoneNumber != null && x.PhoneNumber.Contains(req.keyword))));


                foreach (var user in query.Skip(req.Skip).Take(req.Take).ToList())
                {

                    var roles = await _userManager.GetRolesAsync(user);
                    var UserRoles = roles.ToList();
                    UserListDetails data = new UserListDetails();

                    data.Email = user.Email;
                    data.NormalizedEmail = user.NormalizedEmail;
                    data.NormalizedUserName = user.NormalizedUserName;
                    data.PhoneNumber = user.PhoneNumber;
                    data.UserId = user.Id;
                    data.UserName = user.UserName;
                    data.UserType = user.UserType;
                    data.UserRoles = UserRoles;
                    data.TotalRecords = query.Count();
                    response.UserListDetails.Add(data);
                }
                if (response.UserListDetails.Any())
                {
                    response.Status = true;
                    response.Message = "User's Found";
                }
                else
                {
                    response.Status = false;
                    response.Message = "User Not Found";

                }
            }
            else
            {
                response.Status = false;
                response.Message = "User's Not Found";
            }
            return response;
        }

        public async Task<UpdateUserResponseDTO> GetUser(string UserId)
        {
            UpdateUserResponseDTO identityResponse = new UpdateUserResponseDTO();
            var user = await _userManager.FindByIdAsync(UserId);
            var claimsList = await _userManager.GetClaimsAsync(user);
            if (claimsList != null && claimsList.Any(x => x.Type == "companyid"))
            {
                var value = claimsList.FirstOrDefault(x => x.Type == "companyid").Value;
                if (!string.IsNullOrEmpty(value))
                {
                    identityResponse.CompanyIds = new List<long>();
                    var companyIds = value.Split(',');
                    for (int i = 0; i < companyIds.Length; i++)
                    {
                        identityResponse.CompanyIds.Add(int.Parse(companyIds[i]));
                    }
                }
            }
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var UserRoles = roles.ToList();
                identityResponse.Status = true;
                identityResponse.Email = user.Email;
                identityResponse.MobileNo = user.PhoneNumber;
                identityResponse.Id = user.Id;
                identityResponse.Roles = UserRoles;
                identityResponse.Name = user.UserName;
                identityResponse.UserType = user.UserType;


            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "User Not Found";
            }

            return identityResponse;
        }

        public async Task<GRPCReply<string>> UpdateUserByUserId(UserUpdateByIdRequestDTO req)
        {
            GRPCReply<string> identityResponse = new GRPCReply<string>();
            var allUsers = await _userManager.Users.ToListAsync();
            if (allUsers != null && allUsers.Any())
            {
                var _userexist = allUsers.FirstOrDefault(x => (x.Email == req.Email || x.PhoneNumber == req.MobileNo) && x.Id != req.Id && x.UserType.ToLower() != UserTypeConstants.Customer.ToLower());
                if (_userexist != null)
                {
                    identityResponse.Message = (_userexist.Email == req.Email ? "Email" : "MobileNo") + " already exists!!!";
                    identityResponse.Status = false;
                    identityResponse.Response = "";
                    return identityResponse;
                }
            }
            var user = await _userManager.FindByIdAsync(req.Id);
            if (user != null)
            {
                user.PhoneNumber = req.MobileNo;
                user.Email = req.Email;
                user.UserType = req.UserType;

                if (req.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() && !req.CompanyIds.Any(x => x == req.FinTechId))
                    req.CompanyIds.Add(req.FinTechId);
                List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>> {
                                new KeyValuePair<string,string>("companyid",string.Join(',', req.CompanyIds))
                                };
                var existingClaims = await _userManager.GetClaimsAsync(user);
                var claimList = existingClaims.Where(x => x.Type == "companyid").ToList();
                if (claimList != null && claimList.Any())
                {
                    foreach (var claim in claimList)
                    {
                        await _userManager.RemoveClaimAsync(user, claim);
                    }
                }
                if (Claims != null && Claims.Any())
                {
                    foreach (var claim in Claims)
                    {
                        await _userManager.AddClaimAsync(user, new Claim(claim.Key, claim.Value));
                    }
                }

                var roles = await _userManager.GetRolesAsync(user);
                var UserRoles = roles.ToList();
                foreach (var role in roles)
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, role);
                }
                foreach (var role in req.Roles)
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                }
                var aa = await _userManager.UpdateAsync(user);
                var res = _context.Entry(user).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    identityResponse.Status = true;
                    identityResponse.Message = "User Updated Successfully";
                    identityResponse.Response = "";
                }
                else
                {
                    identityResponse.Status = false;
                    identityResponse.Message = "Failed To Update User";
                    identityResponse.Response = "";
                }

            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "User Not Found";
                identityResponse.Response = "";
            }
            return identityResponse;
        }

        public async Task<IdentityResponse> GetRoleList(string? Keyword)
        {
            IdentityResponse identityResponse = new IdentityResponse();
            var roles = await _roleManager.Roles.ToListAsync();
            roles = roles != null && roles.Any() ? roles.Where(x => !x.Name.ToLower().Contains(UserRoleConstants.SuperAdmin) && (string.IsNullOrEmpty(Keyword) || x.Name.ToLower().Contains(Keyword.ToLower()))).ToList() : null;
            if (roles != null && roles.Any())
            {
                identityResponse.Status = true;
                identityResponse.Message = "Roles Found";
                identityResponse.ReturnObject = roles;
            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "Roles Not Found";
            }
            return identityResponse;
        }
        public async Task<GRPCReply<ChangeUserPasswordResponse>> ResetUserPassword(string UserId)
        {
            GRPCReply<ChangeUserPasswordResponse> identityResponse = new GRPCReply<ChangeUserPasswordResponse>();
            identityResponse.Response = new ChangeUserPasswordResponse();
            var user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string otp = GenerateRandomNumber.GenerateRandomOTP(3, saAllowedCharacters);
                string password = user.PasswordHash.Substring(24, 1).ToUpper() + user.PasswordHash.Substring(21, 3) + otp + "Sk_" + user.PasswordHash.Substring(25, 4);
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var res = await _userManager.ResetPasswordAsync(user, token, password);
                if (res.Succeeded)
                {
                    identityResponse.Status = true;
                    identityResponse.Message = "Password Changed";
                    identityResponse.Response.UserName = user.UserName;
                    identityResponse.Response.Password = password;
                    identityResponse.Response.Email = user.Email;
                }
                else
                {
                    identityResponse.Status = false;
                    identityResponse.Message = "Failed To Change Password!!!";
                }
            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "User Not Found!!!";
            }
            return identityResponse;
        }

        public async Task<GRPCReply<ChangeUserPasswordResponse>> ForgotUserPassword(GRPCRequest<string> UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName.Request);
            if (user == null)
                user = await _userManager.FindByEmailAsync(UserName.Request);
            if (user != null)
                return await ResetUserPassword(user.Id);
            else
                return new GRPCReply<ChangeUserPasswordResponse> { Status = false, Message = "User Not Found!!" };

        }
        public async Task<IdentityResponse> ChangeUserPassword(string UserId, string OldPassword, string NewPassword)
        {
            IdentityResponse identityResponse = new IdentityResponse();
            var user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                bool isPasswordCorrect = await _userManager.CheckPasswordAsync(user, OldPassword);
                if (isPasswordCorrect)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    var res = await _userManager.ResetPasswordAsync(user, token, NewPassword);
                    //var res = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
                    if (res.Succeeded)
                    {
                        identityResponse.Status = true;
                        identityResponse.Message = "Password Changed Successfully";
                    }
                    else
                    {
                        identityResponse.Status = false;
                        identityResponse.Message = "Failed To Change Password!!!";
                    }
                }
                else
                {
                    identityResponse.Status = false;
                    identityResponse.Message = "Old Password is Incorrect !!!";
                }
            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "User Not Found!!!";
            }
            return identityResponse;
        }

        public async Task<IdentityResponse> UpdateRole(string RoleId, string NewRoleName)
        {
            IdentityResponse identityResponse = new IdentityResponse();
            if (string.IsNullOrEmpty(RoleId) || string.IsNullOrEmpty(NewRoleName))
            {
                identityResponse.Status = false;
                identityResponse.Message = "RoleId/NewRoleName is Invalid";
                return identityResponse;
            }
            var role = await _roleManager.FindByIdAsync(RoleId);
            if (role != null)
            {
                role.Name = NewRoleName;
                var newRole = await _roleManager.UpdateAsync(role);
                if (newRole.Succeeded)
                {
                    identityResponse.Status = true;
                    identityResponse.Message = "Role Updated.";
                }
                else
                {
                    identityResponse.Status = false;
                    identityResponse.Message = "Failed to Update Role.";
                }
            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "Role Not Found.";
            }
            return identityResponse;
        }

        public async Task<IdentityResponse> DeleteRole(string RoleId)
        {
            IdentityResponse identityResponse = new IdentityResponse();
            if (string.IsNullOrEmpty(RoleId))
            {
                identityResponse.Status = false;
                identityResponse.Message = "RoleId is Invalid";
                return identityResponse;
            }
            var role = await _roleManager.FindByIdAsync(RoleId);
            if (role != null)
            {
                var deleted = await _roleManager.DeleteAsync(role);
                if (deleted.Succeeded)
                {
                    identityResponse.Status = true;
                    identityResponse.Message = "Role Deleted.";
                }
                else
                {
                    identityResponse.Status = false;
                    identityResponse.Message = "Failed to Delete Role.";
                }
            }
            else
            {
                identityResponse.Status = false;
                identityResponse.Message = "Role Not Found.";
            }
            return identityResponse;
        }

        public async Task<List<ApplicationUser>> GetUserList(string Keyword)
        {
            List<ApplicationUser> list = _userManager.Users.Where(x => x.UserName.Contains(Keyword)).ToList();
            return list;
        }

        public async Task<GRPCReply<CreateClientResponse>> CreateClient()
        {
            try
            {
                GRPCReply<CreateClientResponse> gRPCReply = new GRPCReply<CreateClientResponse>();
                var secret = GenerateApiSecret();
                var key = GenerateApiSecret();

                var client = new IdentityServer4.EntityFramework.Entities.Client
                {
                    ClientId = key,
                    ProtocolType = "oidc",
                    RequireClientSecret = true,
                    AllowRememberConsent = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = true,
                    AllowPlainTextPkce = false,
                    RequireRequestObject = true,
                    AllowAccessTokensViaBrowser = false,
                    FrontChannelLogoutSessionRequired = true,
                    BackChannelLogoutSessionRequired = true,
                    AllowOfflineAccess = false,
                    IdentityTokenLifetime = 3000,
                    AccessTokenLifetime = 3600,
                    AuthorizationCodeLifetime = 3600,
                    AbsoluteRefreshTokenLifetime = 2592000,
                    SlidingRefreshTokenLifetime = 1296000,
                    RefreshTokenExpiration = 3600,
                    RefreshTokenUsage = 3600,
                    IncludeJwtId = true,
                    AccessTokenType = 0,
                    AllowedCorsOrigins = new List<IdentityServer4.EntityFramework.Entities.ClientCorsOrigin>(),

                    Claims = new List<IdentityServer4.EntityFramework.Entities.ClientClaim>(),
                    ClientName = "",
                    RedirectUris = new List<IdentityServer4.EntityFramework.Entities.ClientRedirectUri>(),
                    IdentityProviderRestrictions = new List<IdentityServer4.EntityFramework.Entities.ClientIdPRestriction>(),
                    Properties = new List<IdentityServer4.EntityFramework.Entities.ClientProperty>(),
                    PostLogoutRedirectUris = new List<IdentityServer4.EntityFramework.Entities.ClientPostLogoutRedirectUri>(),
                    AlwaysSendClientClaims = true,
                    Created = DateTime.Now
                };

                client.AllowedGrantTypes = new List<IdentityServer4.EntityFramework.Entities.ClientGrantType> { new IdentityServer4.EntityFramework.Entities.ClientGrantType { GrantType = "client_credentials", Client = client } };
                client.ClientSecrets = new List<IdentityServer4.EntityFramework.Entities.ClientSecret> { new IdentityServer4.EntityFramework.Entities.ClientSecret { Value = secret.Sha256(), Client = client } };

                client.AllowedScopes = new List<IdentityServer4.EntityFramework.Entities.ClientScope> { new IdentityServer4.EntityFramework.Entities.ClientScope { Scope = "crmApi", Client = client } };

                _configContext.Clients.Add(client);

                await _configContext.SaveChangesAsync();
                gRPCReply.Status = true;
                gRPCReply.Response = new CreateClientResponse()
                {
                    ApiKey = key,
                    ApiSecret = secret
                };
                return gRPCReply;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public string GenerateApiKey()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            string base64String = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_");

            var keyLength = 32 - "CT-".Length;

            return "CT-" + base64String[..keyLength];
        }

        public string GenerateApiSecret()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            string base64String = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "-");

            var keyLength = 32;

            return base64String[..keyLength].ToUpper();
        }

        public async Task<GRPCReply<bool>> GetUserByEmailMobile(GRPCRequest<GetUserRequest> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Request.Email || x.PhoneNumber == request.Request.MobileNumber);
            if (user != null)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "User Found";
                gRPCReply.Response = true;
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "User Not Found";
                gRPCReply.Response = false;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> CheckCompanyAdminExist(GRPCRequest<List<string>> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool>();
            var users = await _userManager.GetUsersInRoleAsync(UserRoleConstants.CompanyAdmin);
            var existing = users != null && users.Any(x => request.Request.Contains(x.Id)) ? true : false;
            if (existing)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "CompanyAdmin Exists";
                gRPCReply.Response = true;
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "No CompanyAdmins Found";
                gRPCReply.Response = false;
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<bool>> UpdateUserRole(GRPCRequest<UpdateUserRoleRequest> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool> { Message = "Failed to update!!!" };
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == request.Request.UserId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles != null && roles.Any())
                {
                    foreach (var role in roles)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role);
                    }
                }
                if (request.Request.RoleNames != null && request.Request.RoleNames.Any(x => !string.IsNullOrEmpty(x)))
                {
                    foreach (var role in request.Request.RoleNames.Where(x => !string.IsNullOrEmpty(x)))
                    {
                        await _userManager.AddToRoleAsync(user, role);
                    }
                }
                gRPCReply.Status = true;
                gRPCReply.Message = "Roles Updated";
                gRPCReply.Response = true;
            }
            return gRPCReply;
        }
    }
}
