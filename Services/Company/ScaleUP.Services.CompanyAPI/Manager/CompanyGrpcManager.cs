using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.Services.CompanyAPI.Persistence;
using ScaleUP.Services.CompanyModels.Master;
using ScaleUP.Services.CompanyModels;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using Microsoft.Data.SqlClient;
using ScaleUP.Services.CompanyAPI.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Claims;
using Nito.AsyncEx;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.CompanyAPI.Constants;
using ScaleUP.Services.CompanyDTO.Master;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using System.Linq;
using MassTransit.Initializers;
using MassTransit;
using ScaleUP.Global.Infrastructure.Constants;
using System.Collections.Generic;

namespace ScaleUP.Services.CompanyAPI.Manager
{
    public class CompanyGrpcManager
    {
        private readonly CompanyApplicationDbContext _context;
        public CompanyGrpcManager(CompanyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AddCompanyReply> AddCompany(AddCompanyDTO company)
        {
            AddCompanyReply companyResponse = new AddCompanyReply();
            bool IfExist = !string.IsNullOrEmpty(company.GSTNo) && await _context.Companies.AnyAsync(x => x.GSTNo == company.GSTNo && x.IsDeleted == false);
            if (IfExist)
            {
                companyResponse.Status = false;
                companyResponse.Message = "GST Already Exists!!!";
                return companyResponse;
            }
            IfExist = !company.IsDSA && !string.IsNullOrEmpty(company.BusinessContactEmail) && await _context.Companies.AnyAsync(x => x.BusinessContactEmail == company.BusinessContactEmail && x.IsDeleted == false);
            if (IfExist)
            {
                companyResponse.Status = false;
                companyResponse.Message = "Business Email Already Exists!!!";
                return companyResponse;
            }
            //IfExist = await _context.Companies.AnyAsync(x => x.BusinessContactNo == company.BusinessContactNo && x.IsDeleted == false);
            //if (IfExist)
            //{
            //    companyResponse.Status = false;
            //    companyResponse.Message = "BusinessContactNo Number Already Exists";
            //    return companyResponse;
            //}

            List<CompanyPartnerDetail> CompanyPartnerDetails = new List<CompanyPartnerDetail>();
            if (company.PartnerList != null && company.PartnerList.Any())
            {
                foreach (var partner in company.PartnerList)
                {
                    CompanyPartnerDetail _CompanyPartnerDetail = new CompanyPartnerDetail
                    {
                        PartnerName = partner.PartnerName,
                        MobileNo = partner.MobileNo,
                        CompanyId = 0,
                        Created = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    };
                    CompanyPartnerDetails.Add(_CompanyPartnerDetail);
                }
            }
            Companies _company = new Companies
            {
                GSTNo = company.GSTNo,
                PanNo = company.PanNo,
                BusinessName = company.BusinessName,
                LendingName = company.LandingName,
                BusinessContactEmail = company.BusinessContactEmail,
                BusinessContactNo = company.BusinessContactNo,
                APIKey = company.APIKey,
                APISecretKey = company.APISecretKey,
                LogoURL = company.LogoURL,
                BusinessHelpline = company.BusinessHelpline ?? "",
                BusinessTypeId = company.BusinessTypeId,
                IsDefault = false,
                BankAccountNumber = company.BankAccountNumber,
                BankIFSC = company.BankIFSC,
                BankName = company.BankName,
                BusinessPanURL = company.BusinessPanURL,
                CancelChequeURL = company.CancelChequeURL ?? "",
                WhitelistURL = company.WhitelistURL,
                BusinessPanDocId = company.BusinessPanDocId,
                CancelChequeDocId = company.CancelChequeDocId,
                CompanyCode = company.CompanyCode,
                CompanyType = company.CompanyType,
                GSTDocId = company.GSTDocId,
                GSTDocumentURL = company.GSTDocumentURL,
                MSMEDocId = company.MSMEDocId,
                MSMEDocumentURL = company.MSMEDocumentURL,
                Created = DateTime.Now,
                IsActive = company.CompanyStatus,
                IsDeleted = false,
                IsSelfConfiguration = company.IsSelfConfiguration,
                ContactPersonName = company.ContactPersonName ?? "",
                CompanyPartnerDetails = CompanyPartnerDetails,
                AccountType = company.AccountType,
                PanDocId = company.PanDocId,
                PanURL = company.PanURL,
                CustomerAgreementDocId = company.CustomerAgreementDocId,
                CustomerAgreementURL = company.CustomerAgreementURL,
                AgreementURL = company.AgreementURL,
                AgreementDocId = company.AgreementDocId,
                IsDSA = company.IsDSA,
            };

            _context.Companies.Add(_company);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                companyResponse.Status = true;
                companyResponse.Message = "Company Added Successfully";
                companyResponse.CompanyId = _company.Id;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Issue during company adding.";
            }
            return companyResponse;

        }

        public async Task<CompanyLocationReply> CreateCompanyLocation(BuildingBlocks.GRPC.Contracts.Company.DataContracts.CompanyLocationDTO request)
        {
            CompanyLocationReply companyResponse = new CompanyLocationReply();
            companyResponse.Status = false;
            companyResponse.Message = "Issue during company location adding.";
            CompanyLocation _companyloc = new CompanyLocation
            {
                CompanyId = request.CompanyId,
                LocationId = request.LocationId,
                IsActive = true,
                IsDeleted = false
            };

            _context.CompanyLocations.Add(_companyloc);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                companyResponse.Status = true;
                companyResponse.Message = "Company Location added successfully.";
                companyResponse.CompanyLocationId = _companyloc.Id;
            }
            return companyResponse;
        }

        public async Task<CompanyUserReply> CreateCompanyUser(CompanyUserRequest request)
        {
            CompanyUserReply companyUserReply = new CompanyUserReply();
            companyUserReply.Status = false;
            companyUserReply.Message = "Issue during creating company user.";
            CompanyUsers _companyuser = new CompanyUsers
            {
                CompanyId = request.CompanyId,
                UserId = request.UserId,
                IsActive = true,
                IsDeleted = false
            };

            _context.CompanyUsers.Add(_companyuser);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                companyUserReply.Status = true;
                companyUserReply.Message = "Company user added successfully.";
            }
            return companyUserReply;
        }

        public async Task<CompanyListReply> GetCompanyList(CompanyListRequest request)
        {
            CompanyListReply companyResponse = new CompanyListReply();
            companyResponse.Status = false;

            var query = _context.Companies.Where(x => !x.IsDefault && !x.IsDeleted && x.CompanyType == request.CompanyType &&
            (string.IsNullOrEmpty(request.keyword) || x.BusinessName.Contains(request.keyword) || x.GSTNo.Contains(request.keyword)
            || (x.CompanyCode != null && x.CompanyCode.Contains(request.keyword)))).Include(z => z.CompanyLocations).Include(a => a.CompanyPartnerDetails);

            var totalRecords = await query.CountAsync();
            var gst = await _context.GSTMasters.Where(x => x.IsActive && !x.IsDeleted).Select(x => x.GSTRate).FirstOrDefaultAsync();
            var list = await query.Select(y => new companyList
            {
                Id = y.Id,
                businessName = y.BusinessName,
                landingName = y.LendingName,
                gst = y.GSTNo,
                CompanyType = y.CompanyType,
                Email = y.BusinessContactEmail,
                MobileNumber = y.BusinessContactNo,
                Pan = y.PanNo,
                CompanyCode = y.CompanyCode,
                locationId = y.CompanyLocations.Select(a => a.LocationId).ToList(),
                PartnerDetailsList = y.CompanyPartnerDetails.Select(a => new PartnerDetailsList
                {
                    PartnerCompanyId = a.CompanyId,
                    PartnerMobileNo = a.MobileNo,
                    PartnerName = a.PartnerName
                }).ToList(),
                IsDefault = y.IsDefault,
                GstRate = gst,
                IsActive = y.IsActive,
                CancelChequeURL = y.CancelChequeURL,
                CancelChequeDocId = y.CancelChequeDocId,
                BusinessPanDocId = y.BusinessPanDocId,
                BusinessPanURL = y.BusinessPanURL,
                BusinessTypeId = y.BusinessTypeId,
                totalRecords = totalRecords
            }).OrderByDescending(x => x.Id).Skip(request.Skip).Take(request.Take).ToListAsync();

            if (list != null && list.Any())
            {
                companyResponse.Status = true;
                companyResponse.CompanyList = list;
            }
            return companyResponse;
        }
        public async Task<UserIdsListResponse> GetUserList(UserIdsListResponseRequest compRequest)
        {
            UserIdsListResponse response = new UserIdsListResponse();
            response.Status = false;

            var query = _context.CompanyUsers
                .Where(x => !x.IsDeleted && (compRequest.CompanyIds == null || ((compRequest.CompanyIds != null && compRequest.CompanyIds.Any() && compRequest.CompanyIds.Contains(x.CompanyId)) || !compRequest.CompanyIds.Any())));

            var Userdata = await query
                           .Select(x => new UserCompanyData
                           {
                               UserId = x.UserId,
                               CompanyId = x.CompanyId,
                               IsActive = x.IsActive,
                               CompanyName = x.Companies.BusinessName,
                               CreatedDate = x.Created

                           }).OrderByDescending(x => x.CreatedDate)
                          .ToListAsync();

            if (Userdata != null && Userdata.Any())
            {
                response.Status = true;
                response.UserCompanyDataList = Userdata;
            }

            return response;
        }

        public async Task<AddCompanyUserMappingReply> AddUpdateCompanyUserMapping(AddCompanyUserMappingRequest request)
        {
            AddCompanyUserMappingReply addCompanyUserMappingReply = new AddCompanyUserMappingReply();

            var existingMappings = await _context.CompanyUsers.Where(x => x.UserId == request.UserId && !x.IsDeleted).ToListAsync();
            if (existingMappings != null && existingMappings.Any())
            {
                foreach (var item in existingMappings)
                {
                    item.IsDeleted = true;
                    item.IsActive = false;
                    _context.Entry(item).State = EntityState.Modified;
                }
            }
            CompanyUsers companyUsers = new CompanyUsers
            {
                CompanyId = request.CompanyId,
                UserId = request.UserId,
                IsActive = true,//existingMappings != null && existingMappings.Any() ? existingMappings.FirstOrDefault().IsActive : true,
                IsDeleted = false,
                Created = DateTime.Now
            };
            _context.Add(companyUsers);

            var rowChanged = await _context.SaveChangesAsync();
            if (rowChanged > 0)
            {
                addCompanyUserMappingReply.Status = true;
                addCompanyUserMappingReply.Message = "Created Successfully";
            }
            else
            {
                addCompanyUserMappingReply.Status = false;
                addCompanyUserMappingReply.Message = "Failed To Create Mapping";
            }
            return addCompanyUserMappingReply;
        }


        public async Task<GRPCReply<long>> GetFinTechCompany()
        {

            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            var finTechCompanId = (await _context.Companies.FirstOrDefaultAsync(x => x.IsDefault && x.IsActive && !x.IsDeleted))?.Id;
            if (finTechCompanId.HasValue)
            {
                gRPCReply.Message = "Company Id get successfully.";
                gRPCReply.Status = true;
                gRPCReply.Response = finTechCompanId.Value;
            }
            else
            {
                gRPCReply.Message = "Company not found.";
                gRPCReply.Status = false;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<LeadNbfcResponse>>> GetAllNBFCCompany()
        {
            GRPCReply<List<LeadNbfcResponse>> reply = new GRPCReply<List<LeadNbfcResponse>>();
            var gst = await GetLatestGSTRate(GstConstant.Gst18);

            reply.Response = await _context.Companies.Where(x => (x.CompanyType == "NBFC" || x.IsDefault) && !string.IsNullOrEmpty(x.IdentificationCode) && x.IsActive && !x.IsDeleted).Select(x => new LeadNbfcResponse
            {
                NbfcId = x.Id,
                CompanyIdentificationCode = x.IdentificationCode,
                GST = (gst != null && gst.Response > 0) ? gst.Response : 0,
                NbfcCompanyName = x.BusinessName
            }).ToListAsync();
            reply.Status = true;
            return reply;

        }



        public async Task<GRPCReply<List<long>>> GetAllConfigNBFCCompany(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<long>> reply = new GRPCReply<List<long>>();
            reply.Response = await _context.Companies.Where(x => request.Request.Contains(x.Id) && x.IsActive && !x.IsDeleted && !string.IsNullOrEmpty(x.IdentificationCode)).Select(x => x.Id).ToListAsync();
            reply.Status = true;
            return reply;

        }

        public async Task<GRPCReply<List<long>>> GetDefaultConfigNBFCCompany(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<long>> reply = new GRPCReply<List<long>>();
            reply.Response = await _context.Companies.Where(x => request.Request.Contains(x.Id) && x.IsDefault).Select(x => x.Id).ToListAsync();
            reply.Status = true;
            return reply;

        }



        public async Task<GRPCReply<string>> GetCurrentNumber(GRPCRequest<GetEntityCodeRequest> request)
        {
            GRPCReply<string> res = new GRPCReply<string>();
            res.Status = false;

            var entity_name = new SqlParameter("entityname", request.Request.EntityName);
            var company_type = new SqlParameter("companytype", request.Request.CompanyType);
            var result = _context.Database.SqlQueryRaw<string>("exec spGetCurrentNumber @entityname,@companytype", entity_name, company_type).AsEnumerable().FirstOrDefault();
            if (result != null)
            {
                res.Status = true;
                res.Response = result;
            }
            return res;
        }



        public async Task<GRPCReply<List<long>>> GetCompanyLocationById(GRPCRequest<long> request)
        {
            GRPCReply<List<long>> response = new GRPCReply<List<long>>();
            response.Status = false;
            response.Message = "Company location data not exists.";
            var _companyloc = await _context.CompanyLocations.Where(x => x.CompanyId == request.Request && !x.IsDeleted && x.IsActive).Select(x => x.LocationId).ToListAsync();
            if (_companyloc != null && _companyloc.Any())
            {
                response.Status = true;
                response.Message = "Company location data found.";
                //response.Response = [.. _companyloc];
            }
            else
                response.Response = new List<long>();

            return response;
        }
        public async Task<GRPCReply<double>> GetLatestGSTRate(string Gstcode)
        {
            GRPCReply<double> gRPCReply = new GRPCReply<double>();
            var gstRate = await _context.GSTMasters.FirstOrDefaultAsync(x => x.Code == Gstcode && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActive && !x.IsDeleted);
            if (gstRate != null)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Gst Found";
                gRPCReply.Response = gstRate.GSTRate;
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Gst Not Found";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<GstList>>> GetLatestGSTRateList(GRPCRequest<List<string>> Gstcodes)
        {
            GRPCReply<List<GstList>> reply = new GRPCReply<List<GstList>> { Message = "GST Not Found" };
            if (Gstcodes != null && Gstcodes.Request != null)
            {
                reply.Response = await _context.GSTMasters.Where(x => Gstcodes.Request.Contains(x.Code) && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now
                                    && x.IsActive && !x.IsDeleted).Select(x => new GstList
                                    {
                                        Code = x.Code,
                                        Value = x.GSTRate
                                    }).ToListAsync();

                if (reply.Response != null && reply.Response.Any())
                {
                    reply.Status = true;
                    reply.Message = "GST Found";
                }
            }
            return reply;
        }

        public async Task<GRPCReply<CompanyDetail>> GetCompanyLogo(GRPCRequest<long> request)
        {
            GRPCReply<CompanyDetail> gRPCReply = new GRPCReply<CompanyDetail>();
            var companies = await _context.Companies.FirstOrDefaultAsync(x => x.Id == request.Request && x.IsActive && !x.IsDeleted);
            var gstRate = await _context.GSTMasters.FirstOrDefaultAsync(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActive && !x.IsDeleted);

            if (companies != null && gstRate != null)
            {
                CompanyDetail obj = new CompanyDetail();
                obj.LogoURL = companies.LogoURL;
                obj.GstRate = gstRate.GSTRate;

                gRPCReply.Status = true;
                gRPCReply.Response = obj;
                gRPCReply.Message = "LogoURL Found";
            }
            else
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "LogoURL Not Found";
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<List<AuditLogReply>>> GetAuditLogs(GRPCRequest<AuditLogRequest> request)
        {
            GRPCReply<List<AuditLogReply>> gRPCReply = new GRPCReply<List<AuditLogReply>>();
            AuditLogHelper auditLogHelper = new AuditLogHelper(_context);
            var auditLogs = await auditLogHelper.GetAuditLogs(request.Request.EntityId, request.Request.EntityName, request.Request.Skip, request.Request.Take);
            if (auditLogs != null && auditLogs.Any())
            {
                gRPCReply.Status = true;
                gRPCReply.Response = auditLogs.Select(x => new AuditLogReply
                {
                    ModifiedDate = x.Timestamp,
                    Changes = x.Changes,
                    ModifiedBy = x.UserId,
                    TotalRecords = x.TotalRecords,
                    ActionType = x.Action
                }).ToList();
                gRPCReply.Message = "Data Found";
            }
            else
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Not Found";
            }
            return gRPCReply;
        }
        public async Task<GRPCReply<List<CompanyIdentificationCodeDc>>> GetCompanyIdentificationCode(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<CompanyIdentificationCodeDc>> gRPCReply = new GRPCReply<List<CompanyIdentificationCodeDc>>();
            var identificationCodes = await _context.Companies.Where(x => x.IsActive && !x.IsDeleted).Select(x => new
            CompanyIdentificationCodeDc
            {
                CompanyId = x.Id,
                IdentificationCode = x.IdentificationCode ?? ""
            }).ToListAsync();
            if (identificationCodes != null && identificationCodes.Any())
            {
                gRPCReply.Status = true;
                gRPCReply.Response = identificationCodes;
                gRPCReply.Message = "Data Found";
            }
            else
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Not Found";
            }
            return gRPCReply;
        }


        public async Task<GRPCReply<List<NBFCCompanyReply>>> GetNBFCCompanyById(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<NBFCCompanyReply>> reply = new GRPCReply<List<NBFCCompanyReply>>();
            var companys = await _context.Companies.Where(x => request.Request.Contains(x.Id) && x.IsActive && !x.IsDeleted)
           .Select(x => new NBFCCompanyReply
           {
               BusinessName = x.BusinessName,
               CompanyId = x.Id,
               IdentificationCode = x.IdentificationCode,
               LendingName = x.LendingName
           }).ToListAsync();
            if (companys != null)
            {
                reply.Status = true;
                reply.Response = companys;
            }
            return reply;

        }

        public async Task<GRPCReply<CompanyAddressAndDetailsResponse>> GetCompanyAddressAndDetails(CompanyAddressAndDetailsReq request)
        {
            GRPCReply<CompanyAddressAndDetailsResponse> reply = new GRPCReply<CompanyAddressAndDetailsResponse>();

            CompanyAddressAndDetailsResponse? _company = await _context.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted).Include(z => z.CompanyPartnerDetails)
                .Select(x => new CompanyAddressAndDetailsResponse
                {
                    APIKey = x.APIKey,
                    APISecretKey = x.APISecretKey,
                    BusinessContactEmail = x.BusinessContactEmail,
                    BusinessContactNo = x.BusinessContactNo,
                    BusinessHelpline = x.BusinessHelpline,
                    BusinessName = x.BusinessName,
                    BusinessTypeId = x.BusinessTypeId,
                    CompanyType = x.CompanyType,
                    GSTNo = x.GSTNo,
                    BankAccountNumber = x.BankAccountNumber,
                    BankIFSC = x.BankIFSC,
                    BankName = x.BankName,
                    BusinessPanURL = x.BusinessPanURL,
                    CancelChequeURL = x.CancelChequeURL,
                    WhitelistIRL = x.WhitelistURL,
                    IsDefault = x.IsDefault,
                    LandingName = x.LendingName,
                    LogoURL = x.LogoURL,
                    PanNo = x.PanNo,
                    CompanyCode = x.CompanyCode,
                    BusinessPanDocId = x.BusinessPanDocId,
                    CancelChequeDocId = x.CancelChequeDocId,
                    IsSelfConfiguration = x.IsSelfConfiguration,
                    ContactPersonName = x.ContactPersonName,
                    GSTDocId = x.GSTDocId,
                    GSTDocumentURL = x.GSTDocumentURL,
                    MSMEDocId = x.MSMEDocId,
                    MSMEDocumentURL = x.MSMEDocumentURL,
                    CompanyStatus = x.IsActive,
                    AccountType = x.AccountType,
                    AgreementDocId = x.AgreementDocId,
                    AgreementURL = x.AgreementURL,
                    CustomerAgreementDocId = x.CustomerAgreementDocId,
                    CustomerAgreementURL = x.CustomerAgreementURL,
                    PanDocId = x.PanDocId,
                    PanURL = x.PanURL,
                    AccountHolderName = x.AccountHolderName,
                    BranchName = x.BranchName,
                    CompanyPartnerDc = x.CompanyPartnerDetails.Where(x => x.IsActive && !x.IsDeleted).Select(z => new PartnerListDTO { MobileNo = z.MobileNo, PartnerId = z.Id, PartnerName = z.PartnerName }).ToList()
                }
                ).FirstOrDefaultAsync();
            if (_company != null)
            {
                var _companyloc = await _context.CompanyLocations.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsActive).Select(x => x.LocationId).ToListAsync();

                if (_companyloc != null && _companyloc.Any())
                {
                    _company.companyLocationIds = _companyloc;
                }

                var _financialLiaisonDetail = await _context.FinancialLiaisonDetails.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsActive).FirstOrDefaultAsync();
                if (_financialLiaisonDetail != null)
                {
                    _company.FinancialLiaisonDetailDTO = new FinancialLiaisonDetailDTO
                    {
                        FirstName = _financialLiaisonDetail.FirstName,
                        ContactNo = _financialLiaisonDetail.ContactNo,
                        EmailAddress = _financialLiaisonDetail.EmailAddress,
                        LastName = _financialLiaisonDetail.LastName
                    };
                }

                reply.Status = true;
                reply.Message = "Data Found.";
                reply.Response = _company;
            }
            else
            {
                reply.Status = false;
                reply.Message = "Company not exists.";
            }
            return reply;
        }

        public async Task<GRPCReply<CompanyDataDc>> GetCompanyDataById(GRPCRequest<long> request)
        {
            GRPCReply<CompanyDataDc> reply = new GRPCReply<CompanyDataDc>();
            var company = await _context.Companies.FirstOrDefaultAsync(x => request.Request == x.Id && x.IsActive && !x.IsDeleted);
            if (company != null)
            {
                reply.Status = true;
                reply.Response = new CompanyDataDc { CompanyName = !string.IsNullOrEmpty(company.LendingName) ? company.LendingName : company.BusinessName, IdentificationCode = company.IdentificationCode, LogoURL = company.LogoURL };
            }
            return reply;
        }

        public async Task<GRPCReply<CompanyDataDc>> GetCompanyDataByCode(GRPCRequest<string> request)
        {
            GRPCReply<CompanyDataDc> reply = new GRPCReply<CompanyDataDc>();
            var company = await _context.Companies.FirstOrDefaultAsync(x => request.Request == x.CompanyCode && x.IsActive && !x.IsDeleted);
            if (company != null)
            {
                reply.Status = true;
                reply.Response = new CompanyDataDc { CompanyId = company.Id, CompanyName = !string.IsNullOrEmpty(company.LendingName) ? company.LendingName : company.BusinessName, IdentificationCode = company.IdentificationCode, LogoURL = company.LogoURL };
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GetCompanyCodeById(GRPCRequest<long> request)
        {
            GRPCReply<string> reply = new GRPCReply<string>();
            var company = (await _context.Companies.FirstOrDefaultAsync(x => x.Id == request.Request && x.IsActive && !x.IsDeleted))?.CompanyCode;
            if (!string.IsNullOrEmpty(company))
            {
                reply.Status = true;
                reply.Response = company;
            }
            return reply;
        }
        public async Task<GRPCReply<UpdateCompanyRequest>> UpdateCompanyAsync(UpdateCompanyRequest company)
        {
            GRPCReply<UpdateCompanyRequest> companyResponse = new GRPCReply<UpdateCompanyRequest>();

            Companies? _company = await _context.Companies.FirstOrDefaultAsync(x => x.Id == company.CompanyId && !x.IsDeleted);
            FinancialLiaisonDetails? _financialLiaisonDetails = await _context.FinancialLiaisonDetails.FirstOrDefaultAsync(x => x.CompanyId == company.CompanyId && !x.IsDeleted);
            if (_company != null)
            {
                bool IfExist = await _context.Companies.AnyAsync(x => x.BusinessContactEmail == company.BusinessContactEmail && x.Id != company.CompanyId && x.IsDeleted == false);
                if (IfExist)
                {
                    companyResponse.Status = false;
                    companyResponse.Message = "Business Email Already Exists!!!";
                    return companyResponse;
                }
                _company.BankName = company.BankName;
                _company.BankAccountNumber = company.BankAccountNumber;
                _company.BankIFSC = company.BankIFSC;
                _company.CancelChequeDocId = company.CancelChequeDocId;
                _company.CancelChequeURL = company.CancelChequeURL;
                _company.ContactPersonName = company.ContactPersonName;
                _company.BusinessContactEmail = company.BusinessContactEmail;
                _company.BusinessContactNo = company.BusinessContactNo ?? "";
                _company.BusinessHelpline = company.BusinessHelpline ?? "";
                _company.LogoURL = company.LogoURL;
                _company.IsActive = company.CompanyStatus;
                _company.AccountType = company.AccountType;
                _company.AccountHolderName = company.AccountHolderName;
                _company.BranchName = company.BranchName;
                _context.Entry(_company).State = EntityState.Modified;

                List<CompanyPartnerDetail> CompanyPartnerDetails = new List<CompanyPartnerDetail>();

                if (company.PartnerList != null && company.PartnerList.Any())
                {
                    foreach (var partner in company.PartnerList)
                    {
                        var ExistpartnerDetail = await _context.CompanyPartnerDetails.FirstOrDefaultAsync(x => x.Id == partner.PartnerId);
                        if (ExistpartnerDetail != null)
                        {
                            ExistpartnerDetail.PartnerName = partner.PartnerName;
                            ExistpartnerDetail.MobileNo = partner.MobileNo;
                            var isUpdatePartner = _context.Entry(ExistpartnerDetail).State = EntityState.Modified;
                        }
                        else
                        {
                            CompanyPartnerDetail _CompanyPartnerDetail = new CompanyPartnerDetail
                            {
                                PartnerName = partner.PartnerName,
                                MobileNo = partner.MobileNo,
                                CompanyId = _company.Id,
                                Created = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false
                            };
                            CompanyPartnerDetails.Add(_CompanyPartnerDetail);
                        }
                    }
                }
                if (_financialLiaisonDetails != null)
                {
                    _financialLiaisonDetails.FirstName = company.FinancialLiaisonFirstName;
                    _financialLiaisonDetails.LastName = company.FinancialLiaisonLastName;
                    _financialLiaisonDetails.ContactNo = company.FinancialLiaisonContactNo;
                    _financialLiaisonDetails.EmailAddress = company.FinancialLiaisonEmailAddress;
                    _context.Entry(_company).State = EntityState.Modified;
                }
                else
                {
                    FinancialLiaisonDetails _financialLiaison = new FinancialLiaisonDetails
                    {
                        FirstName = company.FinancialLiaisonFirstName,
                        LastName = company.FinancialLiaisonLastName,
                        ContactNo = company.FinancialLiaisonContactNo,
                        EmailAddress = company.FinancialLiaisonEmailAddress,
                        CompanyId = _company.Id,
                        Created = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                    };
                    _context.FinancialLiaisonDetails.Add(_financialLiaison);
                    companyResponse.Message = "Financial Liaison Details Added Successfully";
                }
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    companyResponse.Status = true;
                    companyResponse.Message = "Company updated successfully.";
                    companyResponse.Response = company;
                }
                else
                {
                    companyResponse.Status = false;
                    companyResponse.Message = "Issue during company updating.";
                }
            }
            else
            {
                companyResponse.Message = "Company not found.";
            }
            return companyResponse;
        }

        public async Task<GRPCReply<CompanyDetailDc>> GetCompany(GRPCRequest<long> request)
        {
            GRPCReply<CompanyDetailDc> reply = new GRPCReply<CompanyDetailDc>();
            var company = await _context.Companies.FirstOrDefaultAsync(x => request.Request == x.Id && x.IsActive && !x.IsDeleted);
            if (company != null)
            {
                reply.Status = true;
                reply.Response = new CompanyDetailDc
                {
                    CompanyName = company.BusinessName,
                    CompanyId = company.Id,
                    IdentificationCode = company.IdentificationCode
                };
            }
            return reply;
        }

        public async Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync()
        {
            GRPCReply<List<GetTemplateMasterListResponseDc>> reply = new GRPCReply<List<GetTemplateMasterListResponseDc>>();
            var leadTemplateList = (await _context.CompanyTemplateMasters.Where(x => !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
            {
                DLTID = x.DLTID,
                Template = x.Template,
                TemplateCode = x.TemplateCode,
                TemplateType = x.TemplateType,
                TemplateId = x.Id,
                IsActive = x.IsActive,
                CreatedDate = x.Created
            }).ToListAsync());
            if (leadTemplateList != null && leadTemplateList.Any())
            {
                reply.Response = leadTemplateList;
                reply.Status = true;
                reply.Message = "data found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "data not found";
            }
            return reply;
        }

        public async Task<GRPCReply<GetTemplateMasterListResponseDc>> GetTemplateById(GRPCRequest<long> request)
        {
            GRPCReply<GetTemplateMasterListResponseDc> reply = new GRPCReply<GetTemplateMasterListResponseDc>();
            var leadTemplate = (await _context.CompanyTemplateMasters.Where(x => x.Id == request.Request && !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
            {
                DLTID = x.DLTID,
                Template = x.Template,
                TemplateCode = x.TemplateCode,
                TemplateType = x.TemplateType,
                TemplateId = x.Id,
                IsActive = x.IsActive
            }).FirstOrDefaultAsync());
            if (leadTemplate != null)
            {
                reply.Response = leadTemplate;
                reply.Status = true;
                reply.Message = "data found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "data not found";
            }
            return reply;
        }

        public async Task<GRPCReply<GSTDetailReply>> GetGSTDetail(GRPCRequest<string> GSTNO)
        {
            GRPCReply<GSTDetailReply> gSTDetailDTO = new GRPCReply<GSTDetailReply>
            {
                Status = true,
                Response = new GSTDetailReply()
            };
            var dbGST = await _context.GSTverifiedRequests.FirstOrDefaultAsync(x => x.RefNo == GSTNO.Request);
            if (dbGST == null)
            {

                var GSTDetail = await GSTDetailHelper.GetGSTVerify(GSTNO.Request, EnvironmentConstants.GSTURL);
                if (GSTDetail != null)
                {
                    gSTDetailDTO.Response.status = GSTDetail.Status;
                    gSTDetailDTO.Message = GSTDetail.Message;
                    if (GSTDetail.custverify != null)
                    {
                        gSTDetailDTO.Response.Name = GSTDetail.custverify.Name ?? "";
                        gSTDetailDTO.Response.AddressLine1 = string.Format("{0}, {1}, {2}", GSTDetail.custverify.HomeNo, GSTDetail.custverify.HomeName, GSTDetail.custverify.ShippingAddress);
                        gSTDetailDTO.Response.State = GSTDetail.custverify.State ?? "";
                        gSTDetailDTO.Response.City = GSTDetail.custverify.City ?? "";
                        gSTDetailDTO.Response.ShopName = GSTDetail.custverify.ShopName ?? "";
                        gSTDetailDTO.Response.Zipcode = GSTDetail.custverify.Zipcode ?? "";

                        dbGST = new GSTverifiedRequest
                        {
                            Active = GSTDetail.custverify.Active,
                            City = GSTDetail.custverify.City,
                            Citycode = GSTDetail.custverify.Citycode,
                            CreateDate = DateTime.Now,
                            CustomerBusiness = GSTDetail.custverify.CustomerBusiness,
                            Delete = GSTDetail.custverify.Delete,
                            HomeName = GSTDetail.custverify.HomeName,
                            HomeNo = GSTDetail.custverify.HomeNo,
                            LastUpdate = GSTDetail.custverify.LastUpdate,
                            lat = GSTDetail.custverify.lat,
                            lg = GSTDetail.custverify.lg,
                            Message = GSTDetail.custverify.Message,
                            Name = GSTDetail.custverify.Name,
                            PlotNo = GSTDetail.custverify.PlotNo,
                            RefNo = GSTDetail.custverify.RefNo,
                            RegisterDate = GSTDetail.custverify.RegisterDate,
                            RequestPath = GSTDetail.custverify.RequestPath,
                            ShippingAddress = GSTDetail.custverify.ShippingAddress,
                            ShopName = GSTDetail.custverify.ShopName,
                            State = GSTDetail.custverify.State,
                            UpdateDate = GSTDetail.custverify.UpdateDate,
                            Zipcode = GSTDetail.custverify.Zipcode
                        };
                        await _context.GSTverifiedRequests.AddAsync(dbGST);
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                gSTDetailDTO.Response.status = dbGST.Active == "Active";
                gSTDetailDTO.Response.Message = dbGST.Active == "Active" ? "Customer GST Number Is Verify Successfully." : ("Customer GST Number Is " + dbGST.Active);
                gSTDetailDTO.Response.Name = dbGST.Name ?? "";
                gSTDetailDTO.Response.AddressLine1 = string.Format("{0}, {1}, {2}", dbGST.HomeNo, dbGST.HomeName, dbGST.ShippingAddress); ;
                gSTDetailDTO.Response.State = dbGST.State ?? "";
                gSTDetailDTO.Response.City = dbGST.City ?? "";
                gSTDetailDTO.Response.ShopName = dbGST.ShopName ?? "";
                gSTDetailDTO.Response.Zipcode = dbGST.Zipcode ?? "";
            }
            return gSTDetailDTO;
        }

        public async Task<GRPCReply<List<CompanySummaryReply>>> GetCompanySummary(GRPCRequest<CompanySummaryRequest> request)
        {
            GRPCReply<List<CompanySummaryReply>> gRPCReply = new GRPCReply<List<CompanySummaryReply>>();
            List<CompanySummaryReply> companySummaryReply = new List<CompanySummaryReply>();
            var companyType = _context.Companies.Where(x => x.CompanyType == "Anchor" && x.IsActive && !x.IsDeleted).OrderBy(x => x.BusinessName).
                Skip(request.Request.Skip).Take(request.Request.Take).ToList();
            if (companyType != null && companyType.Count > 0)
            {
                foreach (var item in companyType)
                {
                    companySummaryReply.Add(new CompanySummaryReply
                    {
                        BusinessName = item.BusinessName,
                        CompanyId = item.Id,
                        LogoURL = item.LogoURL
                    });

                }
                gRPCReply.Status = true;
                gRPCReply.Message = "Successfully";
                gRPCReply.Response = companySummaryReply;

            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Failed";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<GetCustomerDetailReply>> GetCustomerDetails()
        {
            GRPCReply<GetCustomerDetailReply> gRPCReply = new GRPCReply<GetCustomerDetailReply>();
            var res = await _context.Companies.Where(p => p.IsActive && !p.IsDeleted && p.IsDefault)
                .Select(x => new { x.BusinessContactNo, x.BusinessContactEmail }).FirstOrDefaultAsync();
            if (res != null)
            {
                GetCustomerDetailReply data = new GetCustomerDetailReply();
                data.CustomerCareEmail = res.BusinessContactEmail;
                data.CustomerCareMobile = res.BusinessContactNo;
                gRPCReply.Response = data;
                gRPCReply.Status = true;
                gRPCReply.Message = "SuccesFully Found Data!!";
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Data Not Found!!";
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> GetCompanyShortName(GRPCRequest<long> request)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            var company = await _context.Companies.Where(p => p.Id == request.Request)
                .Select(x => new { x.BusinessName, x.LendingName }).FirstOrDefaultAsync();
            if (company != null)
            {
                gRPCReply.Status = true;
                gRPCReply.Response = !string.IsNullOrEmpty(company.LendingName) ? company.LendingName : company.BusinessName;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<CompanyBankDetailsDc>>> GetCompanyBankDetailsById(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<CompanyBankDetailsDc>> reply = new GRPCReply<List<CompanyBankDetailsDc>>();
            var company = await _context.Companies.Where(x => request.Request.Contains(x.Id) && x.IsActive && !x.IsDeleted).Select(y =>
            new CompanyBankDetailsDc { bankName = y.BankName, bankAccountNumber = y.BankAccountNumber, CompanyId = y.Id }).ToListAsync();
            if (company != null)
            {
                reply.Status = true;
                reply.Response = company;
                //reply.Response = new CompanyBankDetailsDc { bankName = company.BankName , bankAccountNumber = company.BankAccountNumber, CompanyId = company.Id};
            }
            return reply;
        }
        public async Task<GRPCReply<List<NBFCCompanyNameResponseDC>>> GetNBFCCompanyName()
        {
            GRPCReply<List<NBFCCompanyNameResponseDC>> reply = new GRPCReply<List<NBFCCompanyNameResponseDC>>();
            var company = await _context.Companies.Where(x => x.IsActive && !x.IsDeleted && x.CompanyType == "NBFC").Select(y =>
            new NBFCCompanyNameResponseDC { NBFCCompanyId = y.Id, NBFCCompanyName = y.BusinessName, BusinessContactEmail = y.BusinessContactEmail, NBFCCompanyShortName = y.LendingName }).ToListAsync();
            if (company != null)
            {
                reply.Status = true;
                reply.Response = company;
            }
            return reply;
        }

        public async Task<GRPCReply<string>> GetFintechNBFCInvoiceUrl()
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            var company = await _context.Companies.Where(p => p.IsDefault)
                .Select(x => new { x.NBFCInvoiceTemplateURL }).FirstOrDefaultAsync();
            if (company != null)
            {
                gRPCReply.Status = true;
                gRPCReply.Response = company.NBFCInvoiceTemplateURL;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<string>> AddFinancialLiaisonDetails(AddfinancialLiaisonDetailsDTO request)
        {
            GRPCReply<string> gRPCReply = new GRPCReply<string>();
            FinancialLiaisonDetails _financialLiaison = new FinancialLiaisonDetails
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                ContactNo = request.ContactNo,
                EmailAddress = request.EmailAddress,
                CompanyId = request.CompanyId,
                Created = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
            };

            _context.FinancialLiaisonDetails.Add(_financialLiaison);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Financial Liaison Details Added Successfully";
            }
            else
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Issue during company adding.";
            }
            return gRPCReply;

        }

        public async Task<GRPCReply<FintechCompanyResponse>> GetFinTechCompanyDetail()
        {

            GRPCReply<FintechCompanyResponse> gRPCReply = new GRPCReply<FintechCompanyResponse>();
            var defaultCompanyDetail = (await _context.Companies.FirstOrDefaultAsync(x => x.IsDefault && x.IsActive && !x.IsDeleted));
            if (defaultCompanyDetail != null)
            {
                var FintechCompanyResponse = new FintechCompanyResponse
                {
                    CompanyCode = defaultCompanyDetail.CompanyCode,
                    CompanyId = defaultCompanyDetail.Id
                };
                gRPCReply.Message = "Company data get successfully.";
                gRPCReply.Status = true;
                gRPCReply.Response = FintechCompanyResponse;
            }
            else
            {
                gRPCReply.Message = "Company not found.";
                gRPCReply.Status = false;
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<List<GetCompanyDetailListResponse>>> GetCompanyDetailList(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<GetCompanyDetailListResponse>> reply = new GRPCReply<List<GetCompanyDetailListResponse>> { Message = "Data Not Found!!!" };
            reply.Response = await (from c in _context.Companies
                                    join fl in _context.FinancialLiaisonDetails on c.Id equals fl.CompanyId
                                    join req in request.Request on c.Id equals req
                                    where c.IsActive && !c.IsDeleted && fl.IsActive && !fl.IsDeleted
                                    select new GetCompanyDetailListResponse
                                    {
                                        CompanyId = c.Id,
                                        BusinessName = c.BusinessName,
                                        LendingName = c.LendingName,
                                        EmailAddress = fl.EmailAddress,
                                        BeneficiaryName = c.AccountHolderName ?? "",
                                        BeneficiaryAccountNumber = c.BankAccountNumber ?? ""
                                    }).ToListAsync();
            if (reply.Response != null && reply.Response.Any())
            {
                reply.Message = "Data Found";
                reply.Status = true;
            }
            return reply;
        }

        public async Task<GRPCReply<bool>> GetGSTCompany(GRPCRequest<string> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var company = await _context.Companies.FirstOrDefaultAsync(x => request.Request == x.GSTNo && !x.IsDeleted);
            if (company != null)
            {
                reply.Status = true;
                reply.Response = true;
            }
            return reply;
        }
        public async Task<GRPCReply<List<GetAllCompanyDetailDc>>> GetAllCompanies()
        {
            GRPCReply<List<GetAllCompanyDetailDc>> reply = new GRPCReply<List<GetAllCompanyDetailDc>> { Message = "Data Not Found!!!" };
            reply.Response = await _context.Companies.Where(x => x.IsActive && !x.IsDeleted).Select(x => new GetAllCompanyDetailDc
            {
                Id = x.Id,
                BusinessName = x.BusinessName,
                IdentificationCode = x.IdentificationCode ?? "",
                CompanyType = x.CompanyType,
                IsDefault = x.IsDefault,
                IsDSA = x.IsDSA,
                LendinName = x.LendingName ?? ""
            }).OrderBy(x => x.BusinessName).ToListAsync();
            if (reply.Response != null && reply.Response.Any())
            {
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }
    }
}
