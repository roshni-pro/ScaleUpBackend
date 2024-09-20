using Elasticsearch.Net;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.CompanyAPI.Constants;
using ScaleUP.Services.CompanyAPI.Helpers;
using ScaleUP.Services.CompanyAPI.Migrations;
using ScaleUP.Services.CompanyAPI.Persistence;
using ScaleUP.Services.CompanyDTO.Master;
using ScaleUP.Services.CompanyModels;
using ScaleUP.Services.CompanyModels.Master;
using Serilog;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using static IdentityServer4.Models.IdentityResources;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using LanguageMaster = ScaleUP.Services.CompanyModels.Master.LanguageMaster;

namespace ScaleUP.Services.CompanyAPI.Manager
{
    public class CompanyManager
    {
        public readonly CompanyApplicationDbContext _context;
        public CompanyManager(CompanyApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<CompanyResponse> GetCompanyList()
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var _company = await _context.Companies.Where(x => x.IsActive && !x.IsDeleted).Select(x => new { Id = x.Id, BusinessName = x.BusinessName, CompanyType = x.CompanyType, IsDefault = x.IsDefault, LendinName = x.LendingName, IsDSA = x.IsDSA, IdentificationCode = x.IdentificationCode }).ToListAsync();
            if (_company != null && _company.Any())
            {
                companyResponse.Status = true;
                companyResponse.Message = "Data Found.";
                companyResponse.ReturnObject = _company;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Company not exists.";
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> GetCompanyListByids(List<long> CompanyIds)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var _company = await _context.Companies.Where(x => CompanyIds.Contains(x.Id) && x.IsActive && !x.IsDeleted).Select(x => new { Id = x.Id, BusinessName = x.BusinessName, CompanyType = x.CompanyType, IsDefault = x.IsDefault, LendinName = x.LendingName, IsDSA = x.IsDSA, IdentificationCode = x.IdentificationCode }).ToListAsync();
            if (_company != null && _company.Any())
            {
                companyResponse.Status = true;
                companyResponse.Message = "Data Found.";
                companyResponse.ReturnObject = _company;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Company not exists.";
            }
            return companyResponse;
        }
        public async Task<CompanyResponse> GetCompanyListByCompanyType(string CompanyType, List<long> CompanyIds)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var _company = new List<CompanyListByCompanyTypeDc>();
            if (CompanyIds != null && CompanyIds.Any())
            {
                _company = await _context.Companies.Where(x => CompanyIds.Contains(x.Id) && x.CompanyType.ToLower() == CompanyType.ToLower() && !x.IsDefault && x.IsActive && !x.IsDeleted).Select(x => new CompanyListByCompanyTypeDc { Id = x.Id, BusinessName = x.BusinessName }).ToListAsync();
            }
            else
            {
                _company = await _context.Companies.Where(x => x.CompanyType.ToLower() == CompanyType.ToLower() && !x.IsDefault && x.IsActive && !x.IsDeleted).Select(x => new CompanyListByCompanyTypeDc { Id = x.Id, BusinessName = x.BusinessName }).ToListAsync();
            }
            if (_company != null && _company.Any())
            {
                companyResponse.Status = true;
                companyResponse.Message = "Data Found.";
                companyResponse.ReturnObject = _company;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Company not exists.";
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> UpdateCompany(CompanyDTO.Master.CompanyDTO company)
        {
            CompanyResponse companyResponse = new CompanyResponse();

            Companies? _company = await _context.Companies.FirstOrDefaultAsync(x => x.Id == company.CompanyId && !x.IsDeleted);
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
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    companyResponse.Status = true;
                    companyResponse.Message = "Company updated successfully.";
                    companyResponse.ReturnObject = company;
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

        public async Task<CompanyResponse> CompanyExists(string companyName, long? companyId)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            companyResponse.Status = false;
            companyResponse.Message = "Company not exists.";
            bool exists = false;
            if (companyId.HasValue && companyId.Value > 0)
            {
                exists = await _context.Companies.AnyAsync(x => x.Id == companyId.Value && x.BusinessName.ToLower() == companyName.ToLower());
            }
            else
            {
                exists = await _context.Companies.AnyAsync(x => x.BusinessName.ToLower() == companyName.ToLower());

            }

            if (exists)
            {
                companyResponse.Status = true;
                companyResponse.Message = "Company already exists";
            }

            return companyResponse;
        }

        public async Task<CompanyResponse> DeleteCompany(long companyId)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            companyResponse.Status = false;
            companyResponse.Message = "Company not exists.";
            Companies? _company = await _context.Companies.FirstOrDefaultAsync(x => x.Id == companyId);
            if (_company != null)
            {
                _company.Deleted = DateTime.Now;
                _context.Entry(_company).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    companyResponse.Status = true;
                    companyResponse.Message = "Company deleted successfully.";

                }
                else
                {
                    companyResponse.Status = false;
                    companyResponse.Message = "Issue during company deleting.";
                }
            }

            return companyResponse;
        }

        public async Task<GetCompanyRes> GetCompanyById(long companyId)
        {
            GetCompanyRes companyResponse = new GetCompanyRes();
            CompanyDc? _company = await _context.Companies.Where(x => x.Id == companyId && !x.IsDeleted).Include(z => z.CompanyPartnerDetails)
                .Select(x => new CompanyDc
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
                    CompanyPartnerDc = x.CompanyPartnerDetails.Select(z => new PartnerListDc { MobileNo = z.MobileNo, PartnerId = z.Id, PartnerName = z.PartnerName }).ToList()
                }
                ).FirstOrDefaultAsync();
            if (_company != null)
            {
                companyResponse.Status = true;
                companyResponse.Message = "Data Found.";
                companyResponse.ReturnObject = _company;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Company not exists.";
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> ActiveInActiveCompany(long CompanyId, bool IsActive)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var company = await _context.Companies.FirstOrDefaultAsync(x => x.Id == CompanyId && !x.IsDeleted);
            if (company != null)
            {
                company.IsActive = IsActive;
                var res = _context.Entry(company).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    companyResponse.Status = true;
                    companyResponse.Message = $"Company " + (IsActive ? "Activated" : "InActivated") + " Successfully";
                }
                else
                {
                    companyResponse.Status = false;
                    companyResponse.Message = "Failed To Update";
                }
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Company Not Found";
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> GetCompanyByType(int BusinessTypeId)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            companyResponse.Status = false;
            companyResponse.Message = "Company not exists.";
            List<Companies> _companies = await _context.Companies.Where(x => x.BusinessTypeId == BusinessTypeId).ToListAsync();
            if (_companies != null && _companies.Any())
            {
                companyResponse.Status = true;
                companyResponse.Message = "Company deleted successfully.";
                companyResponse.ReturnObject = _companies;
            }
            return companyResponse;
        }

        public CompanyResponse GenerateApiKey()
        {
            CompanyResponse companyResponse = new CompanyResponse();
            string apiKey = "";

            string source = Guid.NewGuid().ToString();

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(source));
                apiKey = new Guid(hash).ToString();
            }
            if (!string.IsNullOrEmpty(apiKey))
            {
                companyResponse.Status = true;
                companyResponse.Message = "ApiKey Generated successfully";
                companyResponse.ReturnObject = apiKey;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Failed To Generate ApiKey";
            }
            return companyResponse;
        }

        public CompanyResponse GenerateSecretKey()
        {
            CompanyResponse companyResponse = new CompanyResponse();
            string secretKey = "";
            string source = Guid.NewGuid().ToString();

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(source));
                secretKey = new Guid(hash).ToString();
            }
            if (!string.IsNullOrEmpty(secretKey))
            {
                companyResponse.Status = true;
                companyResponse.ReturnObject = secretKey;
                companyResponse.Message = "SecretKey Generated successfully";
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Failed To Generate SecretKey";
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> GetBusinessTypeMasterList(string CompanyType)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var BusinessTypeList = await _context.BusinessTypeMasters.
                Where(x => (CompanyType.ToLower() == "nbfc" && (x.Value.ToLower().Contains("llp") || x.Value.ToLower().Contains("private_ltd_company") || x.Value.ToLower().Contains("public_ltd_company")) && x.IsActive && !x.IsDeleted) || (CompanyType.ToLower() != "nbfc" && x.IsActive && !x.IsDeleted)).ToListAsync();
            if (BusinessTypeList != null && BusinessTypeList.Any())
            {
                companyResponse.Status = true;
                companyResponse.ReturnObject = BusinessTypeList;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Data Not Exist";
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> GetCompanyPartnersList(long CompanyId)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var companyPartnersList = await _context.CompanyPartnerDetails.Where(x => x.CompanyId == CompanyId && x.IsActive && !x.IsDeleted).ToListAsync();
            if (companyPartnersList != null && companyPartnersList.Any())
            {
                companyResponse.Status = true;
                companyResponse.ReturnObject = companyPartnersList;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Data Not Exist";
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> UserActiveInactive(string UserId, bool IsActive)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var user = await _context.CompanyUsers.Where(x => x.UserId == UserId && !x.IsDeleted).FirstAsync();
            if (user != null)
            {
                user.IsActive = IsActive;
                user.LastModified = DateTime.Now;
                var res = _context.Entry(user).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    companyResponse.Status = true;
                    companyResponse.Message = "User Updated Successfully";
                }
                else
                {
                    companyResponse.Status = false;
                    companyResponse.Message = "Failed To Update";
                }
            }
            else
            {

                companyResponse.Status = false;
                companyResponse.Message = "User Not Found";
            }
            return companyResponse;
        }


        public async Task<CompanyResponse> InsertCompanyLocation(CompanyLocationDTO companylocation)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            companyResponse.Status = false;
            companyResponse.Message = "Issue during company location adding.";
            CompanyLocation _companyloc = new CompanyLocation
            {
                CompanyId = companylocation.CompanyId,
                LocationId = companylocation.LocationId,
                IsActive = true,
                IsDeleted = false
            };

            _context.CompanyLocations.Add(_companyloc);
            int rowchanged = await _context.SaveChangesAsync();
            if (rowchanged > 0)
            {
                companyResponse.Status = true;
                companyResponse.Message = "Company Location added successfully.";
            }
            return companyResponse;
        }


        public async Task<CompanyResponse> GetCompanyLocationById(long companyId)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            companyResponse.Status = false;
            companyResponse.Message = "Company location data not exists.";
            var _companyloc = await _context.CompanyLocations.Where(x => x.CompanyId == companyId && !x.IsDeleted && x.IsActive).ToListAsync();
            if (_companyloc != null && _companyloc.Any())
            {
                List<CompanyLocationDTO> companylocations = _companyloc.Select(x => new CompanyLocationDTO
                {
                    CompanyId = x.CompanyId,
                    LocationId = x.LocationId
                }).ToList();
                companyResponse.Status = true;
                companyResponse.Message = "Company location data exists.";
                companyResponse.ReturnObject = companylocations;
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> ActiveInActiveCompanyLocation(long companyId, long locationId, bool IsActive)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            companyResponse.Status = false;
            companyResponse.Message = "Company location data not exists.";
            var _companyloc = await _context.CompanyLocations.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.LocationId == locationId && !x.IsDeleted);
            if (_companyloc != null)
            {
                _companyloc.IsActive = IsActive;
                _context.Entry(_companyloc).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    companyResponse.Status = true;
                    companyResponse.Message = "Company location Updated successfully.";

                }
                else
                {
                    companyResponse.Status = false;
                    companyResponse.Message = "Issue during company location Updating.";
                }
            }
            return companyResponse;
        }

        public async Task<CompanyResponse> DeleteCompanyLocation(long companyId, long locationId)
        {
            CompanyResponse companyResponse = new CompanyResponse();
            companyResponse.Status = false;
            companyResponse.Message = "Company location data not exists.";
            var _companyloc = await _context.CompanyLocations.Where(x => x.CompanyId == companyId && x.LocationId == locationId && !x.IsDeleted && x.IsActive).ToListAsync();
            if (_companyloc != null && _companyloc.Any())
            {
                foreach (var item in _companyloc)
                {
                    item.IsDeleted = true;
                    item.Deleted = DateTime.Now;
                    _context.Entry(item).State = EntityState.Modified;
                }
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    companyResponse.Status = true;
                    companyResponse.Message = "Company location deleted successfully.";

                }
                else
                {
                    companyResponse.Status = false;
                    companyResponse.Message = "Issue during company location deleting.";
                }
            }
            return companyResponse;
        }

        public async Task<GSTDetailDTO> GetGSTDetail(string GSTNO)
        {
            GSTDetailDTO gSTDetailDTO = new GSTDetailDTO();
            var dbGST = await _context.GSTverifiedRequests.FirstOrDefaultAsync(x => x.RefNo == GSTNO);
            if (dbGST == null)
            {

                var GSTDetail = await GSTDetailHelper.GetGSTVerify(GSTNO, EnvironmentConstants.GSTURL);
                if (GSTDetail != null)
                {
                    gSTDetailDTO.status = GSTDetail.Status;
                    gSTDetailDTO.Message = GSTDetail.Message;
                    if (GSTDetail.custverify != null)
                    {
                        gSTDetailDTO.Name = GSTDetail.custverify.Name ?? "";
                        gSTDetailDTO.AddressLine1 = string.Format("{0}, {1}, {2}", GSTDetail.custverify.HomeNo, GSTDetail.custverify.HomeName, GSTDetail.custverify.ShippingAddress); ;
                        gSTDetailDTO.State = GSTDetail.custverify.State ?? "";
                        gSTDetailDTO.City = GSTDetail.custverify.City ?? "";
                        gSTDetailDTO.ShopName = GSTDetail.custverify.ShopName ?? "";
                        gSTDetailDTO.Zipcode = GSTDetail.custverify.Zipcode ?? "";

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
                        _context.GSTverifiedRequests.AddAsync(dbGST);
                        _context.SaveChanges();
                    }
                }
            }
            else
            {
                gSTDetailDTO.status = dbGST.Active == "Active";
                gSTDetailDTO.Message = dbGST.Active == "Active" ? "Customer GST Number Is Verify Successfully." : ("Customer GST Number Is " + dbGST.Active);
                gSTDetailDTO.Name = dbGST.Name ?? "";
                gSTDetailDTO.AddressLine1 = string.Format("{0}, {1}, {2}", dbGST.HomeNo, dbGST.HomeName, dbGST.ShippingAddress); ;
                gSTDetailDTO.State = dbGST.State ?? "";
                gSTDetailDTO.City = dbGST.City ?? "";
                gSTDetailDTO.ShopName = dbGST.ShopName ?? "";
                gSTDetailDTO.Zipcode = dbGST.Zipcode ?? "";
            }
            return gSTDetailDTO;
        }

        public async Task<bool> CheckCompanyIsDefault(long companyId)
        {
            bool IsDefault = await _context.Companies.AnyAsync(x => x.Id == companyId && x.IsDefault);
            return IsDefault;
        }
        public async Task<CompanyResponse> GetLatestGSTRate()
        {
            CompanyResponse companyResponse = new CompanyResponse();
            var gstRate = await _context.GSTMasters.FirstOrDefaultAsync(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsActive && !x.IsDeleted);
            if (gstRate != null)
            {
                companyResponse.Status = true;
                companyResponse.Message = "Gst Found";
                companyResponse.ReturnObject = gstRate.GSTRate;
            }
            else
            {
                companyResponse.Status = false;
                companyResponse.Message = "Gst Not Found";
            }
            return companyResponse;
        }

        public async Task<GSTDetailDTO> CheckCompanyGSTExist(string GSTNo)
        {
            GSTDetailDTO gSTDetailDTO = new GSTDetailDTO();
            bool ifExist = await _context.Companies.AnyAsync(x => x.GSTNo == GSTNo && x.IsDeleted == false);
            if (ifExist)
            {
                gSTDetailDTO.status = true;
                gSTDetailDTO.Message = "GST No Already Exists!!";
            }
            else
            {
                gSTDetailDTO.status = false;
                gSTDetailDTO.Message = "GST No Not Exists";
            }
            return gSTDetailDTO;
        }
        public async Task<CompanyResponse<List<CompanyLogDc>>> GetCompanyAuditLogs(long CompanyId)
        {
            CompanyResponse<List<CompanyLogDc>> companyResponse = new CompanyResponse<List<CompanyLogDc>>();
            AuditLogHelper auditLogHelper = new AuditLogHelper(_context);
            var auditLogs = await auditLogHelper.GetAuditLogsByRegex(CompanyId, "Companies");
            if (auditLogs != null && auditLogs.Any())
            {
                companyResponse.ReturnObject = new List<CompanyLogDc>();
                foreach (var auditLog in auditLogs)
                {
                    CompanyLogDc companyLogDc = new CompanyLogDc();
                    foreach (var field in auditLog.Fields)
                    {
                        if (field.FieldName.Contains("BankName")) companyLogDc.BankName = field.NewValue;
                        else if (field.FieldName.Contains("BankAccountNumber")) companyLogDc.BankAccountNumber = field.NewValue;
                        else if (field.FieldName.Contains("BankIFSC")) companyLogDc.BankIFSC = field.NewValue;
                        else if (field.FieldName.Contains("CancelChequeDocId")) companyLogDc.CancelChequeDocId = field.NewValue;
                        else if (field.FieldName.Contains("CancelChequeURL")) companyLogDc.CancelChequeURL = field.NewValue;
                        else if (field.FieldName.Contains("ContactPersonName")) companyLogDc.ContactPersonName = field.NewValue;
                        else if (field.FieldName.Contains("BusinessContactNo")) companyLogDc.BusinessContactNo = field.NewValue;
                        else if (field.FieldName.Contains("BusinessContactEmail")) companyLogDc.BusinessContactEmail = field.NewValue;
                        else if (field.FieldName.Contains("BusinessHelpline")) companyLogDc.BusinessHelpline = field.NewValue;
                        else if (field.FieldName.Contains("LogoURL")) companyLogDc.LogoURL = field.NewValue;
                        else if (field.FieldName.Contains("LastModifiedBy")) companyLogDc.ModifiedBy = field.NewValue;
                        else if (field.FieldName.Contains("LastModified")) companyLogDc.ModifiedDate = field.NewValue;
                        else if (field.FieldName.Contains("IsActive")) companyLogDc.IsActive = field.NewValue;
                        else if (field.FieldName.Contains("IsDeleted")) companyLogDc.IsDeleted = field.NewValue;
                    }
                    companyResponse.ReturnObject.Add(companyLogDc);
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

        public async Task<CompanyTemplateResponseDc> SaveModifyTemplateMaster(CompanyTemplateDc templatedc)
        {
            CompanyTemplateResponseDc temp = new CompanyTemplateResponseDc();

            if (templatedc != null && templatedc.TemplateCode != null && templatedc.TemplateType != null && templatedc.TemplateID == null)
            {
                var exist = await _context.CompanyTemplateMasters.Where(x => x.TemplateType == templatedc.TemplateType && x.TemplateCode == templatedc.TemplateCode && x.IsDeleted == false).FirstOrDefaultAsync();
                if (exist == null)
                {
                    var smstemp = new CompanyTemplateMaster
                    {
                        DLTID = templatedc.DLTID,
                        TemplateType = templatedc.TemplateType,
                        TemplateCode = templatedc.TemplateCode,
                        Template = templatedc.Template,
                        IsActive = templatedc.Status,
                        IsDeleted = false,
                        Created = DateTime.Now
                    };
                    _context.CompanyTemplateMasters.Add(smstemp);
                    await _context.SaveChangesAsync();
                    temp.Status = true;
                    temp.Message = "successfully added!";
                }
                else
                {
                    temp.Message = "Template code already exist!!";
                }
            }
            else
            {

                var data = await _context.CompanyTemplateMasters.Where(x => x.Id == templatedc.TemplateID && x.IsDeleted == false).Select(x => x).FirstOrDefaultAsync();
                if (data != null)
                {
                    if (data.TemplateType == "SMS")
                    {
                        data.DLTID = templatedc.DLTID;
                    }
                    data.Template = templatedc.Template;
                    data.IsActive = templatedc.Status;
                    data.LastModified = DateTime.Now;
                    _context.Entry(data).State = EntityState.Modified;
                    int isSaved = await _context.SaveChangesAsync();
                    if (isSaved > 0)
                    {
                        temp.Status = true;
                        temp.Message = "successfully Modified!";
                    }
                    else
                    {
                        temp.Status = false;
                        temp.Message = "failed to Save";
                    }
                }
                else
                {
                    temp.Status = false;
                    temp.Message = "data not found!";
                }
            }
            return temp;
        }

        public async Task<CompanyResponse<List<GetEducationMasterListDc>>> GetEducationMasterList()
        {
            CompanyResponse<List<GetEducationMasterListDc>> response = new CompanyResponse<List<GetEducationMasterListDc>> { Message = "Data Not Found!!!" };
            response.ReturnObject = await _context.EducationMasters.Where(x => x.IsActive && !x.IsDeleted).Select(x => new GetEducationMasterListDc { Id = x.Id, Name = x.Name }).ToListAsync();
            if (response.ReturnObject != null && response.ReturnObject.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
            }
            return response;
        }

        public async Task<CompanyResponse<List<LanguageMasterDTO>>> GetLangauageMasterList()
        {
            CompanyResponse<List<LanguageMasterDTO>> response = new CompanyResponse<List<LanguageMasterDTO>> { Message = "Data Not Found!!!" };
            response.ReturnObject = await _context.LanguageMasters.Where(x => x.IsActive && !x.IsDeleted).Select(x => new LanguageMasterDTO { Id = x.Id, Name = x.Name }).ToListAsync();
            if (response.ReturnObject != null && response.ReturnObject.Any())
            {
                response.Status = true;
                response.Message = "Data Found";
            }
            return response;
        }

        public async Task<CompanyResponse<bool>> AddLangauage(string LanguageName)
        {
            CompanyResponse<bool> response = new CompanyResponse<bool> { Message = "Data Not Found!!!" };
            LanguageMaster languageMasterData = new LanguageMaster();
            var languageData = await _context.LanguageMasters.Where(x => x.Name.ToLower() == LanguageName.ToLower() && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (languageData != null)
            {
                languageData.Name = LanguageName;
                languageData.LastModifiedBy = "By System";
                languageData.LastModified = DateTime.Today;
                _context.Entry(languageData).State = EntityState.Modified;
            }
            else
            {
                languageMasterData.Name = LanguageName;
                languageMasterData.CreatedBy = "By System";
                languageMasterData.Created = DateTime.Today;
                languageMasterData.IsActive = true;
                languageMasterData.IsDeleted = false;
                _context.LanguageMasters.Add(languageMasterData);
            }
            _context.SaveChangesAsync();
                response.Status = true;
                response.ReturnObject = true;
                response.Message = "Data Saved Successfully.";
            return response;
        }

    }
}



