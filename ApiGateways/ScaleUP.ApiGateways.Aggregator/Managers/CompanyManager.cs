using Elasticsearch.Net;
using MassTransit;
using ScaleUP.ApiGateways.Aggregator.DTOs;
using ScaleUP.ApiGateways.Aggregator.Services;
using ScaleUP.ApiGateways.Aggregator.Services.Interfaces;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Communication.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Identity.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Location.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Common;
using ScaleUP.Global.Infrastructure.Constants;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace ScaleUP.ApiGateways.Aggregator.Managers
{
    public class CompanyManager
    {
        private ICompanyService _companyService;
        private ILocationService _locationService;
        private IIdentityService _identityService;
        private IProductService _productService;
        private ILeadService _leadService;
        private ICommunicationService _communicationService;
        private ILoanAccountService _loanAccountService;
        public CompanyManager(ICompanyService companyService, ILeadService leadService, IProductService productService, ILocationService locationService, IIdentityService identityService, ICommunicationService communicationService, ILoanAccountService loanAccountService)
        {
            _companyService = companyService;
            _locationService = locationService;
            _identityService = identityService;
            _communicationService = communicationService;
            _productService = productService;
            _leadService = leadService;
            _loanAccountService = loanAccountService;
        }

        public async Task<CompanyResponse> CreateCompanyLocationAsync(CreateAddressDTO createAddressDTO)
        {
            GRPCRequest<long> CompanyIdRequest = new GRPCRequest<long> { Request = createAddressDTO.CompanyId };
            var companyLocations = await _companyService.GetCompanyLocationById(CompanyIdRequest);
            var locationReply = await _locationService.CreateLocation(new CreateLocationRequest
            {
                AddressLineOne = createAddressDTO.AddressLineOne,
                AddressLineThree = createAddressDTO.AddressLineThree,
                AddressLineTwo = createAddressDTO.AddressLineTwo,
                AddressTypeId = createAddressDTO.AddressTypeId,
                CityId = createAddressDTO.CityId,
                ZipCode = createAddressDTO.ZipCode,
                ExistingLocationIds = companyLocations.Response != null && companyLocations.Response.Any() ? new List<long>(companyLocations.Response) : new List<long>()
            }); ;
            if (locationReply.Status)
            {
                var companyLocReply = await _companyService.CreateCompanyLocation(new CompanyLocationDTO
                {
                    CompanyId = createAddressDTO.CompanyId,
                    LocationId = locationReply.LocationId
                });
                if (companyLocReply.Status)
                {
                    return new CompanyResponse
                    {
                        Message = "Address Added Successfully",
                        Status = true
                    };
                }
                else
                {
                    return new CompanyResponse
                    {

                        Message = "Failed to add Address",
                        Status = false
                    };
                }
            }
            else
            {
                return new CompanyResponse
                {

                    Message = locationReply.Message,
                    Status = false
                };
            }

        }

        public async Task<UserResponse> CreateUserAsync(CreateUserDTO createUserDTO, string UserType)
        {
            UserResponse userResponse = new UserResponse();
            //Check ExistingCompanyAdmin
            if (createUserDTO.UserRoles.Any(x => x.ToLower() == UserRoleConstants.CompanyAdmin))
            {
                UserIdsListResponseRequest compRequest = new UserIdsListResponseRequest { CompanyIds = createUserDTO.CompanyIds };
                var companyIdsReply = await _companyService.GetUserList(compRequest);
                if (companyIdsReply.Status && companyIdsReply.UserCompanyDataList != null && companyIdsReply.UserCompanyDataList.Any(x => x.IsActive))
                {
                    var req = new GRPCRequest<List<string>> { Request = companyIdsReply.UserCompanyDataList.Where(x => x.IsActive).Select(x => x.UserId).ToList() };
                    var checkAdmin = await _identityService.CheckCompanyAdminExist(req);
                    if (checkAdmin.Status)
                    {
                        userResponse.Status = false;
                        userResponse.Message = "CompanyAdmin Already exists for this Company!!!";
                        return userResponse;
                    }
                }
            }

            var FinTechId = await _companyService.GetFinTechCompany();
            if (UserType.ToLower() != UserTypeConstants.SuperAdmin && UserType.ToLower() != UserTypeConstants.AdminUser.ToLower() && createUserDTO.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower())
            {
                return new UserResponse { Message = "Not Allowed To Create ScaleUp User", Status = false };
            }
            if (createUserDTO.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() && !createUserDTO.CompanyIds.Any(x => x == FinTechId.Response))
                createUserDTO.CompanyIds.Add(FinTechId.Response);
            List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>> {
                                new KeyValuePair<string,string>("companyid",string.Join(',', createUserDTO.CompanyIds))
                                };

            var userReply = await _identityService.CreateUser(new CreateUserRequest
            {
                UserType = createUserDTO.UserType,
                UserName = createUserDTO.UserName,
                UserRoles = createUserDTO.UserRoles,
                Claims = Claims,
                EmailId = createUserDTO.EmailId,
                MobileNo = createUserDTO.MobileNo,
                Password = createUserDTO.Password
            });

            if (userReply.Status)
            {
                var companyReply = await _companyService.AddUpdateCompanyUserMapping(new AddCompanyUserMappingRequest
                {
                    CompanyId = createUserDTO.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() ? FinTechId.Response : createUserDTO.CompanyIds.FirstOrDefault(),
                    UserId = userReply.UserId
                });
                if (companyReply.Status)
                {
                    userResponse.Status = true;
                    userResponse.Message = userReply.Message;
                }
                else
                {
                    userResponse.Status = false;
                    userResponse.Message = "Failed to Create User";
                }
            }
            else
            {
                userResponse.Status = false;
                userResponse.Message = userReply.Message;
            }
            return userResponse;
        }

        public async Task<UserUpdateByIdResponseDTO> UpdateUserByUserId(UserUpdateByIdDTO request, string UserType)
        {
            UserUpdateByIdResponseDTO reply = new UserUpdateByIdResponseDTO();
            //Check ExistingCompanyAdmin
            if (request.Roles.Any(x => x.ToLower() == UserRoleConstants.CompanyAdmin))
            {
                UserIdsListResponseRequest compRequest = new UserIdsListResponseRequest { CompanyIds = request.CompanyIds };
                var companyIdsReply = await _companyService.GetUserList(compRequest);
                if (companyIdsReply.Status && companyIdsReply.UserCompanyDataList != null && companyIdsReply.UserCompanyDataList.Any(x => x.IsActive && x.UserId != request.Id))
                {
                    var userIds = companyIdsReply.UserCompanyDataList.Where(x => x.IsActive && x.UserId != request.Id).Select(x => x.UserId).ToList();
                    if (userIds != null && userIds.Any())
                    {
                        var req = new GRPCRequest<List<string>> { Request = userIds };
                        var checkAdmin = await _identityService.CheckCompanyAdminExist(req);
                        if (checkAdmin.Status)
                        {
                            reply.Status = false;
                            reply.Message = "CompanyAdmin Already exists for this Company!!!";
                            return reply;
                        }
                    }
                }
            }
            var FinTechId = await _companyService.GetFinTechCompany();
            if (UserType.ToLower() != UserTypeConstants.SuperAdmin && UserType.ToLower() != UserTypeConstants.AdminUser.ToLower() && request.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower())
            {
                return new UserUpdateByIdResponseDTO { Message = "Not Allowed To Create ScaleUp User", Status = false };
            }
            var userReply = await _identityService.UpdateUserByUserId(new UserUpdateByIdRequestDTO
            {
                CompanyIds = request.CompanyIds,
                Email = request.Email,
                Id = request.Id,
                MobileNo = request.MobileNo,
                Roles = request.Roles,
                UserType = request.UserType,
                FinTechId = FinTechId.Response
            });
            if (userReply != null && userReply.Status)
            {
                var mappingReply = await _companyService.AddUpdateCompanyUserMapping(new AddCompanyUserMappingRequest
                {
                    CompanyId = request.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() ? FinTechId.Response : request.CompanyIds.FirstOrDefault(),
                    UserId = request.Id
                });
                reply.Status = userReply.Status;
                reply.Message = userReply.Message;
            }
            else
            {
                reply.Status = false;
                reply.Message = userReply.Message;
            }
            return reply;

        }

        public async Task<CreateCompanyResponse> CreateCompanyAsync(CreateCompanyDTO createCompanyDTO)
        {
            var identityResponse = await _identityService.CreateClient();

            CreateCompanyResponse createCompanyResponse = new CreateCompanyResponse();
            GRPCRequest<GetEntityCodeRequest> companyCodeReq = new GRPCRequest<GetEntityCodeRequest>
            {
                Request = new GetEntityCodeRequest
                {
                    EntityName = "Company",
                    CompanyType = createCompanyDTO.CompanyType
                }
            };
            var companyCodeRes = await _companyService.GetCurrentNumber(companyCodeReq);
            var request = new AddCompanyDTO
            {
                GSTNo = createCompanyDTO.GSTNo,
                PanNo = createCompanyDTO.PanNo,
                BusinessName = createCompanyDTO.BusinessName,
                LandingName = createCompanyDTO.LandingName,
                BusinessContactEmail = createCompanyDTO.BusinessContactEmail,
                BusinessContactNo = createCompanyDTO.BusinessContactNo,
                APIKey = identityResponse.Response.ApiKey,
                APISecretKey = identityResponse.Response.ApiSecret,
                LogoURL = createCompanyDTO.LogoURL,
                BusinessHelpline = createCompanyDTO.BusinessHelpline,
                BusinessTypeId = createCompanyDTO.BusinessTypeId,
                AgreementEndDate = createCompanyDTO.AgreementEndDate,
                AgreementStartDate = createCompanyDTO.AgreementStartDate,
                AgreementURL = createCompanyDTO.AgreementURL,
                BankAccountNumber = createCompanyDTO.BankAccountNumber,
                BankIFSC = createCompanyDTO.BankIFSC,
                BankName = createCompanyDTO.BankName,
                BusinessPanURL = createCompanyDTO.BusinessPanURL,
                CancelChequeURL = createCompanyDTO.CancelChequeURL,
                CancelChequeDocId = createCompanyDTO.CancelChequeDocId,
                AgreementDocId = createCompanyDTO.AgreementDocId,
                BusinessPanDocId = createCompanyDTO.BusinessPanDocId,
                CustomerAgreementDocId = createCompanyDTO.CustomerAgreementDocId,
                CustomerAgreementURL = createCompanyDTO.CustomerAgreementURL,
                ContactPersonName = createCompanyDTO.ContactPersonName,
                WhitelistURL = createCompanyDTO.WhitelistURL,
                CompanyType = createCompanyDTO.CompanyType,
                IsSelfConfiguration = createCompanyDTO.IsSelfConfiguration,
                GSTDocId = createCompanyDTO.GSTDocId,
                GSTDocumentURL = createCompanyDTO.GSTDocumentURL,
                MSMEDocId = createCompanyDTO.MSMEDocId,
                MSMEDocumentURL = createCompanyDTO.MSMEDocumentURL,
                CompanyCode = companyCodeRes.Status ? companyCodeRes.Response : createCompanyDTO.GSTNo.Substring(3) + createCompanyDTO.BusinessContactNo.ToString().Substring(3),
                PartnerList = createCompanyDTO.PartnerList.Select(x => new BuildingBlocks.GRPC.Contracts.Company.DataContracts.PartnerListDc
                {
                    MobileNo = x.MobileNo,
                    PartnerId = x.PartnerId,
                    PartnerName = x.PartnerName
                }
                ).ToList()
            };

            var companyReply = await _companyService.AddCompany(request);
            if (companyReply.Status)
            {
                string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string otp = GenerateRandomNumber.GenerateRandomOTP(3, saAllowedCharacters);
                string userName = createCompanyDTO.BusinessContactEmail.Trim().Length <= 5 ? createCompanyDTO.BusinessContactEmail.Trim().ToLower() : createCompanyDTO.BusinessContactEmail.Trim().ToLower().Substring(0, 7) + otp;
                string password = createCompanyDTO.BusinessName.Trim().Substring(0, 1).ToUpper() + createCompanyDTO.GSTNo.Trim().Substring(3, 3) + "@sk_" + createCompanyDTO.BusinessContactNo.Substring(3, 5);
                List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("companyid",companyReply.CompanyId.ToString())
                };
                var userReply = await _identityService.CreateUser(new CreateUserRequest
                {
                    UserType = UserTypeConstants.CompanyUser,
                    UserName = userName,
                    UserRoles = new List<string> { "CompanyAdmin" },
                    Claims = Claims,
                    EmailId = createCompanyDTO.BusinessContactEmail,
                    MobileNo = createCompanyDTO.BusinessContactNo,
                    Password = password
                });
                if (userReply.Status)
                {
                    var mappingReply = await _companyService.AddUpdateCompanyUserMapping(new AddCompanyUserMappingRequest
                    {
                        CompanyId = companyReply.CompanyId,
                        UserId = userReply.UserId
                    });
                    var emailReply = await _communicationService.SendEmail(new SendEmailRequest
                    {
                        From = "",
                        To = createCompanyDTO.BusinessContactEmail,
                        Message = $@"Your Login Credential for ScaleUp :-  <br>
                                        UserName :-  {userName}  <br>
                                        Password :-  {password}  <br>
                                        Please Change Your Password <br>
                                        Regards:- ScaleUp",
                        Subject = "Login Credential for ScaleUp",
                        File = "",
                        BCC = ""
                    });
                    if (emailReply.Status)
                    {
                        createCompanyResponse.Status = true;
                        createCompanyResponse.Message = "Company Created Successfully";
                        createCompanyResponse.CompanyId = companyReply.CompanyId;
                    }
                    else
                    {
                        createCompanyResponse.Status = true;
                        createCompanyResponse.Message = "Failed to Send Email";
                        createCompanyResponse.CompanyId = companyReply.CompanyId;
                    }
                }
                else
                {
                    createCompanyResponse.Status = true;
                    createCompanyResponse.Message = userReply.Message;
                    createCompanyResponse.CompanyId = companyReply.CompanyId;
                }
            }
            else
            {
                createCompanyResponse.Status = false;
                createCompanyResponse.Message = companyReply.Message;
            }


            return createCompanyResponse;
        }

        public async Task<CompanyListResponse> GetCompanyListAsync(CompanyListDTO companyDc)
        {
            CompanyListResponse companyResponse = new CompanyListResponse();
            var companyReply = await _companyService.GetCompanyList(new CompanyListRequest
            {
                CompanyType = companyDc.CompanyType,
                keyword = companyDc.keyword,
                Skip = companyDc.skip,
                Take = companyDc.take,
            });

            if (companyReply.Status)
            {
                var companylocationIdsList = companyReply.CompanyList.Where(x => x.locationId != null && x.locationId.Any()).Select(y => y.locationId).ToList();
                var locationIds = companylocationIdsList.SelectMany(list => list).ToList();

                var locationAddressReply = await _locationService.GetCompanyAddress(locationIds);

                var companyIdsList = companyReply.CompanyList.Select(x => x.Id).ToList();
                var companyProductReply = await _productService.GetCompanyProductConfig(new GetCompanyProductConfigRequest
                {
                    CompanyIds = companyIdsList,
                    CompanyType = companyDc.CompanyType,
                });
                companyResponse.Companylist = new List<Companylist>();

                foreach (var company in companyReply.CompanyList)
                {
                    Companylist companies = new Companylist();

                    companies.Id = company.Id;
                    companies.LendingName = company.landingName;
                    companies.GSTNumber = company.gst;
                    companies.BusinessName = company.businessName;
                    companies.CompanyType = company.CompanyType;
                    companies.CompanyCode = company.CompanyCode;
                    companies.IsActive = company.IsActive;
                    companies.MobileNumber = company.MobileNumber;
                    companies.Email = company.Email;
                    //companies.AgreementEndDate = company.AgreementEndDate;
                    //companies.AgreementStartDate = company.AgreementStartDate;
                    //companies.AgreementUrl = company.AgreementUrl;
                    //companies.AgreementDocId = company.AgreementDocId;
                    companies.Pan = company.Pan;
                    companies.IsDefault = company.IsDefault;
                    companies.GstRate = company.GstRate;
                    companies.TotalRecords = company.totalRecords;
                    companies.CancelChequeURL = company.CancelChequeURL;
                    companies.CancelChequeDocId = company.CancelChequeDocId;
                    companies.CustomerAgreementURL = company.CustomerAgreementURL;
                    companies.CustomerAgreementDocId = company.CustomerAgreementDocId;
                    companies.BusinessPanURL = company.BusinessPanURL;
                    companies.BusinessPanDocId = company.BusinessPanDocId;
                    companies.BusinessTypeId = company.BusinessTypeId;

                    if (locationAddressReply.GetAddressDTO != null && locationAddressReply.GetAddressDTO.Any())
                    {
                        companies.Addresses = locationAddressReply.GetAddressDTO.Where(y => companyReply.CompanyList.Any(z => z.Id == company.Id && (z.locationId != null && z.locationId.Any() && z.locationId.Contains(y.Id))))
                            .Select(x => new Address
                            {
                                AddressLineOne = x.AddressLineOne,
                                AddressLineThree = x.AddressLineThree,
                                AddressLineTwo = x.AddressLineTwo,
                                AddressTypeId = x.AddressTypeId,
                                AddressTypeName = x.AddressTypeName,
                                CityId = x.CityId,
                                CityName = x.CityName,
                                CountryId = x.CountryId,
                                CountryName = x.CountryName,
                                Id = x.Id,
                                StateId = x.StateId,
                                StateName = x.StateName,
                                ZipCode = x.ZipCode
                            }).ToList();
                    }
                    if (companyProductReply.Status)
                    {
                        companies.Configuration = companyProductReply.GetCompanyProductConfigList.Where(x => x.CompanyId == company.Id).Select(y =>
                            new configuration
                            {
                                CompanyId = y.CompanyId,
                                BounceCharges = y.BounceCharges,
                                CompanyProductId = y.ProductCompanyId,
                                ProductId = y.ProductId,
                                ProductName = y.ProductName,
                                AnnualInterestRate = y.AnnualInterestRate,
                                //CreditDays = y.CreditDays,
                                DelayPenaltyFee = y.DelayPenaltyFee,
                                GstRate = y.GstRate,
                                ProcessingCreditDays = y.ProcessingCreditDays,
                                ProcessingFee = y.ProcessingFee,
                                AgreementDocId = y.AgreementDocId,
                                AgreementEndDate = y.AgreementEndDate,
                                AgreementStartDate = y.AgreementStartDate,
                                AgreementUrl = y.AgreementUrl
                            }).ToList();
                    }
                    else
                    {
                        companies.Configuration = new List<configuration>();
                    }
                    companyResponse.Companylist.Add(companies);

                }
                companyResponse.Status = true;
                companyResponse.Message = "Data Found";
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "No Data Found";
            }
            return companyResponse;
        }


        public async Task<UserListResponse> GetUserList(CompanyDTO req)
        {
            UserListResponse userListResponse = new UserListResponse();

            UserIdsListResponseRequest compRequest = new UserIdsListResponseRequest();
            compRequest.CompanyIds = req.companyIds;
            var companyIdsReply = await _companyService.GetUserList(compRequest);
            if (companyIdsReply.Status)
            {
                UserListDetailsRequest filterDataDc = new UserListDetailsRequest
                {
                    userIds = companyIdsReply.UserCompanyDataList.Select(x => x.UserId).ToList(),
                    keyword = req.keyword,
                    Skip = req.Skip,
                    Take = req.Take
                };
                var userReply = await _identityService.GetUserById(filterDataDc);

                if (userReply.Status)
                {

                    var userCompanydata = companyIdsReply.UserCompanyDataList;

                    var query =
                        from companyData in userCompanydata
                        join user in userReply.UserListDetails on companyData.UserId equals user.UserId
                        select new Users
                        {
                            Email = user.Email,
                            NormalizedEmail = user.NormalizedEmail,
                            NormalizedUserName = user.NormalizedUserName,
                            PhoneNumber = user.PhoneNumber,
                            UserId = user.UserId,
                            UserName = user.UserName,
                            UserType = user.UserType,
                            UserRoles = user.UserRoles,
                            CompanyName = companyData.CompanyName,
                            CompanyId = companyData.CompanyId,
                            IsActive = companyData.IsActive,
                            TotalRecords = user.TotalRecords
                        };
                    userListResponse.Status = true;
                    userListResponse.Users = query.ToList();

                }
                else
                {
                    userListResponse.Status = userReply.Status;
                    userListResponse.Message = userReply.Message;
                }

            }
            else
            {
                userListResponse.Status = companyIdsReply.Status;
                userListResponse.Message = companyIdsReply.Message;
            }
            return userListResponse;
        }


        public async Task<string> GetProductName(long productId)
        {
            return await _productService.GetProductName(productId);
        }


        public async Task<bool> ResetUserPassword(string UserId)
        {
            var reply = await _identityService.ResetUserPassword(UserId);
            if (reply.Status && reply.Response.Email != null)
            {
                var emailReply = await _communicationService.SendEmail(new SendEmailRequest
                {
                    From = "",
                    To = reply.Response.Email,
                    Message = $@"Your Password for username  '{reply.Response.UserName}' <br>
                                            Has been Reset.<br>
                                            Your New Password is :- '{reply.Response.Password}'  <br>
                                            Please Change Your Password <br>
                                            Regards:- ScaleUp",
                    Subject = "Password Reset",
                    File = "",
                    BCC = ""
                });
                return emailReply.Status;
            }
            return false;
        }


        public async Task GetFinTechCompany()
        {
            var data = await _companyService.GetFinTechCompany();
        }


        public async Task<string> GetCurrentNumber(string EntityName, string CompanyType)
        {
            GRPCRequest<GetEntityCodeRequest> request = new GRPCRequest<GetEntityCodeRequest>
            {
                Request = new GetEntityCodeRequest
                {
                    CompanyType = CompanyType,
                    EntityName = EntityName
                },
            };
            var data = await _companyService.GetCurrentNumber(request);
            return data.Response;
        }

        public async Task<ForgotPasswordResponse> ForgotUserPassword(string UserName)
        {
            ForgotPasswordResponse forgotPasswordResponse = new ForgotPasswordResponse();
            var reply = await _identityService.ForgotUserPassword(new GRPCRequest<string> { Request = UserName });
            if (reply.Status && reply.Response.Email != null)
            {
                var emailReply = await _communicationService.SendEmail(new SendEmailRequest
                {
                    From = "",
                    To = reply.Response.Email,
                    Message = $@"Your Password for username  '{reply.Response.UserName}' <br>
                                            Has been Reset.<br>
                                            Your New Password is :- '{reply.Response.Password}'  <br>
                                            Please Change Your Password <br>
                                            Regards:- ScaleUp",
                    Subject = "Password Reset",
                    File = "",
                    BCC = ""
                });
                forgotPasswordResponse.Status = emailReply.Status;
                forgotPasswordResponse.Message = emailReply.Message;
            }
            else
            {
                forgotPasswordResponse.Status = reply.Status;
                forgotPasswordResponse.Message = reply.Message;
            }
            return forgotPasswordResponse;
        }
        public async Task<AuditLogAggResponseDc> GetAuditLogs(AuditLogAggRequestDc auditLogAggRequestDc)
        {
            AuditLogAggResponseDc auditLogAggResponseDc = new AuditLogAggResponseDc();
            if (auditLogAggRequestDc != null)
            {
                GRPCRequest<AuditLogRequest> request = new GRPCRequest<AuditLogRequest>
                {
                    Request = new AuditLogRequest
                    {
                        DatabaseName = auditLogAggRequestDc.DatabaseName,
                        EntityId = auditLogAggRequestDc.EntityId,
                        EntityName = auditLogAggRequestDc.EntityName,
                        Skip = auditLogAggRequestDc.Skip,
                        Take = auditLogAggRequestDc.Take
                    }
                };
                GRPCReply<List<AuditLogReply>> reply = new GRPCReply<List<AuditLogReply>>();
                if (auditLogAggRequestDc.DatabaseName.ToLower().Contains("company"))
                {
                    reply = await _companyService.GetAuditLogs(request);
                }
                else if (auditLogAggRequestDc.DatabaseName.ToLower().Contains("product"))
                {
                    reply = await _productService.GetAuditLogs(request);
                }
                else if (auditLogAggRequestDc.DatabaseName.ToLower().Contains("location"))
                {
                    reply = await _locationService.GetAuditLogs(request);
                }
                else if (auditLogAggRequestDc.DatabaseName.ToLower().Contains("lead"))
                {
                    reply = await _leadService.GetAuditLogs(request);
                }
                else if (auditLogAggRequestDc.DatabaseName.ToLower().Contains("loanaccount"))
                {
                    reply = await _loanAccountService.GetAuditLogs(request);
                }
                auditLogAggResponseDc.Status = reply.Status;
                auditLogAggResponseDc.Message = reply.Message;
                if (reply.Response != null && reply.Response.Any())
                {
                    UserListDetailsRequest userReq = new UserListDetailsRequest
                    {
                        userIds = reply.Response.Select(x => x.ModifiedBy).Distinct().ToList(),
                        keyword = null,
                        Skip = 0,
                        Take = 10
                    };
                    var userReply = await _identityService.GetUserById(userReq);
                    auditLogAggResponseDc.AuditLogs = reply.Response.Select(x => new AuditLog
                    {
                        Changes = x.Changes,
                        ModifiedBy = userReply.UserListDetails != null ? userReply.UserListDetails.Where(z => z.UserId == x.ModifiedBy).Select(y => y.UserName).FirstOrDefault() : "",
                        ModifiedDate = x.ModifiedDate,
                        ActionType = x.ActionType
                    }).ToList();
                    auditLogAggResponseDc.TotalRecords = reply.Response.FirstOrDefault().TotalRecords;
                }
            }
            return auditLogAggResponseDc;
        }

        public async Task<CreateCompanyResponse> SaveCompanyAndLocationAsync(SaveCompanyAndLocationDTO createCompanyDTO)
        {
            CreateCompanyResponse createCompanyResponse = new CreateCompanyResponse();
            if (createCompanyDTO.CompanyType.ToLower() == "anchor")
            {
                var userResponse = await _identityService.GetUserByEmailMobile(new GRPCRequest<GetUserRequest>
                {
                    Request = new GetUserRequest
                    {
                        Email = createCompanyDTO.BusinessContactEmail,
                        MobileNumber = createCompanyDTO.BusinessContactNo
                    }
                });
                if (userResponse != null && userResponse.Response)
                {
                    createCompanyResponse.Status = false;
                    createCompanyResponse.Message = "BusinessContactEmail/BusinessContactNo is already used by user!!!";
                    return createCompanyResponse;
                }
            }


            var identityResponse = await _identityService.CreateClient();

            GRPCRequest<GetEntityCodeRequest> companyCodeReq = new GRPCRequest<GetEntityCodeRequest>
            {
                Request = new GetEntityCodeRequest
                {
                    EntityName = "Company",
                    CompanyType = createCompanyDTO.CompanyType
                }
            };
            var companyCodeRes = await _companyService.GetCurrentNumber(companyCodeReq);
            var request = new AddCompanyDTO
            {
                GSTNo = createCompanyDTO.GSTNo,
                PanNo = createCompanyDTO.PanNo,
                BusinessName = createCompanyDTO.BusinessName,
                LandingName = string.IsNullOrEmpty(createCompanyDTO.LandingName) ? createCompanyDTO.BusinessName : createCompanyDTO.LandingName,
                BusinessContactEmail = createCompanyDTO.BusinessContactEmail,
                BusinessContactNo = createCompanyDTO.BusinessContactNo,
                APIKey = identityResponse.Response.ApiKey,
                APISecretKey = identityResponse.Response.ApiSecret,
                LogoURL = createCompanyDTO.LogoURL,
                BusinessHelpline = createCompanyDTO.BusinessHelpline,
                BusinessTypeId = createCompanyDTO.BusinessTypeId,
                AgreementEndDate = createCompanyDTO.AgreementEndDate,
                AgreementStartDate = createCompanyDTO.AgreementStartDate,
                AgreementURL = createCompanyDTO.AgreementURL,
                AgreementDocId = createCompanyDTO.AgreementDocId,
                BankAccountNumber = createCompanyDTO.BankAccountNumber,
                BankIFSC = createCompanyDTO.BankIFSC,
                BankName = createCompanyDTO.BankName,
                BusinessPanURL = createCompanyDTO.BusinessPanURL,
                CancelChequeURL = createCompanyDTO.CancelChequeURL,
                CancelChequeDocId = createCompanyDTO.CancelChequeDocId,
                BusinessPanDocId = createCompanyDTO.BusinessPanDocId,
                CustomerAgreementDocId = createCompanyDTO.CustomerAgreementDocId,
                CustomerAgreementURL = createCompanyDTO.CustomerAgreementURL,
                ContactPersonName = createCompanyDTO.ContactPersonName,
                WhitelistURL = createCompanyDTO.WhitelistURL,
                CompanyType = createCompanyDTO.CompanyType,
                IsSelfConfiguration = createCompanyDTO.IsSelfConfiguration,
                GSTDocId = createCompanyDTO.GSTDocId,
                GSTDocumentURL = createCompanyDTO.GSTDocumentURL,
                MSMEDocId = createCompanyDTO.MSMEDocId,
                MSMEDocumentURL = createCompanyDTO.MSMEDocumentURL,
                CompanyStatus = createCompanyDTO.CompanyStatus,
                AccountType = createCompanyDTO.AccountType,
                PanURL = createCompanyDTO.PanURL,
                PanDocId = createCompanyDTO.PanDocId,
                CompanyCode = companyCodeRes.Status ? companyCodeRes.Response : createCompanyDTO.GSTNo.Substring(3) + createCompanyDTO.BusinessContactNo.ToString().Substring(3),
                PartnerList = createCompanyDTO.PartnerList.Select(x => new BuildingBlocks.GRPC.Contracts.Company.DataContracts.PartnerListDc
                {
                    MobileNo = x.MobileNo,
                    PartnerId = x.PartnerId,
                    PartnerName = x.PartnerName
                }
                ).ToList()
            };

            var companyReply = await _companyService.AddCompany(request);
            if (companyReply.Status)
            {
                if (createCompanyDTO.financialLiaisonDetails != null)
                {
                    var AddFinancialLiaisonrequest = new AddfinancialLiaisonDetailsDTO
                    {
                        FirstName = createCompanyDTO.financialLiaisonDetails.financialLiaisonFirstName,
                        LastName = createCompanyDTO.financialLiaisonDetails.financialLiaisonLastName,
                        ContactNo = createCompanyDTO.financialLiaisonDetails.financialLiaisonContactNo,
                        EmailAddress = createCompanyDTO.financialLiaisonDetails.financialLiaisonEmailAddress,
                        CompanyId = companyReply.CompanyId
                    };
                    var financialLiaisonDetails = await _companyService.AddFinancialLiaisonDetails(AddFinancialLiaisonrequest);
                }
                GRPCRequest<long> CompanyIdRequest = new GRPCRequest<long> { Request = companyReply.CompanyId };
                var companyLocations = await _companyService.GetCompanyLocationById(CompanyIdRequest);
                var locationReply = await _locationService.CreateLocation(new CreateLocationRequest
                {
                    AddressLineOne = createCompanyDTO.CompanyAddress.AddressLineOne,
                    AddressLineThree = createCompanyDTO.CompanyAddress.AddressLineThree,
                    AddressLineTwo = createCompanyDTO.CompanyAddress.AddressLineTwo,
                    AddressTypeId = createCompanyDTO.CompanyAddress.AddressTypeId,
                    CityId = createCompanyDTO.CompanyAddress.CityId,
                    ZipCode = createCompanyDTO.CompanyAddress.ZipCode,
                    ExistingLocationIds = companyLocations.Response != null && companyLocations.Response.Any() ? new List<long>(companyLocations.Response) : new List<long>()
                }); ;
                if (locationReply.Status)
                {
                    var companyLocReply = await _companyService.CreateCompanyLocation(new CompanyLocationDTO
                    {
                        CompanyId = companyReply.CompanyId,
                        LocationId = locationReply.LocationId
                    });
                    if (companyLocReply.Status)
                    {
                        if (createCompanyDTO.CompanyType.ToLower() == "nbfc")
                        {
                            createCompanyResponse.Status = true;
                            createCompanyResponse.Message = "Company Created Successfully";
                            createCompanyResponse.CompanyId = companyReply.CompanyId;
                            return createCompanyResponse;
                        }
                        string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                        string otp = GenerateRandomNumber.GenerateRandomOTP(3, saAllowedCharacters);
                        string userName = createCompanyDTO.BusinessContactEmail.Trim().Length <= 5 ? createCompanyDTO.BusinessContactEmail.Trim().ToLower() : createCompanyDTO.BusinessContactEmail.Trim().ToLower().Substring(0, 7) + otp;
                        string password = createCompanyDTO.BusinessName.Trim().Substring(0, 1).ToUpper() + createCompanyDTO.GSTNo.Trim().Substring(3, 3) + "@sk_" + createCompanyDTO.BusinessContactNo.Substring(3, 5);
                        List<KeyValuePair<string, string>> Claims = new List<KeyValuePair<string, string>> {
                            new KeyValuePair<string, string>("companyid",companyReply.CompanyId.ToString())
                        };
                        var userReply = await _identityService.CreateUser(new CreateUserRequest
                        {
                            UserType = "CompanyUser",
                            UserName = userName,
                            UserRoles = new List<string> { "CompanyAdmin" },
                            Claims = Claims,
                            EmailId = createCompanyDTO.BusinessContactEmail,
                            MobileNo = createCompanyDTO.BusinessContactNo,
                            Password = password
                        });
                        if (userReply.Status)
                        {
                            var mappingReply = await _companyService.AddUpdateCompanyUserMapping(new AddCompanyUserMappingRequest
                            {
                                CompanyId = companyReply.CompanyId,
                                UserId = userReply.UserId
                            });
                            var emailReply = await _communicationService.SendEmail(new SendEmailRequest
                            {
                                From = "",
                                To = createCompanyDTO.BusinessContactEmail,
                                Message = $@"Your Login Credential for ScaleUp :-  <br>
                                        UserName :-  {userName}  <br>
                                        Password :-  {password}  <br>
                                        Please Change Your Password <br>
                                        Regards:- ScaleUp",
                                Subject = "Login Credential for ScaleUp",
                                File = "",
                                BCC = ""
                            });
                            if (emailReply.Status)
                            {
                                createCompanyResponse.Status = true;
                                createCompanyResponse.Message = "Company Created Successfully";
                                createCompanyResponse.CompanyId = companyReply.CompanyId;
                            }
                            else
                            {
                                createCompanyResponse.Status = true;
                                createCompanyResponse.Message = "Failed to Send Email";
                                createCompanyResponse.CompanyId = companyReply.CompanyId;
                            }
                        }
                        else
                        {
                            createCompanyResponse.Status = true;
                            createCompanyResponse.Message = userReply.Message;
                            createCompanyResponse.CompanyId = companyReply.CompanyId;
                        }
                    }
                    else
                    {
                        createCompanyResponse.Status = true;
                        createCompanyResponse.Message = "Failed to add Address";
                        createCompanyResponse.CompanyId = companyReply.CompanyId;
                    }
                }
                else
                {
                    createCompanyResponse.Status = false;
                    createCompanyResponse.Message = locationReply.Message;
                }
            }
            else
            {
                createCompanyResponse.Status = false;
                createCompanyResponse.Message = companyReply.Message;
            }


            return createCompanyResponse;
        }

        public async Task<CompanyAddressDetailsDTO> GetCompanyAddressAndDetails(long companyId)
        {
            CompanyAddressDetailsDTO reply = new CompanyAddressDetailsDTO();
            var companyReply = await _companyService.GetCompanyAddressAndDetails(new CompanyAddressAndDetailsReq { CompanyId = companyId });
            var list = new List<GetAddressdc>();
            var partnerlist = new List<CompanyPartnersDc>();
            if (companyReply.Status)
            {
                if (companyReply.Response.companyLocationIds != null && companyReply.Response.companyLocationIds.Any())
                {
                    var locationAddressReply = await _locationService.GetCompanyAddress(companyReply.Response.companyLocationIds);
                    if (locationAddressReply.Status)
                    {
                        foreach (var _locationAdd in locationAddressReply.GetAddressDTO)
                        {
                            var obj = new GetAddressdc
                            {
                                AddressLineOne = _locationAdd.AddressLineOne,
                                AddressLineThree = _locationAdd.AddressLineThree,
                                AddressLineTwo = _locationAdd.AddressLineTwo,
                                AddressTypeId = _locationAdd.AddressTypeId,
                                AddressTypeName = _locationAdd.AddressTypeName,
                                CityId = _locationAdd.CityId,
                                CityName = _locationAdd.CityName,
                                CountryId = _locationAdd.CountryId,
                                CountryName = _locationAdd.CountryName,
                                Id = _locationAdd.Id,
                                StateId = _locationAdd.StateId,
                                StateName = _locationAdd.StateName,
                                ZipCode = _locationAdd.ZipCode
                            };
                            list.Add(obj);
                        }
                    }
                    else
                    {
                        reply.Status = locationAddressReply.Status;
                        reply.Message = locationAddressReply.Message;
                    }
                }
                if (companyReply.Response.CompanyPartnerDc != null)
                {
                    foreach (var _partner in companyReply.Response.CompanyPartnerDc)
                    {
                        var obj = new CompanyPartnersDc
                        {
                            PartnerId = _partner.PartnerId,
                            MobileNo = _partner.MobileNo,
                            PartnerName = _partner.PartnerName
                        };
                        partnerlist.Add(obj);
                    }
                }
                CompanyAddressDetails comp = new CompanyAddressDetails();
                comp.GSTDocId = companyReply.Response.GSTDocId;
                comp.PanNo = companyReply.Response.PanNo;
                comp.CancelChequeURL = companyReply.Response.CancelChequeURL != null ? companyReply.Response.CancelChequeURL : "";
                comp.CancelChequeDocId = companyReply.Response.CancelChequeDocId;
                comp.GSTNo = companyReply.Response.GSTNo;
                comp.CompanyCode = companyReply.Response.CompanyCode;
                comp.WhitelistURL = companyReply.Response.WhitelistIRL;
                comp.BankIFSC = companyReply.Response.BankIFSC;
                comp.BusinessHelpline = companyReply.Response.BusinessHelpline;
                comp.BusinessContactEmail = companyReply.Response.BusinessContactEmail;
                comp.BusinessPanURL = companyReply.Response.BusinessPanURL != null ? companyReply.Response.BusinessPanURL : "";
                comp.BankAccountNumber = companyReply.Response.BankAccountNumber;
                comp.BankName = companyReply.Response.BankName;
                comp.BusinessContactNo = companyReply.Response.BusinessContactNo;
                comp.BusinessTypeId = companyReply.Response.BusinessTypeId;
                comp.BusinessPanDocId = companyReply.Response.BusinessPanDocId;
                comp.AgreementDocId = companyReply.Response.AgreementDocId;
                comp.AgreementEndDate = companyReply.Response.AgreementEndDate;
                comp.AgreementStartDate = companyReply.Response.AgreementStartDate;
                comp.AgreementURL = companyReply.Response.AgreementURL != null ? companyReply.Response.AgreementURL : "";
                comp.GSTDocumentURL = companyReply.Response.GSTDocumentURL != null ? companyReply.Response.GSTDocumentURL : "";
                comp.InterestRate = companyReply.Response.InterestRate;
                comp.APIKey = companyReply.Response.APIKey;
                comp.APISecretKey = companyReply.Response.APISecretKey;
                comp.BusinessName = companyReply.Response.BusinessName;
                comp.CompanyType = companyReply.Response.CompanyType;
                comp.ContactPersonName = companyReply.Response.ContactPersonName;
                comp.CustomerAgreementDocId = companyReply.Response.CustomerAgreementDocId;
                comp.CustomerAgreementURL = companyReply.Response.CustomerAgreementURL != null ? companyReply.Response.CustomerAgreementURL : "";
                comp.MSMEDocId = companyReply.Response.MSMEDocId;
                comp.IsDefault = companyReply.Response.IsDefault;
                comp.IsSelfConfiguration = companyReply.Response.IsSelfConfiguration;
                comp.LandingName = companyReply.Response.LandingName;
                comp.LogoURL = companyReply.Response.LogoURL != null ? companyReply.Response.LogoURL : "";
                comp.MSMEDocId = companyReply.Response.MSMEDocId;
                comp.MSMEDocumentURL = companyReply.Response.MSMEDocumentURL != null ? companyReply.Response.MSMEDocumentURL : "";
                comp.OfferMaxRate = companyReply.Response.OfferMaxRate;
                comp.CompanyStatus = companyReply.Response.CompanyStatus;
                comp.CompanyAddress = list;
                comp.PartnerList = partnerlist;
                comp.AccountType = companyReply.Response.AccountType;
                comp.PanDocId = companyReply.Response.PanDocId;
                comp.PanURL = companyReply.Response.PanURL;
                comp.AccountHolderName = companyReply.Response.AccountHolderName;
                comp.BranchName = companyReply.Response.BranchName;
                if (companyReply.Response.FinancialLiaisonDetailDTO != null)
                {
                    comp.FinancialLiaisonDetails = new FinancialLiaisonDetailDc
                    {
                        FinancialLiaisonFirstName = companyReply.Response.FinancialLiaisonDetailDTO?.FirstName,
                        FinancialLiaisonContactNo = companyReply.Response.FinancialLiaisonDetailDTO?.ContactNo,
                        FinancialLiaisonEmailAddress = companyReply.Response.FinancialLiaisonDetailDTO?.EmailAddress,
                        FinancialLiaisonLastName = companyReply.Response.FinancialLiaisonDetailDTO?.LastName
                    };
                }
                reply.Status = true;
                reply.Message = "Data found";
                reply.Response = comp;
            }
            else
            {
                reply.Status = companyReply.Status;
                reply.Message = companyReply.Message;
            }

            return reply;

        }

        public async Task<UpdateCompanyResponse> UpdateCompanyAsync(SaveCompanyAndLocationDTO companydc)
        {
            UpdateCompanyResponse res = new UpdateCompanyResponse();
            var list = new List<BuildingBlocks.GRPC.Contracts.Company.DataContracts.PartnerListDc>();
            if (companydc.PartnerList != null && companydc.PartnerList.Any())
            {
                foreach (var _partner in companydc.PartnerList)
                {
                    var obj = new BuildingBlocks.GRPC.Contracts.Company.DataContracts.PartnerListDc
                    {
                        PartnerId = _partner.PartnerId,
                        MobileNo = _partner.MobileNo,
                        PartnerName = _partner.PartnerName
                    };
                    list.Add(obj);
                }
            }
            var companyReply = await _companyService.UpdateCompanyAsync(new UpdateCompanyRequest
            {
                AgreementDocId = companydc.CustomerAgreementDocId,
                AgreementEndDate = companydc.AgreementEndDate,
                AgreementStartDate = companydc.AgreementStartDate,
                AgreementURL = companydc.AgreementURL,
                AccountType = companydc.AccountType,
                APIKey = companydc.APIKey,
                APISecretKey = companydc.APISecretKey,
                BankAccountNumber = companydc.BankAccountNumber,
                BankName = companydc.BankName,
                BusinessContactEmail = companydc.BusinessContactEmail,
                BusinessContactNo = companydc.BusinessContactNo,
                BusinessHelpline = companydc.BusinessHelpline,
                BusinessName = companydc.BusinessName,
                BusinessPanDocId = companydc.BusinessPanDocId,
                BusinessPanURL = companydc.BusinessPanURL,
                BusinessTypeId = companydc.BusinessTypeId,
                CancelChequeDocId = companydc.CancelChequeDocId,
                CancelChequeURL = companydc.CancelChequeURL,
                CompanyId = companydc.CompanyId,
                CompanyType = companydc.CompanyType,
                BankIFSC = companydc.BankIFSC,
                ContactPersonName = companydc.ContactPersonName,
                CustomerAgreementDocId = companydc.CustomerAgreementDocId,
                CustomerAgreementURL = companydc.CustomerAgreementURL,
                GSTDocId = companydc.GSTDocId,
                GSTDocumentURL = companydc.GSTDocumentURL,
                GSTNo = companydc.GSTNo,
                IsSelfConfiguration = companydc.IsSelfConfiguration,
                LandingName = companydc.LandingName,
                LogoURL = companydc.LogoURL,
                MSMEDocId = companydc.MSMEDocId,
                MSMEDocumentURL = companydc.MSMEDocumentURL,
                PanNo = companydc.PanNo,
                WhitelistURL = companydc.WhitelistURL,
                PartnerList = list,
                CompanyStatus = companydc.CompanyStatus,
                AccountHolderName = companydc.AccountHolderName,
                BranchName = companydc.BranchName,
                FinancialLiaisonFirstName = companydc.financialLiaisonDetails.financialLiaisonFirstName,
                FinancialLiaisonLastName = companydc.financialLiaisonDetails.financialLiaisonLastName,
                FinancialLiaisonContactNo = companydc.financialLiaisonDetails.financialLiaisonContactNo,
                FinancialLiaisonEmailAddress = companydc.financialLiaisonDetails.financialLiaisonEmailAddress,
            });
            if (companyReply.Status)
            {
                if (companydc.CompanyAddress != null)
                {
                    var AddressReply = await _locationService.UpdateAddress(new UpdateCompanyAddressRequest
                    {
                        AddressId = companydc.CompanyAddress.AddressTypeId,
                        AddressLineOne = companydc.CompanyAddress.AddressLineOne,
                        AddressLineThree = companydc.CompanyAddress.AddressLineThree,
                        AddressLineTwo = companydc.CompanyAddress.AddressLineTwo,
                        CityId = companydc.CompanyAddress.CityId,
                        ZipCode = companydc.CompanyAddress.ZipCode
                    });
                    if (AddressReply.Status)
                    {
                        res.Status = AddressReply.Status;
                        res.Message = "Updated Successfully!";
                        //res.UpdatedCompanyDTO = companyReply.Response;
                        //res.UpdatedAddressDc = AddressReply.Response;
                    }
                    else
                    {
                        res.Status = AddressReply.Status;
                        res.Message = AddressReply.Message;
                    }
                }
                res.Status = companyReply.Status;
                res.Message = companyReply.Message;
            }
            else
            {
                res.Status = companyReply.Status;
                res.Message = companyReply.Message;
            }
            return res;
        }
        public async Task<GRPCReply<CompanyDetailDc>> GetCompany(long companyId)
        {
            GRPCReply<CompanyDetailDc> reply = new GRPCReply<CompanyDetailDc>();
            reply = await _companyService.GetCompany(new GRPCRequest<long> { Request = companyId });
            return reply;

        }

        public async Task<TemplatesReplyDTO> GetTemplateMasterAsync()
        {
            TemplatesReplyDTO reply = new TemplatesReplyDTO();
            reply.Message = "Data not found";
            reply.Status = false;
            List<TemplateMasterResponseDTO> templist = new List<TemplateMasterResponseDTO>();
            var LeadReply = await _leadService.GetTemplateMasterAsync();
            if (LeadReply.Status)
            {
                var list = LeadReply.Response.Select(x => new TemplateMasterResponseDTO
                {
                    DLTID = x.DLTID,
                    Template = x.Template,
                    TemplateCode = x.TemplateCode,
                    TemplateType = x.TemplateType,
                    TemplateId = x.TemplateId,
                    CreatedDate = x.CreatedDate,
                    IsActive = x.IsActive,
                    TemplateFor = "Lead"
                }).ToList();
                templist.AddRange(list);
            }
            var companyReply = await _companyService.GetTemplateMasterAsync();
            if (companyReply.Status)
            {
                var list = companyReply.Response.Select(x => new TemplateMasterResponseDTO
                {
                    DLTID = x.DLTID,
                    Template = x.Template,
                    TemplateCode = x.TemplateCode,
                    TemplateType = x.TemplateType,
                    TemplateId = x.TemplateId,
                    CreatedDate = x.CreatedDate,
                    IsActive = x.IsActive,
                    TemplateFor = "Company"
                }).ToList();
                templist.AddRange(list);
            }
            var ProductReply = await _productService.GetTemplateMasterAsync();
            if (ProductReply.Status)
            {
                var list = ProductReply.Response.Select(x => new TemplateMasterResponseDTO
                {
                    DLTID = x.DLTID,
                    Template = x.Template,
                    TemplateCode = x.TemplateCode,
                    TemplateType = x.TemplateType,
                    TemplateId = x.TemplateId,
                    CreatedDate = x.CreatedDate,
                    IsActive = x.IsActive,
                    TemplateFor = "Product"
                }).ToList();
                templist.AddRange(list);
            }
            var LoanAccountReply = await _loanAccountService.GetTemplateMasterAsync();
            if (LoanAccountReply.Status)
            {
                var list = LoanAccountReply.Response.Select(x => new TemplateMasterResponseDTO
                {
                    DLTID = x.DLTID,
                    Template = x.Template,
                    TemplateCode = x.TemplateCode,
                    TemplateType = x.TemplateType,
                    TemplateId = x.TemplateId,
                    CreatedDate = x.CreatedDate,
                    IsActive = x.IsActive,
                    TemplateFor = "LoanAccount"
                }).ToList();
                templist.AddRange(list);
            }
            if (templist != null && templist.Any())
            {
                reply.Response = templist.OrderByDescending(x => x.CreatedDate).ToList();
                reply.Message = "Data Found";
                reply.Status = true;
            }
            return reply;
        }

        public async Task<TemplateByIdResDTO> GetTemplateById(long Id, string type)
        {
            TemplateByIdResDTO reply = new TemplateByIdResDTO();
            reply.Status = false;
            reply.Message = "Data not exist";

            GRPCRequest<long> request = new GRPCRequest<long>();
            request.Request = Id;
            TemplateMasterResponseDTO templateMasterObj = new TemplateMasterResponseDTO();
            if (type == "Lead")
            {
                var LeadReply = await _leadService.GetTemplateById(request);
                if (LeadReply != null)
                {
                    templateMasterObj.Template = LeadReply.Response.Template;
                    templateMasterObj.DLTID = LeadReply.Response.DLTID;
                    templateMasterObj.TemplateType = LeadReply.Response.TemplateType;
                    templateMasterObj.TemplateCode = LeadReply.Response.TemplateCode;
                    templateMasterObj.IsActive = LeadReply.Response.IsActive;
                    templateMasterObj.TemplateId = LeadReply.Response.TemplateId;
                    templateMasterObj.TemplateFor = "Lead";
                }
            }
            if (type == "Company")
            {
                var companyReply = await _companyService.GetTemplateById(request);
                if (companyReply != null)
                {
                    templateMasterObj.Template = companyReply.Response.Template;
                    templateMasterObj.DLTID = companyReply.Response.DLTID;
                    templateMasterObj.TemplateType = companyReply.Response.TemplateType;
                    templateMasterObj.TemplateCode = companyReply.Response.TemplateCode;
                    templateMasterObj.IsActive = companyReply.Response.IsActive;
                    templateMasterObj.TemplateId = companyReply.Response.TemplateId;
                    templateMasterObj.TemplateFor = "Company";
                }
            }
            if (type == "Product")
            {
                var ProductReply = await _productService.GetTemplateById(request);
                if (ProductReply != null)
                {
                    templateMasterObj.Template = ProductReply.Response.Template;
                    templateMasterObj.DLTID = ProductReply.Response.DLTID;
                    templateMasterObj.TemplateType = ProductReply.Response.TemplateType;
                    templateMasterObj.TemplateCode = ProductReply.Response.TemplateCode;
                    templateMasterObj.IsActive = ProductReply.Response.IsActive;
                    templateMasterObj.TemplateId = ProductReply.Response.TemplateId;
                    templateMasterObj.TemplateFor = "Product";
                }
            }
            if (type == "LoanAccount")
            {
                var LoanAccountReply = await _loanAccountService.GetTemplateById(request);
                if (LoanAccountReply != null)
                {
                    templateMasterObj.Template = LoanAccountReply.Response.Template;
                    templateMasterObj.DLTID = LoanAccountReply.Response.DLTID;
                    templateMasterObj.TemplateType = LoanAccountReply.Response.TemplateType;
                    templateMasterObj.TemplateCode = LoanAccountReply.Response.TemplateCode;
                    templateMasterObj.IsActive = LoanAccountReply.Response.IsActive;
                    templateMasterObj.TemplateId = LoanAccountReply.Response.TemplateId;
                    templateMasterObj.TemplateFor = "Loan Account";
                }
            }
            if (templateMasterObj != null)
            {
                reply.Status = true;
                reply.Message = "Data Found";
                reply.Response = templateMasterObj;
            }
            return reply;
        }


        public async Task<GSTDetailReply> GetGSTDetails(string GSTNo)
        {
            GSTDetailReply gSTDetailReply = new GSTDetailReply();
            var companyReply = await _companyService.GetGSTDetail(new GRPCRequest<string> { Request = GSTNo });
            if (companyReply != null && companyReply.Status)
            {
                gSTDetailReply = companyReply.Response;
                await _locationService.AddCity(new GRPCRequest<AddCityRequest>
                {
                    Request = new AddCityRequest
                    {
                        CityName = companyReply.Response.City,
                        StateName = companyReply.Response.State
                    }
                });
            }
            return gSTDetailReply;
        }

        public async Task<GRPCReply<List<CompanySummaryReply>>> GetCompanySummary(GRPCRequest<CompanySummaryRequest> request)
        {
            var result = await _companyService.GetCompanySummary(request);
            return result;
        }

        public async Task<UserResponse> CheckCompanyAdminExist(CheckCompanyAdminDTO request)
        {
            UserResponse reply = new UserResponse
            {
                Status = false,
                Message = "Company Admin does Not Exists for this Company!!!"
            };
            UserIdsListResponseRequest compRequest = new UserIdsListResponseRequest { CompanyIds = new List<long> { request.CompanyId } };
            var companyIdsReply = await _companyService.GetUserList(compRequest);
            if (companyIdsReply.Status && companyIdsReply.UserCompanyDataList != null && companyIdsReply.UserCompanyDataList.Any(x => x.IsActive && x.UserId != request.UserId))
            {
                var req = new GRPCRequest<List<string>>
                {
                    Request = companyIdsReply.UserCompanyDataList.Where(x => x.IsActive && x.UserId != request.UserId).Select(x => x.UserId).ToList()
                };

                var checkAdmin = await _identityService.CheckCompanyAdminExist(req);
                if (checkAdmin.Status)
                {
                    reply.Status = true;
                    reply.Message = "CompanyAdmin Already exists for this Company!!!";
                }
            }
            return reply;
        }

        public async Task<List<LeadActivityHistoryDc>> LeadActivityHistory(long LeadId)
        {
            GRPCRequest<long> gRPCRequest = new GRPCRequest<long>();
            gRPCRequest.Request = LeadId;

            var res = await _leadService.LeadActivityHistory(gRPCRequest);

            UserListDetailsRequest userListDetailsRequest = new UserListDetailsRequest();
            if (res != null && res.Count > 0)
            {
                var userids = res.Where(x => x.UserId != null).Select(y => y.UserId).ToList();
                userListDetailsRequest.Take = userids.Count();
                userListDetailsRequest.userIds = userids;
                userListDetailsRequest.Skip = 0;
                userListDetailsRequest.keyword = "";
            }
            var userReply = new UserListDetailsResponse();
            if (userListDetailsRequest != null)
            {
                userReply = await _identityService.GetUserById(userListDetailsRequest);
            }
            if (userReply != null && userReply.UserListDetails != null)
            {
                var users = userReply.UserListDetails.Where(x => userListDetailsRequest.userIds.Contains(x.UserId)).ToList();

                if (users != null && res != null)
                {
                    foreach (var item in res)
                    {
                        var data = users.FirstOrDefault(x => x.UserId == item.UserId);
                        if (data != null && data.UserName != null)
                        {
                            item.UserName = data.UserName;
                        }
                    }
                }
            }
            return res;
        }

        public async Task<GRPCReply<List<LeadNbfcResponse>>> GetCompaniesByProduct(long productId)
        {
            GRPCReply<List<LeadNbfcResponse>> nbfcCompanies = new GRPCReply<List<LeadNbfcResponse>>();
            nbfcCompanies = await _companyService.GetAllNBFCCompany();
            if (nbfcCompanies.Status)
            {
                List<long> companyIds = new List<long>();
                companyIds = nbfcCompanies.Response.Select(x => x.NbfcId).ToList();
                if (companyIds != null && companyIds.Count > 0)
                {
                    var companyData = await _productService.GetNbfcCompaniesByProduct(new GetCompanyByProductRequestDc
                    {
                        ProductId = productId,
                        CompanyIds = companyIds
                    });

                    if (companyData != null && companyData.Response != null && companyData.Status)
                    {
                        //foreach (var item in companyData.Response)
                        //{
                        //    var response = nbfcCompanies.Response.Where(x => x.NbfcId == item).FirstOrDefault();
                        //    if (response != null)
                        //    {
                        //        nbfcData.Add(response);
                        //    }
                        //}
                        nbfcCompanies.Response = nbfcCompanies.Response.Where(x => companyData.Response.Any(y => y == x.NbfcId)).ToList();
                    }
                    if (nbfcCompanies.Response != null && nbfcCompanies.Response.Any())
                    {
                        nbfcCompanies.Status = true;
                        nbfcCompanies.Message = "Data Found";
                    }
                }
            }
            return nbfcCompanies;
        }

        public async Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetAnchorCompaniesByProduct(string productType)
        {
            GRPCReply<List<GetAllCompanyDetailDc>> allCompanies = new GRPCReply<List<GetAllCompanyDetailDc>>();
            allCompanies = await _companyService.GetAllCompanies();
            var prodId = await _productService.GetProductByProductType(new GRPCRequest<string> { Request = productType});
            if (allCompanies.Status && prodId.Status)
            {
                List<long> companyIds = new List<long>();
                companyIds = allCompanies.Response.Select(x => x.Id).ToList();
                if (companyIds != null && companyIds.Count > 0)
                {
                    var companyData = await _productService.GetAnchorCompaniesByProduct(new GetCompanyByProductRequestDc
                    {
                        ProductId = prodId.Response.Select(x => x.ProductId).FirstOrDefault(),
                        CompanyIds = companyIds
                    });

                    if (companyData != null && companyData.Response != null && companyData.Status)
                    {
                        allCompanies.Response = allCompanies.Response.Where(x => companyData.Response.Any(y => y == x.Id)).OrderBy(y => y.Id).ToList();
                    }
                    if (allCompanies.Response != null && allCompanies.Response.Any())
                    {
                        allCompanies.Status = true;
                        allCompanies.Message = "Data Found";
                    }
                }
            }
            return allCompanies;
        }
        public async Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetCompanyListForDropDown(string UserType, List<long> CompanyIds)
        {
            GRPCReply<List<GetAllCompanyDetailDc>> reply = new GRPCReply<List<GetAllCompanyDetailDc>> { Message = "Data not Found!!!" };
            var companyReply = await _companyService.GetAllCompanies();
            if (companyReply.Status)
            {
                if (UserType == UserTypeConstants.CompanyUser && CompanyIds != null && CompanyIds.Any())
                {
                    var company = companyReply.Response.FirstOrDefault(x => x.Id == CompanyIds.First());
                    if (company != null)
                    {
                        var productReply = await _productService.GetCompanyListByProduct(new GRPCRequest<GetCompanyListByProductRequest>
                        {
                            Request = new GetCompanyListByProductRequest
                            {
                                CompanyId = company.Id,
                                CompanyType = company.CompanyType
                            }
                        });
                        if (productReply.Status)
                        {
                            reply.Response = companyReply.Response.Where(x => productReply.Response != null && productReply.Response.Contains(x.Id)).ToList();
                            if (reply.Response != null && reply.Response.Any())
                            {
                                reply.Message = "Data Found";
                                reply.Status = true;
                            }
                        }
                    }
                }
                else
                {
                    reply = companyReply;
                }
            }
            return reply;
        }
    }
}
