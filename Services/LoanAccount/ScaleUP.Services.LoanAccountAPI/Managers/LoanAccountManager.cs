using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountModels.Transaction;
using ScaleUP.Services.LoanAccountModels.Master;
using Microsoft.Data.SqlClient;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using ScaleUP.Services.LoanAccountDTO.Loan;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using ScaleUP.Services.LoanAccountDTO;
using System.Data;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Global.Infrastructure.Constants;
using Newtonsoft.Json;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts.BlackSoil;
using ScaleUP.Global.Infrastructure.Constants.NBFC;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.TemplateMaster;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Services.LoanAccountModels.Master.NBFC;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using ScaleUP.Services.LoanAccountAPI.Helpers;
using static MassTransit.ValidationResultExtensions;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using Nito.AsyncEx;
using System.Transactions;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using MassTransit.DependencyInjection;
using ScaleUP.Services.LoanAccountAPI.Helpers.NBFC;
using ScaleUP.Global.Infrastructure.Constants.Product;
using MassTransit.Configuration;
using ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Global.Infrastructure.Enum;
using ScaleUP.Global.Infrastructure.Helper;
using ScaleUP.Services.LoanAccountModels.Master.NBFC.ArthMate;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ScaleUP.Services.LoanAccountAPI.Common.Enum;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts.DSA;
using ScaleUP.Global.Infrastructure.Constants.DSA;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using ScaleUP.Global.Infrastructure.Constants.LoanAccount;
using Microsoft.VisualBasic;
using ScaleUP.Services.LoanAccountModels.Master.BusinessLoan;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using System.Threading.Tasks.Sources;
using System;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ScaleUP.Global.Infrastructure.Constants.AccountLocation;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;


namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class LoanAccountManager
    {

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
        private readonly LoanAccountApplicationDbContext _context;
        private readonly LoanNBFCFactory _loanNBFCFactory;
        private readonly LoanAccountHelper _loanAccountHelper;
        private readonly OrderPlacementManager _orderPlacementManager;
        private readonly TransactionSettlementManager _TransactionSettlementManager;
        private IHostEnvironment _hostingEnvironment;
        private readonly ArthmateNBFCHelper _ArthMateNBFCHelper;
        public LoanAccountManager(LoanAccountApplicationDbContext context, LoanNBFCFactory loanNBFCFactory, LoanAccountHelper loanAccountHelper, OrderPlacementManager orderPlacementManager, TransactionSettlementManager transactionSettlementManager, IHostEnvironment hostingEnvironment, ArthmateNBFCHelper arthMateNBFCHelper)
        {
            _loanNBFCFactory = loanNBFCFactory;
            _loanAccountHelper = loanAccountHelper;
            _context = context;
            _orderPlacementManager = orderPlacementManager;
            _TransactionSettlementManager = transactionSettlementManager;
            _hostingEnvironment = hostingEnvironment;
            _ArthMateNBFCHelper = arthMateNBFCHelper;
        }

        public async Task<GRPCReply<long>> SaveLoanAccount(GRPCRequest<SaveLoanAccountRequestDC> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            var LoanAccountTbl = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted);

            if (LoanAccountTbl != null)
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Loan already exists for this lead";
                gRPCReply.Response = 0;
            }
            else
            {
                request.Request.NBFCCompanyCode = string.IsNullOrEmpty(request.Request.NBFCCompanyCode) ? LeadNBFCConstants.Default.ToString() : request.Request.NBFCCompanyCode;
                string accountcode = await GetCurrentNumber("AccountCode");

                BlackSoilLineActivated blackSoilAccount = null;
                if (!string.IsNullOrEmpty(request.Request.Webhookresposne))
                {
                    blackSoilAccount = JsonConvert.DeserializeObject<BlackSoilLineActivated>(request.Request.Webhookresposne);
                }


                LoanAccount loanAccount = new LoanAccount
                {
                    LeadId = request.Request.LeadId,
                    LeadCode = request.Request.LeadCode,
                    ProductId = request.Request.ProductId,
                    UserId = request.Request.UserId,
                    AccountCode = accountcode,
                    CustomerName = request.Request.CustomerName, //TransactionTypeId.Id,
                    MobileNo = request.Request.MobileNo,
                    NBFCCompanyId = request.Request.NBFCCompanyId,
                    AnchorCompanyId = request.Request.AnchorCompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Now,
                    DisbursalDate = DateTime.Now,
                    ApplicationDate = Convert.ToDateTime(request.Request.ApplicationDate),
                    AgreementRenewalDate = Convert.ToDateTime(request.Request.AgreementRenewalDate),
                    IsDefaultNBFC = request.Request.IsDefaultNBFC,
                    CityName = request.Request.CityName,
                    AnchorName = request.Request.AnchorName,
                    ProductType = request.Request.ProductType,
                    IsAccountActive = request.Request.IsAccountActive ?? true,
                    IsBlock = request.Request.IsBlock ?? false,
                    IsBlockComment = "",
                    IsBlockHideLimit = false,
                    NBFCIdentificationCode = request.Request.NBFCCompanyCode,
                    ThirdPartyLoanCode = blackSoilAccount != null ? blackSoilAccount.data.identifiers.business_id : request.Request.BusinessCode,
                    ShopName = request.Request.ShopName,
                    CustomerImage = request.Request.CustomerImage,
                    Type = LoanAccountUserTypeConstants.Customer
                };
                await _context.LoanAccounts.AddAsync(loanAccount);
                int rowChanged = await _context.SaveChangesAsync();
                if (rowChanged > 0)
                {
                    NBFCDetailDTO nBFCDetailDTO = null;

                    if (!string.IsNullOrEmpty(request.Request.NBFCCompanyCode) && request.Request.NBFCCompanyCode == LeadNBFCConstants.BlackSoil.ToString())
                    {
                        nBFCDetailDTO = new NBFCDetailDTO();
                        nBFCDetailDTO.ApplicationId = request.Request.ApplicationId;
                        nBFCDetailDTO.ApplicationCode = request.Request.ApplicationCode;
                        nBFCDetailDTO.BusinessId = request.Request.BusinessId;
                        nBFCDetailDTO.BusinessCode = request.Request.BusinessCode;
                    }


                    var nbfcFactory = _loanNBFCFactory.GetService(request.Request.NBFCCompanyCode);
                    await nbfcFactory.SaveNBFCLoanData(loanAccount.Id, request.Request.Webhookresposne, nBFCDetailDTO);
                    gRPCReply.Status = true;
                    gRPCReply.Message = "Success";
                    gRPCReply.Response = loanAccount.Id;
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Fail";
                    gRPCReply.Response = 0;
                }
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<long>> AddLoanAccountCredit(GRPCRequest<LoanAccountCreditsRequest> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            var LoanAccountTbl = await _context.LoanAccountCredits.FirstOrDefaultAsync(x => x.LoanAccountId == request.Request.LoanAccountId);

            if (LoanAccountTbl != null)
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Fail";
                gRPCReply.Response = 0;
            }
            else
            {
                LoanAccountCredit _loanaccountcredit = new LoanAccountCredit
                {
                    LoanAccountId = request.Request.LoanAccountId,
                    //GstRate = request.Request.GstRate,
                    //ProcessingFeeRate = request.Request.ProcessingFeeRate,
                    DisbursalAmount = request.Request.DisbursalAmount,
                    //ConvenienceFeeRate = request.Request.ConvenienceFeeRate,
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Now,
                    //BounceCharge = request.Request.BounceCharge,
                    //CreditDays = request.Request.CreditDays,
                    CreditLimitAmount = request.Request.DisbursalAmount,
                    //DelayPenaltyRate = request.Request.DelayPenaltyRate,
                };
                await _context.LoanAccountCredits.AddAsync(_loanaccountcredit);
                int rowChanged = await _context.SaveChangesAsync();
                if (rowChanged > 0)
                {
                    AccountTransactionManager objAccounttransManager = new AccountTransactionManager(_context, _loanNBFCFactory);
                    var res = objAccounttransManager.SaveTransactionDetailHead("GST" + Convert.ToString(request.Request.GstRate));

                    gRPCReply.Status = true;
                    gRPCReply.Message = "Success";
                    gRPCReply.Response = _loanaccountcredit.Id;
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Fail";
                    gRPCReply.Response = 0;
                }
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<long>> SaveLoanAccountCompanyLead(GRPCRequest<SaveLoanAccountCompanyLeadRequestDC> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            var LoanAccountTbl = await _context.LoanAccountCompanyLead.FirstOrDefaultAsync(x => x.LoanAccountId == request.Request.LoanAccountId && x.IsActive == true && x.IsDeleted == false);

            if (LoanAccountTbl != null)
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Fail";
                gRPCReply.Response = 0;
            }
            else
            {
                LoanAccountCompanyLead _loanAccountCompanyLead = new LoanAccountCompanyLead
                {
                    LoanAccountId = request.Request.LoanAccountId,
                    CompanyId = request.Request.CompanyId,
                    LeadProcessStatus = request.Request.LeadProcessStatus,
                    UserUniqueCode = request.Request.UserUniqueCode,
                    AnchorName = request.Request.AnchorName,
                    LogoURL = request.Request.LogoURL,
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Now,
                };
                await _context.LoanAccountCompanyLead.AddAsync(_loanAccountCompanyLead);
                int rowChanged = await _context.SaveChangesAsync();
                if (rowChanged > 0)
                {
                    gRPCReply.Status = true;
                    gRPCReply.Message = "Success";
                    gRPCReply.Response = _loanAccountCompanyLead.Id;
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Fail";
                    gRPCReply.Response = 0;
                }
            }
            return gRPCReply;

        }
        public async Task<GRPCReply<List<LoanAccountDisbursementResponse>>> GetLoanAccountDisbursement(GRPCRequest<List<long>> request)
        {
            GRPCReply<List<LoanAccountDisbursementResponse>> gRPCReply = new GRPCReply<List<LoanAccountDisbursementResponse>>();
            gRPCReply.Status = false;
            gRPCReply.Message = "";
            var AccountTransaction = (from la in _context.LoanAccounts
                                      join req in request.Request on la.LeadId equals req
                                      join ac in _context.AccountTransactions on la.Id equals ac.LoanAccountId
                                      join ty in _context.TransactionTypes on ac.TransactionTypeId equals ty.Id
                                      //join req in request.Request on la.LeadId equals req
                                      where ty.Code == TransactionTypesConstants.Disbursement && la.IsActive == true && ac.IsActive == true

                                      select new
                                      {
                                          LeadId = la.LeadId,
                                          DisbursementDate = ac.DisbursementDate,
                                          OrderAmount = ac.OrderAmount
                                      }).ToList();


            if (AccountTransaction != null && AccountTransaction.Count > 0)
            {
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";

            }



            return gRPCReply;
        }




        public async Task<GRPCReply<DisbursementResponse>> GetDisbursement(GRPCRequest<long> request)
        {
            GRPCReply<DisbursementResponse> gRPCReply = new GRPCReply<DisbursementResponse>();
            var LoanAccount = await _context.LoanAccounts.Where(x => x.LeadId == request.Request).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();
            if (LoanAccount != null)
            {
                //&& h.Code != TransactionDetailHeadsConstants.Initial
                var AccountTransaction = (from k in _context.AccountTransactions
                                          join ld in _context.AccountTransactionDetails on k.Id equals ld.AccountTransactionId
                                          join adm in _context.TransactionTypes on k.TransactionTypeId equals adm.Id
                                          join h in _context.TransactionDetailHeads on ld.TransactionDetailHeadId equals h.Id
                                          where k.LoanAccountId == LoanAccount.Id && ld.IsActive == true
                                          && adm.Code == TransactionTypesConstants.Disbursement
                                          select new
                                          {
                                              h.Code,
                                              ld.Amount,
                                              //k.ConvenienceFeeRate,
                                              k.InterestRate,
                                              k.ProcessingFeeRate,
                                              k.GstRate,
                                              k.PayableBy,
                                              k.ProcessingFeeType,
                                              k.InterestType
                                          }
                                       ).ToList();


                DisbursementResponse res = new DisbursementResponse();
                res.AccountCode = LoanAccount.AccountCode;
                res.AccountId = LoanAccount.Id;
                //res.ConvenienceFeeRate = LoanAccount.LoanAccountCredits.ConvenienceFeeRate;
                res.DisbursalAmount = LoanAccount.LoanAccountCredits.DisbursalAmount;
                res.LeadCode = LoanAccount.LeadCode;
                res.LeadId = LoanAccount.Id;
                res.MobileNo = LoanAccount.MobileNo;
                //res.ProcessingFeeRate = LoanAccount.LoanAccountCredits.ConvenienceFeeRate;
                res.UserName = LoanAccount.CustomerName;
                //res.GstRate = LoanAccount.LoanAccountCredits.GstRate;
                res.ProductType = LoanAccount.ProductType;
                res.ThirdPartyLoanCode = LoanAccount.ThirdPartyLoanCode;
                res.AnchorName = LoanAccount.AnchorName;

                foreach (var item in AccountTransaction)
                {
                    string GSTper = "GST" + Convert.ToString(item.GstRate);
                    res.ConvenienceFeeRate = 0;
                    res.InterestRate = item.InterestRate;
                    res.ProcessingFeeRate = item.ProcessingFeeRate;
                    res.GstRate = item.GstRate;

                    if (item.Code == TransactionDetailHeadsConstants.ProcessingFee)
                    { res.ProcessingFeeAmount = Math.Abs(item.Amount); }

                    if (item.Code == GSTper)
                    { res.GSTProcessingFeeAmount = Math.Abs(item.Amount); }

                    res.PayableBy = item.PayableBy;
                    res.ProcessingFeeType = item.ProcessingFeeType;
                    res.InterestRateType = item.InterestType;
                }

                gRPCReply.Response = res;
                gRPCReply.Status = true;


            }
            return gRPCReply;
        }
        public async Task<string> GetCurrentNumber(string entityname)
        {
            var entity_name = new SqlParameter("entityname", entityname);
            return _context.Database.SqlQueryRaw<string>("exec spGetCurrentNumber @entityname", entity_name).AsEnumerable().FirstOrDefault();
        }

        public async Task<CommonResponse> GetLoanAccountList(LoanAccountListRequest obj)
        {
            CommonResponse result = new CommonResponse();

            var productType = new SqlParameter("ProductType", obj.ProductType);
            var status = new SqlParameter("Status", obj.Status);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var cityname = new SqlParameter("CityName", obj.CityName);
            var keyword = new SqlParameter("Keyward", obj.Keyword);
            var min = new SqlParameter("Min", obj.Min);
            var max = new SqlParameter("Max", obj.Max);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            var anchorId = new SqlParameter("AnchorId", obj.AnchorId);


            var list = _context.Database.SqlQueryRaw<LoanAccountListDc>(" exec GetLoanAccountList @ProductType,  @Status,@Fromdate,@ToDate,@CityName,@Keyward,@Min,@Max,@Skip,@Take,@AnchorId", productType, status, fromdate, toDate, cityname, keyword, min, max, skip, take, anchorId).AsEnumerable().ToList();
            if (list.Any())
            {
                foreach (var item in list)
                {
                    var IntransitPaidAmt = await _context.AccountTransactions.Where(x => x.LoanAccountId == item.LoanAccountId && x.IsActive && !x.IsDeleted && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement
                                               && (x.TransactionStatus.Code == TransactionStatuseConstants.Initiate || x.TransactionStatus.Code == TransactionStatuseConstants.Intransit)).SumAsync(x => x.TransactionAmount);

                    var loanfactory = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.BlackSoil);
                    item.AvailableCreditLimit = await loanfactory.GetAvailableCreditLimit(item.LoanAccountId);

                    if (item.AvailableCreditLimit > 0 && IntransitPaidAmt > 0 && item.AvailableCreditLimit > IntransitPaidAmt)
                    {
                        item.AvailableCreditLimit -= IntransitPaidAmt;
                    }
                }
                result.Result = list;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<LoanAccountListDc>();
                result.status = false;
                result.Message = "Not Found";
            }
            return result;
        }

        public async Task<GRPCReply<LoanCreditLimit>> GetAvailableCreditLimitByLeadId(long LoanAccountId)
        {
            return await _loanAccountHelper.GetAvailableCreditLimitByLoanId(LoanAccountId, "");
        }

        public async Task<CommonResponse> AnchorCityProductList(List<long>? Companies)
        {
            CommonResponse result = new CommonResponse();
            AnchorCityProductListDc DDList = new AnchorCityProductListDc();
            DDList.ProductDcs = new List<ProductDc>();
            DDList.CityNameDcs = new List<CityNameDc>();
            DDList.AnchorNameDcs = new List<AnchorNameDc>();

            List<CityNameDc> cities = new List<CityNameDc>();
            List<AnchorNameDc> anchors = new List<AnchorNameDc>();
            List<ProductDc> products = new List<ProductDc>();

            List<AnchorCityProductdc> list = new List<AnchorCityProductdc>();
            list = await _context.LoanAccounts.Where(x => x.IsActive == true && x.IsDeleted == false && (Companies == null || Companies.Contains(x.NBFCCompanyId) || Companies.Contains(x.AnchorCompanyId ?? 0))).Select(x => new AnchorCityProductdc { CityName = x.CityName, AnchorId = x.AnchorCompanyId, AnchorName = x.AnchorName, ProductId = x.ProductId, ProductType = x.ProductType }).ToListAsync();
            if (list.Any())
            {
                var citieslist = list.Select(x => x.CityName).Distinct().ToList();
                var Products = list.Select(x => new { ProductId = x.ProductId, ProductType = x.ProductType }).Distinct().ToList();
                var anchores = list.Where(x => !string.IsNullOrEmpty(x.AnchorName)).Select(x => new { AnchorId = x.AnchorId, AnchorName = x.AnchorName }).Distinct().ToList();

                foreach (var item in citieslist.OrderBy(x => x))
                {
                    CityNameDc city = new CityNameDc();
                    city.CityName = item;
                    cities.Add(city);
                }
                foreach (var item in Products.OrderBy(x => x.ProductId))
                {
                    ProductDc product = new ProductDc();
                    product.ProductId = item.ProductId;
                    product.ProductType = item.ProductType;
                    products.Add(product);
                }
                foreach (var item in anchores.OrderBy(x => x.AnchorName))
                {
                    AnchorNameDc anchor = new AnchorNameDc();
                    anchor.AnchorName = item.AnchorName;
                    anchor.AnchorId = item.AnchorId;
                    anchors.Add(anchor);
                }

                if (cities.Count > 0)
                {
                    var city = cities.Distinct();
                    DDList.CityNameDcs.AddRange(cities.Distinct());
                }
                if (anchors.Count > 0)
                {
                    DDList.AnchorNameDcs.AddRange(anchors.Distinct());
                }
                if (products.Count > 0)
                {
                    DDList.ProductDcs.AddRange(products.Distinct());
                }

                result.Result = DDList;
                result.status = true;
                result.Message = "Success";
            }
            else
            {
                result.Result = new List<AnchorCityProductListDc>();
                result.status = false;
                result.Message = "Not Found";
            }

            return result;
        }

        public async Task<GRPCReply<LoanAccountReplyDC>> GetLoanAccountById(GRPCRequest<string> request)
        {
            GRPCReply<LoanAccountReplyDC> gRPCReply = new GRPCReply<LoanAccountReplyDC>();
            LoanAccountReplyDC loanAccountReplyDC = new LoanAccountReplyDC();
            var loanAccount = _context.LoanAccounts.Where(x => x.UserId == request.Request && x.IsActive && !x.IsDeleted).FirstOrDefault();
            if (loanAccount != null)
            {
                loanAccountReplyDC.LoanAccountId = loanAccount.Id;
                loanAccountReplyDC.IsBlock = loanAccount.IsBlock;
                loanAccountReplyDC.LeadId = loanAccount.LeadId;
                loanAccountReplyDC.IsBlockHideLimit = loanAccount.IsBlockHideLimit;
                loanAccountReplyDC.IsAccountActive = loanAccount.IsAccountActive;
                loanAccountReplyDC.IsBlockComment = loanAccount.IsBlockComment;
                //loanAccountReplyDC.Status = true;
                //loanAccountReplyDC.Message = "Lead Disbursed";
                gRPCReply.Response = loanAccountReplyDC;
                gRPCReply.Status = true;
                gRPCReply.Message = "Lead Disbursed";
            }
            else
            {
                //loanAccountReplyDC.Status = false;
                //loanAccountReplyDC.Message = "Lead Not Disbursement!";
                gRPCReply.Status = true;
                gRPCReply.Message = "Lead Disbursed";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<long>> BlacksoilCallback(GRPCRequest<BlackSoilWebhookRequest> request)
        {
            var result = new GRPCReply<long>();
            if (request != null)
            {
                string EventName = request.Request.EventName;
                long loanAccountId = 0;
                long transactionId = 0;
                string status = "";

                BlackSoilAccountDetail blackSoilAccountDetails = null;
                BlackSoilAccountTransaction blackSoilAccountTransactions = null;
                BlackSoilWebhookResponse blackSoilWebhookResponse = new BlackSoilWebhookResponse
                {
                    Response = request.Request.Data,
                    EventName = request.Request.EventName,
                    LoanAccountId = null,
                    IsActive = true,
                    CreatedBy = "",
                    IsDeleted = false
                };
                _context.BlackSoilWebhookResponses.Add(blackSoilWebhookResponse);
                await _context.SaveChangesAsync();

                try
                {
                    var item = JsonConvert.DeserializeObject<BlackSoilCommonWebhookResponse>(request.Request.Data);

                    if (item != null)
                    {
                        switch (item.@event)
                        {
                            case BlackSoilWebhookConstant.InvoiceApproved:
                                var invoiceApproved = JsonConvert.DeserializeObject<BlackSoilInvoiceApproved>(request.Request.Data);
                                blackSoilAccountDetails = await _context.BlackSoilAccountDetails.Where(x => x.ApplicationId == invoiceApproved.data.application).FirstOrDefaultAsync();
                                //blackSoilAccountDetails.BlackSoilLoanId = invoiceApproved.data.loan_data.HasValue? invoiceApproved.data.loan_data.id.Value: 0;

                                blackSoilAccountTransactions = await _context.BlackSoilAccountTransactions.FirstOrDefaultAsync(x => x.WithdrawalId == invoiceApproved.data.id && x.IsActive && !x.IsDeleted);
                                status = invoiceApproved.data.status;
                                loanAccountId = blackSoilAccountDetails.LoanAccountId;

                                if (blackSoilAccountTransactions != null)
                                {
                                    blackSoilAccountTransactions.Status = status;
                                    _context.Entry(blackSoilAccountTransactions).State = EntityState.Modified;
                                    _context.SaveChanges();
                                    transactionId = blackSoilAccountTransactions.Id;
                                }
                                break;

                            case BlackSoilWebhookConstant.InvoiceDisbursalProcessed:
                                var disbursalProcessed = JsonConvert.DeserializeObject<BlackSoilInvoiceDisbursalProcessed>(request.Request.Data);
                                blackSoilAccountDetails = await _context.BlackSoilAccountDetails.Where(x => x.ApplicationId == disbursalProcessed.data.application).FirstOrDefaultAsync();
                                blackSoilAccountDetails.BlackSoilLoanId = disbursalProcessed.data.loan_account.HasValue ? disbursalProcessed.data.loan_account.Value : 0;

                                blackSoilAccountTransactions = await _context.BlackSoilAccountTransactions.FirstOrDefaultAsync(x => x.WithdrawalId == disbursalProcessed.data.invoice.id && x.IsActive && !x.IsDeleted);
                                //status = disbursalProcessed.data.invoice.status;

                                result.Response = blackSoilAccountTransactions.Id;

                                status = "DisbursalProcessed";

                                loanAccountId = blackSoilAccountDetails.LoanAccountId;

                                if (blackSoilAccountTransactions != null)
                                {
                                    long ApplicationId = disbursalProcessed.data.application ?? 0;
                                    long loan_account = disbursalProcessed.data.loan_account.HasValue ? disbursalProcessed.data.loan_account.Value : 0;
                                    long disbursalProcessed_data_invoiceid = disbursalProcessed.data.invoice.id ?? 0;
                                    string disbursalProcessedDisbursementDate = disbursalProcessed.data.disbursement_date;
                                    double total_invoice_amount = Convert.ToDouble(disbursalProcessed.data.invoice.total_invoice_amount);
                                    string NBFCStatus = disbursalProcessed.data.invoice.status;
                                    string NBFCUTR = disbursalProcessed.data.utr;
                                    string TopUpNumber = disbursalProcessed.data.topup_id ?? "";

                                    var returnReply = AsyncContext.Run(() => InvoiceDisbursalProcessed(ApplicationId, loan_account, disbursalProcessed_data_invoiceid, disbursalProcessedDisbursementDate, total_invoice_amount, NBFCStatus, NBFCUTR, TopUpNumber));
                                    transactionId = returnReply.Response;

                                    //blackSoilAccountTransactions.Status = status;
                                    //_context.Entry(blackSoilAccountTransactions).State = EntityState.Modified;

                                    //DateTime disbursement_date;
                                    //if (!DateTime.TryParse(disbursalProcessed.data.disbursement_date, out disbursement_date))
                                    //{
                                    //    disbursement_date = DateTime.Now;
                                    //}
                                    //_context.InvoiceDisbursalProcesseds.Add(new InvoiceDisbursalProcessed
                                    //{
                                    //    AccountTransactionId = blackSoilAccountTransactions.Id,
                                    //    LoanAccountId = blackSoilAccountDetails.LoanAccountId,
                                    //    InvoiceDisbursalDate = disbursement_date,
                                    //    amount = Convert.ToDouble(disbursalProcessed.data.invoice.total_invoice_amount),
                                    //    Status = status,
                                    //    IsActive = true,
                                    //    IsDeleted = false
                                    //});

                                    ////----S------ Recalculate DueDate & Intrest According to Disbursement Date  --------------
                                    //var TransactionType_OrderPlacement = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement);

                                    //var accountTransactionExist = await _context.AccountTransactions.Where(x => x.LoanAccountId == loanAccountId && x.InvoiceId == blackSoilAccountTransactions.LoanInvoiceId && x.TransactionTypeId == TransactionType_OrderPlacement.Id && x.IsActive && !x.IsDeleted).ToListAsync();
                                    //foreach (var accountTrans in accountTransactionExist)
                                    //{
                                    //    double OutstandingAmount = 0;
                                    //    var PendingTransactionsList = await _TransactionSettlementManager.GetOutstandingTransactionsList("", accountTrans.LoanAccountId, disbursement_date);
                                    //    if (PendingTransactionsList != null && PendingTransactionsList.Any())
                                    //    {
                                    //        //var transactions = PendingTransactionsList.Where(x => x.Id == accountTrans.Id).ToList();
                                    //        //foreach (var tra in transactions)
                                    //        //{
                                    //        //    OutstandingAmount = tra.Outstanding ?? 0;
                                    //        //}
                                    //        var transactions = PendingTransactionsList.Where(x => x.Id == accountTrans.Id).FirstOrDefault();
                                    //        OutstandingAmount = transactions.Outstanding ?? 0;
                                    //    }


                                    //    accountTrans.DisbursementDate = disbursement_date;
                                    //    accountTrans.DueDate = disbursement_date.AddDays(Convert.ToInt64(accountTrans.CreditDays));
                                    //    _context.Entry(accountTrans).State = EntityState.Modified;

                                    //    var loanAccount = _context.LoanAccounts.FirstOrDefault(x => x.Id == accountTrans.LoanAccountId);

                                    //    double transactionBalanceAmount = 0;
                                    //    var results = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                                    //    double InterestRatePerDay = await results.CalculatePerDayInterest(Convert.ToDouble(accountTrans.InterestRate));
                                    //    var perDayInterestAmount = accountTrans.InterestType == "Percentage" ? (OutstandingAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                                    //    DateTime firstDate = disbursement_date;
                                    //    DateTime secondDate = accountTrans.DueDate.Value;
                                    //    long creditDays = Convert.ToInt64((secondDate.Date - firstDate.Date).Days);
                                    //    var interestTransDetails = await _orderPlacementManager.CalculateIntrest(accountTrans.Id, perDayInterestAmount, Convert.ToInt64(creditDays), disbursement_date, accountTrans.PayableBy);
                                    //    if (interestTransDetails != null && interestTransDetails.Any())
                                    //    {
                                    //        await _context.AccountTransactionDetails.AddRangeAsync(interestTransDetails);

                                    //    }

                                    //}
                                    ////----E------ Recalculate DueDate & Intrest According to Disbursement Date  --------------
                                    //await _context.SaveChangesAsync();
                                    //transactionId = blackSoilAccountTransactions.Id;
                                }

                                break;

                            case BlackSoilWebhookConstant.RepaymentCredited:
                                var repaymentCreditedVM = JsonConvert.DeserializeObject<BlackSoilRepaymentCredited>(request.Request.Data);
                                if (repaymentCreditedVM != null)
                                {

                                    blackSoilAccountDetails = await _context.BlackSoilAccountDetails.Where(x => x.BlackSoilLoanId == repaymentCreditedVM.data.loan_account).FirstOrDefaultAsync();
                                    loanAccountId = blackSoilAccountDetails != null ? blackSoilAccountDetails.LoanAccountId : 0;

                                    var blackSoilRecalculateAccounting = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilRecalculateAccounting && !x.IsDeleted && x.IsActive);
                                    if (blackSoilRecalculateAccounting != null)
                                    {

                                        string url = blackSoilRecalculateAccounting.APIUrl.Replace("{{loan_account_id}}", repaymentCreditedVM.data.loan_account.Value.ToString());
                                        BlackSoilNBFCHelper helper = new BlackSoilNBFCHelper();
                                        var ress = await helper.BlackSoilRecalculateAccounting(url, blackSoilRecalculateAccounting.TAPIKey, blackSoilRecalculateAccounting.TAPISecretKey);
                                        _context.BlackSoilCommonAPIRequestResponses.Add(ress);
                                        await _context.SaveChangesAsync();

                                        if (ress.IsSuccess)
                                        {
                                            var service = _loanNBFCFactory.GetService(LeadNBFCConstants.BlackSoil);
                                            await service.SettlePayment(repaymentCreditedVM.data.id.Value, repaymentCreditedVM.data.loan_account.Value);
                                        }
                                    }
                                    else
                                    {
                                        var service = _loanNBFCFactory.GetService(LeadNBFCConstants.BlackSoil);
                                        await service.SettlePayment(repaymentCreditedVM.data.id.Value, repaymentCreditedVM.data.loan_account.Value);
                                    }
                                }
                                break;

                        }

                        if (loanAccountId > 0)
                        {
                            blackSoilWebhookResponse.LoanAccountId = loanAccountId;
                            blackSoilWebhookResponse.BlackSoilAccountTransactionId = transactionId;
                            _context.Entry(blackSoilWebhookResponse).State = EntityState.Modified;

                            await _context.SaveChangesAsync();
                        }

                        result.Status = true;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return result;
        }

        public async Task<TemplateResponseDc> SaveModifyTemplateMaster(TemplateDc templatedc)
        {
            TemplateResponseDc temp = new TemplateResponseDc();

            if (templatedc != null && templatedc.TemplateCode != null && templatedc.TemplateType != null && templatedc.TemplateID == null)
            {
                var exist = await _context.LoanAccountTemplateMasters.Where(x => x.TemplateType == templatedc.TemplateType && x.TemplateCode == templatedc.TemplateCode && x.IsDeleted == false).FirstOrDefaultAsync();
                if (exist == null)
                {
                    var smstemp = new LoanAccountTemplateMaster
                    {
                        DLTID = templatedc.DLTID,
                        TemplateType = templatedc.TemplateType,
                        TemplateCode = templatedc.TemplateCode,
                        Template = templatedc.Template,
                        IsActive = templatedc.Status,
                        IsDeleted = false,
                        Created = DateTime.Now
                    };
                    _context.LoanAccountTemplateMasters.Add(smstemp);
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

                var data = await _context.LoanAccountTemplateMasters.Where(x => x.Id == templatedc.TemplateID && x.IsDeleted == false).Select(x => x).FirstOrDefaultAsync();
                if (data != null)
                {
                    if (data.TemplateType == "SMS")
                    {
                        data.DLTID = templatedc.DLTID;
                    }
                    data.TemplateCode = templatedc.TemplateCode;
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

        public async Task<GRPCReply<List<GetTemplateMasterListResponseDc>>> GetTemplateMasterAsync()
        {
            GRPCReply<List<GetTemplateMasterListResponseDc>> reply = new GRPCReply<List<GetTemplateMasterListResponseDc>>();
            var leadTemplateList = (await _context.LoanAccountTemplateMasters.Where(x => !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
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
            var leadTemplate = (await _context.LoanAccountTemplateMasters.Where(x => x.Id == request.Request && !x.IsDeleted).Select(x => new GetTemplateMasterListResponseDc
            {
                DLTID = x.DLTID,
                Template = x.Template,
                TemplateCode = x.TemplateCode,
                TemplateType = x.TemplateType,
                IsActive = x.IsActive,
                TemplateId = x.Id
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

        public async Task<GRPCReply<long>> SaveNBFCCompanyAPIs(GRPCRequest<List<CompanyApiReply>> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            if (request.Request != null && request.Request.Any())
            {
                foreach (var item in request.Request)
                {
                    var NbcCode = _context.NBFCCompanyAPIs.Where(x => x.Code == item.Code).FirstOrDefault();

                    if (NbcCode != null)
                    {
                        NbcCode.APIUrl = string.IsNullOrEmpty(item.APIUrl) ? "" : item.APIUrl;
                        NbcCode.TAPISecretKey = string.IsNullOrEmpty(item.TAPISecretKey) ? "" : item.TAPISecretKey;
                        NbcCode.Code = item.Code;
                        NbcCode.TAPIKey = string.IsNullOrEmpty(item.TAPIKey) ? "" : item.TAPIKey;
                        NbcCode.TReferralCode = item.TReferralCode;

                        gRPCReply.Status = false;
                        gRPCReply.Response = 0;
                        gRPCReply.Message = "Already Exists!!";
                        _context.SaveChanges();
                    }
                    else
                    {
                        NBFCCompanyAPI nBFCCompanyAPI = new NBFCCompanyAPI
                        {
                            APIUrl = string.IsNullOrEmpty(item.APIUrl) ? "" : item.APIUrl,
                            Code = item.Code,
                            TAPIKey = string.IsNullOrEmpty(item.TAPIKey) ? "" : item.TAPIKey,
                            TAPISecretKey = string.IsNullOrEmpty(item.TAPISecretKey) ? "" : item.TAPISecretKey,
                            TReferralCode = item.TReferralCode,
                            IsActive = true,
                            IsDeleted = false
                        };
                        _context.NBFCCompanyAPIs.Add(nBFCCompanyAPI);
                        _context.SaveChanges();

                        gRPCReply.Status = true;
                        gRPCReply.Response = nBFCCompanyAPI.Id;
                        gRPCReply.Message = "Succesfully!!";
                    }
                }
            }
            return gRPCReply;

        }


        public async Task<CommonResponse> PostLoanAccountToAnchor(long AccountId)
        {
            CommonResponse commonResponse = new CommonResponse
            {
                status = false,
                Message = "Failed To Notify Anchor !!!"
            };
            var anchorAccount = await _context.LoanAccountCompanyLead.Where(x => x.LoanAccountId == AccountId && x.UserUniqueCode != null && x.UserUniqueCode != "").ToListAsync();
            if (anchorAccount != null && anchorAccount.Any())
            {
                var loan = await _context.LoanAccounts.Where(x => x.Id == AccountId).Select(x => new { LeadId = x.LeadId, CreditLimitAmount = x.LoanAccountCredits.CreditLimitAmount }).FirstOrDefaultAsync();
                var companyIds = anchorAccount.Select(x => x.CompanyId).ToList();

                var anchorCompanies = await _context.CompanyAPIs.Where(x => companyIds.Contains(x.CompanyId) && x.Code == "Disbursement").ToListAsync();

                if (anchorCompanies != null && anchorCompanies.Any())
                {
                    foreach (var account in anchorAccount)
                    {
                        var companyApi = anchorCompanies.FirstOrDefault(x => x.CompanyId == account.CompanyId);
                        if (companyApi != null)
                        {
                            AccountDisbursementNotify disbursementNotify = new AccountDisbursementNotify
                            {
                                AccountId = account.LoanAccountId,
                                CustomerUniqueCode = account.UserUniqueCode,
                                LeadId = loan.LeadId,
                                Status = true,
                                Message = "Loan disburse",
                                CreditLimit = loan.CreditLimitAmount
                            };
                            //var response = await CallAnchorAPI("https://uat.shopkirana.in//api/ScaleUpIntegration/UpdateCustomerAccount", "", "", "", disbursementNotify);
                            var response = await CallAnchorAPI(companyApi.APIUrl, companyApi.Authtype, companyApi.APIKey, companyApi.APISecret, disbursementNotify);
                            var reqmsg = JsonConvert.SerializeObject(disbursementNotify);
                            _context.ThirdPartyRequest.Add(new LoanAccountModels.Transaction.ThirdPartyRequest
                            {
                                CompanyId = companyApi.CompanyId,
                                CompanyAPI = companyApi.Id,
                                IsActive = true,
                                IsDeleted = false,
                                IsError = response.StatusCode == 500,
                                Request = reqmsg,
                                Response = JsonConvert.SerializeObject(response)
                            });
                            if (response.StatusCode == 200)
                            {
                                commonResponse.status = true;
                                commonResponse.Message = "Anchor Notified Successfully";
                            }
                        }

                    }

                    _context.SaveChanges();
                }
            }
            return commonResponse;
        }
        public async Task<DisbursementAnchorResponse> CallAnchorAPI(string ApiUrl, string authtype, string key, string secretKey, AccountDisbursementNotify disbursementNotify)
        {
            DisbursementAnchorResponse disbursementAnchorResponse = new DisbursementAnchorResponse();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                    {
                        string authenticationString = "";
                        if (authtype == "Basic")
                        {
                            authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(key + ":" + secretKey));
                        }
                        else if (authtype == "Authorization")
                        {
                            authenticationString = key + " " + secretKey;
                        }

                        if (!string.IsNullOrEmpty(authenticationString))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue(authtype, authenticationString);
                        }

                        var reqmsg = JsonConvert.SerializeObject(disbursementNotify);

                        request.Content = new StringContent(reqmsg);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        disbursementAnchorResponse.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            disbursementAnchorResponse.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            disbursementAnchorResponse.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                disbursementAnchorResponse.StatusCode = 500;
                disbursementAnchorResponse.Response = ex.ToString();
            }
            return disbursementAnchorResponse;
        }


        public async Task<LoanAccountDetailResponse> GetLoanAccountDetail(long loanAccountId)
        {
            LoanAccountDetailResponse loanAccountDetailResponse = new LoanAccountDetailResponse();
            var LoanAccountId = new SqlParameter("LoanAccountId", loanAccountId);
            var LoanAccount = _context.Database.SqlQueryRaw<GetLoanAccountDetailDTO>("exec GetLoanAccountDetail @LoanAccountId", LoanAccountId).AsEnumerable().FirstOrDefault();

            if (LoanAccount != null)
            {
                double CreditLimit = 0;
                var res = await _loanAccountHelper.GetAvailableCreditLimitByLoanId(loanAccountId, "");

                CreditLimit = res.Response.CreditLimit;

                loanAccountDetailResponse.LoanAccountNumber = LoanAccount.AccountCode;
                loanAccountDetailResponse.ShopName = LoanAccount.ShopName;
                loanAccountDetailResponse.UserName = LoanAccount.CustomerName;
                loanAccountDetailResponse.PhoneNumber = LoanAccount.MobileNo;
                loanAccountDetailResponse.UserId = LoanAccount.AccountCode;
                loanAccountDetailResponse.MobileNumber = LoanAccount.MobileNo;
                loanAccountDetailResponse.CityName = LoanAccount.CityName;
                loanAccountDetailResponse.ProductType = LoanAccount.ProductType;
                loanAccountDetailResponse.LoanImage = LoanAccount.LoanImage;
                loanAccountDetailResponse.IsAccountActive = LoanAccount.IsAccountActive;
                loanAccountDetailResponse.IsBlock = LoanAccount.IsBlock;
                loanAccountDetailResponse.IsBlockComment = LoanAccount.IsBlockComment;
                loanAccountDetailResponse.NBFCIdentificationCode = LoanAccount.NBFCIdentificationCode;
                loanAccountDetailResponse.ThirdPartyLoanCode = LoanAccount.ThirdPartyLoanCode;
                loanAccountDetailResponse.CreditLineInfo = new CreditLineInfo
                {
                    TotalSanctionedAmount = Math.Round(LoanAccount.TotalSanctionedAmount, 2),
                    TotalCreditLimit = Math.Round(LoanAccount.TotalSanctionedAmount, 2),// Math.Round(CreditLimit, 2),
                    UtilizedAmount = Math.Round(LoanAccount.UtilizedAmount, 2),
                    LTDUtilizedAmount = Math.Round(LoanAccount.LTDUtilizedAmount, 2),
                    AvailableLimit = Math.Round(CreditLimit, 2),
                    AvailableLimitPercentage = LoanAccount.TotalSanctionedAmount > 0 ? Math.Round((CreditLimit / LoanAccount.TotalSanctionedAmount) * 100, 2) : 0,
                    PenalAmount = Math.Round(LoanAccount.PenalAmount, 2),
                    ProcessingFee = LoanAccount.ProcessingFee
                };
                loanAccountDetailResponse.Repayments = new Repayments
                {
                    InterestAmount = Math.Round(LoanAccount.InterestRepaymentAmount, 2),
                    OverdueInterestAmount = Math.Round(LoanAccount.OverdueInterestPaymentAmount, 2),
                    PenalInterestAmount = Math.Round(LoanAccount.PenalRePaymentAmount, 2),
                    PrincipalAmount = Math.Round(LoanAccount.PrincipalRepaymentAmount, 2),
                    TotalPaidAmount = Math.Round(LoanAccount.TotalRepayment, 2),
                    BounceRePaymentAmount = Math.Round(LoanAccount.BounceRePaymentAmount, 2),
                    ExtraPaymentAmount = Math.Round(LoanAccount.ExtraPaymentAmount, 2)
                };
                loanAccountDetailResponse.Outstanding = new Outstanding
                {
                    InterestAmount = Math.Round(LoanAccount.InterestOutstanding, 2),
                    OverdueInterestAmount = Math.Round(LoanAccount.OverdueInterestOutStanding, 2),
                    PenalInterestAmount = Math.Round(LoanAccount.PenalOutStanding, 2),
                    PrincipalAmount = Math.Round(LoanAccount.PrincipleOutstanding, 2),
                    TotalOutstandingAmount = Math.Round(LoanAccount.TotalOutStanding, 2)
                };
                loanAccountDetailResponse.CreditLine = new CreditLine
                {
                    TotalCreditLimit = Math.Round(CreditLimit, 2),
                    Percentage = Math.Round(100 - loanAccountDetailResponse.CreditLineInfo.AvailableLimitPercentage, 2),
                    UtilizedAmount = Math.Round(LoanAccount.UtilizedAmount, 2)
                };
                //loanAccountDetailResponse.Transactions = new List<Transaction>();

                loanAccountDetailResponse.Status = true;
                loanAccountDetailResponse.Message = "Data Found";
            }
            else
            {
                loanAccountDetailResponse.Status = false;
                loanAccountDetailResponse.Message = "Data Not Found";
            }
            return loanAccountDetailResponse;

        }

        public async Task<CommonResponse> BlockUnblockAccount(long loanAccountId, bool isBlock, string comment)
        {
            CommonResponse commonResponse = new CommonResponse
            {
                Message = "Failed to Update",
                status = false
            };
            var loanAccount = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == loanAccountId);
            if (loanAccount != null)
            {
                loanAccount.IsBlock = isBlock;
                if (isBlock) loanAccount.IsBlockComment = comment;
                _context.Entry(loanAccount).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    commonResponse.status = true;
                    commonResponse.Message = "Updated Successfully";
                }
            }
            return commonResponse;
        }

        public async Task<CommonResponse> ActiveInActiveAccount(long loanAccountId, bool IsAccountActive)
        {
            CommonResponse commonResponse = new CommonResponse
            {
                Message = "Failed to Update",
                status = false
            };
            var loanAccount = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == loanAccountId);
            if (loanAccount != null)
            {
                loanAccount.IsAccountActive = IsAccountActive;

                _context.Entry(loanAccount).State = EntityState.Modified;
                int rowchanged = await _context.SaveChangesAsync();
                if (rowchanged > 0)
                {
                    commonResponse.status = true;
                    commonResponse.Message = "Updated Successfully";
                }
            }
            return commonResponse;
        }

        public async Task<TemplateMasterResponseDc> GetLoanAccountNotificationTemplate(TemplateMasterRequestDc request)
        {
            TemplateMasterResponseDc Reply = new TemplateMasterResponseDc();
            if (request != null)
            {
                var data = await _context.LoanAccountTemplateMasters.Where(x => x.TemplateCode == request.TemplateCode && x.IsActive && !x.IsDeleted).Select(x =>
                new GetTemplateMasterListResponseDc
                {
                    DLTID = x.DLTID,
                    TemplateCode = request.TemplateCode,
                    IsActive = x.IsActive,
                    TemplateType = x.TemplateType,
                    CreatedDate = x.Created,
                    Template = x.Template,
                    TemplateId = x.Id
                }).FirstOrDefaultAsync();
                if (data != null)
                {
                    Reply.Response = data;
                    Reply.Status = true;
                }
                else
                {
                    Reply.Status = false;
                    Reply.Message = "data not found";
                }
            }
            else
            {
                Reply.Status = false;
                Reply.Message = "empty request";
            }
            return Reply;
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

        public async Task<CommonResponse> NotifyAnchorOrderCanceled(long AccountId)
        {
            CommonResponse commonResponse = new CommonResponse
            {
                status = false,
                Message = "Failed To Notify Anchor Initiate or Intransit Canceled !!!"
            };
            var trasnStatues = new List<string> {
            TransactionStatuseConstants.Initiate,
            TransactionStatuseConstants.Intransit
            };

            var accountTransactions = await _context.AccountTransactions.Where(x => x.LoanAccountId == AccountId
            && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement && trasnStatues.Contains(x.TransactionStatus.Code) && x.IsActive && x.IsActive && !x.IsDeleted).Include(x => x.Invoice).ToListAsync();
            var CanceledtransactionStatuse = await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.IsActive && x.Code == TransactionStatuseConstants.Canceled);

            if (accountTransactions != null && accountTransactions.Any() && CanceledtransactionStatuse != null)
            {
                List<NotifyAnchorOrderCanceled> CancelTrans = new List<NotifyAnchorOrderCanceled>();
                foreach (var item in accountTransactions)
                {
                    item.TransactionStatusId = CanceledtransactionStatuse.Id;
                    _context.Entry(item).State = EntityState.Modified;

                }
                CancelTrans.AddRange(accountTransactions.Select(x => new NotifyAnchorOrderCanceled
                {
                    amount = x.Invoice.TotalTransAmount,
                    OrderNo = x.Invoice.OrderNo,
                    TransactionNo = x.ReferenceId
                }).GroupBy(x => new { x.OrderNo, x.TransactionNo }).Select(
                    x => new NotifyAnchorOrderCanceled
                    {
                        amount = x.Sum(y => y.amount),
                        OrderNo = x.Key.OrderNo,
                        TransactionNo = x.Key.TransactionNo,
                        Comment = "Canceled due to overdue.",
                    }).ToList()
                );

                await _context.SaveChangesAsync();

                var anchorAccount = await _context.LoanAccountCompanyLead.Where(x => x.LoanAccountId == AccountId && x.UserUniqueCode != null && x.UserUniqueCode != "").ToListAsync();
                var companyIds = anchorAccount.Select(x => x.CompanyId).ToList();
                var anchorCompanies = await _context.CompanyAPIs.Where(x => companyIds.Contains(x.CompanyId) && x.Code == "NotifyAnchorOrderCanceled").ToListAsync();
                if (anchorCompanies != null && anchorCompanies.Any())
                {
                    foreach (var account in anchorAccount)
                    {
                        var companyApi = anchorCompanies.FirstOrDefault(x => x.CompanyId == account.CompanyId);
                        if (companyApi != null)
                        {
                            var response = await CallNotifyAnchorOrderCanceled(companyApi.APIUrl, companyApi.Authtype, companyApi.APIKey, companyApi.APISecret, CancelTrans);
                            var reqmsg = JsonConvert.SerializeObject(CancelTrans);

                            _context.ThirdPartyRequest.Add(new LoanAccountModels.Transaction.ThirdPartyRequest
                            {
                                CompanyId = companyApi.CompanyId,
                                CompanyAPI = companyApi.Id,
                                IsActive = true,
                                IsDeleted = false,
                                IsError = response.StatusCode == 500,
                                Request = reqmsg,
                                Response = JsonConvert.SerializeObject(response)
                            });
                            if (response.StatusCode == 200)
                            {
                                commonResponse.status = true;
                                commonResponse.Message = "Anchor Notify Initiate or Intransit Canceled Successfully";
                            }

                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return commonResponse;
        }
        public async Task<DisbursementAnchorResponse> CallNotifyAnchorOrderCanceled(string ApiUrl, string authtype, string key, string secretKey, List<NotifyAnchorOrderCanceled> Notify)
        {
            DisbursementAnchorResponse AnchorResponse = new DisbursementAnchorResponse();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), ApiUrl))
                    {
                        string authenticationString = "";
                        if (authtype == "Basic")
                        {
                            authenticationString = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(key + ":" + secretKey));
                        }
                        else if (authtype == "Authorization")
                        {
                            authenticationString = key + " " + secretKey;
                        }

                        if (!string.IsNullOrEmpty(authenticationString))
                        {
                            request.Headers.Authorization = new AuthenticationHeaderValue(authtype, authenticationString);
                        }

                        var reqmsg = JsonConvert.SerializeObject(Notify);

                        request.Content = new StringContent(reqmsg);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);

                        string jsonString = string.Empty;
                        AnchorResponse.StatusCode = Convert.ToInt32(response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = (await response.Content.ReadAsStringAsync()).Replace("\\", "").Trim(new char[1] { '"' });
                            AnchorResponse.Response = jsonString;
                        }
                        else
                        {
                            jsonString = (await response.Content.ReadAsStringAsync());
                            AnchorResponse.Response = jsonString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AnchorResponse.StatusCode = 500;
                AnchorResponse.Response = ex.ToString();
            }
            return AnchorResponse;
        }


        public async Task<GRPCReply<DisbursmentSMSDetailDC>> GetDisbursmentSMSDetail(GRPCRequest<long> request)
        {
            GRPCReply<DisbursmentSMSDetailDC> reply = new GRPCReply<DisbursmentSMSDetailDC>();
            reply.Status = false;
            reply.Message = "Something went wrong.";
            var blackSoilAccountTransaction = await _context.BlackSoilAccountTransactions.FirstOrDefaultAsync(x => x.Id == request.Request);
            if (blackSoilAccountTransaction != null)
            {
                var TransactionStatusId = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Failed).FirstOrDefault();

                var q = from t in _context.AccountTransactions
                        join d in _context.AccountTransactionDetails on t.Id equals d.AccountTransactionId
                        join l in _context.LoanAccounts on t.LoanAccountId equals l.Id
                        join b in _context.LoanBankDetails on t.LoanAccountId equals b.LoanAccountId
                        where t.TransactionStatusId != TransactionStatusId.Id && t.DueDate.HasValue &&
                        t.IsActive && !t.IsDeleted && d.IsActive && !d.IsDeleted && t.InvoiceId == blackSoilAccountTransaction.LoanInvoiceId
                        group new { t, d, l, b } by new { l.Id, t.DueDate.Value.Date, t.InvoiceNo, l.MobileNo, l.CustomerName, b.AccountNumber } into g
                        select new DisbursmentSMSDetailDC
                        {
                            amount = g.Sum(x => x.d.Amount),
                            BankAccountNo = g.Key.AccountNumber.Substring(g.Key.AccountNumber.Length - 4),
                            DueDate = g.Key.Date,
                            OrderNo = g.Key.InvoiceNo,
                            MobileNo = g.Key.MobileNo,
                            UserName = g.Key.CustomerName
                        };
                var disbursmentSMSDetail = q.FirstOrDefault();
                if (disbursmentSMSDetail != null)
                {
                    reply.Status = true;
                    reply.Response = disbursmentSMSDetail;
                }
            }
            return reply;
        }

        public async Task<CommonResponse> ClearInitiateLimit(long LeadAccountId, long AccountTransactionId)
        {
            CommonResponse res = new CommonResponse();

            var transactionStatuse = _context.TransactionStatuses.Where(x => x.IsActive).ToList();


            var loanaccount = _context.LoanAccounts.Where(x => x.Id == LeadAccountId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).FirstOrDefault();
            if (loanaccount != null)
            {
                double InitiateAmt = 0;
                var accounttransaction = await _context.AccountTransactions.Where(x => x.LoanAccountId == LeadAccountId && x.Id == AccountTransactionId && x.IsActive && !x.IsDeleted && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement
                                           && (x.TransactionStatus.Code == TransactionStatuseConstants.Initiate)).FirstOrDefaultAsync();

                if (accounttransaction != null)
                {
                    InitiateAmt = accounttransaction.TransactionAmount;
                    loanaccount.LoanAccountCredits.CreditLimitAmount += InitiateAmt;

                    accounttransaction.TransactionStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Canceled).Id; ;
                    _context.Entry(accounttransaction).State = EntityState.Modified;
                    if (_context.SaveChanges() > 0)
                    {
                        res.status = true;
                        res.Message = "Limit Updated";
                    }
                    else
                    {
                        res.status = false;
                        res.Message = "Failed";
                    }
                }
            }
            else
            {
                res.status = false;
                res.Message = "Account not found";
            }
            return res;
        }

        public async Task<GRPCReply<bool>> SaveLoanBankDetails(GRPCRequest<List<LeadBankDetailResponse>> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();

            List<LoanBankDetail> bankdetail = new List<LoanBankDetail>();
            foreach (var item in request.Request)
            {
                var loanBankDetails = new LoanBankDetail
                {
                    LoanAccountId = item.LoanAccountId,
                    Type = item.Type,
                    BankName = item.BankName,
                    IFSCCode = item.IFSCCode,
                    AccountType = item.AccountType,
                    AccountNumber = item.AccountNumber,
                    AccountHolderName = item.AccountHolderName,
                    IsActive = true,
                    IsDeleted = false
                };
                bankdetail.Add(loanBankDetails);
            }
            await _context.AddRangeAsync(bankdetail);

            //var loanBankDetails = new LoanBankDetail
            //{
            //    LoanAccountId = request.Request.LoanAccountId,
            //    Type = request.Request.Type,
            //    BankName = request.Request.BankName,
            //    IFSCCode = request.Request.IFSCCode,
            //    AccountType = request.Request.AccountType,
            //    AccountNumber = request.Request.AccountNumber,
            //    AccountHolderName = request.Request.AccountHolderName,
            //    IsActive = true,
            //    IsDeleted = false
            //};
            //await _context.AddAsync(loanBankDetails);

            if (await _context.SaveChangesAsync() > 0)
            {
                reply.Status = true;
                reply.Response = true;
                reply.Message = "Details Saved";
            }
            else
            {
                reply.Status = false;
                reply.Response = false;
                reply.Message = "Details Not Saved";
            }
            return reply;
        }

        public async Task<GRPCReply<long>> SaveLoanAccountCompLead(GRPCRequest<SaveLoanAccountCompLeadReqDC> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            long loanAccountId = 0;

            var LoanAccountTbl = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted);
            if (LoanAccountTbl == null)
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Loan not exists for this lead";
                gRPCReply.Response = 0;
            }
            else
            {
                loanAccountId = LoanAccountTbl.Id;

                var LoanAccountCopLeadTbl = await _context.LoanAccountCompanyLead.FirstOrDefaultAsync(x => x.LoanAccountId == loanAccountId && x.CompanyId == request.Request.CompanyId && x.IsActive && !x.IsDeleted);
                if (LoanAccountTbl != null)
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Company already exists for this lead";
                    gRPCReply.Response = 0;
                }
                else
                {
                    foreach (var item in request.Request.SaveLoanAccountCompanyLeadListDC)
                    {
                        var reply = AsyncContext.Run(() => SaveLoanAccountCompanyLead(new GRPCRequest<SaveLoanAccountCompanyLeadRequestDC>
                        {
                            Request = new SaveLoanAccountCompanyLeadRequestDC
                            {
                                LoanAccountId = loanAccountId,
                                CompanyId = item.CompanyId,
                                LeadProcessStatus = item.LeadProcessStatus,
                                UserUniqueCode = item.UserUniqueCode != null ? item.UserUniqueCode : "",
                                AnchorName = item.AnchorName,
                                LogoURL = item.LogoURL,
                            }
                        }));
                    }

                    gRPCReply.Status = true;
                    gRPCReply.Message = "Success";
                    gRPCReply.Response = 0;
                }
            }

            return gRPCReply;
        }
        public async Task<GRPCReply<List<GetOverdueAccountsResponse>>> GetOverdueAccounts()
        {
            GRPCReply<List<GetOverdueAccountsResponse>> reply = new GRPCReply<List<GetOverdueAccountsResponse>>();
            reply.Response = await _context.Database.SqlQueryRaw<GetOverdueAccountsResponse>("exec SendOverdueMessageJobData").ToListAsync();

            if (reply.Response != null && reply.Response.Any())
            {
                reply.Status = true;
                reply.Message = "Data Found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "Data Not Found";
            }
            return reply;
        }
        public async Task<GRPCReply<List<DisbursmentSMSDetailDC>>> GetDueDisbursmentDetails()
        {
            GRPCReply<List<DisbursmentSMSDetailDC>> reply = new GRPCReply<List<DisbursmentSMSDetailDC>>();
            reply.Response = await _context.Database.SqlQueryRaw<DisbursmentSMSDetailDC>("exec GetDueDisbursmentDetails").ToListAsync();

            if (reply.Response != null && reply.Response.Any())
            {
                reply.Status = true;
                reply.Message = "Data Found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "Data Not Found";
            }
            return reply;
        }

        public async Task<GRPCReply<long>> InvoiceDisbursalProcessed(long ApplicationId, long loan_account, long disbursalProcessed_data_invoiceid, string disbursalProcessedDisbursementDate, double total_invoice_amount, string NBFCStatus, string NBFCUTR, string TopUpNumber)
        {
            GRPCReply<long> reply = new GRPCReply<long>();
            BlackSoilAccountDetail blackSoilAccountDetails = null;
            BlackSoilAccountTransaction blackSoilAccountTransactions = null;

            reply.Status = false;
            reply.Message = "Something went wrong.";
            reply.Response = 0;

            //var disbursalProcessed = JsonConvert.DeserializeObject<BlackSoilInvoiceDisbursalProcessed>(request.Request.Data);
            blackSoilAccountDetails = await _context.BlackSoilAccountDetails.Where(x => x.ApplicationId == ApplicationId).FirstOrDefaultAsync();
            blackSoilAccountDetails.BlackSoilLoanId = loan_account;

            blackSoilAccountTransactions = await _context.BlackSoilAccountTransactions.FirstOrDefaultAsync(x => x.WithdrawalId == disbursalProcessed_data_invoiceid && x.IsActive && !x.IsDeleted);

            //result.Response = blackSoilAccountTransactions.Id;
            string status = "DisbursalProcessed";
            long loanAccountId = blackSoilAccountDetails.LoanAccountId;


            if (blackSoilAccountTransactions != null && !string.IsNullOrEmpty(disbursalProcessedDisbursementDate))
            {
                blackSoilAccountTransactions.Status = status;
                _context.Entry(blackSoilAccountTransactions).State = EntityState.Modified;

                DateConvertHelper dateConvertHelper = new DateConvertHelper();

                DateTime disbursement_date;
                if (!DateTime.TryParse(disbursalProcessedDisbursementDate, out disbursement_date))
                {
                    disbursement_date = dateConvertHelper.GetIndianStandardTime();
                }
                _context.InvoiceDisbursalProcesseds.Add(new InvoiceDisbursalProcessed
                {
                    AccountTransactionId = blackSoilAccountTransactions.Id,
                    LoanAccountId = blackSoilAccountDetails.LoanAccountId,
                    InvoiceDisbursalDate = disbursement_date,
                    amount = Convert.ToDouble(total_invoice_amount),
                    Status = status,
                    //TopUpNumber = TopUpNumber,
                    TopUpNumber = "",
                    IsActive = true,
                    IsDeleted = false
                });
                var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == blackSoilAccountTransactions.LoanInvoiceId);
                if (invoice != null)
                {
                    //invoice.Status = AccountInvoiceStatus.Disbursement.ToString();
                    invoice.Status = AccountInvoiceStatus.Disbursed.ToString();
                    invoice.NBFCStatus = NBFCStatus;
                    invoice.NBFCUTR = NBFCUTR;
                    _context.Entry(invoice).State = EntityState.Modified;
                }
                //----S------ Recalculate DueDate & Intrest According to Disbursement Date  --------------
                var TransactionType_OrderPlacement = await _context.TransactionTypes.FirstOrDefaultAsync(x => x.Code == TransactionTypesConstants.OrderPlacement);
                var TransactionStatusesID_Pending = await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.Code == TransactionStatuseConstants.Pending);
                var accountTransactionExist = await _context.AccountTransactions.Where(x => x.TransactionStatusId == TransactionStatusesID_Pending.Id && x.LoanAccountId == loanAccountId && x.InvoiceId == blackSoilAccountTransactions.LoanInvoiceId && x.TransactionTypeId == TransactionType_OrderPlacement.Id && x.IsActive && !x.IsDeleted).ToListAsync();
                foreach (var accountTrans in accountTransactionExist)
                {
                    double OutstandingAmount = 0;
                    //var PendingTransactionsList = await _TransactionSettlementManager.GetOutstandingTransactionsList("", accountTrans.LoanAccountId, disbursement_date);
                    var PendingTransactionsList = await _TransactionSettlementManager.GetOutstandingTransactionsListForDisbursement("", accountTrans.LoanAccountId, disbursement_date);
                    if (PendingTransactionsList != null && PendingTransactionsList.Any())
                    {
                        //var transactions = PendingTransactionsList.Where(x => x.Id == accountTrans.Id).ToList();
                        //foreach (var tra in transactions)
                        //{
                        //    OutstandingAmount = tra.Outstanding ?? 0;
                        //}
                        var transactions = PendingTransactionsList.Where(x => x.Id == accountTrans.Id).FirstOrDefault();
                        if (transactions != null)
                        {
                            OutstandingAmount = transactions.Outstanding ?? 0;
                        }
                    }


                    accountTrans.DisbursementDate = disbursement_date;
                    accountTrans.DueDate = disbursement_date.AddDays(Convert.ToInt64(accountTrans.CreditDays));
                    _context.Entry(accountTrans).State = EntityState.Modified;

                    if (OutstandingAmount > 0)
                    {
                        var loanAccount = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == accountTrans.LoanAccountId);

                        double transactionBalanceAmount = 0;
                        var results = _loanNBFCFactory.GetService(loanAccount.NBFCIdentificationCode);
                        double InterestRatePerDay = await results.CalculatePerDayInterest(Convert.ToDouble(accountTrans.InterestRate));
                        var perDayInterestAmount = accountTrans.InterestType == "Percentage" ? (OutstandingAmount * InterestRatePerDay / 100.0) : InterestRatePerDay;

                        DateTime firstDate = disbursement_date;
                        DateTime secondDate = accountTrans.DueDate.Value;
                        long creditDays = Convert.ToInt64((secondDate.Date - firstDate.Date).Days);
                        var interestTransDetails = await _orderPlacementManager.CalculateIntrest(accountTrans.Id, perDayInterestAmount, Convert.ToInt64(creditDays), disbursement_date, accountTrans.PayableBy);
                        if (interestTransDetails != null && interestTransDetails.Any())
                        {
                            await _context.AccountTransactionDetails.AddRangeAsync(interestTransDetails);
                        }
                    }
                }
                //----E------ Recalculate DueDate & Intrest According to Disbursement Date  --------------
                if (await _context.SaveChangesAsync() > 0)
                {

                    reply.Status = true;
                    reply.Message = "found records";
                    reply.Response = blackSoilAccountTransactions.Id;

                }
            }
            return reply;
        }

        public async Task<GRPCReply<long>> DisbursementReprocess(long loanAccountId, long invoiceId)
        {
            GRPCReply<long> reply = new GRPCReply<long>();
            long blackSoilLoanId = 0;
            long blackSoilLoanAccountId = 0;

            //DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddMinutes(-15), INDIAN_ZONE);  //DateTime.Now.AddMinutes(-15);

            reply.Status = false;
            reply.Message = "Something went wrong.";
            reply.Response = 0;

            //var blackSoilAccountDetail = await _context.BlackSoilAccountDetails.FirstOrDefaultAsync(x => x.LoanAccountId == loanAccountId && x.IsActive && !x.IsDeleted);
            var blackSoilAccountDetailList = await _context.BlackSoilAccountDetails.Where(x => x.BlackSoilLoanId != 0 && x.IsActive && !x.IsDeleted
            && (loanAccountId == 0 || x.LoanAccountId == loanAccountId)).ToListAsync();

            if (blackSoilAccountDetailList != null && blackSoilAccountDetailList.Count > 0)
            //if (blackSoilAccountDetail != null && blackSoilAccountDetail.BlackSoilLoanId > 0)
            {
                foreach (var blackSoilAccountDetail in blackSoilAccountDetailList)
                {

                    blackSoilLoanId = blackSoilAccountDetail.BlackSoilLoanId;
                    blackSoilLoanAccountId = blackSoilAccountDetail.LoanAccountId; //bsadLoanAccountId

                    var query = from t in _context.AccountTransactions //.Where(x => x.LoanAccountId == loanAccountId)
                                join tt in _context.TransactionTypes on t.TransactionTypeId equals tt.Id
                                join ts in _context.TransactionStatuses on t.TransactionStatusId equals ts.Id
                                join bb in _context.BlackSoilAccountTransactions on t.InvoiceId equals bb.LoanInvoiceId
                                where t.DisbursementDate == null
                                && tt.Code == TransactionTypesConstants.OrderPlacement
                                && ts.Code == TransactionStatuseConstants.Pending
                                && t.IsActive && !t.IsDeleted && tt.IsActive && !tt.IsDeleted && ts.IsActive && !ts.IsDeleted
                                && t.LoanAccountId == blackSoilLoanAccountId
                                && (invoiceId == 0 || bb.WithdrawalId == invoiceId)
                                select bb;

                    var blackSoilAccountTransactions = await query.ToListAsync();
                    if (blackSoilAccountTransactions != null && blackSoilAccountTransactions.Count > 0)
                    {

                        //--------------------ss---------
                        //BlackSoilRepaymentDc blackSoilRepayment = null;
                        BlackSoilLoanAccountExpandDc accountDetails = null;

                        string url = "";
                        var loanAccountDetailAPI = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanAccountDetail && x.IsActive && !x.IsDeleted);
                        if (loanAccountDetailAPI != null)
                        {
                            url = loanAccountDetailAPI.APIUrl;
                            url = url.Replace("{{accountid}}", blackSoilLoanId.ToString()); //item.ThirdPartyLoanAccountId

                            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
                            var response = await blackSoilNBFCHelper.GetLoanAccountDetail(url, loanAccountDetailAPI.TAPIKey, loanAccountDetailAPI.TAPISecretKey, 0);
                            _context.BlackSoilCommonAPIRequestResponses.Add(response);
                            await _context.SaveChangesAsync();
                            if (response != null && response.IsSuccess)
                            {
                                accountDetails = JsonConvert.DeserializeObject<BlackSoilLoanAccountExpandDc>(response.Response);

                                //item.Status = BlackSoilRepaymentConstants.Settled;
                                //_context.Entry(item).State = EntityState.Modified;
                                //_context.SaveChanges();

                                //--------------------ee----------
                                if (accountDetails != null && accountDetails.topups != null && accountDetails.topups.Any())
                                {
                                    foreach (var doc in blackSoilAccountTransactions)
                                    {
                                        //foreach (var topup in accountDetails.topups)
                                        var topup = accountDetails.topups.FirstOrDefault(x => x.invoice.id == doc.WithdrawalId);
                                        if (topup != null)
                                        {
                                            if (doc.WithdrawalId == topup.invoice.id)
                                            {
                                                if (topup.invoice.status == "disbursed")
                                                {
                                                    long ApplicationId = topup.application ?? 0;
                                                    long loan_account = accountDetails.extra.loan_account.HasValue ? accountDetails.extra.loan_account.Value : 0;
                                                    long disbursalProcessed_data_invoiceid = topup.invoice.id ?? 0;
                                                    string disbursalProcessedDisbursementDate = topup.disbursement_date.ToString();
                                                    double total_invoice_amount = Convert.ToDouble(topup.invoice.total_invoice_amount);
                                                    string NBFCStatus = topup.invoice.status;
                                                    string NBFCUTR = topup.utr != null ? topup.utr.ToString() : "";
                                                    string TopUpNumber = topup.topup_id ?? "";

                                                    var returnReply = AsyncContext.Run(() => InvoiceDisbursalProcessed(ApplicationId, loan_account, disbursalProcessed_data_invoiceid, disbursalProcessedDisbursementDate, total_invoice_amount, NBFCStatus, NBFCUTR, TopUpNumber));
                                                    //transactionId = returnReply.Response;

                                                    reply.Status = true;
                                                    reply.Message = "successfully";
                                                    reply.Response = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        reply.Status = false;
                        reply.Message = "blackSoil LoanAccountId / BlackSoilLoanId not maintain";
                        reply.Response = 0;
                    }
                }
            }


            return reply;

        }


        public async Task<GRPCReply<bool>> UpdateTransactionStatusJob()
        {
            GRPCReply<bool> res = new GRPCReply<bool>
            {
                Message = "Failed",
                Response = false,
                Status = false
            };
            var transactionStatuse = _context.TransactionStatuses.Where(x => x.IsActive).ToList();

            var PendingStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Pending)?.Id;
            long DueStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Due).Id;
            long OverDueStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Overdue).Id;
            long DelinquentStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Delinquent).Id;

            int days = 181;
            //var Transactiontypes = _context.TransactionTypes.Where(x => x.IsActive).ToList();
            DateTime currentdate = indianTime;
            var Transactions = await _context.AccountTransactions.Where(x => ((x.DueDate.Value.Date <= currentdate.Date && (x.TransactionStatusId == PendingStatusId || x.TransactionStatusId == DueStatusId))
            || (x.DueDate.Value.Date.AddDays(days) == currentdate.Date && (x.TransactionStatusId == OverDueStatusId))) && x.DisbursementDate.HasValue && x.IsActive && !x.IsDeleted).ToListAsync();
            if (Transactions.Any())
            {
                foreach (var transaction in Transactions)
                {
                    if (transaction.DueDate.Value.Date == currentdate.Date)
                    {
                        transaction.TransactionStatusId = DueStatusId;
                        transaction.LastModified = currentdate;
                    }
                    if (transaction.DueDate.Value.AddDays(1).Date <= currentdate.Date)
                    {
                        transaction.TransactionStatusId = OverDueStatusId;
                        transaction.LastModified = currentdate;
                    }
                    if (transaction.TransactionStatusId == OverDueStatusId && transaction.DueDate.Value.AddDays(days).Date == currentdate.Date)
                    {
                        transaction.TransactionStatusId = DelinquentStatusId;
                        transaction.LastModified = currentdate;
                    }
                    _context.Entry(transaction).State = EntityState.Modified;
                }

                var invoiceIds = Transactions.Select(x => x.InvoiceId).Distinct().ToList();
                var invoices = await _context.Invoices.Where(x => invoiceIds.Contains(x.Id)).ToListAsync();
                foreach (var invoice in invoices)
                {
                    var transStatusId = Transactions.FirstOrDefault(x => x.InvoiceId == invoice.Id).TransactionStatusId;
                    if (DueStatusId == transStatusId)
                        invoice.Status = AccountInvoiceStatus.Due.ToString();
                    else if (OverDueStatusId == transStatusId)
                        invoice.Status = AccountInvoiceStatus.Overdue.ToString();
                    else if (DelinquentStatusId == transStatusId)
                        invoice.Status = AccountInvoiceStatus.Delinquent.ToString();

                    _context.Entry(invoice).State = EntityState.Modified;
                }

                if (_context.SaveChanges() > 0)
                {
                    res.Message = "Success";
                    res.Status = true;
                    res.Response = true;
                }
            }
            return res;
        }

        public async Task<GRPCReply<List<AnchorMISListResponseDC>>> GetAnchorMISList(AnchorMISRequestDC obj)
        {
            GRPCReply<List<AnchorMISListResponseDC>> reply = new GRPCReply<List<AnchorMISListResponseDC>>();
            List<AnchorMISListResponseDC> anchorMISListResponseDCs = new List<AnchorMISListResponseDC>();
            var anchorId = new SqlParameter("AnchorId", obj.AnchorId);
            var status = new SqlParameter("Status", obj.Status);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);


            anchorMISListResponseDCs = _context.Database.SqlQueryRaw<AnchorMISListResponseDC>(" exec GetAnchorMISList @AnchorId,  @Status,@Fromdate,@ToDate", anchorId, status, fromdate, toDate).AsEnumerable().ToList();
            if (anchorMISListResponseDCs != null && anchorMISListResponseDCs.Any())
            {
                reply.Response = anchorMISListResponseDCs;
                reply.Status = true;
                reply.Message = "Data Found";
            }
            else
            {
                reply.Status = false;
                reply.Message = "Not Found";
            }
            return reply;
        }

        public async Task<GRPCReply<List<NbfcMisListResponseDc>>> GetNbfcMISList(NbfcMisListRequestDc obj)
        {
            GRPCReply<List<NbfcMisListResponseDc>> reply = new GRPCReply<List<NbfcMisListResponseDc>>();
            List<NbfcMisListResponseDc> nbfcMISListResponseDCs = new List<NbfcMisListResponseDc>();
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var nbfcCompanyId = new SqlParameter("NbfcCompanyId", obj.NbfcCompanyId);


            nbfcMISListResponseDCs = _context.Database.SqlQueryRaw<NbfcMisListResponseDc>(" exec GetScaleUpInvoiceWithScaleUpShare  @FromDate,@ToDate,@NbfcCompanyId", fromdate, toDate, nbfcCompanyId).AsEnumerable().ToList();
            if (nbfcMISListResponseDCs.Any() && nbfcMISListResponseDCs.Count > 0)
            {
                reply.Response = nbfcMISListResponseDCs;
                reply.Status = true;
                reply.Message = "Data Found";
            }
            else
            {
                //reply.Response = new List<LoanAccountListDc>();
                reply.Status = false;
                reply.Message = "Not Found";
            }
            return reply;
        }
        public async Task<LoanAccountDashboardResponse> ScaleupLoanAccountDashboardDetails(DashboardLoanAccountDetailDc req)
        {
            LoanAccountDashboardResponse loanAccountDetailResponse = new LoanAccountDashboardResponse();
            var CityName = new DataTable();
            CityName.Columns.Add("stringValue");
            foreach (var name in req.CityName)
            {
                var dr = CityName.NewRow();
                dr["stringValue"] = name;
                CityName.Rows.Add(dr);
            }
            var allCityNames = new SqlParameter
            {
                ParameterName = "CityName",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.stringValues",
                Value = CityName
            };
            var AnchorId1 = new DataTable();
            AnchorId1.Columns.Add("IntValue");
            foreach (var id in req.AnchorId)
            {
                var dr = AnchorId1.NewRow();
                dr["IntValue"] = id;
                AnchorId1.Rows.Add(dr);
            }
            var allAnchIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = AnchorId1
            };
            var ProductType = new SqlParameter("ProductType", req.ProductType);
            var FromDate = new SqlParameter("FromDate", req.FromDate);
            var ToDate = new SqlParameter("ToDate", req.ToDate);

            var LoanAccount = _context.Database.SqlQueryRaw<ScaleupleadDashboardReplyDc>
                                ("exec GetLoanAccountDashboardDetails @ProductType, @CityName, @AnchorId, @FromDate, @ToDate"
                                , ProductType, allCityNames, allAnchIds, FromDate, ToDate).AsEnumerable().ToList();

            if (LoanAccount != null)
            {
                ScaleupleadDashboardReplyDc sumRecord = new ScaleupleadDashboardReplyDc();
                foreach (var data in LoanAccount)
                {
                    sumRecord.TotalSanctionedAmount += data.TotalSanctionedAmount;
                    sumRecord.CreditLimitAmount += data.CreditLimitAmount;
                    sumRecord.UtilizedAmount += data.UtilizedAmount;
                    sumRecord.TotalRepayment += data.TotalRepayment;
                    sumRecord.PrincipalRepaymentAmount += data.PrincipalRepaymentAmount;
                    sumRecord.InterestRepaymentAmount += data.InterestRepaymentAmount;
                    sumRecord.OverdueInterestPaymentAmount += data.OverdueInterestPaymentAmount;
                    sumRecord.PenalRePaymentAmount += data.PenalRePaymentAmount;
                    sumRecord.BounceRePaymentAmount += data.BounceRePaymentAmount;
                    sumRecord.TotalOutStanding += data.TotalOutStanding;
                    sumRecord.PrincipleOutstanding += data.PrincipleOutstanding;
                    sumRecord.InterestOutstanding += data.InterestOutstanding;
                    sumRecord.OverdueInterestOutStanding += data.OverdueInterestOutStanding;
                    sumRecord.PenalOutStanding += data.PenalOutStanding;
                }

                loanAccountDetailResponse.CreditLineInfo = new CreditLineInfoDC
                {
                    TotalSanctionedAmount = Math.Round(sumRecord.TotalSanctionedAmount, 2),
                    TotalCreditLimit = Math.Round(sumRecord.TotalSanctionedAmount, 2),
                    UtilizedAmount = Math.Round(sumRecord.UtilizedAmount, 2),
                    AvailableLimit = (Math.Round(sumRecord.TotalSanctionedAmount, 2) - Math.Round(sumRecord.UtilizedAmount, 2)),
                    AvailableLimitPercentage = sumRecord.TotalSanctionedAmount > 0 ? Math.Round(((Math.Round(sumRecord.TotalSanctionedAmount, 2) - Math.Round(sumRecord.UtilizedAmount, 2)) / sumRecord.TotalSanctionedAmount) * 100, 2) : 0,
                    DashboardCreditLimitGrapthDc = new List<DashboardCreditLimitGrapth>(),
                    DashboardUtilizedAmounttGrapthDc = new List<DashboardUtilizedAmounttGrapth>(),
                    DashboardAvailableLimitGrapthDc = new List<DashboardAvailableLimitGrapth>()
                };
                loanAccountDetailResponse.Repayments = new RepaymentsDC
                {
                    TotalPaidAmount = Math.Round(sumRecord.TotalRepayment, 2),
                    PrincipalAmount = Math.Round(sumRecord.PrincipalRepaymentAmount, 2),
                    InterestAmount = Math.Round(sumRecord.InterestRepaymentAmount, 2),
                    OverdueInterestAmount = Math.Round(sumRecord.OverdueInterestPaymentAmount, 2),
                    PenalInterestAmount = Math.Round(sumRecord.PenalRePaymentAmount, 2),
                    BounceRePaymentAmount = Math.Round(sumRecord.BounceRePaymentAmount, 2),
                    //ExtraPaymentAmount = Math.Round(ExtraPaymentAmount, 2)
                    RepaymentsCreditLimitGrapthDc = new List<DashboardCreditLimitGrapth>(),
                    RepaymentsUtilizedAmounttGrapthDc = new List<DashboardUtilizedAmounttGrapth>(),
                    RepaymentsAvailableLimitGrapthDc = new List<DashboardAvailableLimitGrapth>()
                };

                loanAccountDetailResponse.Outstanding = new OutstandingDC
                {
                    TotalOutstandingAmount = Math.Round(sumRecord.TotalOutStanding, 2),
                    PrincipalAmount = Math.Round(sumRecord.PrincipleOutstanding, 2),
                    InterestAmount = Math.Round(sumRecord.InterestOutstanding, 2),
                    OverdueInterestAmount = Math.Round(sumRecord.OverdueInterestOutStanding, 2),
                    PenalInterestAmount = Math.Round(sumRecord.PenalOutStanding, 2),
                    OutstandingCreditLimitGrapthDc = new List<DashboardCreditLimitGrapth>(),
                    OutstandingUtilizedAmounttGrapthDc = new List<DashboardUtilizedAmounttGrapth>(),
                    OutstandingAvailableLimitGrapthDc = new List<DashboardAvailableLimitGrapth>()
                };

                #region graph Code
                //if (true)
                //{
                //    int daysBetween = req.FromDate.Date == req.ToDate.Date ? 0 : (req.ToDate.Date - req.FromDate.Date).Days + 1;
                //    var m = req.FromDate.Date.Month;
                //    var y = req.FromDate.Date.Year;
                //    int days = DateTime.DaysInMonth(y, m);
                //    if (daysBetween == 0)
                //    {
                //        var hourLabelList = new List<string> { "00:00 - 04:00", "04:00 - 08:00", "08:00 - 12:00", "12:00 - 16:00", "16:00 - 20:00", "20:00 - 24:00" };
                //        int a = 0;
                //        foreach (var hour in hourLabelList)
                //        {
                //            //----------------------------------- CreditLimit ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.CreditLineInfo.DashboardCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => x.CreditLimitAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => x.UtilizedAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => (x.TotalSanctionedAmount - x.UtilizedAmount))
                //            });
                //            //----------------------------------- Repayments ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Repayments.RepaymentsCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => x.PrincipalRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => x.InterestRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => (x.OverdueInterestPaymentAmount))
                //            });
                //            //----------------------------------- Outstanding ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Outstanding.OutstandingCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => x.PrincipleOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => x.InterestOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = hour,
                //                YValue = LoanAccount.Where(x => x.Created >= DateTime.Today.AddHours(a) && x.Created <= DateTime.Today.AddHours(a + 4)).Sum(x => (x.OverdueInterestOutStanding))
                //            });
                //            a += 4;
                //        }
                //    }
                //    else if (daysBetween < 7)
                //    {
                //        DateTime b = req.FromDate.Date;
                //        while (b <= req.ToDate.Date)
                //        {
                //            //----------------------------------- CreditLimit ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.CreditLineInfo.DashboardCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.CreditLimitAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.UtilizedAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => (x.TotalSanctionedAmount - x.UtilizedAmount))
                //            });
                //            //----------------------------------- Repayments ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Repayments.RepaymentsCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.PrincipalRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.InterestRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => (x.OverdueInterestPaymentAmount))
                //            });
                //            //----------------------------------- Outstanding ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Outstanding.OutstandingCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.PrincipleOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.InterestOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = b.ToString("dd/MM/yyyy"),
                //                YValue = LoanAccount.Where(x => x.Created.Date == b.Date && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => (x.OverdueInterestOutStanding))
                //            });
                //            b = b.AddDays(1);
                //        }
                //    }
                //    else if (daysBetween <= days)
                //    {
                //        DateTime start = req.FromDate.Date;
                //        DateTime startDate = req.FromDate.Date;
                //        DateTime endDate = req.ToDate.Date;

                //        startDate = startDate.AddDays((int)DayOfWeek.Sunday - (int)startDate.DayOfWeek);

                //        var weekStartEndDateList = new List<DateDC>();
                //        while (startDate <= endDate)
                //        {
                //            DateDC obj = new DateDC();
                //            if (start > startDate)
                //            {
                //                obj.StartDate = start;
                //            }
                //            else
                //            {
                //                obj.StartDate = startDate;
                //            }
                //            DateTime weekEnd = startDate.AddDays(6);
                //            if (weekEnd > endDate)
                //            {
                //                obj.EndDate = endDate;
                //            }
                //            else
                //            {
                //                obj.EndDate = weekEnd;
                //            }
                //            weekStartEndDateList.Add(obj);
                //            startDate = weekEnd.AddDays(1);
                //        }
                //        var i = 0;
                //        foreach (var week in weekStartEndDateList)
                //        {
                //            //----------------------------------- CreditLimit ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.CreditLineInfo.DashboardCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => x.CreditLimitAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => x.UtilizedAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => (x.TotalSanctionedAmount - x.UtilizedAmount))
                //            });
                //            //----------------------------------- Repayments ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Repayments.RepaymentsCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => x.PrincipalRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => x.InterestRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => (x.OverdueInterestPaymentAmount))
                //            });
                //            //----------------------------------- Outstanding ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Outstanding.OutstandingCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => x.PrincipleOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => x.InterestOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = "week " + (i + 1),
                //                YValue = LoanAccount.Where(x => x.Created.Date >= week.StartDate && x.Created.Date <= week.EndDate).Sum(x => (x.OverdueInterestOutStanding))
                //            });
                //            i = i + 1;
                //        }
                //    }
                //    else if (daysBetween > days)
                //    {
                //        var MonthLabelList = new List<string>();
                //        DateTime startDate = req.FromDate.AddMonths(1);
                //        DateTime endDate = req.ToDate.AddMonths(1);

                //        DateTime current = startDate;
                //        while (current < endDate)
                //        {
                //            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(current.Month);
                //            int year = current.Year;
                //            MonthLabelList.Add(monthName + "," + year.ToString());
                //            current = current.AddMonths(1);
                //        }
                //        foreach (var month in MonthLabelList)
                //        {
                //            //----------------------------------- CreditLimit ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.CreditLineInfo.DashboardCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.CreditLimitAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.UtilizedAmount)
                //            });
                //            loanAccountDetailResponse.CreditLineInfo.DashboardAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => (x.TotalSanctionedAmount - x.UtilizedAmount))
                //            });
                //            //----------------------------------- Repayments ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Repayments.RepaymentsCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.PrincipalRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.InterestRepaymentAmount)
                //            });
                //            loanAccountDetailResponse.Repayments.RepaymentsAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => (x.OverdueInterestPaymentAmount))
                //            });
                //            //----------------------------------- Outstanding ------------------------------------------------------------------------------------------------------
                //            loanAccountDetailResponse.Outstanding.OutstandingCreditLimitGrapthDc.Add(new DashboardCreditLimitGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.PrincipleOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingUtilizedAmounttGrapthDc.Add(new DashboardUtilizedAmounttGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => x.InterestOutstanding)
                //            });
                //            loanAccountDetailResponse.Outstanding.OutstandingAvailableLimitGrapthDc.Add(new DashboardAvailableLimitGrapth
                //            {
                //                XValue = month,
                //                YValue = LoanAccount.Where(x => x.Created.ToString("MMM") == month && x.Created.Date >= req.FromDate.Date && x.Created.Date <= req.ToDate.Date).Sum(x => (x.OverdueInterestOutStanding))
                //            });
                //        }
                //    }
                //}
                #endregion 

                var loanAccountdata = await _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted && x.ProductType == req.ProductType).Select(y => new loanAccountDc { LeadId = y.LeadId, ProductType = y.ProductType, IsBlock = y.IsBlock }).ToListAsync();
                loanAccountDetailResponse.loanAccountData = loanAccountdata;

                loanAccountDetailResponse.Status = true;
                loanAccountDetailResponse.Message = "Data Found";
            }
            else
            {
                loanAccountDetailResponse.Status = false;
                loanAccountDetailResponse.Message = "Data Not Found";
            }
            return loanAccountDetailResponse;
        }

        public async Task<GRPCReply<long>> PostArthMateDataLeadToLoan(ArthMateLoanDataResDc req)
        {
            GRPCReply<long> res = new GRPCReply<long>();

            var LoanAccountTbl = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.LeadId == req.leaddc.LeadId && x.IsActive && !x.IsDeleted);

            if (LoanAccountTbl != null)
            {
                res.Status = false;
                res.Message = "Loan already exists for this lead";
                //res.Response = "";
                return res;
            }
            var businessloan = await _context.BusinessLoans.FirstOrDefaultAsync(x => x.LeadMasterId == req.leaddc.LeadId && x.IsActive && !x.IsDeleted);

            if (businessloan != null)
            {
                res.Status = false;
                res.Message = "Business Loan already exists for this lead";
                //res.Response = "";
                return res;
            }
            string accountcode = await GetCurrentNumber("AccountCode");

            LoanAccount loanAccount = new LoanAccount
            {
                LeadId = req.leaddc.LeadId,
                LeadCode = req.leaddc.LeadCode,
                ProductId = req.leaddc.ProductId,
                UserId = req.leaddc.UserName,
                AccountCode = accountcode,
                CustomerName = req.leaddc.CustomerName, //TransactionTypeId.Id,
                MobileNo = req.leaddc.MobileNo,
                NBFCCompanyId = req.leaddc.OfferCompanyId ?? 0,
                AnchorCompanyId = req.leaddc.NBFCCompanyId,
                IsActive = true,
                IsDeleted = false,
                Created = indianTime,
                DisbursalDate = indianTime,
                ApplicationDate = Convert.ToDateTime(req.leaddc.ApplicationDate),
                AgreementRenewalDate = Convert.ToDateTime(req.leaddc.AgreementDate),
                IsDefaultNBFC = false,
                CityName = req.leaddc.CustomerCurrentCityName,
                AnchorName = req.leaddc.AnchorName,
                ProductType = req.leaddc.ProductType,
                IsAccountActive = true,
                IsBlock = false,
                IsBlockComment = "",
                IsBlockHideLimit = false,
                NBFCIdentificationCode = req.leaddc.NBFCCompanyCode,
                ThirdPartyLoanCode = "",
                ShopName = req.leaddc.ShopName,
                CustomerImage = req.leaddc.CustomerImage,
                Type = LoanAccountUserTypeConstants.Customer,
                LoanAccountCredits = new LoanAccountCredit
                {
                    DisbursalAmount = req.Loan.net_disbur_amt ?? 0,
                    CreditLimitAmount = req.Loan.net_disbur_amt ?? 0,
                    IsActive = true,
                    IsDeleted = false,
                    Created = indianTime
                }
            };
            await _context.LoanAccounts.AddAsync(loanAccount);
            _context.SaveChanges();

            #region BusinessLoan

            BusinessLoan LoanData = new BusinessLoan();

            LoanData.LeadMasterId = req.Loan.LeadMasterId;
            LoanData.LoanAccountId = loanAccount.Id;
            LoanData.IsActive = true;
            LoanData.IsDeleted = false;

            LoanData.loan_app_id = req.Loan.loan_app_id ?? "";
            LoanData.loan_id = req.Loan.loan_id ?? "";
            LoanData.borrower_id = req.Loan.borrower_id ?? "";
            LoanData.partner_loan_app_id = req.Loan.partner_loan_app_id ?? "";
            LoanData.partner_loan_id = req.Loan.partner_loan_id ?? "";
            LoanData.partner_borrower_id = req.Loan.partner_borrower_id ?? "";
            LoanData.company_id = req.Loan.company_id ?? 0;
            LoanData.product_id = req.Loan.product_id.ToString() ?? "";
            LoanData.loan_app_date = req.Loan.loan_app_date ?? "";
            LoanData.sanction_amount = req.Loan.sanction_amount ?? 0;
            LoanData.gst_on_pf_amt = req.Loan.gst_on_pf_amt ?? 0;
            LoanData.gst_on_pf_perc = Convert.ToDouble(req.Loan.gst_on_pf_perc);
            LoanData.net_disbur_amt = req.Loan.net_disbur_amt ?? 0;
            LoanData.status = req.Loan.status ?? "";
            LoanData.stage = req.Loan.stage ?? 0;
            LoanData.exclude_interest_till_grace_period = req.Loan.exclude_interest_till_grace_period ?? "";
            LoanData.borro_bank_account_type = req.Loan.borro_bank_account_type ?? "";
            LoanData.borro_bank_account_holder_name = req.Loan.borro_bank_account_holder_name ?? "";
            LoanData.loan_int_rate = "24";//req.Loan.loan_int_rate?? "";
            LoanData.processing_fees_amt = req.Loan.processing_fees_amt ?? 0;
            LoanData.processing_fees_perc = req.Loan.processing_fees_perc ?? 0;
            LoanData.tenure = req.Loan.tenure ?? "";
            LoanData.tenure_type = req.Loan.tenure_type ?? "";
            LoanData.int_type = req.Loan.int_type ?? "";
            LoanData.borro_bank_ifsc = req.Loan.borro_bank_ifsc ?? "";
            LoanData.borro_bank_acc_num = req.Loan.borro_bank_acc_num ?? "";
            LoanData.borro_bank_name = req.Loan.borro_bank_name ?? "";
            LoanData.first_name = req.Loan.first_name ?? "";
            LoanData.last_name = req.Loan.last_name ?? "";
            LoanData.current_overdue_value = req.Loan.current_overdue_value;
            LoanData.bureau_score = req.Loan.bureau_score ?? "";
            LoanData.loan_amount_requested = req.Loan.loan_amount_requested ?? 0;
            LoanData.bene_bank_name = req.Loan.bene_bank_name ?? "";
            LoanData.bene_bank_acc_num = req.Loan.bene_bank_acc_num ?? "";
            LoanData.bene_bank_ifsc = req.Loan.bene_bank_ifsc ?? "";
            LoanData.bene_bank_account_holder_name = req.Loan.bene_bank_account_holder_name ?? "";
            LoanData.created_at = req.Loan.created_at;
            LoanData.updated_at = req.Loan.updated_at;
            LoanData.v = req.Loan.v ?? 0;
            LoanData.co_lender_assignment_id = req.Loan.co_lender_assignment_id ?? 0;
            LoanData.co_lender_id = req.Loan.co_lender_id ?? 0;
            LoanData.co_lend_flag = req.Loan.co_lend_flag ?? "";
            LoanData.itr_ack_no = req.Loan.itr_ack_no ?? "";
            LoanData.penal_interest = req.Loan.penal_interest ?? 0;
            LoanData.bounce_charges = req.Loan.bounce_charges ?? 0;
            LoanData.repayment_type = req.Loan.repayment_type ?? "";
            LoanData.first_inst_date = req.Loan.first_inst_date;
            LoanData.final_approve_date = req.Loan.final_approve_date;
            LoanData.final_remarks = req.Loan.final_remarks ?? "";
            LoanData.foir = req.Loan.foir ?? "";
            LoanData.upfront_interest = req.Loan.upfront_interest ?? "";
            LoanData.business_vintage_overall = req.Loan.business_vintage_overall ?? "";
            LoanData.loan_int_amt = req.Loan.loan_int_amt ?? 0;
            LoanData.conv_fees = req.Loan.conv_fees ?? 0;
            LoanData.ninety_plus_dpd_in_last_24_months = req.Loan.ninety_plus_dpd_in_last_24_months ?? "";
            LoanData.dpd_in_last_9_months = req.Loan.dpd_in_last_9_months ?? "";
            LoanData.dpd_in_last_3_months = req.Loan.dpd_in_last_3_months ?? "";
            LoanData.dpd_in_last_6_months = req.Loan.dpd_in_last_6_months ?? "";
            LoanData.insurance_company = req.Loan.insurance_company ?? "";
            LoanData.credit_card_settlement_amount = req.Loan.credit_card_settlement_amount ?? 0;
            LoanData.emi_amount = req.Loan.emi_amount ?? 0;
            LoanData.emi_allowed = req.Loan.emi_allowed ?? "";
            LoanData.igst_amount = req.Loan.igst_amount ?? 0;
            LoanData.cgst_amount = req.Loan.cgst_amount ?? 0;
            LoanData.sgst_amount = req.Loan.sgst_amount ?? 0;
            LoanData.emi_count = req.Loan.emi_count ?? 0;
            LoanData.broken_interest = req.Loan.broken_interest ?? 0;
            LoanData.dpd_in_last_12_months = req.Loan.dpd_in_last_12_months ?? 0;
            LoanData.dpd_in_last_3_months_credit_card = req.Loan.dpd_in_last_3_months_credit_card ?? 0;
            LoanData.dpd_in_last_3_months_unsecured = req.Loan.dpd_in_last_3_months_unsecured ?? 0;
            LoanData.broken_period_int_amt = req.Loan.broken_period_int_amt ?? 0;
            LoanData.dpd_in_last_24_months = req.Loan.dpd_in_last_24_months ?? 0;
            LoanData.avg_banking_turnover_6_months = 0;// req.Loan.avg_banking_turnover_6_months?? "";
            LoanData.enquiries_bureau_30_days = req.Loan.enquiries_bureau_30_days ?? 0;
            LoanData.cnt_active_unsecured_loans = req.Loan.cnt_active_unsecured_loans ?? 0;
            LoanData.total_overdues_in_cc = req.Loan.total_overdues_in_cc ?? 0;
            LoanData.insurance_amount = req.Loan.insurance_amount;
            LoanData.bureau_outstanding_loan_amt = req.Loan.bureau_outstanding_loan_amt ?? 0;
            LoanData.purpose_of_loan = req.Loan.purpose_of_loan ?? "";
            LoanData.business_name = req.Loan.business_name ?? "";
            LoanData.co_app_or_guar_name = req.Loan.co_app_or_guar_name ?? "";
            LoanData.co_app_or_guar_address = req.Loan.co_app_or_guar_address ?? "";
            LoanData.co_app_or_guar_mobile_no = req.Loan.co_app_or_guar_mobile_no ?? "";
            LoanData.co_app_or_guar_pan = req.Loan.co_app_or_guar_pan ?? "";
            LoanData.business_address_ownership = req.Loan.business_address_ownership ?? "";
            LoanData.business_pan = req.Loan.business_pan ?? "";
            LoanData.bureau_fetch_date = req.Loan.bureau_fetch_date;// req.Loan.bureau_fetch_date?? "";
            LoanData.enquiries_in_last_3_months = req.Loan.enquiries_in_last_3_months ?? 0;
            LoanData.gst_on_conv_fees = req.Loan.gst_on_conv_fees ?? 0;
            LoanData.cgst_on_conv_fees = req.Loan.cgst_on_conv_fees ?? 0;
            LoanData.sgst_on_conv_fees = req.Loan.sgst_on_conv_fees ?? 0;
            LoanData.igst_on_conv_fees = req.Loan.igst_on_conv_fees ?? 0;
            LoanData.interest_type = req.Loan.interest_type ?? "";
            LoanData.conv_fees_excluding_gst = req.Loan.conv_fees_excluding_gst ?? 0;
            LoanData.a_score_request_id = req.Loan.a_score_request_id ?? "";
            LoanData.a_score = req.Loan.a_score ?? 0;
            LoanData.b_score = req.Loan.b_score ?? 0;
            LoanData.offered_amount = req.Loan.offered_amount ?? 0;
            LoanData.offered_int_rate = req.Loan.offered_int_rate;
            LoanData.monthly_average_balance = req.Loan.monthly_average_balance;
            LoanData.monthly_imputed_income = req.Loan.monthly_imputed_income;
            LoanData.party_type = req.Loan.party_type ?? "";
            LoanData.co_app_or_guar_dob = req.Loan.co_app_or_guar_dob; //req.Loan.co_app_or_guar_dob?? "";
            LoanData.co_app_or_guar_gender = req.Loan.co_app_or_guar_gender ?? "";
            LoanData.co_app_or_guar_ntc = req.Loan.co_app_or_guar_ntc ?? "";
            LoanData.udyam_reg_no = req.Loan.udyam_reg_no ?? "";
            LoanData.program_type = req.Loan.program_type ?? "";
            LoanData.written_off_settled = req.Loan.written_off_settled ?? 0;
            LoanData.upi_handle = req.Loan.upi_handle ?? "";
            LoanData.upi_reference = req.Loan.upi_reference ?? "";
            LoanData.fc_offer_days = req.Loan.fc_offer_days ?? 0;
            LoanData.foreclosure_charge = req.Loan.foreclosure_charge ?? "";
            LoanData.eligible_loan_amount = 0;
            LoanData.UrlSlaDocument = "";
            LoanData.UrlSlaUploadSignedDocument = "";
            LoanData.IsUpload = false;
            LoanData.UrlSlaUploadDocument_id = "";
            LoanData.UMRN = "";
            LoanData.abb = "";
            LoanData.application_fees_excluding_gst = "";
            LoanData.bounces_in_three_month = "";
            LoanData.business_address = "";
            LoanData.business_city = "";
            LoanData.business_pin_code = "";
            LoanData.business_state = "";
            LoanData.cgst_on_application_fees = "";
            LoanData.cgst_on_subvention_fees = "";
            LoanData.co_app_or_guar_bureau_score = "";
            LoanData.customer_type_ntc = "";
            LoanData.emi_obligation = "";
            LoanData.gst_number = "";
            LoanData.gst_on_application_fees = "";
            LoanData.gst_on_subvention_fees = "";
            LoanData.igst_on_application_fees = "";
            LoanData.igst_on_subvention_fees = "";
            LoanData.monthly_income = "";
            LoanData.sgst_on_application_fees = "";
            LoanData.sgst_on_subvention_fees = "";
            LoanData.subvention_fees_amount = "";
            LoanData.PlatFormFee = req.Loan.PlatFormFee;
            await _context.BusinessLoans.AddAsync(LoanData);

            #endregion

            if (req.arthmateDisbursementdc != null)
            {
                _context.ArthmateDisbursement.Add(new ArthmateDisbursement
                {
                    Id = req.arthmateDisbursementdc.Id,
                    loan_id = req.arthmateDisbursementdc.loan_id,
                    net_disbur_amt = req.arthmateDisbursementdc.net_disbur_amt,
                    partner_loan_id = req.arthmateDisbursementdc.partner_loan_id,
                    status_code = req.arthmateDisbursementdc.status_code,
                    utr_date_time = req.arthmateDisbursementdc.utr_date_time,
                    utr_number = req.arthmateDisbursementdc.utr_number,
                    CreatedDate = req.arthmateDisbursementdc.CreatedDate,
                    LoanAccountId = loanAccount.Id
                });
            }

            var repayment = await SaveRepaymentScheduleData(req.Loan.loan_id, req.Loan.product_id, req.Loan.company_id ?? 0, req.Loan.first_inst_date, loanAccount.Id);

            BusinessLoanTransactionDc txndata = new BusinessLoanTransactionDc
            {
                AnchorCompanyId = req.leaddc.ProductId,
                ProductId = req.leaddc.NBFCCompanyId,
                Amount = req.Loan.sanction_amount ?? 0,
                BounceCharge = req.Loan.bounce_charges,
                ConvenienceFee = req.Loan.conv_fees ?? 0,
                ProcessingFeeRate = Convert.ToInt32(req.Loan.loan_int_rate),
                CreditDay = 0,
                DelayPenaltyRate = 0,
                GstRate = Convert.ToDouble(req.Loan.gst_on_pf_perc),
                InterestPayableBy = "",
                InterestType = req.Loan.interest_type,
                LoanAccountId = loanAccount.Id,
                TransactionReqNo = "",
                ProcessingFee = req.Loan.processing_fees_amt ?? 0,
                ProcessingFeeGST = req.Loan.gst_on_pf_amt ?? 0,
                InterestRate = 0,
                InsuranceAmount = req.Loan.insurance_amount
            };
            var transaction = await BusinessLoanAccountTransaction(txndata);
            #region SalesAgentLoanDisbursments
            if (!string.IsNullOrEmpty(req.leaddc.LeadCreatedUserId))
            {
                await _context.SalesAgentLoanDisbursments.AddAsync(new LoanAccountModels.DSA.SalesAgentLoanDisbursment
                {
                    DisbursedLoanAccountId = loanAccount.Id,
                    IsProcess = false,
                    IsActive = true,
                    LeadCreatedUserId = req.leaddc.LeadCreatedUserId
                });
            }

            #endregion
            if (_context.SaveChanges() > 0)
            {
                res.Response = loanAccount.Id;
                res.Status = true;
                res.Message = "Success";
            }
            else
            {
                res.Response = loanAccount.Id;
                res.Status = true;
                res.Message = "Failed";
            }

            return res;
        }

        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetBusinessLoanAccountList(LoanAccountListRequestDc obj)
        {
            GRPCReply<List<LoanAccountListResponseDc>> gRPCReply = new GRPCReply<List<LoanAccountListResponseDc>>();
            if (obj.leadIds == null) { obj.leadIds = new List<long>(); }

            if (obj.UserType.ToLower() == UserTypeConstants.AdminUser.ToLower() && obj.UserType.ToLower() != UserTypeConstants.SuperAdmin.ToLower())
            {
                obj.NbfcCompanyId = 0;
            }

            var programtype = new SqlParameter("ProductType", obj.ProductType);
            var status = new SqlParameter("Status", obj.Status);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var cityname = new SqlParameter("CityName", obj.CityName ?? (object)DBNull.Value);
            var keyword = new SqlParameter("Keyword", obj.Keyword);
            //var min = new SqlParameter("Min", obj.Min);
            //var max = new SqlParameter("Max", obj.Max);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            //var anchorId = new SqlParameter("AnchorId", obj.AnchorId);
            var isdsa = new SqlParameter("isDSA", obj.IsDSA);
            //var role = new SqlParameter("Role", obj.Role);
            var nbfcCompanyId = new SqlParameter("NbfcCompanyId", obj.NbfcCompanyId);
            var leadIds = new DataTable();
            leadIds.Columns.Add("IntValue");
            foreach (var lead in obj.leadIds)
            {
                var dr = leadIds.NewRow();
                dr["IntValue"] = lead;
                leadIds.Rows.Add(dr);
            }
            var LeadIds = new SqlParameter
            {
                ParameterName = "leadIds",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = leadIds
            };

            var anchorIds = new DataTable();
            anchorIds.Columns.Add("IntValue");
            foreach (var anchor in obj.AnchorId)
            {
                var dr = anchorIds.NewRow();
                dr["IntValue"] = anchor;
                anchorIds.Rows.Add(dr);
            }
            var AnchorIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = anchorIds
            };
            var list = _context.Database.SqlQueryRaw<LoanAccountListResponseDc>("exec GetBusinessLoanAccountList @ProductType,@Status, @Fromdate,@ToDate,@CityName,@Keyword,@Skip,@Take,@AnchorId,@leadIds,@isDSA,@NbfcCompanyId", programtype, status, fromdate, toDate, cityname, keyword, skip, take, AnchorIds, LeadIds, isdsa, nbfcCompanyId).AsEnumerable().ToList();
            if (list.Any())
            {
                gRPCReply.Response = list;
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<LoanAccountListResponseDc>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> AddUpdateAnchorProductConfig(AddUpdateAnchorProductConfigRequest request)
        {
            GRPCReply<bool> response = new GRPCReply<bool>();
            var existing = await _context.ProductAnchorCompany.Where(x => x.Id == request.Id && x.CompanyId == request.CompanyId && x.ProductId == request.ProductId).FirstOrDefaultAsync();
            if (existing == null && _context.ProductAnchorCompany.Any(x => x.CompanyId == request.CompanyId && x.ProductId == request.ProductId && !x.IsDeleted))
            {
                response.Status = false;
                response.Response = false;
                response.Message = "This Type of Product is Already Exists!!!";
                return response;
            }
            bool isAdded = true;
            if (existing != null)
            {
                //var existingCompanyEmis = await _context.CompanyEMIOptions.Where(x => x.ProductAnchorCompanyId == existing.Id).ToListAsync();
                //if (existingCompanyEmis != null && existingCompanyEmis.Any())
                //{
                //    foreach (var item in existingCompanyEmis)
                //    {
                //        item.IsActive = false;
                //        item.IsDeleted = true;
                //        _context.Entry(item).State = EntityState.Modified;
                //    }
                //}
                //var existingCompanyCreditDays = await _context.CompanyCreditDays.Where(x => x.ProductAnchorCompanyId == existing.Id).ToListAsync();
                //if (existingCompanyCreditDays != null && existingCompanyCreditDays.Any())
                //{
                //    foreach (var item in existingCompanyCreditDays)
                //    {
                //        item.IsActive = false;
                //        item.IsDeleted = true;
                //        _context.Entry(item).State = EntityState.Modified;
                //    }
                //}

                if (existing.AgreementURL == request.AgreementURL && existing.AgreementStartDate == request.AgreementStartDate && existing.AgreementEndDate == request.AgreementEndDate)
                {
                    existing.CompanyId = request.CompanyId;
                    existing.ProductId = request.ProductId;
                    existing.ProcessingFeePayableBy = request.ProcessingFeePayableBy;
                    existing.ProcessingFeeType = request.ProcessingFeeType;
                    existing.ProcessingFeeRate = request.ProcessingFeeRate;
                    existing.AnnualInterestPayableBy = request.AnnualInterestPayableBy;
                    existing.DelayPenaltyRate = request.DelayPenaltyRate;
                    existing.BounceCharge = request.BounceCharge;
                    existing.DisbursementTAT = request.DisbursementTAT;
                    existing.AnnualInterestRate = request.AnnualInterestRate;
                    existing.MinTenureInMonth = request.MinTenureInMonth;
                    existing.MaxTenureInMonth = request.MaxTenureInMonth;
                    existing.EMIBounceCharge = request.EMIBounceCharge;
                    existing.EMIPenaltyRate = request.EMIPenaltyRate;
                    existing.EMIProcessingFeeRate = request.EMIProcessingFeeRate;
                    existing.EMIRate = request.EMIRate;
                    existing.MinLoanAmount = request.MinLoanAmount;
                    existing.MaxLoanAmount = request.MaxLoanAmount;
                    existing.CommissionPayout = request.CommissionPayout;
                    existing.ConsiderationFee = request.ConsiderationFee;
                    existing.DisbursementSharingCommission = request.DisbursementSharingCommission;

                    existing.AgreementDocId = request.AgreementDocId;
                    existing.OfferMaxRate = request.OfferMaxRate;
                    existing.CustomCreditDays = request.CustomCreditDays;
                    existing.BlackSoilReferralCode = request.BlackSoilReferralCode; //new

                    _context.Entry(existing).State = EntityState.Modified;

                    //List<CompanyEMIOptions> emiList = new List<CompanyEMIOptions>();
                    //if (request.CompanyEMIOptions != null && request.CompanyEMIOptions.Any())
                    //{
                    //    foreach (var item in request.CompanyEMIOptions)
                    //    {
                    //        CompanyEMIOptions companyEMIOptions = new CompanyEMIOptions
                    //        {
                    //            EMIOptionMasterId = item.EMIOptionMasterId,
                    //            ProductAnchorCompanyId = existing.Id,
                    //            IsActive = true,
                    //            IsDeleted = false,
                    //        };
                    //        emiList.Add(companyEMIOptions);
                    //    }
                    //    _context.AddRange(emiList);
                    //}

                    //List<CompanyCreditDays> creditDaysList = new List<CompanyCreditDays>();
                    //if (request.CompanyCreditDays != null && request.CompanyCreditDays.Any())
                    //{
                    //    foreach (var item in request.CompanyCreditDays)
                    //    {
                    //        CompanyCreditDays companyCreditDays = new CompanyCreditDays
                    //        {
                    //            CreditDaysMasterId = item.CreditDaysMasterId,
                    //            ProductAnchorCompanyId = existing.Id,
                    //            IsActive = true,
                    //            IsDeleted = false
                    //        };
                    //        creditDaysList.Add(companyCreditDays);
                    //    }
                    //    _context.AddRange(creditDaysList);
                    //}
                    isAdded = false;
                }
                else
                {
                    existing.IsActive = false;
                    existing.IsDeleted = true;
                    _context.Entry(existing).State = EntityState.Modified;
                }
            }
            if (isAdded)
            {
                //List<CompanyEMIOptions> emiList = new List<CompanyEMIOptions>();
                //if (request.CompanyEMIOptions != null && request.CompanyEMIOptions.Any())
                //{
                //    foreach (var item in request.CompanyEMIOptions)
                //    {
                //        CompanyEMIOptions companyEMIOptions = new CompanyEMIOptions
                //        {
                //            EMIOptionMasterId = item.EMIOptionMasterId,
                //            ProductAnchorCompanyId = 0,
                //            IsActive = true,
                //            IsDeleted = false,
                //        };
                //        emiList.Add(companyEMIOptions);
                //    }
                //}
                //List<CompanyCreditDays> creditDaysList = new List<CompanyCreditDays>();
                //if (request.CompanyCreditDays != null && request.CompanyCreditDays.Any())
                //{
                //    foreach (var item in request.CompanyCreditDays)
                //    {
                //        CompanyCreditDays companyCreditDays = new CompanyCreditDays
                //        {
                //            CreditDaysMasterId = item.CreditDaysMasterId,
                //            ProductAnchorCompanyId = 0,
                //            IsActive = true,
                //            IsDeleted = false
                //        };
                //        creditDaysList.Add(companyCreditDays);
                //    }
                //}

                LoanAccountModels.Master.ProductAnchorCompany productAnchorCompany = new LoanAccountModels.Master.ProductAnchorCompany
                {
                    CompanyId = request.CompanyId,
                    ProductId = request.ProductId,
                    ProcessingFeePayableBy = request.ProcessingFeePayableBy,
                    ProcessingFeeType = request.ProcessingFeeType,
                    ProcessingFeeRate = request.ProcessingFeeRate,
                    AnnualInterestPayableBy = request.AnnualInterestPayableBy,
                    DelayPenaltyRate = request.DelayPenaltyRate,
                    BounceCharge = request.BounceCharge,
                    DisbursementTAT = request.DisbursementTAT,
                    AnnualInterestRate = request.AnnualInterestRate,
                    MinTenureInMonth = request.MinTenureInMonth,
                    MaxTenureInMonth = request.MaxTenureInMonth,
                    EMIBounceCharge = request.EMIBounceCharge,
                    EMIPenaltyRate = request.EMIPenaltyRate,
                    EMIProcessingFeeRate = request.EMIProcessingFeeRate,
                    EMIRate = request.EMIRate,
                    MinLoanAmount = request.MinLoanAmount,
                    MaxLoanAmount = request.MaxLoanAmount,
                    CommissionPayout = request.CommissionPayout,
                    ConsiderationFee = request.ConsiderationFee,
                    DisbursementSharingCommission = request.DisbursementSharingCommission,

                    AgreementEndDate = request.AgreementEndDate,
                    AgreementStartDate = request.AgreementStartDate,
                    AgreementURL = request.AgreementURL,
                    AgreementDocId = request.AgreementDocId,
                    OfferMaxRate = request.OfferMaxRate,
                    CustomCreditDays = request.CustomCreditDays,
                    IsActive = true,
                    IsDeleted = false,
                    //CompanyEMIOptions = emiList,
                    //CompanyCreditDays = creditDaysList,
                    BlackSoilReferralCode = request.BlackSoilReferralCode
                };
                _context.Add(productAnchorCompany);
            }
            int rowChanged = await _context.SaveChangesAsync();

            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = isAdded ? "Product Added Successfully" : "Product Updated Successfully";
                response.Response = true;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed to Add/Update Product";
            }
            return response;
        }

        public async Task<GRPCReply<bool>> AddUpdateNBFCProductConfig(AddUpdateNBFCProductConfigRequest request)
        {
            GRPCReply<bool> response = new GRPCReply<bool>();
            var existing = await _context.ProductNBFCCompany.Where(x => x.Id == request.Id && x.CompanyId == request.CompanyId && x.ProductId == request.ProductId).FirstOrDefaultAsync();
            if (existing == null && _context.ProductNBFCCompany.Any(x => x.CompanyId == request.CompanyId && x.ProductId == request.ProductId && !x.IsDeleted))
            {
                response.Status = false;
                response.Response = false;
                response.Message = "This Type of Product is Already Exists!!!";
                return response;
            }
            bool isAdded = false;
            if (existing != null)
            {
                if (existing.AgreementURL == request.AgreementURL && existing.AgreementStartDate == request.AgreementStartDate && existing.AgreementEndDate == request.AgreementEndDate)
                {
                    existing.CompanyId = request.CompanyId;
                    existing.ProductId = request.ProductId;
                    existing.BounceCharges = request.BounceCharges;
                    existing.AnnualInterestRate = request.InterestRate;
                    existing.PenaltyCharges = request.PenaltyCharges;
                    existing.PlatformFee = request.PlatformFee;
                    existing.ProcessingFeeType = request.ProcessingFeeType;
                    existing.ProcessingFee = request.ProcessingFee;
                    existing.CustomerAgreementType = request.CustomerAgreementType;
                    existing.CustomerAgreementURL = request.CustomerAgreementURL;
                    existing.CustomerAgreementDocId = request.CustomerAgreementDocId;
                    existing.AgreementDocId = request.AgreementDocId;
                    existing.SanctionLetterDocId = request.SanctionLetterDocId;
                    existing.SanctionLetterURL = request.SanctionLetterURL;
                    existing.IsInterestRateCoSharing = request.IsInterestRateCoSharing;
                    existing.IsPenaltyChargeCoSharing = request.IsPenaltyChargeCoSharing;
                    existing.IsBounceChargeCoSharing = request.IsBounceChargeCoSharing;
                    existing.IsPlatformFeeCoSharing = request.IsPlatformFeeCoSharing;
                    existing.DisbursementType = request.DisbursementType;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    existing.IsActive = false;
                    existing.IsDeleted = true;
                    _context.Entry(existing).State = EntityState.Modified;

                    LoanAccountModels.Master.ProductNBFCCompany productNBFCCompany = new LoanAccountModels.Master.ProductNBFCCompany
                    {
                        CompanyId = request.CompanyId,
                        ProductId = request.ProductId,
                        BounceCharges = request.BounceCharges,
                        AnnualInterestRate = request.InterestRate,
                        PenaltyCharges = request.PenaltyCharges,
                        PlatformFee = request.PlatformFee,
                        ProcessingFeeType = request.ProcessingFeeType,
                        ProcessingFee = request.ProcessingFee,
                        AgreementEndDate = request.AgreementEndDate,
                        AgreementStartDate = request.AgreementStartDate,
                        AgreementURL = request.AgreementURL,
                        CustomerAgreementType = request.CustomerAgreementType,
                        CustomerAgreementURL = request.CustomerAgreementURL,
                        CustomerAgreementDocId = request.CustomerAgreementDocId,
                        AgreementDocId = request.AgreementDocId,
                        SanctionLetterDocId = request.SanctionLetterDocId,
                        SanctionLetterURL = request.SanctionLetterURL,
                        IsInterestRateCoSharing = request.IsInterestRateCoSharing,
                        IsPenaltyChargeCoSharing = request.IsPenaltyChargeCoSharing,
                        IsBounceChargeCoSharing = request.IsBounceChargeCoSharing,
                        IsPlatformFeeCoSharing = request.IsPlatformFeeCoSharing,
                        DisbursementType = request.DisbursementType,
                        TAPIKey = existing.TAPIKey,
                        TAPISecretKey = existing.TAPISecretKey,
                        TReferralCode = existing.TReferralCode,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.Add(productNBFCCompany);
                    isAdded = true;
                }
            }
            else
            {
                LoanAccountModels.Master.ProductNBFCCompany productNBFCCompany = new LoanAccountModels.Master.ProductNBFCCompany
                {
                    CompanyId = request.CompanyId,
                    ProductId = request.ProductId,
                    BounceCharges = request.BounceCharges,
                    AnnualInterestRate = request.InterestRate,
                    PenaltyCharges = request.PenaltyCharges,
                    PlatformFee = request.PlatformFee,
                    ProcessingFeeType = request.ProcessingFeeType,
                    ProcessingFee = request.ProcessingFee,
                    AgreementEndDate = request.AgreementEndDate,
                    AgreementStartDate = request.AgreementStartDate,
                    AgreementURL = request.AgreementURL,
                    CustomerAgreementType = request.CustomerAgreementType,
                    CustomerAgreementURL = request.CustomerAgreementURL,
                    CustomerAgreementDocId = request.CustomerAgreementDocId,
                    AgreementDocId = request.AgreementDocId,
                    SanctionLetterDocId = request.SanctionLetterDocId,
                    SanctionLetterURL = request.SanctionLetterURL,
                    IsInterestRateCoSharing = request.IsInterestRateCoSharing,
                    IsPenaltyChargeCoSharing = request.IsPenaltyChargeCoSharing,
                    IsBounceChargeCoSharing = request.IsBounceChargeCoSharing,
                    IsPlatformFeeCoSharing = request.IsPlatformFeeCoSharing,
                    DisbursementType = request.DisbursementType,
                    IsActive = true,
                    IsDeleted = false
                };
                _context.Add(productNBFCCompany);
                isAdded = true;
            }
            int rowChanged = await _context.SaveChangesAsync();

            if (rowChanged > 0)
            {
                response.Status = true;
                response.Message = isAdded ? "Product Added Successfully" : "Product Updated Successfully";
                response.Response = true;
            }
            else
            {
                response.Status = false;
                response.Message = "Failed to Add/Update Product";
            }
            return response;
        }
        public async Task<GRPCReply<List<CompanyInvoicesChargesResponseDc>>> GetCompanyInvoiceCharges(CompanyInvoiceChargesRequestDc obj)
        {
            GRPCReply<List<CompanyInvoicesChargesResponseDc>> gRPCReply = new GRPCReply<List<CompanyInvoicesChargesResponseDc>>();

            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var NBFCId = new SqlParameter("NBFCId", obj.NBFCId);
            var InvoiceNo = new SqlParameter("InvoiceNo", obj.InvoiceNo);
            var AnchorId = new DataTable();
            AnchorId.Columns.Add("IntValue");
            foreach (var id in obj.AnchorId)
            {
                var dr = AnchorId.NewRow();
                dr["IntValue"] = id;
                AnchorId.Rows.Add(dr);
            }
            var allAnchIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = AnchorId
            };

            var list = _context.Database.SqlQueryRaw<CompanyInvoicesChargesResponseDc>(" exec GetCompanyInvoicesCharges @NBFCId,@AnchorId,@Fromdate,@ToDate,@InvoiceNo", NBFCId, allAnchIds, fromdate, toDate, InvoiceNo).AsEnumerable().ToList();
            if (list.Any())
            {
                gRPCReply.Response = list;
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<CompanyInvoicesChargesResponseDc>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<List<CompanyInvoicesListResponseDc>>> GetCompanyInvoiceList(CompanyInvoiceRequestDc obj)
        {
            GRPCReply<List<CompanyInvoicesListResponseDc>> gRPCReply = new GRPCReply<List<CompanyInvoicesListResponseDc>>();

            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var NBFCId = new SqlParameter("NBFCId", obj.NBFCId);
            var Status = new SqlParameter("Status", obj.Status);
            var AnchorId = new DataTable();
            AnchorId.Columns.Add("IntValue");
            foreach (var id in obj.AnchorId)
            {
                var dr = AnchorId.NewRow();
                dr["IntValue"] = id;
                AnchorId.Rows.Add(dr);
            }
            var allAnchIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = AnchorId
            };

            var list = _context.Database.SqlQueryRaw<CompanyInvoicesListResponseDc>("exec GetCompanyInvoicesList @NBFCId,@AnchorId,@Fromdate,@ToDate,@Skip,@Take,@Status", NBFCId, allAnchIds, fromdate, toDate, skip, take, Status).AsEnumerable().ToList();
            if (list != null && list.Any())
            {
                foreach (var item in list)
                {
                    if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.Inprocess))
                        item.StatusName = CompanyInvoiceStatusEnum.Inprocess.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.MakerApproved))
                        item.StatusName = CompanyInvoiceStatusEnum.MakerApproved.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerApproved))
                        item.StatusName = CompanyInvoiceStatusEnum.CheckerApproved.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.MakerReject))
                        item.StatusName = CompanyInvoiceStatusEnum.MakerReject.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerReject))
                        item.StatusName = CompanyInvoiceStatusEnum.CheckerReject.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.Settled))
                        item.StatusName = CompanyInvoiceStatusEnum.Settled.ToString();
                }
                gRPCReply.Response = list;
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<CompanyInvoicesListResponseDc>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }

            return gRPCReply;
        }

        public async Task<GRPCReply<List<CompanyInvoiceDetailsResponseDc>>> GetCompanyInvoiceDetails(CompanyInvoiceDetailsRequestDC request)
        {

            GRPCReply<List<CompanyInvoiceDetailsResponseDc>> gRPCReply = new GRPCReply<List<CompanyInvoiceDetailsResponseDc>>();

            var invoiceNo = new SqlParameter("InvoiceNo", request.InvoiceNo);
            var roleName = new SqlParameter("RoleName", request.RoleName);

            var list = _context.Database.SqlQueryRaw<CompanyInvoiceDetailsResponseDc>(" exec GetCompanyInvoiceDetails @InvoiceNo,@RoleName", invoiceNo, roleName).AsEnumerable().ToList();
            if (list != null && list.Any())
            {
                foreach (var item in list)
                {
                    if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.Inprocess))
                        item.StatusName = CompanyInvoiceStatusEnum.Inprocess.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.MakerApproved))
                        item.StatusName = CompanyInvoiceStatusEnum.MakerApproved.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerApproved))
                        item.StatusName = CompanyInvoiceStatusEnum.CheckerApproved.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.MakerReject))
                        item.StatusName = CompanyInvoiceStatusEnum.MakerReject.ToString();
                    else if (item.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerReject))
                        item.StatusName = CompanyInvoiceStatusEnum.CheckerReject.ToString();
                }
                gRPCReply.Response = list;
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<CompanyInvoiceDetailsResponseDc>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }

            return gRPCReply;
        }


        public async Task<GRPCReply<UpdateCompanyInvoiceReplyDC>> UpdateCompanyInvoiceDetails(GRPCRequest<UpdateCompanyInvoiceRequestDC> request)
        {
            GRPCReply<UpdateCompanyInvoiceReplyDC> gRPCReply = new GRPCReply<UpdateCompanyInvoiceReplyDC>();
            gRPCReply.Message = "";
            gRPCReply.Status = false;
            gRPCReply.Response = new UpdateCompanyInvoiceReplyDC { IsPDFGenerate = false };
            if (request.Request != null && request.Request.CompanyInvoiceId > 0)
            {
                var invoicId = request.Request.CompanyInvoiceId;
                var companyInvoice = await _context.CompanyInvoices.Where(x => invoicId == x.Id).Include(x => x.CompanyInvoiceDetails).FirstOrDefaultAsync();

                if (companyInvoice != null)
                {
                    if (request.Request.UserType == CompanyInvoiceUserTypeConstants.MakerUser && request.Request.IsApproved)
                    {
                        if (!(companyInvoice.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.Inprocess) || companyInvoice.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.MakerReject) || companyInvoice.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerReject)))
                        {
                            gRPCReply.Message = "Company Invoice not Inprogress or Rejected Status.";
                            return gRPCReply;
                        }
                        else
                        {
                            companyInvoice.Status = Convert.ToInt32(CompanyInvoiceStatusEnum.MakerApproved);
                        }
                    }
                    if (request.Request.UserType == CompanyInvoiceUserTypeConstants.MakerUser && !request.Request.IsApproved)
                    {
                        if (!(companyInvoice.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.Inprocess)))
                        {
                            gRPCReply.Message = "Company Invoice not Inprogress Status.";
                            return gRPCReply;
                        }
                        else
                        {
                            companyInvoice.Status = Convert.ToInt32(CompanyInvoiceStatusEnum.MakerReject);
                        }
                    }

                    if (request.Request.UserType == CompanyInvoiceUserTypeConstants.CheckerUser)
                    {
                        if ((companyInvoice.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.MakerApproved) || companyInvoice.Status == Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerReject)))
                        {
                            companyInvoice.Status = request.Request.IsApproved ? Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerApproved) : Convert.ToInt32(CompanyInvoiceStatusEnum.CheckerReject);
                        }
                        else
                        {
                            gRPCReply.Message = "Company Invoice not Maker approved or Checker Reject Status.";
                            return gRPCReply;
                        }
                    }

                    if (companyInvoice.CompanyInvoiceDetails != null && companyInvoice.CompanyInvoiceDetails.Any())
                    {
                        foreach (var companyInvoiceDetail in companyInvoice.CompanyInvoiceDetails)
                        {
                            var req = request.Request.UpdateCompanyInvoiceDetailsRequestDC.FirstOrDefault(x => x.AccountTransactionId == companyInvoiceDetail.AccountTransactionId);
                            if (req != null)
                            {
                                companyInvoiceDetail.IsActive = req.Active;
                                companyInvoiceDetail.IsDeleted = !req.Active;
                                _context.Entry(companyInvoiceDetail).State = EntityState.Modified;
                            }

                        }

                        //companyInvoice.InvoiceAmount = companyInvoice.CompanyInvoiceDetails.Where(x => x.IsActive && !x.IsDeleted).Sum(x => x.TotalAmount);

                        companyInvoice.InvoiceAmount = Math.Round(companyInvoice.CompanyInvoiceDetails.Where(x => x.IsActive && !x.IsDeleted)
                                                                    //.Sum(x => x.ScaleupShare) * 0.91, 2);
                                                                    .Sum(x => x.ScaleupShare), 2);

                        if (request.Request.UserType == CompanyInvoiceUserTypeConstants.CheckerUser)
                        {
                            companyInvoice.PaymentVerifierUser = request.Request.UserId;
                        }
                        if (request.Request.UserType == CompanyInvoiceUserTypeConstants.MakerUser)
                        {
                            companyInvoice.PaymentCheckerUser = request.Request.UserId;

                        }
                        _context.Entry(companyInvoice).State = EntityState.Modified;
                    }

                    int rowChanged = await _context.SaveChangesAsync();
                    if (rowChanged > 0)
                    {
                        if (request.Request.UserType == CompanyInvoiceUserTypeConstants.CheckerUser && request.Request.IsApproved && string.IsNullOrEmpty(companyInvoice.InvoiceUrl))
                        {
                            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
                            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();

                            //var now = DateTime.Now;
                            //var DaysInMonth = DateTime.DaysInMonth(companyInvoice.InvoiceDate.Year, companyInvoice.InvoiceDate.Month);
                            //var lastDay = new DateTime(companyInvoice.InvoiceDate.Year, companyInvoice.InvoiceDate.Month, DaysInMonth);

                            gRPCReply.Response.IsPDFGenerate = true;
                            gRPCReply.Response.InvoicePdfdetailDC = new InvoicePdfdetailDC
                            {
                                //InvoiceDate = companyInvoice.InvoiceDate.ToString("MMM d, yyyy"),

                                InvoiceDate = currentDateTime.ToString("MMM d, yyyy"),
                                InvoiceNo = companyInvoice.InvoiceNo,
                                InvoiceParticulars = "",
                                InvoiceRate = companyInvoice.InvoiceAmount,
                                InvoiceTaxableValue = companyInvoice.InvoiceAmount,
                                //DueDate = companyInvoice.InvoiceDate.AddDays(15).ToString("MMM d, yyyy"),
                                DueDate = currentDateTime.AddDays(15).ToString("MMM d, yyyy"),


                                InvoicePeriodFrom = companyInvoice.InvoiceDate.ToString("MMM yyyy"),
                                InvoicePeriodTo = companyInvoice.InvoiceDate.ToString("MMM yyyy"),
                                //InvoicePeriodFrom = companyInvoice.InvoiceDate.ToString("MMM") + ",01 " + companyInvoice.InvoiceDate.ToString("yyyy"),
                                //InvoicePeriodTo = companyInvoice.InvoiceDate.ToString("MMM") + "," + DateTime.DaysInMonth(companyInvoice.InvoiceDate.Year, companyInvoice.InvoiceDate.Month) + " " + companyInvoice.InvoiceDate.ToString("yyyy"),
                                NBFCCompanyId = companyInvoice.CompanyId


                            };
                        }
                        gRPCReply.Status = true;
                        gRPCReply.Message = "Invoice Details Updated";
                    }
                }
            }

            return gRPCReply;
        }


        public async Task<GRPCReply<bool>> UpdateCompnayInvoicePdfPath(GRPCRequest<UpdateCompanyInvoicePDFDC> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool> { Status = false };
            var companyInvoice = await _context.CompanyInvoices.Where(x => request.Request.CompanyInvoiceId == x.Id).FirstOrDefaultAsync();
            if (companyInvoice != null)
            {
                companyInvoice.InvoiceUrl = request.Request.PDFPath;
                _context.Entry(companyInvoice).State = EntityState.Modified;
                if ((await _context.SaveChangesAsync()) > 0)
                {
                    reply.Status = true;
                }
            }

            return reply;
        }


        public async Task<GRPCReply<List<GetInvoiceRegisterDataResponse>>> GetInvoiceRegisterData(GRPCRequest<GetInvoiceRegisterDataRequest> request)
        {
            GRPCReply<List<GetInvoiceRegisterDataResponse>> reply = new GRPCReply<List<GetInvoiceRegisterDataResponse>> { Message = "Data Not Found!!!" };

            reply.Response = await _context.CompanyInvoices.Where(x => request.Request.StartDate <= x.InvoiceDate && request.Request.EndDate >= x.InvoiceDate && (x.Status == Convert.ToInt16(CompanyInvoiceStatusEnum.CheckerApproved) || x.Status == Convert.ToInt16(CompanyInvoiceStatusEnum.Settled)) && x.IsActive && !x.IsDeleted).Select(x => new GetInvoiceRegisterDataResponse
            {
                InvoiceDate = x.InvoiceDate.ToString("dd/MM/yyyy"),
                InvoiceNumber = x.InvoiceNo,
                Month = x.InvoiceDate.ToString("MMM yyyy"),
                CustomerName = "",
                CustomerCityState = "",
                CustomerGSTIN = "",
                Description = "Credit Facilitation Service Fees",
                SAC = "",
                TaxableAmount = x.InvoiceAmount,
                CGSTPercentage = 0,
                SGSTPercentage = 0,
                IGSTPercentage = 0,
                CGSTAmount = 0,
                SGSTAmount = 0,
                IGSTAmount = 0,
                InvoiceAmount = 0,
                TDSAmount = 0,
                NetReceivable = 0,
                Status = "Invoice Raised",
                NBFCCompanyId = x.CompanyId
            }).ToListAsync();
            if (reply.Response != null && reply.Response.Any())
            {
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        public async Task<ResultViewModel<BusinessLoanDetailDc>> GetBusinessLoanDetails(long loanAccountId)
        {
            ResultViewModel<BusinessLoanDetailDc> reply = new ResultViewModel<BusinessLoanDetailDc>() { Message = "Data Not Found!!!" };
            var loanAccountDetails = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == loanAccountId && x.IsActive && !x.IsDeleted);
            //var businessLoanDetail = await _context.BusinessLoans.FirstOrDefaultAsync(x => x.LoanAccountId == loanAccountId && x.IsActive && !x.IsDeleted);
            var blDetail = await _context.BusinessLoanDisbursementDetail.FirstOrDefaultAsync(x => x.LoanAccountId == loanAccountId && x.IsActive && !x.IsDeleted);
            if (blDetail != null && loanAccountDetails != null)
            {
                double InsuranceAmount = blDetail.InsuranceAmount ?? 0;
                double othercharges = blDetail.OtherCharges ?? 0;

                reply.Result = new BusinessLoanDetailDc
                {
                    LoanAccountId = blDetail.LoanAccountId,
                    emi_amount = blDetail.MonthlyEMI,
                    first_name = loanAccountDetails.CustomerName,
                    gst_on_pf_amt = blDetail.ProcessingFeeTax,
                    gst_on_pf_perc = blDetail.GST,

                    loan_id = blDetail.LoanId ?? "",
                    loan_int_amt = blDetail.LoanInterestAmount,
                    loan_int_rate = blDetail.InterestRate.ToString(),
                    //net_disbur_amt = blDetail.LoanAmount - (blDetail.ProcessingFeeAmount + blDetail.PFDiscount + blDetail.ProcessingFeeTax),
                    net_disbur_amt = blDetail.LoanAmount - ((blDetail.ProcessingFeeAmount - blDetail.PFDiscount) + blDetail.ProcessingFeeTax + InsuranceAmount + othercharges),

                    processing_fees_amt = blDetail.ProcessingFeeAmount,
                    processing_fees_perc = blDetail.ProcessingFeeRate,
                    sanction_amount = blDetail.LoanAmount,
                    tenure = blDetail.Tenure.ToString(),

                    AccountCode = loanAccountDetails.AccountCode,
                    CityName = loanAccountDetails.CityName ?? "",
                    CustomerName = loanAccountDetails.CustomerName,
                    MobileNo = loanAccountDetails.MobileNo,
                    NBFCIdentificationCode = loanAccountDetails.NBFCIdentificationCode,
                    ProductType = loanAccountDetails.ProductType,
                    ShopName = loanAccountDetails.ShopName ?? "",
                    ThirdPartyLoanCode = loanAccountDetails.ThirdPartyLoanCode ?? "",
                    PfDiscount = blDetail.PFDiscount,


                    InsuranceAmount = InsuranceAmount,
                    Othercharges = othercharges,
                    broken_period_int_amt = blDetail.brokenPeriodinterestAmount
                };

                reply.IsSuccess = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        #region DSA
        public async Task<GRPCReply<DSADashboardLoanResponse>> GetDSALoanDashboardData(GRPCRequest<DSALoanDashboardDataRequest> request)
        {
            GRPCReply<DSADashboardLoanResponse> reply = new GRPCReply<DSADashboardLoanResponse>
            {
                Message = "Data Not found",
                Status = false,
                Response = new DSADashboardLoanResponse()
            };
            if (request.Request.LeadIds != null && request.Request.LeadIds.Any())
            {
                var agentUserId = new SqlParameter("AgentUserId", request.Request.AgentUserId);
                var leadIds = new DataTable();
                leadIds.Columns.Add("IntValue");
                foreach (var lead in request.Request.LeadIds)
                {
                    var dr = leadIds.NewRow();
                    dr["IntValue"] = lead;
                    leadIds.Rows.Add(dr);
                }
                var LeadIds = new SqlParameter
                {
                    ParameterName = "leadIds",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = leadIds
                };
                var payoutData = _context.Database.SqlQueryRaw<LoanPayoutDetailDc>("exec GetDSALoanPayoutList @leadIds,@agentUserId", LeadIds, agentUserId).ToList();

                if (payoutData != null && payoutData.Any())
                {
                    reply.Response.TotalDisbursedAmount = payoutData.Sum(x => x.DisbursmentAmount ?? 0);
                    reply.Response.TotalPayoutAmount = payoutData.Sum(x => x.PayoutAmount);

                    reply.Status = true;
                    reply.Message = "Data Found";
                }
            }
            return reply;
        }


        public async Task<GRPCReply<List<GetSalesAgentLoanDisbursmentDc>>> GetSalesAgentLoanDisbursment()
        {
            GRPCReply<List<GetSalesAgentLoanDisbursmentDc>> gRPCReply = new GRPCReply<List<GetSalesAgentLoanDisbursmentDc>> { Message = "Data not found" };
            var _SalesAgentLoanDisbursments = await _context.SalesAgentLoanDisbursments.Where(x => !x.IsProcess && x.IsActive && !x.IsDeleted).Select(x => new GetSalesAgentLoanDisbursmentDc
            {
                DisbursedLoanAccountId = x.DisbursedLoanAccountId,
                LeadCreatedUserId = x.LeadCreatedUserId
            }).ToListAsync();

            if (_SalesAgentLoanDisbursments != null && _SalesAgentLoanDisbursments.Any())
            {

                gRPCReply.Response = _SalesAgentLoanDisbursments;
                gRPCReply.Status = true;

            }
            return gRPCReply;
        }



        public async Task<GRPCReply<long>> AddDSALoanAccount(GRPCRequest<AddDSALoanAccountRequest> request)
        {
            GRPCReply<long> reply = new GRPCReply<long> { Message = "Data not saved" };
            string accountcode = await GetCurrentNumber("AccountCode");
            LoanAccount loanAccount = new LoanAccount
            {
                LeadId = request.Request.LeadId,
                LeadCode = request.Request.LeadCode,
                ProductId = request.Request.ProductId,
                UserId = request.Request.UserId,
                AccountCode = accountcode,
                CustomerName = request.Request.CustomerName,
                MobileNo = request.Request.MobileNo,
                NBFCCompanyId = request.Request.NBFCCompanyId,
                AnchorCompanyId = request.Request.AnchorCompanyId,
                IsDefaultNBFC = request.Request.IsDefaultNBFC,
                CityName = request.Request.CityName,
                AnchorName = request.Request.AnchorName,
                ProductType = request.Request.ProductType,
                IsAccountActive = request.Request.IsAccountActive,
                IsBlock = request.Request.IsBlock,
                NBFCIdentificationCode = request.Request.NBFCCompanyCode,
                ShopName = request.Request.ShopName,
                CustomerImage = request.Request.CustomerImage,
                ThirdPartyLoanCode = request.Request.ThirdPartyLoanCode,
                ApplicationDate = request.Request.ApplicationDate,
                AgreementRenewalDate = request.Request.AgreementRenewalDate,
                Type = request.Request.Type,
                DisbursalDate = request.Request.DisbursalDate,
                IsBlockComment = "",
                IsBlockHideLimit = false,
                IsActive = true,
                IsDeleted = false
            };
            _context.LoanAccounts.Add(loanAccount);
            if (_context.SaveChanges() > 0)
            {
                reply.Status = true;
                reply.Message = "Data Saved";
                reply.Response = loanAccount.Id;
            }
            return reply;
        }

        public async Task<GRPCReply<List<EmailAnchorMISDataJobResponse>>> GetEmailAnchorMISDataJob()
        {
            GRPCReply<List<EmailAnchorMISDataJobResponse>> reply = new GRPCReply<List<EmailAnchorMISDataJobResponse>> { Message = "Data Not Found" };
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            var fromDate = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
            var toDate = currentDateTime.AddDays(-1).Date;
            if (currentDateTime.Day == 1)
            {
                fromDate = new DateTime(currentDateTime.Year, currentDateTime.Month - 1, 1);
                toDate = currentDateTime.AddDays(-1).Date;
            }
            var misDataList = await GetAnchorMISList(new AnchorMISRequestDC
            {
                AnchorId = 0,
                Status = "All",
                FromDate = fromDate,
                ToDate = toDate
            });
            if (misDataList.Status && misDataList.Response != null)
            {
                reply.Response = new List<EmailAnchorMISDataJobResponse>();
                foreach (var misData in misDataList.Response.GroupBy(x => x.AnchorCompanyId))
                {
                    reply.Response.Add(new EmailAnchorMISDataJobResponse
                    {
                        AnchorCompanyId = misData.Key,
                        NBFCCompanyId = misData.First().NBFCCompanyId,
                        AnchorMISExcelDatas = misData.Select(x => new AnchorMISExcelDC
                        {
                            BeneficiaryAccountNumber = x.BeneficiaryAccountNumber,
                            BeneficiaryName = x.BeneficiaryName,
                            BusinessName = x.BusinessName,
                            DisbursalAmount = x.DisbursalAmount,
                            DisbursementDate = x.DisbursementDate != null ? x.DisbursementDate.Value.ToString("dd/MM/yyyy") : "NULL",
                            GST = x.GST,
                            InvoiceAmount = x.InvoiceAmount,
                            InvoiceDate = x.InvoiceDate != null ? x.InvoiceDate.Value.ToString("dd/MM/yyyy") : "NULL",
                            InvoiceNo = x.InvoiceNo,
                            InvoiceStatus = x.InvoiceStatus,
                            NBFCName = x.NBFCName,
                            OrderNo = x.OrderNo,
                            ReferenceId = x.ReferenceId,
                            ServiceFee = x.ServiceFee,
                            AnchorCode = x.AnchorCode,
                            AnchorName = x.AnchorName,
                            LoanID = x.LoanID,
                            Status = x.Status,
                            UTR = x.UTR
                        }).ToList()
                    });
                }
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }
        [AllowAnonymous]
        public async Task<GRPCReply<bool>> SaveSalesAgentLoanDisbursmentData(GRPCRequest<SalesAgentDisbursmentDataDc> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();

            List<string> transactionDetailHeads = new List<string>
                                                {
                                                          TransactionDetailHeadsConstants.Commission,
                                                          TransactionDetailHeadsConstants.Gst18,
                                                          TransactionDetailHeadsConstants.Tds5,
                                                          TransactionDetailHeadsConstants.DisbursementAmount,
                                                };

            var TransactionTypes = await _context.TransactionTypes.Where(x => x.Code == TransactionTypesConstants.Commission || x.Code == TransactionTypesConstants.Disbursement).ToListAsync();
            var TransactionStatusId = await _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Pending).Select(x => x.Id).FirstOrDefaultAsync();
            var TransactionDetailHeadlist = await _context.TransactionDetailHeads.Where(x => transactionDetailHeads.Contains(x.Code) && x.IsActive && !x.IsDeleted).ToListAsync();
            long DisbursementHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.DisbursementAmount).Id;

            //List<long> DisbursedLoandIds = request.Request.GetSalesAgentLoanDisbursments.Select(x => x.DisbursedLoanAccountId).Distinct().ToList();
            var salesagentIds = request.Request.SalesAgentDetails.Select(x => x.UserId).ToList();
            var mnth = indianTime.Month;
            var year = indianTime.Year;
            var salesAgentLoanDisbursments = await _context.SalesAgentLoanDisbursments.Where(x => salesagentIds.Contains(x.LeadCreatedUserId) && x.Created.Month == mnth && x.Created.Year == year).ToListAsync();
            List<long> DisbursedLoandIds = salesAgentLoanDisbursments.Select(x => x.DisbursedLoanAccountId).Distinct().ToList();
            var Disbursedaccounts = await _context.LoanAccounts.Where(x => DisbursedLoandIds.Contains(x.Id)).ToListAsync();

            //var salesAgentLoanDisbursments = await _context.SalesAgentLoanDisbursments.Where(x => DisbursedLoandIds.Contains(x.DisbursedLoanAccountId)).ToListAsync();

            foreach (var item in request.Request.GetSalesAgentLoanDisbursments)
            {
                long DSAorConnectorRecalculateLoanId = 0;
                double DSAorConnectorRecalculatePayout = 0;
                var salesAgentLoanDisbursment = salesAgentLoanDisbursments.Where(x => x.DisbursedLoanAccountId == item.DisbursedLoanAccountId).FirstOrDefault();
                if (salesAgentLoanDisbursment != null && !salesAgentLoanDisbursment.IsProcess)
                {
                    var Disbursedaccount = Disbursedaccounts.Where(x => x.Id == item.DisbursedLoanAccountId).FirstOrDefault();
                    long DisbTypeId = TransactionTypes.First(c => c.Code == TransactionTypesConstants.Disbursement).Id;
                    var salesagent = request.Request.SalesAgentDetails.Where(x => x.UserId == item.LeadCreatedUserId).FirstOrDefault();
                    if (Disbursedaccount != null && salesagent != null)
                    {
                        var BusinessAccountTransaction = await _context.AccountTransactions.Where(x => x.LoanAccountId == Disbursedaccount.Id && x.TransactionTypeId == DisbTypeId).Include(x => x.AccountTransactionDetails).FirstOrDefaultAsync();
                        var mnthLoanIds = salesAgentLoanDisbursments.Where(x => x.LeadCreatedUserId == item.LeadCreatedUserId).Select(x => x.DisbursedLoanAccountId).ToList();
                        var mnthTotalDisbAmt = await _context.AccountTransactions.Where(x => mnthLoanIds.Contains(x.LoanAccountId) && x.TransactionTypeId == DisbTypeId).SumAsync(x => x.OrderAmount);

                        double payoutpercentage = 0;

                        var payout = salesagent.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId && mnthTotalDisbAmt >= x.MinAmount && mnthTotalDisbAmt <= x.MaxAmount).FirstOrDefault();
                        if (payout == null)
                        {
                            if (mnthTotalDisbAmt > salesagent.SalesAgentProductPayouts.Max(x => x.MaxAmount))
                                payout = salesagent.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId).OrderByDescending(x => x.PayOutPercentage).FirstOrDefault();
                            else
                                payout = salesagent.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId).OrderBy(x => x.PayOutPercentage).FirstOrDefault();
                        }
                        if (salesagent.Type == DSAProfileTypeConstants.DSAUser)
                        {
                            // User Calculation                            

                            var SalesAgentaccount = await _context.LoanAccounts.Where(x => x.ProductId == Disbursedaccount.ProductId && x.Type == LoanAccountUserTypeConstants.DSAUser && x.UserId == item.LeadCreatedUserId).FirstOrDefaultAsync();
                            if (SalesAgentaccount != null && !await _context.AccountTransactions.AnyAsync(x => x.IsActive && !x.IsDeleted && x.ParentAccountTransactionsID == BusinessAccountTransaction.Id && x.LoanAccountId == SalesAgentaccount.Id))
                            {
                                payoutpercentage = payout.PayOutPercentage;
                                AccountTransaction accountTransaction = new AccountTransaction
                                {
                                    AnchorCompanyId = SalesAgentaccount.AnchorCompanyId,
                                    LoanAccountId = SalesAgentaccount.Id,
                                    ReferenceId = "C_" + Disbursedaccount.Id.ToString(),
                                    TransactionTypeId = TransactionTypes.First(x => x.Code == TransactionTypesConstants.Commission).Id, //
                                    IsActive = true,
                                    IsDeleted = false,
                                    CompanyProductId = SalesAgentaccount.ProductId,
                                    TransactionStatusId = TransactionStatusId,
                                    OrderAmount = 0,
                                    TransactionAmount = 0,
                                    PaidAmount = 0,
                                    ProcessingFeeType = "",
                                    ProcessingFeeRate = 0,
                                    GstRate = 0,
                                    InterestType = "",
                                    InterestRate = payoutpercentage,
                                    CreditDays = 0,
                                    BounceCharge = 0,
                                    DelayPenaltyRate = 0,
                                    PayableBy = "",
                                    OrderDate = indianTime,
                                    ParentAccountTransactionsID = BusinessAccountTransaction.Id// 
                                };

                                double Payoutvalue = payoutpercentage * BusinessAccountTransaction.AccountTransactionDetails.First(x => x.TransactionDetailHeadId == DisbursementHeadId).Amount / 100;

                                List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();

                                //TDS
                                double TDSvalue = 0;
                                TDSvalue = (-1) * request.Request.TDSRate * Payoutvalue / 100;
                                transactionDetails.Add(new AccountTransactionDetail()
                                {
                                    IsActive = true,
                                    IsDeleted = false,
                                    Amount = TDSvalue,
                                    TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Tds5).Id,
                                    IsPayableBy = true,
                                    Status = "Success",
                                    TransactionDate = indianTime
                                });

                                //Commision
                                transactionDetails.Add(new AccountTransactionDetail()
                                {
                                    IsActive = true,
                                    IsDeleted = false,
                                    Amount = Payoutvalue,
                                    TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Commission).Id,
                                    IsPayableBy = true,
                                    Status = "Success",
                                    TransactionDate = indianTime
                                });
                                accountTransaction.AccountTransactionDetails = transactionDetails;
                                accountTransaction.OrderAmount = transactionDetails.Sum(x => x.Amount);
                                _context.AccountTransactions.Add(accountTransaction);

                                // User DSA  Calculation
                                var salesagentDSA = request.Request.SalesAgentDetails.Where(x => x.Type == DSAProfileTypeConstants.DSA && x.AnchorCompanyId == salesagent.AnchorCompanyId).FirstOrDefault();
                                if (salesagentDSA != null)
                                {
                                    var DsamnthLoanIds = salesAgentLoanDisbursments.Where(x => x.LeadCreatedUserId == salesagentDSA.UserId).Select(x => x.DisbursedLoanAccountId).ToList();
                                    var DsamnthTotalDisbAmt = await _context.AccountTransactions.Where(x => mnthLoanIds.Contains(x.LoanAccountId) && x.TransactionTypeId == DisbTypeId).SumAsync(x => x.OrderAmount);

                                    double payoutpercentageDSA = 0;

                                    var payoutDSA = salesagentDSA.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId && DsamnthTotalDisbAmt >= x.MinAmount && DsamnthTotalDisbAmt <= x.MaxAmount).FirstOrDefault();
                                    if (payoutDSA == null)
                                    {
                                        if (DsamnthTotalDisbAmt > salesagentDSA.SalesAgentProductPayouts.Max(x => x.MaxAmount))
                                            payoutDSA = salesagentDSA.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId).OrderByDescending(x => x.PayOutPercentage).FirstOrDefault();
                                        else
                                            payoutDSA = salesagentDSA.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId).OrderBy(x => x.PayOutPercentage).FirstOrDefault();
                                    }


                                    //double payoutpercentageDSA = 0;
                                    //var payoutDSA = salesagentDSA.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId).FirstOrDefault();

                                    var SalesAgentaccountDSA = await _context.LoanAccounts.Where(x => x.ProductId == Disbursedaccount.ProductId && x.Type == LoanAccountUserTypeConstants.DSA && x.UserId == salesagentDSA.UserId).FirstOrDefaultAsync();
                                    if (SalesAgentaccountDSA != null)
                                    {
                                        payoutpercentageDSA = payoutDSA.PayOutPercentage;
                                        AccountTransaction DSAaccountTransaction = new AccountTransaction
                                        {
                                            AnchorCompanyId = SalesAgentaccountDSA.AnchorCompanyId,
                                            LoanAccountId = SalesAgentaccountDSA.Id,
                                            ReferenceId = "C_" + Disbursedaccount.Id.ToString(),
                                            TransactionTypeId = TransactionTypes.First(x => x.Code == TransactionTypesConstants.Commission).Id,
                                            IsActive = true,
                                            IsDeleted = false,
                                            CompanyProductId = SalesAgentaccountDSA.ProductId,
                                            TransactionStatusId = TransactionStatusId,
                                            OrderAmount = 0,
                                            TransactionAmount = 0,
                                            PaidAmount = 0,
                                            ProcessingFeeType = "",
                                            ProcessingFeeRate = 0,
                                            GstRate = 0,
                                            InterestType = "",
                                            InterestRate = payoutpercentageDSA,
                                            CreditDays = 0,
                                            BounceCharge = 0,
                                            DelayPenaltyRate = 0,
                                            PayableBy = "",
                                            OrderDate = indianTime,
                                            ParentAccountTransactionsID = BusinessAccountTransaction.Id// 
                                        };

                                        List<AccountTransactionDetail> DSAtransactionDetails = new List<AccountTransactionDetail>();
                                        double DSAPayoutvalue = payoutpercentageDSA * BusinessAccountTransaction.AccountTransactionDetails.First(x => x.TransactionDetailHeadId == DisbursementHeadId).Amount / 100;
                                        //GST
                                        double DSAGSTvalue = 0;
                                        if (!string.IsNullOrEmpty(salesagentDSA.GstnNo))
                                        {
                                            //DSAGSTvalue = DSAPayoutvalue + (DSAPayoutvalue * request.Request.GstRate / 100);
                                            DSAGSTvalue = (DSAPayoutvalue * request.Request.GstRate / 100);
                                            DSAtransactionDetails.Add(new AccountTransactionDetail()
                                            {
                                                IsActive = true,
                                                IsDeleted = false,
                                                Amount = DSAGSTvalue,
                                                TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Gst18).Id,
                                                IsPayableBy = true,
                                                Status = "Success",
                                                TransactionDate = indianTime
                                            });
                                        }
                                        //TDS
                                        double DSATDSvalue = 0;
                                        DSATDSvalue = (-1) * request.Request.TDSRate * DSAPayoutvalue / 100;
                                        DSAtransactionDetails.Add(new AccountTransactionDetail()
                                        {
                                            IsActive = true,
                                            IsDeleted = false,
                                            Amount = DSATDSvalue,
                                            TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Tds5).Id,
                                            IsPayableBy = true,
                                            Status = "Success",
                                            TransactionDate = indianTime
                                        });
                                        //Commision
                                        DSAtransactionDetails.Add(new AccountTransactionDetail()
                                        {
                                            IsActive = true,
                                            IsDeleted = false,
                                            Amount = DSAPayoutvalue,
                                            TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Commission).Id,
                                            IsPayableBy = true,
                                            Status = "Success",
                                            TransactionDate = indianTime
                                        });
                                        DSAaccountTransaction.AccountTransactionDetails = DSAtransactionDetails;
                                        DSAaccountTransaction.OrderAmount = DSAtransactionDetails.Sum(x => x.Amount);

                                        _context.AccountTransactions.Add(DSAaccountTransaction);
                                        DSAorConnectorRecalculateLoanId = SalesAgentaccountDSA.Id;
                                        DSAorConnectorRecalculatePayout = payoutpercentageDSA;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //DSA or Connector

                            //var payout = salesagent.SalesAgentProductPayouts.Where(x => x.ProductId == Disbursedaccount.ProductId).FirstOrDefault();
                            var SalesAgentaccount = await _context.LoanAccounts.Where(x => x.ProductId == Disbursedaccount.ProductId && (x.Type == LoanAccountUserTypeConstants.DSA || x.Type == LoanAccountUserTypeConstants.Connector) && x.UserId == item.LeadCreatedUserId).FirstOrDefaultAsync();
                            //if (SalesAgentaccount != null)
                            //{
                            if (SalesAgentaccount != null && !await _context.AccountTransactions.AnyAsync(x => x.IsActive && !x.IsDeleted && x.ParentAccountTransactionsID == BusinessAccountTransaction.Id && x.LoanAccountId == SalesAgentaccount.Id))
                            {
                                payoutpercentage = payout.PayOutPercentage;
                                AccountTransaction accountTransaction = new AccountTransaction
                                {
                                    AnchorCompanyId = SalesAgentaccount.AnchorCompanyId,
                                    LoanAccountId = SalesAgentaccount.Id,
                                    ReferenceId = "C_" + Disbursedaccount.Id.ToString(),
                                    TransactionTypeId = TransactionTypes.First(x => x.Code == TransactionTypesConstants.Commission).Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    CompanyProductId = SalesAgentaccount.ProductId,
                                    TransactionStatusId = TransactionStatusId,
                                    OrderAmount = 0,
                                    TransactionAmount = 0,
                                    PaidAmount = 0,
                                    ProcessingFeeType = "",
                                    ProcessingFeeRate = 0,
                                    GstRate = 0,
                                    InterestType = "",
                                    InterestRate = payoutpercentage,
                                    CreditDays = 0,
                                    BounceCharge = 0,
                                    DelayPenaltyRate = 0,
                                    PayableBy = "",
                                    OrderDate = indianTime,
                                    ParentAccountTransactionsID = BusinessAccountTransaction.Id// 
                                };


                                List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();
                                double Payoutvalue = payoutpercentage * BusinessAccountTransaction.AccountTransactionDetails.First(x => x.TransactionDetailHeadId == DisbursementHeadId).Amount / 100;
                                //GST
                                double GSTvalue = 0;
                                if (!string.IsNullOrEmpty(salesagent.GstnNo))
                                {
                                    //GSTvalue = Payoutvalue + (Payoutvalue * request.Request.GstRate / 100);
                                    GSTvalue = (Payoutvalue * request.Request.GstRate / 100);
                                    transactionDetails.Add(new AccountTransactionDetail()
                                    {
                                        IsActive = true,
                                        IsDeleted = false,
                                        Amount = GSTvalue,
                                        TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Gst18).Id,
                                        IsPayableBy = true,
                                        Status = "Success",
                                        TransactionDate = indianTime
                                    });
                                }
                                //TDS
                                double TDSvalue = 0;
                                TDSvalue = (-1) * request.Request.TDSRate * Payoutvalue / 100;
                                transactionDetails.Add(new AccountTransactionDetail()
                                {
                                    IsActive = true,
                                    IsDeleted = false,
                                    Amount = TDSvalue,
                                    TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Tds5).Id,
                                    IsPayableBy = true,
                                    Status = "Success",
                                    TransactionDate = indianTime
                                });

                                //Commision
                                transactionDetails.Add(new AccountTransactionDetail()
                                {
                                    IsActive = true,
                                    IsDeleted = false,
                                    Amount = Payoutvalue,
                                    TransactionDetailHeadId = TransactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Commission).Id,
                                    IsPayableBy = true,
                                    Status = "Success",
                                    TransactionDate = indianTime
                                });
                                accountTransaction.AccountTransactionDetails = transactionDetails;
                                accountTransaction.OrderAmount = transactionDetails.Sum(x => x.Amount);
                                _context.AccountTransactions.Add(accountTransaction);

                            }
                            DSAorConnectorRecalculateLoanId = SalesAgentaccount.Id;
                            DSAorConnectorRecalculatePayout = payout.PayOutPercentage;

                        }

                        salesAgentLoanDisbursment.IsProcess = true;
                        _context.Entry(salesAgentLoanDisbursment).State = EntityState.Modified;

                        await _context.SaveChangesAsync();

                        //// Update old payout when percent change
                        if (DSAorConnectorRecalculateLoanId > 0)
                        {
                            await UpdateSalesAgentPreviousTransactions(DSAorConnectorRecalculateLoanId, DSAorConnectorRecalculatePayout, request.Request.GstRate, request.Request.TDSRate, TransactionDetailHeadlist);
                        }

                    }
                }
            }
            reply.Status = true;

            return reply;
        }
        public async Task<bool> UpdateSalesAgentPreviousTransactions(long SalesAgentLoanAccountId, double NewPayout, double gstrate, double tdsrate, List<TransactionDetailHead> transactionDetailHeadlist)
        {
            var mnth = indianTime.Month;
            var year = indianTime.Year;
            var oldTransactions = await _context.AccountTransactions.Where(x => x.LoanAccountId == SalesAgentLoanAccountId && x.ParentAccountTransactionsID != null && x.Created.Month == mnth && x.Created.Year == year && x.InterestRate > 0 && x.InterestRate < NewPayout && x.IsActive && !x.IsDeleted).Include(x => x.AccountTransactionDetails).ToListAsync();
            if (oldTransactions != null && oldTransactions.Any())
            {
                foreach (var item in oldTransactions)
                {
                    var BusinessAccountTransaction = await _context.AccountTransactions.Where(x => x.Id == item.ParentAccountTransactionsID && x.IsActive && !x.IsDeleted).Include(x => x.AccountTransactionDetails).FirstOrDefaultAsync();

                    if (BusinessAccountTransaction != null && item.AccountTransactionDetails != null && item.AccountTransactionDetails.Any())
                    {
                        double Payoutvalue = NewPayout * BusinessAccountTransaction.AccountTransactionDetails.First(x => x.TransactionDetailHead.Code == TransactionDetailHeadsConstants.DisbursementAmount).Amount / 100;

                        foreach (var detail in item.AccountTransactionDetails)
                        {
                            if (detail.TransactionDetailHeadId == transactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Commission).Id)
                            {
                                detail.Amount = Payoutvalue;
                            }
                            else if (detail.Amount > 0 && detail.TransactionDetailHeadId == transactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Gst18).Id)
                            {
                                detail.Amount = (Payoutvalue * gstrate / 100);

                            }
                            else if (detail.TransactionDetailHeadId == transactionDetailHeadlist.First(x => x.Code == TransactionDetailHeadsConstants.Tds5).Id)
                            {
                                detail.Amount = (-1) * tdsrate * Payoutvalue / 100;
                            }
                        }
                        item.OrderAmount = item.AccountTransactionDetails.Sum(x => x.Amount);
                        item.InterestRate = NewPayout;

                        _context.Entry(item).State = EntityState.Modified;
                    }
                }
                await _context.SaveChangesAsync();
            }
            return true;
        }
        public async Task<bool> SaveRepaymentScheduleData(string loan_id, string product_id, long company_id, DateTime? first_inst_date, long loanAccountId)
        {
            ResultViewModel<List<LoanRepaymentScheduleDetailDc>> result = new ResultViewModel<List<LoanRepaymentScheduleDetailDc>>();
            var Postrepayment_schedule = new Postrepayment_scheduleDc
            {
                loan_id = loan_id,
                product_id = product_id,
                company_id = company_id,
            };
            var repaymentScheduleApi = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateRepaymentSchedule && x.IsActive && !x.IsDeleted);
            var Response = await _ArthMateNBFCHelper.repayment_schedule(Postrepayment_schedule, repaymentScheduleApi.APIUrl, repaymentScheduleApi.TAPIKey, repaymentScheduleApi.TAPISecretKey, repaymentScheduleApi.Id);
            _context.ArthMateCommonAPIRequestResponses.Add(Response);
            _context.SaveChanges();
            result.IsSuccess = Response.IsSuccess;
            if (Response.IsSuccess)
            {
                var RepaymentAllResponse = JsonConvert.DeserializeObject<repay_scheduleDc>(Response.Response);
                if (first_inst_date.Value != null && RepaymentAllResponse != null && RepaymentAllResponse.data != null && RepaymentAllResponse.data.rows != null && RepaymentAllResponse.data.rows.Any())
                {
                    List<RepaymentSchedule> repayments = new List<RepaymentSchedule>();
                    foreach (var item in RepaymentAllResponse.data.rows)
                    {
                        RepaymentSchedule Repay = new RepaymentSchedule
                        {
                            LoanAccountId = loanAccountId,
                            CompanyId = item.company_id,
                            loanId = loan_id,
                            ProductId = product_id,
                            EMIAmount = item.emi_amount,
                            EMINo = item.emi_no,
                            InterestAmount = item.int_amount,
                            DueDate = first_inst_date.Value.AddMonths(item.emi_no - 1),
                            Principal = item.prin,
                            PrincipalBalance = item.principal_bal,
                            PrincipalOutstanding = item.principal_outstanding,
                            RepayScheduleId = item.repay_schedule_id,
                            Created = indianTime,
                            IsActive = true,
                            IsDeleted = false,
                        };
                        repayments.Add(Repay);
                    }
                    _context.repaymentSchedules.AddRange(repayments);
                    _context.SaveChanges();

                }
            }
            return Response.IsSuccess;
        }
        public async Task<GRPCReply<DSADashboardPayoutResponse>> GetDSALoanPayoutList(GRPCRequest<DSALoanDashboardDataRequest> request)
        {
            GRPCReply<DSADashboardPayoutResponse> reply = new GRPCReply<DSADashboardPayoutResponse>
            {
                Message = "Data found",
                Status = true,
                Response = new DSADashboardPayoutResponse()
            };
            if (request.Request.LeadIds != null && request.Request.LeadIds.Any())
            {
                var agentUserId = new SqlParameter("AgentUserId", request.Request.AgentUserId);
                var leadIds = new DataTable();
                leadIds.Columns.Add("IntValue");
                foreach (var lead in request.Request.LeadIds)
                {
                    var dr = leadIds.NewRow();
                    dr["IntValue"] = lead;
                    leadIds.Rows.Add(dr);
                }
                var LeadIds = new SqlParameter
                {
                    ParameterName = "leadIds",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IntValues",
                    Value = leadIds
                };
                var payoutData = _context.Database.SqlQueryRaw<LoanPayoutDetailDc>("exec GetDSALoanPayoutList @leadIds,@agentUserId", LeadIds, agentUserId).ToList();

                if (payoutData != null && payoutData.Any())
                {
                    reply.Response.TotalRecords = payoutData.Count();
                    reply.Response.LoanPayoutDetailList = payoutData.Skip(request.Request.Skip).Take(request.Request.Take).ToList();
                    reply.Response.TotalDisbursedAmount = payoutData.Sum(x => x.DisbursmentAmount ?? 0);
                    reply.Response.TotalPayoutAmount = payoutData.Sum(x => x.PayoutAmount);

                    reply.Status = true;
                    reply.Message = "Data Found";
                }
            }
            return reply;
        }
        #endregion

        public async Task<bool> BusinessLoanAccountTransaction(BusinessLoanTransactionDc obj)
        {
            bool result = false;
            var DisbursementId = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.Disbursement).Id;
            var TransactionStatuses = await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.Code == TransactionStatuseConstants.Paid);
            var TransactionHeads = _context.TransactionDetailHeads.Where(x => x.IsActive).ToList();

            var entityname = new SqlParameter("EntityName", "Transaction");
            var ReferenceNo = _context.Database.SqlQueryRaw<string>("exec GenerateReferenceNoForTrans @EntityName", entityname).AsEnumerable().FirstOrDefault();

            AccountTransaction accountTransaction = new AccountTransaction
            {
                AnchorCompanyId = obj.AnchorCompanyId,
                LoanAccountId = obj.LoanAccountId,
                ReferenceId = ReferenceNo,
                CustomerUniqueCode = "",
                TransactionTypeId = DisbursementId,
                IsActive = true,
                IsDeleted = false,
                CompanyProductId = Convert.ToInt64(obj.ProductId),
                TransactionStatusId = TransactionStatuses.Id,
                OrderAmount = obj.Amount,
                TransactionAmount = obj.Amount,
                PaidAmount = obj.Amount,
                ProcessingFeeType = obj.PFType,
                ProcessingFeeRate = obj.ProcessingFeeRate,
                GstRate = obj.GstRate ?? 0,
                InterestType = obj.InterestType ?? "",
                InterestRate = obj.InterestRate ?? 0,
                CreditDays = 0,
                BounceCharge = obj.BounceCharge ?? 0,
                DelayPenaltyRate = 0,
                PayableBy = "",
                OrderDate = indianTime,
                InvoiceDate = indianTime,
                DisbursementDate = obj.DisbursementDate,
                InvoiceId = obj.InvoiceId,
            };
            List<AccountTransactionDetail> transactionDetails = new List<AccountTransactionDetail>();
            //order
            transactionDetails.Add(new AccountTransactionDetail()
            {
                IsActive = true,
                IsDeleted = false,
                Amount = obj.Amount,
                TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.DisbursementAmount).Id,
                IsPayableBy = false, // PayableBy,
                Status = "initiate",
                TransactionDate = indianTime
            });

            if (obj.ConvenienceFee > 0)
            {
                transactionDetails.Add(new AccountTransactionDetail()
                {
                    IsActive = true,
                    IsDeleted = false,
                    Amount = (-1) * obj.ConvenienceFee,
                    TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.ConvenienceFee).Id,
                    IsPayableBy = false,
                    Status = "initiate"
                });
            }
            if (obj.ProcessingFee > 0)
            {
                transactionDetails.Add(new AccountTransactionDetail()
                {
                    IsActive = true,
                    IsDeleted = false,
                    Amount = (-1) * obj.ProcessingFee,
                    TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.ProcessingFee).Id,
                    IsPayableBy = false,
                    Status = "initiate"
                });
            }
            if (obj.ProcessingFeeGST > 0)
            {
                transactionDetails.Add(new AccountTransactionDetail()
                {
                    IsActive = true,
                    IsDeleted = false,
                    Amount = (-1) * obj.ProcessingFeeGST,
                    TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.Gst).Id,
                    IsPayableBy = false,
                    Status = "initiate"
                });
            }
            if (obj.InsuranceAmount > 0)
            {
                transactionDetails.Add(new AccountTransactionDetail()
                {
                    IsActive = true,
                    IsDeleted = false,
                    Amount = (-1) * obj.InsuranceAmount ?? 0,
                    TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.InsuranceAmount).Id,
                    IsPayableBy = false,
                    Status = "initiate"
                });
            }
            if (obj.OtherCharges > 0)
            {
                transactionDetails.Add(new AccountTransactionDetail()
                {
                    IsActive = true,
                    IsDeleted = false,
                    Amount = (-1) * obj.OtherCharges ?? 0,
                    TransactionDetailHeadId = TransactionHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.OtherCharge).Id,
                    IsPayableBy = false,
                    Status = "initiate"
                });
            }

            accountTransaction.AccountTransactionDetails = transactionDetails;
            _context.AccountTransactions.Add(accountTransaction);
            if (_context.SaveChanges() > 0)
            {

                result = true;
            }
            return result;
        }

        public async Task<ResultViewModel<List<CompanyInvoiceDetailsByTypeDc>>> GetCompanyInvoiceDetailsByType(long AccountTransactionId)
        {
            ResultViewModel<List<CompanyInvoiceDetailsByTypeDc>> reply = new ResultViewModel<List<CompanyInvoiceDetailsByTypeDc>> { Message = "Data Not Found!!!" };

            reply.Result = await _context.CompanyInvoiceDetails.Where(x => x.AccountTransactionId == AccountTransactionId && x.IsActive && !x.IsDeleted).GroupBy(x => new { x.AccountTransactionId, x.InvoiceTransactionType }).Select(x => new CompanyInvoiceDetailsByTypeDc
            {
                AccountTransactionId = x.Key.AccountTransactionId,
                TransactionTypeId = x.Key.InvoiceTransactionType,
                Amount = x.Sum(x => x.ScaleupShare)
            }).ToListAsync();

            if (reply.Result != null && reply.Result.Any())
            {
                reply.Message = "Data found";
                reply.IsSuccess = true;
            }
            return reply;
        }

        public async Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request)
        {
            string htmltable = "";
            GRPCReply<string> res = new GRPCReply<string>();
            var loanAccounts = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.LeadId == request.Request && x.IsActive && !x.IsDeleted);
            if (loanAccounts != null)
            {
                var repayment = _context.repaymentSchedules.Where(x => x.LoanAccountId == loanAccounts.Id).ToList();
                if (repayment.Any())
                {
                    List<DataTable> dt = new List<DataTable>();
                    DataTable Repyment_Scdule = new DataTable();

                    Repyment_Scdule.TableName = "Repyment_Scdule";
                    dt.Add(Repyment_Scdule);
                    Repyment_Scdule.Columns.Add("DueDate");
                    Repyment_Scdule.Columns.Add("OutStanding");
                    Repyment_Scdule.Columns.Add("Principal");//emi_amount
                    Repyment_Scdule.Columns.Add("Interest");//int_amount
                    Repyment_Scdule.Columns.Add("EMI");
                    Repyment_Scdule.Columns.Add("Principal OutStanding");
                    foreach (var item in repayment)
                    {
                        var dr = Repyment_Scdule.NewRow();
                        dr["DueDate"] = item.DueDate;
                        dr["OutStanding"] = item.PrincipalOutstanding;
                        dr["Principal"] = item.Principal;
                        dr["Interest"] = item.InterestAmount;
                        dr["EMI"] = item.EMIAmount;
                        dr["Principal OutStanding"] = item.PrincipalOutstanding;
                        Repyment_Scdule.Rows.Add(dr);
                    }
                    htmltable = OfferEMIDataTableToHTML(Repyment_Scdule);
                    res.Response = htmltable;
                    res.Status = true;
                    res.Message = "success";
                }
                else
                {
                    res.Response = htmltable;
                    res.Status = false;
                    res.Message = "Data Not Found";
                }
            }
            return res;
        }
        public static string OfferEMIDataTableToHTML(DataTable dt)
        {
            string html = "<table style=\"width:100%;\">";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th>" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td style=\"text-align: center;\">" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        public async Task<GRPCReply<string>> PostNBFCDisbursement(GRPCRequest<LeadDetailForDisbursement> request)
        {
            GRPCReply<string> res = new GRPCReply<string>();

            var LoanAccountTbl = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.LeadId == request.Request.leadinfo.LeadId && x.IsActive && !x.IsDeleted);

            if (LoanAccountTbl != null)
            {
                res.Status = false;
                res.Message = "Loan already exists for this lead";
                //res.Response = "";
                return res;
            }
            string accountcode = await GetCurrentNumber("AccountCode");

            LoanAccount loanAccount = new LoanAccount
            {
                LeadId = request.Request.leadinfo.LeadId,
                LeadCode = request.Request.leadinfo.LeadCode,
                ProductId = request.Request.leadinfo.ProductId,
                UserId = request.Request.leadinfo.UserName,
                AccountCode = accountcode,
                CustomerName = request.Request.leadinfo.CustomerName, //TransactionTypeId.Id,
                MobileNo = request.Request.leadinfo.MobileNo,
                NBFCCompanyId = request.Request.leadinfo.OfferCompanyId ?? 0,
                AnchorCompanyId = request.Request.leadinfo.AnchorCompanyId,
                IsActive = true,
                IsDeleted = false,
                Created = indianTime,
                DisbursalDate = indianTime,
                ApplicationDate = Convert.ToDateTime(request.Request.leadinfo.ApplicationDate),
                AgreementRenewalDate = Convert.ToDateTime(request.Request.leadinfo.AgreementDate),
                IsDefaultNBFC = false,
                CityName = request.Request.leadinfo.CustomerCurrentCityName,
                AnchorName = request.Request.leadinfo.AnchorName,
                ProductType = request.Request.leadinfo.ProductType,
                IsAccountActive = true,
                IsBlock = false,
                IsBlockComment = "",
                IsBlockHideLimit = false,
                NBFCIdentificationCode = request.Request.leadinfo.NBFCCompanyCode,
                ThirdPartyLoanCode = "",
                ShopName = request.Request.leadinfo.ShopName,
                CustomerImage = request.Request.leadinfo.CustomerImage,
                Type = LoanAccountUserTypeConstants.Customer,
                LoanAccountCredits = new LoanAccountCredit
                {
                    DisbursalAmount = request.Request.DisbursementDetail.LoanAmount ?? 0,
                    CreditLimitAmount = request.Request.DisbursementDetail.LoanAmount ?? 0,
                    IsActive = true,
                    IsDeleted = false,
                    Created = indianTime
                }
            };
            await _context.LoanAccounts.AddAsync(loanAccount);
            _context.SaveChanges();

            if (request.Request.leadinfo != null && request.Request.leadinfo.LeadCompanies.Any())
            {
                List<LoanAccountCompanyLead> leadcomplist = new List<LoanAccountCompanyLead>();
                foreach (var comlead in request.Request.leadinfo.LeadCompanies)
                {
                    LoanAccountCompanyLead _loanAccountCompanyLead = new LoanAccountCompanyLead
                    {
                        LoanAccountId = loanAccount.Id,
                        CompanyId = comlead.CompanyId,
                        LeadProcessStatus = comlead.LeadProcessStatus,
                        UserUniqueCode = comlead.UserUniqueCode,
                        AnchorName = comlead.AnchorName,
                        LogoURL = comlead.LogoURL,
                        IsActive = true,
                        IsDeleted = false,
                        Created = DateTime.Now,
                    };
                    leadcomplist.Add(_loanAccountCompanyLead);
                };
                await _context.LoanAccountCompanyLead.AddRangeAsync(leadcomplist);
                _context.SaveChanges();
            }

            BusinessLoanDisbursementDetail disbursement = new BusinessLoanDisbursementDetail
            {
                IsActive = true,
                IsDeleted = false,
                LoanAccountId = loanAccount.Id,
                NBFCCompanyId = request.Request.DisbursementDetail.NBFCCompanyId ?? 0,
                CompanyIdentificationCode = request.Request.DisbursementDetail.CompanyIdentificationCode,
                LoanId = request.Request.DisbursementDetail.LoanId,
                Tenure = request.Request.DisbursementDetail.Tenure ?? 0,
                LoanAmount = request.Request.DisbursementDetail.LoanAmount ?? 0,
                LoanInterestAmount = request.Request.DisbursementDetail.LoanInterestAmount ?? 0,
                InterestRate = request.Request.DisbursementDetail.InterestRate,
                MonthlyEMI = request.Request.DisbursementDetail.MonthlyEMI,
                ProcessingFeeRate = request.Request.DisbursementDetail.ProcessingFeeRate ?? 0,
                ProcessingFeeAmount = request.Request.DisbursementDetail.ProcessingFeeAmount ?? 0,
                PFDiscount = request.Request.DisbursementDetail.PFDiscount ?? 0,
                GST = request.Request.DisbursementDetail.GST,
                ProcessingFeeTax = request.Request.DisbursementDetail.ProcessingFeeTax ?? 0,
                PFType = request.Request.DisbursementDetail.PFType,
                Commission = request.Request.DisbursementDetail.Commission,
                CommissionType = request.Request.DisbursementDetail.CommissionType,
                Disbursementdate = request.Request.DisbursementDate,
                InsuranceAmount = request.Request.InsuranceAmount,
                OtherCharges = request.Request.OtherCharges,
                brokenPeriodinterestAmount = request.Request.brokenPeriodinterestAmount,
                FirstEMIDate = request.Request.FirstEMIDate,
                ArrangementType = request.Request.DisbursementDetail.ArrangementType,
                Bounce = request.Request.DisbursementDetail != null && request.Request.DisbursementDetail.Bounce != null ? request.Request.DisbursementDetail.Bounce : 0,
                NBFCBounce = request.Request.DisbursementDetail != null && request.Request.DisbursementDetail.NBFCBounce != null ? request.Request.DisbursementDetail.NBFCBounce : 0,
                NBFCPenal = request.Request.DisbursementDetail != null && request.Request.DisbursementDetail.NBFCPenal != null ? request.Request.DisbursementDetail.NBFCPenal : 0,
                Penal = request.Request.DisbursementDetail != null && request.Request.DisbursementDetail.Penal != null ? request.Request.DisbursementDetail.Penal : 0,
                NBFCInterest = request.Request.DisbursementDetail.NBFCInterest,
                NBFCProcessingFeeType = request.Request.DisbursementDetail.NBFCProcessingFeeType,
                NBFCProcessingFee = request.Request.DisbursementDetail.NBFCProcessingFee,
            };
            _context.BusinessLoanDisbursementDetail.Add(disbursement);
            _context.SaveChanges();

            Invoice invoice = new Invoice
            {
                IsActive = true,
                IsDeleted = false,
                LoanAccountId = loanAccount.Id,
                OrderAmount = request.Request.DisbursementDetail.LoanAmount ?? 0,
                OrderNo = "",
                TotalTransAmount = request.Request.DisbursementDetail.LoanAmount ?? 0,
                InvoiceAmount = request.Request.DisbursementDetail.LoanAmount ?? 0,
                Comment = "Disbursement",
                Status = AccountInvoiceStatus.Disbursed.ToString(),
                NBFCUTR = request.Request.nbfcUTR,
                InvoiceNo = ""
            };
            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            BusinessLoanTransactionDc txndata = new BusinessLoanTransactionDc
            {
                AnchorCompanyId = request.Request.leadinfo.AnchorCompanyId,
                ProductId = request.Request.leadinfo.ProductId,
                Amount = request.Request.DisbursementDetail.LoanAmount ?? 0,
                BounceCharge = 0,
                ConvenienceFee = 0,
                ProcessingFeeRate = request.Request.DisbursementDetail.ProcessingFeeRate ?? 0,
                CreditDay = 0,
                DelayPenaltyRate = 0,
                GstRate = request.Request.DisbursementDetail.GST,
                InterestPayableBy = "",
                InterestType = "",
                LoanAccountId = loanAccount.Id,
                TransactionReqNo = "",
                ProcessingFee = request.Request.DisbursementDetail.ProcessingFeeAmount ?? 0,
                ProcessingFeeGST = request.Request.DisbursementDetail.ProcessingFeeTax ?? 0,
                InterestRate = request.Request.DisbursementDetail.InterestRate,
                InsuranceAmount = request.Request.InsuranceAmount,
                InvoiceId = invoice.Id,
                DisbursementDate = request.Request.DisbursementDate,
                OtherCharges = request.Request.OtherCharges,
                PFType = request.Request.DisbursementDetail.PFType

            };
            var transaction = await BusinessLoanAccountTransaction(txndata);

            await _context.SalesAgentLoanDisbursments.AddAsync(new LoanAccountModels.DSA.SalesAgentLoanDisbursment
            {
                DisbursedLoanAccountId = loanAccount.Id,
                IsProcess = false,
                IsActive = true,
                LeadCreatedUserId = request.Request.leadinfo.CreatedBy
            });

            List<RepaymentSchedule> repayments = new List<RepaymentSchedule>();
            int i = 0;
            double BPIAmount = request.Request.brokenPeriodinterestAmount ?? 0;
            foreach (var item in request.Request.EMISchedule)
            {
                i++;
                RepaymentSchedule Repay = new RepaymentSchedule
                {
                    LoanAccountId = loanAccount.Id,
                    CompanyId = Convert.ToInt32(request.Request.leadinfo.NBFCCompanyId),
                    loanId = request.Request.DisbursementDetail.LoanId,
                    ProductId = request.Request.leadinfo.ProductId.ToString(),
                    EMIAmount = i == 1 ? item.EMIAmount + BPIAmount : item.EMIAmount,
                    EMINo = 0,
                    InterestAmount = item.InterestAmount,
                    DueDate = item.DueDate,
                    Principal = item.Prin,
                    PrincipalBalance = item.PrincipalAmount,
                    PrincipalOutstanding = item.OutStandingAmount,
                    RepayScheduleId = 0,
                    Created = indianTime,
                    IsActive = true,
                    IsDeleted = false,
                };
                repayments.Add(Repay);
            }
            _context.repaymentSchedules.AddRange(repayments);
            _context.SaveChanges();

            res.Response = "success";
            res.Status = true;
            return res;
        }

        public async Task<CommonResponse> GetLoanDetail(long leadid)
        {
            CommonResponse res = new CommonResponse();
            var loanaccount = await _context.LoanAccounts.Where(x => x.LeadId == leadid && x.IsActive && !x.IsDeleted).FirstOrDefaultAsync();
            if (loanaccount != null)
            {
                var bldisbursement = _context.BusinessLoanDisbursementDetail.Where(x => x.LoanAccountId == loanaccount.Id).FirstOrDefault();
                if (bldisbursement != null)
                {
                    res.Result = bldisbursement;
                    res.status = true;
                    res.Message = "success";
                }
                else
                {
                    res.status = false;
                    res.Message = "Record Not Found";
                }
            }
            else
            {
                res.status = false;
                res.Message = "Loan Not Found";
            }
            return res;
        }

        public async Task<GRPCReply<bool>> AddInvoiceSettlementData(GRPCRequest<SettleCompanyInvoiceTransactionsRequest> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool> { Message = "Failed to save!!!" };
            if (request.Request.InvoiceSettlementDatas != null && request.Request.InvoiceSettlementDatas.Any(x => x.Id == 0))
            {
                var companyInvoice = await _context.CompanyInvoices.FirstOrDefaultAsync(x => x.Id == request.Request.CompanyInvoiceId && x.IsActive && !x.IsDeleted);
                if (companyInvoice != null)
                {
                    var companyInvoiceDetails = await _context.CompanyInvoiceDetails.Where(x => x.CompanyInvoiceId == companyInvoice.Id && x.Status != TransactionStatuseConstants.Paid && x.IsActive && !x.IsDeleted).OrderBy(x => x.AccountTransactionId).ToListAsync();
                    if (companyInvoiceDetails != null && companyInvoiceDetails.Any())
                    {
                        List<CompanyInvoiceSettlement> list = new List<CompanyInvoiceSettlement>();
                        foreach (var item in request.Request.InvoiceSettlementDatas.Where(x => x.Id == 0))
                        {
                            list.Add(new CompanyInvoiceSettlement
                            {
                                CompanyInvoiceId = request.Request.CompanyInvoiceId,
                                Amount = item.Amount,
                                PaymentDate = item.PaymentDate,
                                TDSAmount = item.TDSAmount,
                                UTRNumber = item.UTRNumber,
                                IsSattled = false,
                                IsActive = true,
                                IsDeleted = false
                            });
                            companyInvoice.PaymentDate = item.PaymentDate;
                            companyInvoice.PaymentReferenceNo = string.IsNullOrEmpty(companyInvoice.PaymentReferenceNo) ? item.UTRNumber : companyInvoice.PaymentReferenceNo + "," + item.UTRNumber;
                        }
                        _context.CompanyInvoiceSettlements.AddRange(list);
                        _context.SaveChanges();

                        var companyInvoiceSettlements = await _context.CompanyInvoiceSettlements.Where(x => x.CompanyInvoiceId == request.Request.CompanyInvoiceId && !x.IsSattled && x.IsActive && !x.IsDeleted).ToListAsync();
                        if (companyInvoiceSettlements != null && companyInvoiceSettlements.Any())
                        {
                            var sattleData = companyInvoiceSettlements.First();
                            double totalAmountToSettle = Math.Round((sattleData.Amount + sattleData.TDSAmount) / 1.18, 2); //Remove Added GST
                            foreach (var invoiceDetail in companyInvoiceDetails)
                            {
                            x:
                                double outstandingAmount = await GetOutstandingAmountByTransactionId(invoiceDetail.AccountTransactionId);
                                if (outstandingAmount > 0.00)
                                {
                                    bool IsFullPayment = false;
                                    double settleAmount = 0;
                                    if (outstandingAmount <= totalAmountToSettle)
                                    {
                                        settleAmount = outstandingAmount;
                                        totalAmountToSettle = Math.Round(totalAmountToSettle - outstandingAmount, 2);
                                        IsFullPayment = true;
                                    }
                                    else
                                    {
                                        settleAmount = totalAmountToSettle;
                                        totalAmountToSettle = 0;
                                    }
                                    await _TransactionSettlementManager.AddInvoiceSettlementData(new AddInvoiceSettlementDataRequestDC
                                    {
                                        ParentAccountTransactionId = invoiceDetail.AccountTransactionId,
                                        TransactionTypeCode = TransactionTypesConstants.ScaleupSharePayment,
                                        Amount = settleAmount,
                                        PaymentDate = sattleData.PaymentDate,
                                        UTRNumber = sattleData.UTRNumber,
                                        IsFullPayment = IsFullPayment
                                    });
                                    if (totalAmountToSettle == 0.00)
                                    {
                                        companyInvoiceSettlements.Remove(sattleData);
                                        sattleData.IsSattled = true;
                                        _context.Entry(sattleData).State = EntityState.Modified;
                                        if (!companyInvoiceSettlements.Any())
                                        {
                                            foreach (var item in companyInvoiceDetails.Where(x => x.AccountTransactionId == invoiceDetail.AccountTransactionId))
                                            {
                                                item.Status = TransactionStatuseConstants.Paid;
                                                _context.Entry(item).State = EntityState.Modified;
                                            }
                                            break;
                                        }
                                        else totalAmountToSettle = (companyInvoiceSettlements.First().Amount + companyInvoiceSettlements.First().TDSAmount) / 1.18;
                                    }
                                    if (!IsFullPayment)
                                    {
                                        outstandingAmount = await GetOutstandingAmountByTransactionId(invoiceDetail.AccountTransactionId);
                                        if (outstandingAmount > 0.00)
                                        {
                                            goto x;
                                        }
                                    }
                                    else outstandingAmount = 0;
                                }
                                if (outstandingAmount == 0.00)
                                {
                                    invoiceDetail.Status = TransactionStatuseConstants.Paid;
                                    _context.Entry(invoiceDetail).State = EntityState.Modified;
                                }
                            }
                            companyInvoice.Status = Convert.ToInt32(CompanyInvoiceStatusEnum.Settled);
                            if (_context.SaveChanges() > 0)
                            {
                                reply.Status = true;
                                reply.Response = true;
                                reply.Message = "Transaction Settled";
                            }
                        }
                        else reply.Message = "No Pending Settlement Found!!!";
                    }
                    else reply.Message = "all transactions of this invoice are already Settled!!!";
                }
                else reply.Message = "Company Invoice Not Found!!!";
            }
            else reply.Message = "New Settlement Data Not Found!!!";
            return reply;
        }
        public async Task<double> GetOutstandingAmountByTransactionId(long transactionId)
        {
            double outstandingAmount = 0;
            var scaleupTransactions = await _context.AccountTransactions.Where(x => x.ParentAccountTransactionsID == transactionId && (x.TransactionType.Code == TransactionTypesConstants.ScaleupShareAmount || x.TransactionType.Code == TransactionTypesConstants.ScaleupSharePayment) && x.IsActive && !x.IsDeleted).Select(x => x.Id).ToListAsync();
            if (scaleupTransactions != null && scaleupTransactions.Any())
            {
                List<string> detailHeads = new List<string> { TransactionDetailHeadsConstants.ScaleUpShareBounceAmount, TransactionDetailHeadsConstants.ScaleUpShareInterestAmount, TransactionDetailHeadsConstants.ScaleUpShareOverdueAmount, TransactionDetailHeadsConstants.ScaleUpSharePenalAmount, TransactionDetailHeadsConstants.ScaleUpSharePaymentAmount };
                outstandingAmount = await _context.AccountTransactionDetails.Where(x => detailHeads.Contains(x.TransactionDetailHead.Code) && scaleupTransactions.Contains(x.AccountTransactionId) && x.IsActive && !x.IsDeleted).SumAsync(x => x.Amount);
            }
            return Math.Round(outstandingAmount, 2);
        }
        public async Task<List<GetOverdueLoanAccountResponse>> GetOverdueLoanAccount(List<GetOverdueLoanAccountIdRequest> loanAccointIdRequest)
        {
            List<GetOverdueLoanAccountResponse> resultList = new List<GetOverdueLoanAccountResponse>();

            try
            {
                var loanAccointIdlist = new DataTable();
                loanAccointIdlist.Columns.Add("loanAccountID");

                foreach (var item in loanAccointIdRequest)
                {
                    var dr = loanAccointIdlist.NewRow();
                    dr["loanAccountID"] = item.LoanAccountID;
                    //dr["CustomerType"] = item.CustomerType;
                    loanAccointIdlist.Rows.Add(dr);
                }
                var loanAccountID = new SqlParameter("LoanAccountIdLists", loanAccointIdlist);
                loanAccountID.SqlDbType = SqlDbType.Structured;
                loanAccountID.TypeName = "dbo.intvalues";

                resultList = _context.Database.SqlQueryRaw<GetOverdueLoanAccountResponse>("exec GetOverdueLoanAccount @LoanAccountIdLists", loanAccountID).AsEnumerable().ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return resultList;
        }



        public async Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetSettlePaymentJobData()
        {
            GRPCReply<List<GetSettlePaymentJobDataResponse>> reply = new GRPCReply<List<GetSettlePaymentJobDataResponse>>();
            var repaymentLoanIds = await _context.LoanAccountRepayments.Where(x => x.Status != BlackSoilRepaymentConstants.Settled && x.IsActive && !x.IsDeleted).Select(x => x.LoanAccountId).Distinct().ToListAsync();
            if (repaymentLoanIds != null && repaymentLoanIds.Any())
            {
                reply.Response = await _context.LoanAccounts.Where(x => repaymentLoanIds.Contains(x.Id) && x.IsActive && !x.IsDeleted).Select(x => new GetSettlePaymentJobDataResponse
                {
                    AnchorCompanyId = x.AnchorCompanyId ?? 0,
                    NBFCCompanyId = x.NBFCCompanyId,
                    ProductId = x.ProductId
                }).Distinct().ToListAsync();
                if (reply.Response != null && reply.Response.Any())
                {
                    reply.Status = true;
                    reply.Message = "Data Found";
                }
            }
            return reply;
        }

        public async Task<GRPCReply<bool>> SettlePaymentLaterJob(GRPCRequest<SettlePaymentJobRequest> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var service = _loanNBFCFactory.GetService(CompanyIdentificationCodeConstants.BlackSoil);

            await service.SettlePaymentLater(request);
            return reply;
        }

        public async Task<GRPCReply<List<GetSettlePaymentJobDataResponse>>> GetScaleUpShareCompanyData()
        {
            GRPCReply<List<GetSettlePaymentJobDataResponse>> reply = new GRPCReply<List<GetSettlePaymentJobDataResponse>> { Message = "Data Not Found!!!" };
            var loanAccounts = await _context.LoanAccounts.Where(l => l.IsActive && !l.IsDeleted).ToListAsync();

            var accountTransactions = await _context.AccountTransactions.Where(at => at.IsActive && !at.IsDeleted).Include(x => x.TransactionType).Include(x => x.AccountTransactionDetails).ToListAsync();

            var ScaleUpShareTransactionType = await _context.TransactionTypes.Where(tt => tt.IsActive && !tt.IsDeleted && tt.Code == TransactionTypesConstants.ScaleupShareAmount).FirstOrDefaultAsync();

            var orderPaymentTransactions = accountTransactions.Where(x => x.TransactionType.Code == TransactionTypesConstants.OrderPayment &&
            x.IsActive && !x.IsDeleted).ToList();

            if (orderPaymentTransactions != null && orderPaymentTransactions.Any() && ScaleUpShareTransactionType != null)
            {

                var existingTransactions = (from l in loanAccounts
                                            join at in accountTransactions on l.Id equals at.LoanAccountId
                                            join opt in orderPaymentTransactions on at.ParentAccountTransactionsID equals opt.ParentAccountTransactionsID
                                            where at.TransactionTypeId == ScaleUpShareTransactionType.Id
                                            select l.Id).Distinct().ToList();


                var notExistTransactions = orderPaymentTransactions.Where(x => x.AccountTransactionDetails != null && x.AccountTransactionDetails.Any() && !existingTransactions.Contains(x.LoanAccountId) && x.IsActive && !x.IsDeleted).Select(x => x.LoanAccountId).Distinct().ToList();
                if (notExistTransactions != null && notExistTransactions.Any())
                {
                    reply.Response = loanAccounts.Where(x => notExistTransactions.Contains(x.Id) && x.IsActive && !x.IsDeleted).GroupBy(x => new { x.AnchorCompanyId, x.NBFCCompanyId, x.ProductId }).Select(x => new GetSettlePaymentJobDataResponse
                    {
                        AnchorCompanyId = x.Key.AnchorCompanyId ?? 0,
                        NBFCCompanyId = x.Key.NBFCCompanyId,
                        ProductId = x.Key.ProductId
                    }).ToList();
                    if (reply.Response != null && reply.Response.Any())
                    {
                        reply.Status = true;
                        reply.Message = "Data Found";
                    }
                }
            }
            return reply;
        }

        public async Task<GRPCReply<bool>> InsertScaleUpShareTransactions(GRPCRequest<SettlePaymentJobRequest> request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            var loanAccounts = await _context.LoanAccounts.Where(l => l.IsActive && !l.IsDeleted).ToListAsync();

            var accountTransactions = await _context.AccountTransactions.Where(at => at.IsActive && !at.IsDeleted).Include(x => x.TransactionType).ToListAsync();

            var ScaleUpShareTransactionType = await _context.TransactionTypes.Where(tt => tt.IsActive && !tt.IsDeleted && tt.Code == TransactionTypesConstants.ScaleupShareAmount).FirstOrDefaultAsync();

            var orderPaymentTransactions = accountTransactions.Where(x => x.TransactionType.Code == TransactionTypesConstants.OrderPayment &&
            x.IsActive && !x.IsDeleted).ToList();

            if (orderPaymentTransactions != null && orderPaymentTransactions.Any() && ScaleUpShareTransactionType != null)
            {

                var existingTransactions = (from l in loanAccounts
                                            join at in accountTransactions on l.Id equals at.LoanAccountId
                                            join opt in orderPaymentTransactions on at.ParentAccountTransactionsID equals opt.ParentAccountTransactionsID
                                            where at.TransactionTypeId == ScaleUpShareTransactionType.Id
                                            select at.ParentAccountTransactionsID).Distinct().ToList();

                var notExistTransactions = orderPaymentTransactions.Where(x => !existingTransactions.Contains(x.ParentAccountTransactionsID) && x.IsActive && !x.IsDeleted).ToList();
                if (notExistTransactions != null && notExistTransactions.Any())
                {
                    List<string> detailHeads = new List<string> { TransactionDetailHeadsConstants.InterestPaymentAmount, TransactionDetailHeadsConstants.OverduePaymentAmount, TransactionDetailHeadsConstants.PenalPaymentAmount, TransactionDetailHeadsConstants.BouncePaymentAmount };
                    var transactionDetailList = await _context.AccountTransactionDetails.Where(x => detailHeads.Contains(x.TransactionDetailHead.Code) && x.IsActive && !x.IsDeleted).Include(x => x.TransactionDetailHead).ToListAsync();

                    foreach (var transaction in notExistTransactions.OrderBy(x => x.Id))
                    {
                        var transactionDetails = transactionDetailList.Where(x => x.AccountTransactionId == transaction.Id).ToList();
                        if (transactionDetails != null && transactionDetails.Any())
                        {
                            foreach (var transactionDetail in transactionDetails.OrderBy(x => x.Id))
                            {
                                ScaleupSettleTransactionRequestDC transactionRequest = new ScaleupSettleTransactionRequestDC
                                {
                                    ParentAccountTransactionId = transaction.ParentAccountTransactionsID ?? 0,
                                    ProductConfigs = request.Request,
                                    LoanAccountId = transaction.LoanAccount.Id,
                                    PaymentReqNo = transactionDetail.PaymentReqNo ?? "",
                                    TransactionTypeCode = TransactionTypesConstants.ScaleupShareAmount,
                                    PaymentDate = transactionDetail.PaymentDate
                                };
                                if (transactionDetail.TransactionDetailHead.Code == TransactionDetailHeadsConstants.InterestPaymentAmount)
                                    transactionRequest.InterestPaymentAmount = Math.Abs(transactionDetail.Amount);
                                else if (transactionDetail.TransactionDetailHead.Code == TransactionDetailHeadsConstants.OverduePaymentAmount)
                                    transactionRequest.OverDuePaymentAmount = Math.Abs(transactionDetail.Amount);
                                else if (transactionDetail.TransactionDetailHead.Code == TransactionDetailHeadsConstants.PenalPaymentAmount)
                                    transactionRequest.PenalPaymentAmount = Math.Abs(transactionDetail.Amount);
                                else if (transactionDetail.TransactionDetailHead.Code == TransactionDetailHeadsConstants.BouncePaymentAmount)
                                    transactionRequest.BouncePaymentAmount = Math.Abs(transactionDetail.Amount);
                                var res = await _TransactionSettlementManager.SettleTransactionDetail_ScaleUpShareTransaction(transactionRequest);
                            }
                        }
                    }
                }
            }
            return reply;
        }

        public async Task<GRPCReply<List<CompanyInvoiceSettlement>>> CompanyInvoiceSettlement(long CompanyInvoiceId)
        {
            GRPCReply<List<CompanyInvoiceSettlement>> reply = new GRPCReply<List<CompanyInvoiceSettlement>> { Message = "Data Not Found!!!" };
            reply.Response = await _context.CompanyInvoiceSettlements.Where(x => x.CompanyInvoiceId == CompanyInvoiceId && x.IsActive && !x.IsDeleted).ToListAsync();
            if (reply.Response != null && reply.Response.Any())
            {
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        public async Task<GRPCReply<List<LoanAccountListResponseDc>>> GetNBFCBusinessLoanAccountList(LoanAccountListRequestDc obj)
        {
            GRPCReply<List<LoanAccountListResponseDc>> gRPCReply = new GRPCReply<List<LoanAccountListResponseDc>>();

            //if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.MASOperationExecutive.ToLower())
            //{
            //    obj.Role = CompanyIdentificationCodeConstants.MASFIN;
            //}
            //else if (!string.IsNullOrEmpty(obj.Role) && obj.Role.ToLower() == UserRoleConstants.AYEOperationExecutive.ToLower())
            //{
            //    obj.Role = CompanyIdentificationCodeConstants.AYEFIN;
            //}

            var programtype = new SqlParameter("ProductType", obj.ProductType);
            var status = new SqlParameter("Status", obj.Status);
            var fromdate = new SqlParameter("FromDate", obj.FromDate);
            var toDate = new SqlParameter("ToDate", obj.ToDate);
            var cityname = new SqlParameter("CityName", obj.CityName ?? (object)DBNull.Value);
            var keyword = new SqlParameter("Keyword", obj.Keyword);
            var skip = new SqlParameter("Skip", obj.Skip);
            var take = new SqlParameter("Take", obj.Take);
            //var role = new SqlParameter("Role", obj.Role);
            var nbfccompanyid = new SqlParameter("NbfcCompanyId", obj.NbfcCompanyId);

            var anchorIds = new DataTable();
            anchorIds.Columns.Add("IntValue");
            foreach (var anchor in obj.AnchorId)
            {
                var dr = anchorIds.NewRow();
                dr["IntValue"] = anchor;
                anchorIds.Rows.Add(dr);
            }
            var AnchorIds = new SqlParameter
            {
                ParameterName = "AnchorId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = anchorIds
            };

            var list = _context.Database.SqlQueryRaw<LoanAccountListResponseDc>("exec NBFCBusinessLoanAccountList @ProductType,@Status, @Fromdate,@ToDate,@CityName,@Keyword,@Skip,@Take,@AnchorId,@NbfcCompanyId", programtype, status, fromdate, toDate, cityname, keyword, skip, take, AnchorIds, nbfccompanyid).AsEnumerable().ToList();
            if (list.Any())
            {
                gRPCReply.Response = list;
                gRPCReply.Status = true;
                gRPCReply.Message = "Success";
            }
            else
            {
                gRPCReply.Response = new List<LoanAccountListResponseDc>();
                gRPCReply.Status = false;
                gRPCReply.Message = "Not Found";
            }

            return gRPCReply;
        }

        public async Task<DisbursementDashboardDataResponseDc> GetHopTrendData(HopDashboardRequestDc req)
        {
            DisbursementDashboardDataResponseDc disbursementDashboardDataResponseDc = new DisbursementDashboardDataResponseDc();

            DateTime TrendDate = new DateTime(2024, 3, 1);
            DateTime yesterday = DateTime.Now.Date.AddDays(-1);
            DateTime firstDayOfCurrentMonth = new DateTime(yesterday.Year, yesterday.Month, 1);

            var disbursements = await (from
                                        l in _context.LoanAccounts
                                       join a in _context.AccountTransactions on l.Id equals a.LoanAccountId
                                       join t in _context.TransactionTypes on a.TransactionTypeId equals t.Id
                                       where a.IsActive
                                             && !a.IsDeleted && l.IsActive
                                             && !l.IsDeleted
                                             && t.Code == TransactionTypesConstants.Disbursement
                                             && a.Created.Date <= DateTime.Now.Date // assuming currentDate is a variable or parameter
                                       select new
                                       {
                                           ProductType = l.ProductType,
                                           DisbursementAmount = a.OrderAmount,
                                           DisbursementDate = a.Created.Date // casting the Created datetime to date
                                       }).ToListAsync();

            var CityName = new DataTable();
            CityName.Columns.Add("stringValue");
            foreach (var name in req.CityName)
            {
                var dr = CityName.NewRow();
                dr["stringValue"] = name;
                CityName.Rows.Add(dr);
            }
            var allCityNames = new SqlParameter
            {
                ParameterName = "@CityName",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.stringValues",
                Value = CityName.Rows.Count > 0 ? CityName : (object)DBNull.Value
            };
            var NbfcCompantId1 = new DataTable();
            NbfcCompantId1.Columns.Add("IntValue");
            if (req.NbfcCompanyId != null)
            {
                foreach (var id in req.NbfcCompanyId)
                {
                    var dr = NbfcCompantId1.NewRow();
                    dr["IntValue"] = id;
                    NbfcCompantId1.Rows.Add(dr);
                }
            }
            else
            {
                var dr = NbfcCompantId1.NewRow();
                dr["IntValue"] = 0;
                NbfcCompantId1.Rows.Add(dr);
            }
            var allNbfcIds = new SqlParameter
            {
                ParameterName = "@NbfcCompanyId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = NbfcCompantId1.Rows.Count > 0 ? NbfcCompantId1 : (object)DBNull.Value
            };
            var ProductType = new SqlParameter("ProductType", ProductTypeConstants.CreditLine);

            //var FromDate = new SqlParameter("FromDate", req.FromDate);
            var FromDate = new SqlParameter("FromDate", req.FromDate);
            var ToDate = new SqlParameter("ToDate", req.ToDate);

            var ScTrendData = _context.Database.SqlQueryRaw<HopDashboardDc>
                                ("exec GetHopTrendData @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                                , ProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            var ScRevenueData = _context.Database.SqlQueryRaw<HopDashboardRevenueDc>
                                ("exec GetHopScaleUpShareDetail @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                                , ProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            #region SCF

            if (ScTrendData.Any() || ScRevenueData.Any())
            {
                var MonthLabelList = new List<string>();
                DateTime startDate = TrendDate;
                DateTime endDate = DateTime.Now;
                DateTime current = startDate;

                while (current < endDate)
                {
                    string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(current.Month);
                    int year = current.Year;
                    MonthLabelList.Add(monthName + "," + year.ToString());
                    current = current.AddMonths(1);
                }

                GraphData dashboardUtilizedAmountGrapth = new GraphData();
                GraphData dashboardOverDueAmountGrapth = new GraphData();
                GraphData SCdashboardOutstandingAmountGrapth = new GraphData();
                GraphData scRevenueGraph = new GraphData();

                foreach (var month in MonthLabelList)
                {
                    string[] result = month.Split(',');

                    if (result.Length > 0 && result.Any())
                    {
                        int year = int.Parse(result[1]);

                        int monthNumber = DateTime.ParseExact(result[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                        DateTime lastDateOfMonth = new DateTime(year, monthNumber, DateTime.DaysInMonth(year, monthNumber));
                        var UtilizedAmount = ScTrendData.Where(x => x.TransactionDate.ToString("MMM") + "," + x.TransactionDate.ToString("yyyy") == month && ((DateTime.Now.Month == firstDayOfCurrentMonth.Month && x.TransactionDate.Date >= firstDayOfCurrentMonth.Date && x.TransactionDate.Date <= yesterday.Date) || (lastDateOfMonth.Month == x.TransactionDate.Month && x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date))).Sum(x => x.LTDUtilizedAmount);
                        dashboardUtilizedAmountGrapth.XValue.Add(month);
                        dashboardUtilizedAmountGrapth.YValue.Add((int)UtilizedAmount);

                        var OverdueAmount = ScTrendData.Where(x => x.TransactionDate.ToString("MMM") + "," + x.TransactionDate.ToString("yyyy") == month && ((DateTime.Now.Month == firstDayOfCurrentMonth.Month && x.TransactionDate.Date >= firstDayOfCurrentMonth.Date && x.TransactionDate.Date <= yesterday.Date) || (x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OverDueAmount);
                        dashboardOverDueAmountGrapth.XValue.Add(month);
                        dashboardOverDueAmountGrapth.YValue.Add((int)OverdueAmount);

                        var ScaleupShareAmount = ScRevenueData.Where(x => x.TransactionDate.ToString("MMM") + "," + x.TransactionDate.ToString("yyyy") == month && ((DateTime.Now.Month == firstDayOfCurrentMonth.Month && x.TransactionDate.Date >= firstDayOfCurrentMonth.Date && x.TransactionDate.Date <= yesterday.Date) || (x.TransactionDate.Month == lastDateOfMonth.Month && x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date))).Sum(x => x.ScaleupShare);
                        scRevenueGraph.XValue.Add(month);
                        scRevenueGraph.YValue.Add((int)ScaleupShareAmount);
                    }
                }
                disbursementDashboardDataResponseDc.utilizedAmounttGrapth = dashboardUtilizedAmountGrapth;
                disbursementDashboardDataResponseDc.overDueAmountGrapth = dashboardOverDueAmountGrapth;
                disbursementDashboardDataResponseDc.SCRevenueGraph = scRevenueGraph;
            }

            #endregion SCF

            #region BL

            var BlProductType = new SqlParameter("ProductType", ProductTypeConstants.BusinessLoan);
            var TrendBlData = _context.Database.SqlQueryRaw<HopDashboardDc>
                    ("exec GetHopTrendData @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                    , BlProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            var BLRevenueData = _context.Database.SqlQueryRaw<HopDashboardRevenueDc>
                                ("exec GetHopScaleUpShareDetail @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                                , BlProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            if (TrendBlData.Any() && BLRevenueData.Any())
            {
                var MonthLabelList = new List<string>();
                DateTime startDate = TrendDate;
                DateTime endDate = DateTime.Now;
                DateTime current = startDate;
                while (current < endDate)
                {
                    string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(current.Month);
                    int year = current.Year;
                    MonthLabelList.Add(monthName + "," + year.ToString());
                    current = current.AddMonths(1);
                }

                GraphData disbursementGraph = new GraphData();
                GraphData BLOutstandingAmountGrapth = new GraphData();
                GraphData BLRevenueGraph = new GraphData();
                GraphData dashboardOverDueAmountGrapth = new GraphData();

                foreach (var month in MonthLabelList)
                {
                    string[] result = month.Split(',');
                    if (result.Length > 0 && result.Any())
                    {
                        int year = int.Parse(result[1]);

                        int monthNumber = DateTime.ParseExact(result[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                        DateTime lastDateOfMonth = new DateTime(year, monthNumber, DateTime.DaysInMonth(year, monthNumber));
                        var OverDueAmount = TrendBlData.Where(x => x.TransactionDate.ToString("MMM") + "," + x.TransactionDate.ToString("yyyy") == month && ((DateTime.Now.Month == firstDayOfCurrentMonth.Month && x.TransactionDate.Date >= firstDayOfCurrentMonth.Date && x.TransactionDate.Date <= yesterday.Date) || (x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OverDueAmount);
                        dashboardOverDueAmountGrapth.XValue.Add(month);
                        dashboardOverDueAmountGrapth.YValue.Add((int)OverDueAmount);

                        var ScaleupShareAmount = BLRevenueData.Where(x => x.TransactionDate.ToString("MMM") + "," + x.TransactionDate.ToString("yyyy") == month && ((DateTime.Now.Month == firstDayOfCurrentMonth.Month && x.TransactionDate.Date >= firstDayOfCurrentMonth.Date && x.TransactionDate.Date <= yesterday.Date) || (x.TransactionDate.Month == lastDateOfMonth.Month && x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date))).Sum(x => x.ScaleupShare);
                        BLRevenueGraph.XValue.Add(month);
                        BLRevenueGraph.YValue.Add((int)ScaleupShareAmount);
                    }
                    if (disbursements.Any())
                    {
                        var disbursementAmount = disbursements.Where(x => x.DisbursementDate.ToString("MMM") + "," + x.DisbursementDate.ToString("yyyy") == month).Sum(x => x.DisbursementAmount);
                        disbursementGraph.XValue.Add(month);
                        disbursementGraph.YValue.Add((int)disbursementAmount);
                    }
                    else
                    {
                        disbursementGraph.XValue.Add(month);
                        disbursementGraph.YValue.Add(0);
                    }

                }
                disbursementDashboardDataResponseDc.disbursementGrapth = disbursementGraph;
                disbursementDashboardDataResponseDc.BLRevenueGraph = BLRevenueGraph;
                disbursementDashboardDataResponseDc.BLoverDueAmountGrapth = dashboardOverDueAmountGrapth;
            }
            #endregion BL

            #region SCF and BL
            if (ScTrendData.Any() || TrendBlData.Any())
            {
                var MonthLabelList = new List<string>();
                DateTime startDate = TrendDate;
                DateTime endDate = DateTime.Now;
                DateTime current = startDate;
                while (current < endDate)
                {
                    string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(current.Month);
                    int year = current.Year;
                    MonthLabelList.Add(monthName + "," + year.ToString());
                    current = current.AddMonths(1);
                }

                //DateTime today = DateTime.Today;
                //DateTime endOfMonth = new DateTime(today.Year,today.Month,DateTime.DaysInMonth(today.Year,today.Month));

                GraphData loanBookGrapth = new GraphData();
                foreach (var month in MonthLabelList)
                {
                    double scPrincipleOutstanding = 0;
                    double blPrincipleOutstanding = 0;

                    string[] result = month.Split(',');
                    if (result.Length > 0 && result.Any())
                    {
                        int year = int.Parse(result[1]);

                        int monthNumber = DateTime.ParseExact(result[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month;
                        DateTime lastDateOfMonth = new DateTime(year, monthNumber, DateTime.DaysInMonth(year, monthNumber));

                        scPrincipleOutstanding = ScTrendData.Where(x => x.TransactionDate.ToString("MMM") + "," + x.TransactionDate.ToString("yyyy") == month && ((x.TransactionDate.Month == firstDayOfCurrentMonth.Month && x.TransactionDate.Date >= firstDayOfCurrentMonth.Date && x.TransactionDate.Date <= yesterday.Date) || (x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OutStanding);
                        blPrincipleOutstanding = TrendBlData.Where(x => x.TransactionDate.ToString("MMM") + "," + x.TransactionDate.ToString("yyyy") == month && ((x.TransactionDate.Month == firstDayOfCurrentMonth.Month && x.TransactionDate.Date >= firstDayOfCurrentMonth.Date && x.TransactionDate.Date <= yesterday.Date) || (x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OutStanding);
                        var loanBookAmount = scPrincipleOutstanding + blPrincipleOutstanding;

                        loanBookGrapth.XValue.Add(month);
                        loanBookGrapth.YValue.Add((int)loanBookAmount);
                    }
                }
                disbursementDashboardDataResponseDc.LoanBook = loanBookGrapth;
            }
            #endregion SCF and BL
            return disbursementDashboardDataResponseDc;
        }

        public async Task<HopDisbursementDashboardResponseDc> GetHopDisbursementDashboard(HopDashboardRequestDc req)
        {
            HopDisbursementDashboardResponseDc hopDisbursementDashboardResponseDc = new HopDisbursementDashboardResponseDc();
            DateTime TrendDate = new DateTime(2024, 3, 1);
            DateTime yesterday = DateTime.Now.Date.AddDays(-1);
            DateTime firstDayOfCurrentMonth = new DateTime(yesterday.Year, yesterday.Month, 1);
            var RetentionPercentageData = _context.Database.SqlQueryRaw<RetentionPercentage>("exec GetRetentionPercentage").AsEnumerable().FirstOrDefault();
            if (RetentionPercentageData != null)
            {
                hopDisbursementDashboardResponseDc.RetentionPercentage = RetentionPercentageData;
            }

            var CityName = new DataTable();
            CityName.Columns.Add("stringValue");
            foreach (var name in req.CityName)
            {
                var dr = CityName.NewRow();
                dr["stringValue"] = name;
                CityName.Rows.Add(dr);
            }
            var allCityNames = new SqlParameter
            {
                ParameterName = "CityName",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.stringValues",
                Value = CityName.Rows.Count > 0 ? CityName : (object)DBNull.Value
            };
            var NbfcCompantId1 = new DataTable();
            NbfcCompantId1.Columns.Add("IntValue");
            if (req.NbfcCompanyId != null)
            {
                foreach (var id in req.NbfcCompanyId)
                {
                    var dr = NbfcCompantId1.NewRow();
                    dr["IntValue"] = id;
                    NbfcCompantId1.Rows.Add(dr);
                }
            }
            else
            {
                var dr = NbfcCompantId1.NewRow();
                dr["IntValue"] = 0;
                NbfcCompantId1.Rows.Add(dr);
            }
            var allNbfcIds = new SqlParameter
            {
                ParameterName = "NbfcCompanyId",
                SqlDbType = SqlDbType.Structured,
                TypeName = "dbo.IntValues",
                Value = NbfcCompantId1.Rows.Count > 0 ? NbfcCompantId1 : (object)DBNull.Value
            };
            var ProductType = new SqlParameter("ProductType", ProductTypeConstants.CreditLine);

            //var FromDate = new SqlParameter("FromDate", req.FromDate);
            var FromDate = new SqlParameter("FromDate", req.FromDate);
            var ToDate = new SqlParameter("ToDate", req.ToDate);

            var ScLoanDetail = _context.Database.SqlQueryRaw<HopDashboardDc>
                                ("exec GetHopTrendData @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                                , ProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            var ScRevenueData = _context.Database.SqlQueryRaw<HopDashboardRevenueDc>
                                ("exec GetHopScaleUpShareDetail @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                                , ProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            var DPDData = _context.Database.SqlQueryRaw<HopDashboardOverDueDaysDc>
                                ("exec GetHopScaleUpOverDueDays @FromDate, @ToDate"
                                , FromDate, ToDate).AsEnumerable().ToList();

            var disbursements = await (from
                            l in _context.LoanAccounts
                                       join a in _context.AccountTransactions on l.Id equals a.LoanAccountId
                                       join t in _context.TransactionTypes on a.TransactionTypeId equals t.Id
                                       where a.IsActive
                                             && !a.IsDeleted && l.IsActive
                                             && !l.IsDeleted
                                             && t.Code == TransactionTypesConstants.Disbursement
                                             && a.Created.Date <= DateTime.Now.Date // assuming currentDate is a variable or parameter
                                       select new
                                       {
                                           ProductType = l.ProductType,
                                           DisbursementAmount = a.OrderAmount,
                                           DisbursementDate = a.Created.Date // casting the Created datetime to date
                                       }).ToListAsync();

            var AllActiveCustomers = await (
                                        from l in _context.LoanAccounts
                                        join ac in _context.AccountTransactions on l.Id equals ac.LoanAccountId
                                        join t in _context.TransactionTypes on ac.TransactionTypeId equals t.Id
                                        where req.FromDate.Date <= ac.Created.Date && req.ToDate.Date >= ac.Created.Date && l.IsActive && !l.IsDeleted && l.IsAccountActive && t.Code == TransactionTypesConstants.OrderPlacement
                                        select new
                                        {
                                            LoanAccountId = ac.LoanAccountId,
                                            productType = l.ProductType
                                        }).ToListAsync();

            int lastDay = DateTime.DaysInMonth(req.ToDate.Year, req.ToDate.Month);
            DateTime lastDateOfMonth = new DateTime(req.ToDate.Year, req.ToDate.Month, lastDay);

            int fromDateLastDay = DateTime.DaysInMonth(req.FromDate.Year, req.FromDate.Month);
            DateTime lastDateOfMonthFromDate = new DateTime(req.FromDate.Year, req.FromDate.Month, fromDateLastDay);

            int toDateLastDay = DateTime.DaysInMonth(req.ToDate.Year, req.ToDate.Month);
            DateTime lastDateOfMonthToDate = new DateTime(req.ToDate.Year, req.ToDate.Month, toDateLastDay);

            if (ScLoanDetail != null && ScLoanDetail.Count > 0 && ScLoanDetail.Any() || ScRevenueData.Any())
            {
                hopDisbursementDashboardResponseDc.SCActiveCustomer = AllActiveCustomers.Where(x => x.productType == ProductTypeConstants.CreditLine).Distinct().Count();
                hopDisbursementDashboardResponseDc.SCTotalLimit = ScLoanDetail.Where(x => x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date && ((req.FromDate.Month == req.ToDate.Month && req.ToDate.Date == DateTime.Now.Date && x.TransactionDate.Date == yesterday.Date) || (req.FromDate.Month != req.ToDate.Month && (((req.ToDate.Month == lastDateOfMonth.Month || req.ToDate.Date == DateTime.Now.Date) && x.TransactionDate.Date == yesterday.Date) || (req.ToDate.Date == lastDateOfMonth.Date && x.TransactionDate.Date == lastDateOfMonth.Date))))).Sum(y => y.CreditLimitAmount);

                double scRevenue = 0;
                if (ScRevenueData.Any())
                {
                    //it is search only month on month basis
                    scRevenue = ScRevenueData.Where(x => x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date).Sum(x => x.ScaleupShare);
                }
                if (hopDisbursementDashboardResponseDc.SCActiveCustomer > 0 && scRevenue > 0)
                    hopDisbursementDashboardResponseDc.AvgSCRevenue = scRevenue / hopDisbursementDashboardResponseDc.SCActiveCustomer;

                if (ScLoanDetail.Any())
                {
                    var diffcount = 0;
                    if(req.ToDate.Date == DateTime.Now.Date && req.FromDate.Month == req.ToDate.Month)
                    {
                        diffcount = yesterday.Day - req.FromDate.Day;
                    }
                    else if(req.FromDate.Month != req.ToDate.Month) //req.ToDate.Date == lastDateOfMonth.Date ||
                    {
                        diffcount = req.ToDate.Day - req.FromDate.Day;
                    }
                    double outstanding = ScLoanDetail.Where(x => x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date && ((req.FromDate.Month == req.ToDate.Month && req.ToDate.Date == DateTime.Now.Date && x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= yesterday.Date) || (req.FromDate.Month != req.ToDate.Month && x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date))).Sum(x => x.OutStanding);
                    if (outstanding > 0 && (req.FromDate.Month != req.ToDate.Month))
                        hopDisbursementDashboardResponseDc.AvgDailyOutstanding = outstanding / diffcount;
                    else if (outstanding > 0 && req.ToDate.Date == DateTime.Now.Date && req.FromDate.Month == req.ToDate.Month)
                    {
                        hopDisbursementDashboardResponseDc.AvgDailyOutstanding = outstanding / diffcount;
                    }

                    double CreditLimitAmount = ScLoanDetail.Where(x => x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date && ((req.FromDate.Month == req.ToDate.Month && req.ToDate.Date == DateTime.Now.Date && x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= yesterday.Date) || (req.FromDate.Month != req.ToDate.Month && x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date))).Sum(y => y.CreditLimitAmount);
                    if (CreditLimitAmount > 0 && (req.FromDate.Month != req.ToDate.Month))
                        hopDisbursementDashboardResponseDc.AvgDailyLimit = CreditLimitAmount / diffcount;
                    else if (CreditLimitAmount > 0 && req.ToDate.Date == DateTime.Now.Date && req.FromDate.Month == req.ToDate.Month)
                    {
                        hopDisbursementDashboardResponseDc.AvgDailyLimit = CreditLimitAmount / diffcount;
                    }
                }
                if (hopDisbursementDashboardResponseDc.AvgDailyOutstanding > 0 && hopDisbursementDashboardResponseDc.AvgDailyLimit > 0)
                {
                    hopDisbursementDashboardResponseDc.AvgUtilization = hopDisbursementDashboardResponseDc.AvgDailyOutstanding / hopDisbursementDashboardResponseDc.AvgDailyLimit;
                }

                #region SCDPD
                //it is only work month on month
                if (DPDData != null && DPDData.Any())
                {
                    var ScDPDData = DPDData.Where(x => x.ProductType == ProductTypeConstants.CreditLine).ToList();
                    //DateTime fromDate = req.FromDate;
                    //DateTime toDate = req.ToDate;
                    //int interval = 15;
                    //var dateRanges = GetDateRanges(fromDate, toDate, interval);
                    //List<DashboardDPD> dashboardDPDs = new List<DashboardDPD>();
                    //int count = 0;
                    //foreach (var Range in dateRanges)
                    //{
                    //    var setStartRange = Range.Item1.Date == fromDate.Date ? 0 : (count * interval);
                    //    count++;
                    //    var setEndRange = count * interval > 0 ? count * interval : setStartRange + count * interval;
                    //    dashboardDPDs.Add(new DashboardDPD
                    //    {
                    //        Range = ((setStartRange.ToString() == "0") ? 0 : setStartRange + 1) + "-" + setEndRange,
                    //        //Amount = ScLoanDetail.Where(x => x.TransactionDate >= Range.Item1.Date && x.TransactionDate <= Range.Item2.Date).Sum(x => x.Outstanding)
                    //        Amount = ScDPDData.Where(x => x.Aging >= setStartRange + 1 && x.Aging <= setEndRange && ((DateTime.Now.Month == req.ToDate.Month && x.TransactionDate.Date == yesterday.Date) || (req.ToDate.Month == lastDateOfMonth.Month && x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OverdueAmount)
                    //    });
                    //}

                    var ranges = new List<(int Min, int Max)>
                    {
                        (0, 15),
                        (16, 30),
                        (31, 60),
                        (61, 90),
                        (91, 120),
                        (121, 1000) // >120
                    };
                    List<DashboardDPD> dashboardDPDs = new List<DashboardDPD>();
                    foreach (var Range in ranges)
                    {
                        dashboardDPDs.Add(new DashboardDPD
                        {
                            Range = Range.Min + "-" + Range.Max,
                            //Amount = ScLoanDetail.Where(x => x.TransactionDate >= Range.Item1.Date && x.TransactionDate <= Range.Item2.Date).Sum(x => x.Outstanding)
                            Amount = ScDPDData.Where(x => x.Aging >= Range.Min && x.Aging <= Range.Max && (x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date) && ((DateTime.Now.Date == req.ToDate.Date && x.TransactionDate.Date == yesterday.Date) || (lastDateOfMonth.Date == req.ToDate.Date && x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OverdueAmount)
                        });
                    }

                    hopDisbursementDashboardResponseDc.SCDPD = dashboardDPDs;
                }
                #endregion SCDPD 

            }

            var BlProductType = new SqlParameter("ProductType", ProductTypeConstants.BusinessLoan);
            var BlLoanDetail = _context.Database.SqlQueryRaw<HopDashboardDc>
                    ("exec GetHopTrendData @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                    , BlProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            var BlRevenueData = _context.Database.SqlQueryRaw<HopDashboardRevenueDc>
                                ("exec GetHopScaleUpShareDetail @ProductType, @CityName, @NbfcCompanyId, @FromDate, @ToDate"
                                , BlProductType, allCityNames, allNbfcIds, FromDate, ToDate).AsEnumerable().ToList();

            if (BlLoanDetail != null && BlLoanDetail.Count > 0 && BlLoanDetail.Any() || BlRevenueData.Any())
            {
                hopDisbursementDashboardResponseDc.BLActiveCustomer = AllActiveCustomers.Where(x => x.productType == ProductTypeConstants.BusinessLoan).Distinct().Count();
                hopDisbursementDashboardResponseDc.BLTotalLimit = BlLoanDetail.Where(x => x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date && ((req.FromDate.Month == req.ToDate.Month && req.ToDate.Date == DateTime.Now.Date && x.TransactionDate.Date == yesterday.Date) || (req.FromDate.Month != req.ToDate.Month && (((req.ToDate.Month == lastDateOfMonth.Month || req.ToDate.Date == DateTime.Now.Date) && x.TransactionDate.Date == yesterday.Date) || (req.ToDate.Date == lastDateOfMonth.Date && x.TransactionDate.Date == lastDateOfMonth.Date))))).Sum(y => y.CreditLimitAmount);
                double blRevenue = 0;
                if (BlRevenueData.Any())
                {
                    blRevenue = BlRevenueData.Where(x =>  x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date).Sum(x => x.ScaleupShare);
                }

                if (hopDisbursementDashboardResponseDc.BLActiveCustomer > 0 && blRevenue > 0)
                    hopDisbursementDashboardResponseDc.AvgBLRevenue = blRevenue / hopDisbursementDashboardResponseDc.BLActiveCustomer;



                #region BLDPD

                if (DPDData != null && DPDData.Any())
                {
                    var BlDPDData = DPDData.Where(x => x.ProductType == ProductTypeConstants.BusinessLoan).ToList();
                    //DateTime fromDate = req.FromDate;
                    //DateTime toDate = req.ToDate;
                    //int interval = 15;

                    //var dateRanges = GetDateRanges(fromDate, toDate, interval);

                    //var DateRangeLabelList = new List<string>();
                    //List<DashboardDPD> dashboardDPDs = new List<DashboardDPD>();
                    //int count = 0;
                    //foreach (var Range in dateRanges)
                    //{
                    //    var setStartRange = Range.Item1.Date == fromDate.Date ? 0 : (count * interval);
                    //    count++;
                    //    var setEndRange = count * interval > 0 ? count * interval : setStartRange + count * interval;
                    //    dashboardDPDs.Add(new DashboardDPD
                    //    {
                    //        Range = ((setStartRange.ToString() == "0") ? 0 : setStartRange + 1) + "-" + setEndRange,
                    //        //Amount = ScLoanDetail.Where(x => x.TransactionDate >= Range.Item1.Date && x.TransactionDate <= Range.Item2.Date).Sum(x => x.Outstanding)
                    //        Amount = BlDPDData.Where(x => x.Aging >= setStartRange + 1 && x.Aging <= setEndRange && ((DateTime.Now.Month == req.ToDate.Month && x.TransactionDate.Date == yesterday.Date) || (req.ToDate.Month == lastDateOfMonth.Month && x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OverdueAmount)
                    //    });
                    //}

                    var ranges = new List<(int Min, int Max)>
                    {
                        (0, 15),
                        (16, 30),
                        (31, 60),
                        (61, 90),
                        (91, 120),
                        (121, 1000) // >120
                    };
                    List<DashboardDPD> dashboardDPDs = new List<DashboardDPD>();
                    foreach (var Range in ranges)
                    {
                        dashboardDPDs.Add(new DashboardDPD
                        {
                            Range = Range.Min + "-" + Range.Max,
                            Amount = BlDPDData.Where(x => x.Aging >= Range.Min && x.Aging <= Range.Max && (x.TransactionDate.Date >= req.FromDate.Date && x.TransactionDate.Date <= req.ToDate.Date) && ((DateTime.Now.Date == req.ToDate.Date && x.TransactionDate.Date == yesterday.Date) || (lastDateOfMonth.Date == req.ToDate.Date && x.TransactionDate.Date == lastDateOfMonth.Date))).Sum(x => x.OverdueAmount)
                        });
                    }
                    hopDisbursementDashboardResponseDc.BLDPD = dashboardDPDs;
                }
                #endregion BLDPD 

            }
            if ((BlLoanDetail != null && BlLoanDetail.Count > 0 && BlLoanDetail.Any()) || (ScLoanDetail != null && ScLoanDetail.Count > 0 && ScLoanDetail.Any()))
            {
                double blOutstanding = 0;
                double blOverDue = 0;
                double scOutstanding = 0;
                double scOverDue = 0;

                if (ScLoanDetail.Any())
                {
                    scOutstanding = ScLoanDetail.Where(x => x.TransactionDate.Month == req.ToDate.Month && (x.TransactionDate.Date == req.ToDate.Date || x.TransactionDate.Date == yesterday.Date)).Sum(x => x.OutStanding);
                    scOverDue = ScLoanDetail.Where(x => x.TransactionDate.Month == req.ToDate.Month && x.TransactionDate.Date == req.ToDate.Date || x.TransactionDate.Date == yesterday.Date).Sum(x => x.OverDueAmount);
                }
                if (BlLoanDetail.Any())
                {
                    blOutstanding = BlLoanDetail.Where(x => x.TransactionDate.Month == req.ToDate.Month && x.TransactionDate.Date == req.ToDate.Date || x.TransactionDate.Date == yesterday.Date).Sum(x => x.OutStanding);
                    blOverDue = BlLoanDetail.Where(x => x.TransactionDate.Month == req.ToDate.Month && x.TransactionDate.Date == req.ToDate.Date || x.TransactionDate.Date == yesterday.Date).Sum(x => x.OverDueAmount);
                }
                hopDisbursementDashboardResponseDc.TotalOutstanding = scOutstanding + blOutstanding;
                hopDisbursementDashboardResponseDc.TotalOverDueAmount = scOverDue + blOverDue;
            }

            if (hopDisbursementDashboardResponseDc.AvgBLRevenue > 0 || hopDisbursementDashboardResponseDc.AvgSCRevenue > 0)
            {
                hopDisbursementDashboardResponseDc.TotalRevenue = hopDisbursementDashboardResponseDc.AvgBLRevenue + hopDisbursementDashboardResponseDc.AvgSCRevenue;
            }
            return hopDisbursementDashboardResponseDc;
        }
        public static List<Tuple<DateTime, DateTime>> GetDateRanges(DateTime fromDate, DateTime endDate, int interval)
        {
            var dateRanges = new List<Tuple<DateTime, DateTime>>();
            int totalDays = (endDate - fromDate).Days;
            int intervals = totalDays / interval;

            for (int i = 0; i < intervals; i++)
            {
                DateTime start = fromDate.AddDays(i * interval);
                DateTime end = start.AddDays(interval - 1);
                dateRanges.Add(new Tuple<DateTime, DateTime>(start, end));
            }

            // Handle remaining days if any
            if (totalDays % interval != 0)
            {
                DateTime start = fromDate.AddDays(intervals * interval);
                dateRanges.Add(new Tuple<DateTime, DateTime>(start, endDate));
            }

            return dateRanges;
        }

        public async Task<CommonResponse> ClearInitiateLimitByReferenceId(long LeadAccountId, string ReferenceId)
        {
            CommonResponse res = new CommonResponse();

            var transactionStatuse = _context.TransactionStatuses.Where(x => x.IsActive).ToList();


            var loanaccount = _context.LoanAccounts.Where(x => x.Id == LeadAccountId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).FirstOrDefault();
            if (loanaccount != null)
            {
                double InitiateAmt = 0;
                var accounttransaction = await _context.AccountTransactions.Where(x => x.LoanAccountId == LeadAccountId && x.ReferenceId == ReferenceId && x.IsActive && !x.IsDeleted && x.TransactionType.Code == TransactionTypesConstants.OrderPlacement
                                           && (x.TransactionStatus.Code == TransactionStatuseConstants.Initiate)).FirstOrDefaultAsync();

                if (accounttransaction != null)
                {
                    InitiateAmt = accounttransaction.TransactionAmount;
                    loanaccount.LoanAccountCredits.CreditLimitAmount += InitiateAmt;

                    accounttransaction.TransactionStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Canceled).Id; ;
                    _context.Entry(accounttransaction).State = EntityState.Modified;
                    if (_context.SaveChanges() > 0)
                    {
                        res.status = true;
                        res.Message = "Limit Updated";
                    }
                    else
                    {
                        res.status = false;
                        res.Message = "Failed";
                    }
                }
            }
            else
            {
                res.status = false;
                res.Message = "Account not found";
            }
            return res;
        }
        public async Task<GRPCReply<List<long>>> GetLoanAccountData()
        {
            GRPCReply<List<long>> gRPCReply = new GRPCReply<List<long>>();
            var LoanData = _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted).Select(y => y.LeadId).ToList();

            if (LoanData.Any())
            {
                gRPCReply.Response = LoanData;
                gRPCReply.Status = true;
                gRPCReply.Message = "Data Found";
            }
            else
            {
                gRPCReply.Response = new List<long>();
                gRPCReply.Status = false;
                gRPCReply.Message = "No Data Found";

            }
            return gRPCReply;
        }
        public async Task<CommonResponse> AddRepaymentAccountDetails(LoanRepaymentAccountDetailRequestDC loanRepaymentAccountDetailRequestDC, string UserId)
        {
            CommonResponse res = new CommonResponse();
            var loanaccount = _context.LoanAccounts.Where(x => x.Id == loanRepaymentAccountDetailRequestDC.LeadAccountId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).FirstOrDefault();

            if (loanaccount != null)
            {
                var loanBankAccountDetail = _context.LoanBankDetails.Where(x => x.LoanAccountId == loanaccount.Id && x.IsActive && !x.IsDeleted).FirstOrDefault();
                if (loanBankAccountDetail != null)
                {
                    loanBankAccountDetail.VirtualAccountNumber = loanRepaymentAccountDetailRequestDC.VirtualAccountNumber;
                    loanBankAccountDetail.VirtualBankName = loanRepaymentAccountDetailRequestDC.VirtualBankName;
                    loanBankAccountDetail.VirtualIFSCCode = loanRepaymentAccountDetailRequestDC.VirtualIFSCCode;
                    loanBankAccountDetail.VirtualUPIId = loanRepaymentAccountDetailRequestDC.VirtualUPIId;
                    loanBankAccountDetail.LastModified = DateTime.Now;
                    loanBankAccountDetail.LastModifiedBy = UserId;
                    _context.Entry(loanBankAccountDetail).State = EntityState.Modified;
                    if (_context.SaveChanges() > 0)
                    {
                        res.status = true;
                        res.Message = "Detail Added Successfully.";
                    }
                    else
                    {
                        res.status = false;
                        res.Message = "Failed";
                    }
                }
                else
                {
                    res.status = false;
                    res.Message = "Account not found";
                }
            }
            else
            {
                res.status = false;
                res.Message = "Account not found";
            }
            return res;
        }

        public async Task<CommonResponse> GetRepaymentAccountDetails(long LeadId)
        {
            CommonResponse res = new CommonResponse();
            var loanaccount = await _context.LoanAccounts.Where(x => x.LeadId == LeadId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();

            if (loanaccount != null)
            {
                var loanBankAccountDetail = await _context.LoanBankDetails.Where(x => x.LoanAccountId == loanaccount.Id && x.IsActive && !x.IsDeleted).Select(x => new LoanRepaymentAccountDetailResponseDC
                {
                    VirtualAccountNumber = x.VirtualAccountNumber,
                    VirtualBankName = x.VirtualBankName,
                    VirtualIFSCCode = x.VirtualIFSCCode,
                    VirtualUPIId = x.VirtualUPIId
                }).FirstOrDefaultAsync();
                if (loanBankAccountDetail != null)
                {
                    res.Result = loanBankAccountDetail;
                    res.status = true;
                    res.Message = "Detail Found Successfully.";
                }
                else
                {
                    res.status = false;
                    res.Message = "Account not found";
                }
            }
            else
            {
                res.status = false;
                res.Message = "Account not found";
            }
            return res;
        }
        public async Task<CommonResponse> GetRepaymentAccountDetailsForAdmin(long LoanAccountId)
        {
            CommonResponse res = new CommonResponse();
            var loanaccount = await _context.LoanAccounts.Where(x => x.Id == LoanAccountId && x.IsActive && !x.IsDeleted).Include(x => x.LoanAccountCredits).FirstOrDefaultAsync();

            if (loanaccount != null)
            {
                var loanBankAccountDetail = await _context.LoanBankDetails.Where(x => x.LoanAccountId == loanaccount.Id && x.IsActive && !x.IsDeleted).Select(x => new LoanRepaymentAccountDetailResponseDC
                {
                    VirtualAccountNumber = x.VirtualAccountNumber,
                    VirtualBankName = x.VirtualBankName,
                    VirtualIFSCCode = x.VirtualIFSCCode,
                    VirtualUPIId = x.VirtualUPIId
                }).FirstOrDefaultAsync();
                if (loanBankAccountDetail != null && !string.IsNullOrEmpty(loanBankAccountDetail.VirtualAccountNumber) && !string.IsNullOrEmpty(loanBankAccountDetail.VirtualBankName))
                {
                    res.Result = loanBankAccountDetail;
                    res.status = true;
                    res.Message = "Detail Found Successfully.";
                }
                else
                {
                    res.status = false;
                    res.Message = "Detail not found";
                }
            }
            else
            {
                res.status = false;
                res.Message = "Account not found";
            }
            return res;
        }
        public async Task<bool> InsertTopupNumber(long loanAccountId)
        {
            var blackSoilAccountDetailList = await _context.BlackSoilAccountDetails.Where(x => x.BlackSoilLoanId != 0 && x.IsActive && !x.IsDeleted
            && (loanAccountId == 0 || x.LoanAccountId == loanAccountId)).ToListAsync();
            var blackSoilAccountTransactions = await _context.BlackSoilAccountTransactions.Where(x => string.IsNullOrEmpty(x.TopUpNumber) && x.Status != "pending" && x.IsActive && !x.IsDeleted).ToListAsync();
            if (blackSoilAccountDetailList != null && blackSoilAccountDetailList.Any() && blackSoilAccountTransactions != null && blackSoilAccountTransactions.Any())
            {
                foreach (var blackSoilAccountDetail in blackSoilAccountDetailList)
                {
                    string url = "";
                    var loanAccountDetailAPI = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanAccountDetail && x.IsActive && !x.IsDeleted);
                    if (loanAccountDetailAPI != null)
                    {
                        url = loanAccountDetailAPI.APIUrl;
                        url = url.Replace("{{accountid}}", blackSoilAccountDetail.BlackSoilLoanId.ToString());

                        BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
                        var response = await blackSoilNBFCHelper.GetLoanAccountDetail(url, loanAccountDetailAPI.TAPIKey, loanAccountDetailAPI.TAPISecretKey, 0);
                        //_context.BlackSoilCommonAPIRequestResponses.Add(response);
                        //await _context.SaveChangesAsync();
                        if (response != null && response.IsSuccess)
                        {
                            BlackSoilLoanAccountExpandDc accountDetails = JsonConvert.DeserializeObject<BlackSoilLoanAccountExpandDc>(response.Response);

                            if (accountDetails != null && accountDetails.topups != null && accountDetails.topups.Any())
                            {
                                foreach (var topup in accountDetails.topups)
                                {
                                    if (topup.invoice != null)
                                    {
                                        var blackSoilAccountTransaction = blackSoilAccountTransactions.FirstOrDefault(x => x.WithdrawalId == topup.invoice.id);
                                        if (blackSoilAccountTransaction != null)
                                        {
                                            //var topupId = topup.invoice.invoice_id;
                                            blackSoilAccountTransaction.TopUpNumber = topup.invoice.invoice_id;
                                            _context.Entry(blackSoilAccountTransaction).State = EntityState.Modified;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<LoanPaymentsResultDC> validateLoanPayments(long loanAccountId)
        {
            LoanPaymentsResultDC list = new LoanPaymentsResultDC();

            GRPCReply<long> reply = new GRPCReply<long>();
            long blackSoilLoanId = 0;
            long blackSoilLoanAccountId = 0;

            //DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddMinutes(-15), INDIAN_ZONE);  //DateTime.Now.AddMinutes(-15);

            reply.Status = false;
            reply.Message = "Something went wrong.";
            reply.Response = 0;

            //var blackSoilAccountDetail = await _context.BlackSoilAccountDetails.FirstOrDefaultAsync(x => x.LoanAccountId == loanAccountId && x.IsActive && !x.IsDeleted);
            var blackSoilAccountDetailList = await _context.BlackSoilAccountDetails.Where(x => x.BlackSoilLoanId != 0 && x.IsActive && !x.IsDeleted
            && (loanAccountId == 0 || x.LoanAccountId == loanAccountId)).ToListAsync();

            if (blackSoilAccountDetailList != null && blackSoilAccountDetailList.Count > 0)
            //if (blackSoilAccountDetail != null && blackSoilAccountDetail.BlackSoilLoanId > 0)
            {
                foreach (var blackSoilAccountDetail in blackSoilAccountDetailList)
                {

                    blackSoilLoanId = blackSoilAccountDetail.BlackSoilLoanId;
                    blackSoilLoanAccountId = blackSoilAccountDetail.LoanAccountId; //bsadLoanAccountId

                    var blackSoilAccountTransactions = new List<blackSoilAccountTransactionsDC>();
                    var query = from t in _context.AccountTransactions //.Where(x => x.LoanAccountId == loanAccountId)
                                join tt in _context.TransactionTypes on t.TransactionTypeId equals tt.Id
                                join ts in _context.TransactionStatuses on t.TransactionStatusId equals ts.Id
                                join bb in _context.BlackSoilAccountTransactions on t.InvoiceId equals bb.LoanInvoiceId
                                where t.DisbursementDate != null
                                && tt.Code == TransactionTypesConstants.OrderPayment
                                //&& ts.Code != TransactionStatuseConstants.Paid
                                && t.IsActive && !t.IsDeleted && tt.IsActive && !tt.IsDeleted && ts.IsActive && !ts.IsDeleted
                                && t.LoanAccountId == blackSoilLoanAccountId
                                select new blackSoilAccountTransactionsDC
                                {
                                    WithdrawalId = bb.WithdrawalId ?? 0,
                                    AccountTransactionId = t.Id,
                                    InvoiceNo = t.InvoiceNo,
                                    StatusCode = ts.Code
                                };

                    blackSoilAccountTransactions = await query.ToListAsync();
                    if (blackSoilAccountTransactions != null && blackSoilAccountTransactions.Count > 0)
                    {
                        //--------------------ss---------
                        BlackSoilLoanAccountExpandDc accountDetails = null;

                        string url = "";
                        var loanAccountDetailAPI = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.BlackSoilLoanAccountDetail && x.IsActive && !x.IsDeleted);
                        if (loanAccountDetailAPI != null)
                        {
                            url = loanAccountDetailAPI.APIUrl;
                            url = url.Replace("{{accountid}}", blackSoilLoanId.ToString()); //item.ThirdPartyLoanAccountId

                            BlackSoilNBFCHelper blackSoilNBFCHelper = new BlackSoilNBFCHelper();
                            var response = await blackSoilNBFCHelper.GetLoanAccountDetail(url, loanAccountDetailAPI.TAPIKey, loanAccountDetailAPI.TAPISecretKey, 0);
                            _context.BlackSoilCommonAPIRequestResponses.Add(response);
                            await _context.SaveChangesAsync();
                            if (response != null && response.IsSuccess)
                            {
                                accountDetails = JsonConvert.DeserializeObject<BlackSoilLoanAccountExpandDc>(response.Response);

                                //--------------------ee----------
                                if (accountDetails != null && accountDetails.topups != null && accountDetails.topups.Any())
                                {
                                    // var PendingTransactionsList = await _TransactionSettlementManager.GetAllTransactionsList("", blackSoilLoanAccountId, null);
                                    string TransactionNo = "";
                                    DateTime? invoiceDate;

                                    List<GetOutstandingTransactionsDTO> PendingTransactionsList = new List<GetOutstandingTransactionsDTO>();

                                    var Transaction_No = new SqlParameter("TransactionNo", TransactionNo);
                                    var LoanAccount_Id = new SqlParameter("LoanAccountId", blackSoilLoanAccountId);
                                    //var Invoice_Date = new SqlParameter("InvoiceDate", invoiceDate == null ? DBNull.Value : (object)invoiceDate);

                                    try
                                    {
                                        //PendingTransactionsList = await _context.Database.SqlQueryRaw<GetOutstandingTransactionsDTO>("exec GetAllTransactionsList @TransactionNo, @LoanAccountId, @InvoiceDate", Transaction_No, LoanAccount_Id, Invoice_Date).ToListAsync();
                                        PendingTransactionsList = await _context.Database.SqlQueryRaw<GetOutstandingTransactionsDTO>("exec GetAllTransactionsList @LoanAccountId", LoanAccount_Id).ToListAsync();

                                    }
                                    catch (Exception ex)
                                    {
                                        throw;
                                    }

                                    list.loanPaymentsResultList = new List<LoanPaymentsResultListDC>();

                                    foreach (var doc in blackSoilAccountTransactions)
                                    {
                                        var topup = accountDetails.topups.FirstOrDefault(x => x.invoice.id == doc.WithdrawalId);
                                        if (topup != null)
                                        {
                                            if (doc.WithdrawalId == topup.invoice.id)
                                            {
                                                double topup_OrderAmount = 0;
                                                double topup_ExtraAmount = 0;
                                                double topup_InterestAmount = 0;
                                                double topup_BounceAmount = 0;
                                                double topup_PenalAmount = 0;
                                                double topup_OverdueAmount = 0;
                                                topup_OrderAmount = Convert.ToDouble(topup.extra.principal_paid);
                                                topup_ExtraAmount = Convert.ToDouble(topup.extra.extra_payment);
                                                topup_InterestAmount = Convert.ToDouble(topup.extra.interest_paid);
                                                //topup_BounceAmount = Convert.ToDouble(topup.extra.);
                                                topup_PenalAmount = Convert.ToDouble(topup.extra.penal_interest_paid);
                                                topup_OverdueAmount = Convert.ToDouble(topup.extra.overdue_interest_paid);


                                                double OrderPaymentAmount = 0;
                                                double ExtraPaymentAmount = 0;
                                                double InterestPaymentAmount = 0;
                                                double BouncePaymentAmount = 0;
                                                double PenalPaymentAmount = 0;
                                                double OverduePaymentAmount = 0;
                                                if (PendingTransactionsList != null && PendingTransactionsList.Any())
                                                {
                                                    var transactions = PendingTransactionsList.Where(x => x.InvoiceNo == doc.InvoiceNo).FirstOrDefault();
                                                    if (transactions != null)
                                                    {
                                                        OrderPaymentAmount = Math.Round(transactions.PaymentAmount ?? 0, 2);
                                                        ExtraPaymentAmount = Math.Round(transactions.ExtraPaymentAmount ?? 0, 2);
                                                        InterestPaymentAmount = Math.Round(transactions.InterestPaymentAmount ?? 0, 2);
                                                        BouncePaymentAmount = Math.Round(transactions.BouncePaymentAmount ?? 0, 2);
                                                        PenalPaymentAmount = Math.Round(transactions.PenalPaymentAmount ?? 0, 2);
                                                        OverduePaymentAmount = Math.Round(transactions.OverduePaymentAmount ?? 0, 2);
                                                    }
                                                    else
                                                    {
                                                        string head = "Invoice Not Found Head";
                                                        var mismatchValue = new LoanPaymentsResultListDC
                                                        {
                                                            InvoiceNo = doc.InvoiceNo.ToString(),
                                                            WithdrawalId = doc.WithdrawalId.ToString(), //topup.invoice.id
                                                            FieldInfo = head,
                                                            FieldOldValue = OrderPaymentAmount,
                                                            FieldNewValue = topup_OrderAmount
                                                        };
                                                        list.loanPaymentsResultList.Add(mismatchValue);
                                                    }
                                                }

                                                string errorMessage = "";

                                                if (OrderPaymentAmount > 0 && (topup_OrderAmount != OrderPaymentAmount))
                                                {
                                                    string head = "Order Head";
                                                    var mismatchValue = new LoanPaymentsResultListDC
                                                    {
                                                        InvoiceNo = doc.InvoiceNo.ToString(),
                                                        WithdrawalId = doc.WithdrawalId.ToString(), //topup.invoice.id
                                                        FieldInfo = head,
                                                        FieldOldValue = OrderPaymentAmount,
                                                        FieldNewValue = topup_OrderAmount
                                                    };
                                                    list.loanPaymentsResultList.Add(mismatchValue);
                                                    errorMessage += "," + head;
                                                }
                                                if (ExtraPaymentAmount > 0 && (topup_ExtraAmount != ExtraPaymentAmount))
                                                {
                                                    string head = "Extra Head";
                                                    var mismatchValue = new LoanPaymentsResultListDC
                                                    {
                                                        InvoiceNo = doc.InvoiceNo.ToString(),
                                                        WithdrawalId = doc.WithdrawalId.ToString(), //topup.invoice.id
                                                        FieldInfo = head,
                                                        FieldOldValue = ExtraPaymentAmount,
                                                        FieldNewValue = topup_ExtraAmount
                                                    };
                                                    list.loanPaymentsResultList.Add(mismatchValue);
                                                    errorMessage += "," + head;
                                                }
                                                if (InterestPaymentAmount > 0 && (topup_InterestAmount != InterestPaymentAmount))
                                                {
                                                    string head = "Interest Head";
                                                    var mismatchValue = new LoanPaymentsResultListDC
                                                    {
                                                        InvoiceNo = doc.InvoiceNo.ToString(),
                                                        WithdrawalId = doc.WithdrawalId.ToString(), //topup.invoice.id
                                                        FieldInfo = head,
                                                        FieldOldValue = InterestPaymentAmount,
                                                        FieldNewValue = topup_InterestAmount
                                                    };
                                                    list.loanPaymentsResultList.Add(mismatchValue);
                                                    errorMessage += "," + head;
                                                }
                                                //if (BouncePaymentAmount>0 && (topup_BounceAmount != BouncePaymentAmount))
                                                //{ }
                                                if (PenalPaymentAmount > 0 && (topup_PenalAmount != PenalPaymentAmount))
                                                {
                                                    string head = "Penal Head";
                                                    var mismatchValue = new LoanPaymentsResultListDC
                                                    {
                                                        InvoiceNo = doc.InvoiceNo.ToString(),
                                                        WithdrawalId = doc.WithdrawalId.ToString(), //topup.invoice.id
                                                        FieldInfo = head,
                                                        FieldOldValue = PenalPaymentAmount,
                                                        FieldNewValue = topup_PenalAmount
                                                    };
                                                    list.loanPaymentsResultList.Add(mismatchValue);
                                                    errorMessage += "," + head;
                                                }
                                                if (OverduePaymentAmount > 0 && (topup_OverdueAmount != OverduePaymentAmount))
                                                {
                                                    string head = "Overdue Head";
                                                    var mismatchValue = new LoanPaymentsResultListDC
                                                    {
                                                        InvoiceNo = doc.InvoiceNo.ToString(),
                                                        WithdrawalId = doc.WithdrawalId.ToString(), //topup.invoice.id
                                                        FieldInfo = head,
                                                        FieldOldValue = OverduePaymentAmount,
                                                        FieldNewValue = topup_OverdueAmount
                                                    };
                                                    list.loanPaymentsResultList.Add(mismatchValue);
                                                    errorMessage += "," + head;
                                                }

                                                list.Message = errorMessage;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                reply.Message = "expand API not working";
                            }

                        }
                        else
                        {
                            reply.Message = "NBFCCompanyAPI not maintain";
                        }
                    }
                    else
                    {
                        reply.Status = false;
                        reply.Message = "blackSoil LoanAccountId / BlackSoilLoanId not maintain";
                        reply.Response = 0;
                    }
                }
            }
            else
            {
                reply.Message = "blackSoilAccountDetailList record  not maintain";
            }

            //if (loanResult.Any() && loanResult.Count > 0)
            //{ reply.Message = loanResult.ToList(); }
            return list;

        }


        public async Task<GRPCReply<List<DSAMISLoanResponseDC>>> GetDSAMISLoanData(DSAMISRequestDC request)
        {
            GRPCReply<List<DSAMISLoanResponseDC>> response = new GRPCReply<List<DSAMISLoanResponseDC>> { Message = "Data Not Found" };
            var dsaCompanyId = new SqlParameter("@DSACompanyId", request.DSACompanyId);
            var startDate = new SqlParameter("@StartDate", request.FromDate);
            var endDate = new SqlParameter("@EndDate", request.ToDate);
            try
            {

                response.Response = await _context.Database.SqlQueryRaw<DSAMISLoanResponseDC>("exec GetDSAMISList @DSACompanyId,@StartDate,@EndDate", dsaCompanyId, startDate, endDate).ToListAsync();
                if (response.Response != null && response.Response.Any())
                {
                    response.Status = true;
                    response.Message = "Data Found";
                }
                return response;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<GRPCReply<LoanPaymentsResultDC>> GetLedger_RetailerStatement(GetLedger_RetailerStatementDC getLedger_RetailerStatementDC)
        {
            GRPCReply<LoanPaymentsResultDC> lists = new GRPCReply<LoanPaymentsResultDC>()
            {
                Response = new LoanPaymentsResultDC(),
                Status = false,
                Message = "Data Not Found"
            };
            LoanPaymentsResultDC paymentsResultDC = new LoanPaymentsResultDC();
            string replacetext = "";
            string htmldata = "";

            List<Ledger_RetailerStatementDC> RetailerStatementList = new List<Ledger_RetailerStatementDC>();
            List<DataTable> LoanDataTable = new List<DataTable>();
            var LoanAccount_Id = new SqlParameter("LoanAccountId", getLedger_RetailerStatementDC.loanAccountId);
            var FromDate_value = new SqlParameter("FromDate", getLedger_RetailerStatementDC.FromDate);
            var ToDate_value = new SqlParameter("ToDate", getLedger_RetailerStatementDC.ToDate);
            //var Invoice_Date = new SqlParameter("InvoiceDate", invoiceDate == null ? DBNull.Value : (object)invoiceDate);

            try
            {
                RetailerStatementList = await _context.Database.SqlQueryRaw<Ledger_RetailerStatementDC>("exec Ledger_RetailerStatement @LoanAccountId, @FromDate, @ToDate", LoanAccount_Id, FromDate_value, ToDate_value).ToListAsync();

                if (RetailerStatementList.Count() > 0 && RetailerStatementList.Any())
                {
                    byte[] file = null;
                    using (var wc = new WebClient())
                    {
                        file = wc.DownloadData("https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/8b520a76-0e25-440e-9957-52b222e60611.html");
                    }

                    if (file.Length > 0)
                    {
                        htmldata = System.Text.Encoding.UTF8.GetString(file);

                        if (!string.IsNullOrEmpty(htmldata) && htmldata.Any())
                        {

                            string imgUrl = "https://csg10037ffe956af864.blob.core.windows.net/scaleupfiles/ac73448a-296e-4bd1-b40c-f6fe5dae86ad.png";
                            string SignImageBase64 = await FileSaverHelper.GetStreamFromUrlBase64(imgUrl);

                            string logoimage = "<img src=\"data:image/png;base64,{imgUrl}\" alt=\"logo\" style=\" width: 10%; margin-top: 28px; margin-left: 20px;\">";
                            logoimage = logoimage.Replace("{imgUrl}", SignImageBase64);

                            replacetext = $"{logoimage} ";
                            htmldata = htmldata.Replace("[ScaleupLogo]", replacetext);

                            replacetext = $"{RetailerStatementList.FirstOrDefault().Lender} ";
                            htmldata = htmldata.Replace("[Lender]", replacetext);

                            replacetext = $"{RetailerStatementList.FirstOrDefault().ServicePartner} ";
                            htmldata = htmldata.Replace("[ServicePartner]", replacetext);

                            replacetext = $"{RetailerStatementList.FirstOrDefault().CustomerName} ";
                            htmldata = htmldata.Replace("[CustomerName]", replacetext);

                            replacetext = $"{""} ";
                            htmldata = htmldata.Replace("[CustomerAddress]", replacetext);

                            replacetext = $"{indianTime} ";
                            htmldata = htmldata.Replace("[DATE]", replacetext);

                            replacetext = $"{RetailerStatementList.FirstOrDefault().SanctionedLimit} ";
                            htmldata = htmldata.Replace("[SanctionedLimit]", replacetext);

                            replacetext = $"{RetailerStatementList.FirstOrDefault().AvailableLimit} ";
                            htmldata = htmldata.Replace("[AvailableLimit]", replacetext);

                            replacetext = $"{""} ";
                            htmldata = htmldata.Replace("[TransactionStatement]", replacetext);

                            List<DataTable> dt = new List<DataTable>();
                            DataTable payOutTable = new DataTable();
                            payOutTable.TableName = "Retailer Statement Format";
                            dt.Add(payOutTable);
                            payOutTable.Columns.Add("Date");
                            payOutTable.Columns.Add("Invoice Number");
                            payOutTable.Columns.Add("Invoice Amount");
                            payOutTable.Columns.Add("Interest Amount");
                            payOutTable.Columns.Add("OD Interest");
                            payOutTable.Columns.Add("Penal Charges");
                            payOutTable.Columns.Add("Bounce Charges");
                            payOutTable.Columns.Add("TotalAmount");
                            payOutTable.Columns.Add("Due Date");
                            payOutTable.Columns.Add("Status");
                            payOutTable.Columns.Add("Paid Amount");
                            payOutTable.Columns.Add("Payment Date(s)");
                            payOutTable.Columns.Add("Balance Outstanding");
                            foreach (var item in RetailerStatementList)
                            {
                                var dr = payOutTable.NewRow();
                                double? sumamount = item.PaymentAmount + item.OverdueInterestAmount + item.PenaltyChargesAmount + item.BouncePaymentAmount + item.PrincipleAmount;
                                double? paidamount = item.PaymentAmount + item.ExtraPaymentAmount + item.OverduePaymentAmount + item.BouncePaymentAmount + item.PenalPaymentAmount + item.InterestPaymentAmount;
                                dr["Date"] = item.OrderDate.HasValue ? item.OrderDate.Value.ToString("dd/MM/yyyy") : "";
                                dr["Invoice Number"] = item.InvoiceNo;
                                dr["Invoice Amount"] = item.PrincipleAmount;
                                dr["Interest Amount"] = item.InterestAmount;
                                dr["OD Interest"] = item.OverdueInterestAmount;
                                dr["Penal Charges"] = item.PenaltyChargesAmount;
                                dr["Bounce Charges"] = item.BouncePaymentAmount;
                                dr["TotalAmount"] = Math.Round(sumamount.Value, 2);
                                dr["Due Date"] = item.DueDate.HasValue ? item.DueDate.Value.ToString("dd/MM/yyyy") : "";
                                dr["Status"] = item.TransStatus;
                                dr["Paid Amount"] = Math.Round(paidamount.Value, 2);
                                dr["Payment Date(s)"] = item.PaymentDate.HasValue ? item.PaymentDate.Value.ToString("dd/MM/yyyy") : ""; ;
                                dr["Balance Outstanding"] = item.Outstanding;

                                payOutTable.Rows.Add(dr);

                            }
                            var dr1 = payOutTable.NewRow();
                            dr1["Date"] = "Total";
                            dr1["Invoice Number"] = payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["Invoice Number"].ToString())).ToList().Count();
                            dr1["Invoice Amount"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["Invoice Amount"].ToString())).Sum(x => Convert.ToDouble(x["Invoice Amount"])), 2);
                            dr1["Interest Amount"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["Interest Amount"].ToString())).Sum(x => Convert.ToDouble(x["Interest Amount"])), 2);
                            dr1["OD Interest"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["OD Interest"].ToString())).Sum(x => Convert.ToDouble(x["OD Interest"])), 2);
                            dr1["Penal Charges"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["Penal Charges"].ToString())).Sum(x => Convert.ToDouble(x["Penal Charges"])), 2);
                            dr1["Bounce Charges"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["Bounce Charges"].ToString())).Sum(x => Convert.ToDouble(x["Bounce Charges"])), 2);
                            dr1["TotalAmount"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["TotalAmount"].ToString())).Sum(x => Convert.ToDouble(x["TotalAmount"])), 2);
                            dr1["Due Date"] = "";
                            dr1["Status"] = "";
                            dr1["Paid Amount"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["Paid Amount"].ToString())).Sum(x => Convert.ToDouble(x["Paid Amount"])), 2);
                            dr1["Payment Date(s)"] = "";
                            dr1["Balance Outstanding"] = Math.Round(payOutTable.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["Balance Outstanding"].ToString())).Sum(x => Convert.ToDouble(x["Balance Outstanding"])), 2);
                            payOutTable.Rows.Add(dr1);

                            var htmltable = OfferEMIDataTableToHTML1(payOutTable);

                            replacetext = $" {htmltable} ";
                            htmldata = htmldata.Replace("[DYTable]", replacetext);

                            lists.Status = true;
                            lists.Response.HtmlContent = htmldata;
                            lists.Response.Message = "htmlData Generated";
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return lists;
        }

        public static string OfferEMIDataTableToHTML1(DataTable dt)
        {
            string html = "<table style=\"width:100%; border-collapse: collapse; border: 1px solid black;\">";
            // Add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th style=\"border: 1px solid black;\">" + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            // Add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td style=\"text-align: center; border: 1px solid black;page-break-before: always;padding: 10px 10px;\">" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        public async Task<GRPCReply<bool>> InsertBLEmiTransactionsJob(GRPCReply<List<BLNBFCConfigs>> request)
        {
            GRPCReply<bool> gRPCReply = new GRPCReply<bool> { Response = true, Status = true };
            DateConvertHelper _DateConvertHelper = new DateConvertHelper();
            var currentDateTime = _DateConvertHelper.GetIndianStandardTime();
            var currentMonth = currentDateTime.Month;
            var currentYear = currentDateTime.Year;

            var loanAccounts = await _context.LoanAccounts.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
            var accountTransactions = await _context.AccountTransactions.Where(x => x.IsActive && !x.IsDeleted).Include(x => x.TransactionType).ToListAsync();
            var accountTransactionDetails = await _context.AccountTransactionDetails.Where(x => x.IsActive && !x.IsDeleted).Include(x => x.TransactionDetailHead).ToListAsync();
            var transactionType = await _context.TransactionTypes.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.Code == TransactionTypesConstants.EMIAmount);
            var transactionStatus = await _context.TransactionStatuses.FirstOrDefaultAsync(x => x.IsActive && !x.IsDeleted && x.Code == TransactionStatuseConstants.Due);
            var detailHeads = await _context.TransactionDetailHeads.Where(x => x.IsActive && !x.IsDeleted && (x.Code == TransactionDetailHeadsConstants.EMIPrincipalAmount || x.Code == TransactionDetailHeadsConstants.EMIInterestAmount)).ToListAsync();

            var businessLoanDisbursementDetails = await _context.BusinessLoanDisbursementDetail.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
            var repaymentSchedules = await _context.repaymentSchedules.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
            var invoices = await _context.Invoices.Where(x => x.IsActive && !x.IsDeleted && x.InvoiceDate.HasValue && x.InvoiceDate.Value.Month == currentMonth && x.InvoiceDate.Value.Year == currentYear).ToListAsync();

            var loanDetails = (from l in loanAccounts
                               join bl in businessLoanDisbursementDetails on l.Id equals bl.LoanAccountId
                               join r in repaymentSchedules on l.Id equals r.LoanAccountId
                               join config in request.Response on l.NBFCCompanyId equals config.CompanyId
                               where l.ProductId == config.ProductId && l.Type == LoanAccountUserTypeConstants.Customer
                               && r.DueDate.HasValue && r.DueDate.Value.Month == currentMonth && r.DueDate.Value.Year == currentYear
                               select new
                               {
                                   LoanAccountId = l.Id,
                                   l.AnchorCompanyId,
                                   l.NBFCCompanyId,
                                   l.ProductId,
                                   bl.InterestRate,
                                   bl.ProcessingFeeRate,
                                   bl.PFType,
                                   bl.GST,
                                   bl.Disbursementdate,
                                   r.EMIAmount,
                                   r.Principal,
                                   r.InterestAmount,
                                   r.DueDate,
                                   config.BounceCharge,
                                   config.MinPenalPercent
                               }).ToList();

            if (loanDetails != null && loanDetails.Any() && transactionType != null && transactionStatus != null && detailHeads != null && detailHeads.Any())
            {
                foreach (var loanDetail in loanDetails)
                {
                    var existingInvoice = invoices.Any(x => x.LoanAccountId == loanDetail.LoanAccountId);
                    if (!existingInvoice)
                    {
                        //AccountTransactionDetail
                        var detailList = new List<AccountTransactionDetail>();
                        //EMIInterestAmount
                        detailList.Add(new AccountTransactionDetail
                        {
                            Amount = loanDetail.InterestAmount,
                            TransactionDetailHeadId = detailHeads.First(x => x.Code == TransactionDetailHeadsConstants.EMIInterestAmount).Id,
                            IsPayableBy = true,
                            TransactionDate = loanDetail.DueDate,
                            Status = "success",
                            IsActive = true,
                            IsDeleted = false,
                            Created = currentDateTime,
                            CreatedBy = "System"
                        });
                        //EMIPrincipalAmount
                        detailList.Add(new AccountTransactionDetail
                        {
                            Amount = loanDetail.Principal,
                            TransactionDetailHeadId = detailHeads.First(x => x.Code == TransactionDetailHeadsConstants.EMIPrincipalAmount).Id,
                            IsPayableBy = true,
                            TransactionDate = loanDetail.DueDate,
                            Status = "success",
                            IsActive = true,
                            IsDeleted = false,
                            Created = currentDateTime,
                            CreatedBy = "System"
                        });

                        //AccountTransaction
                        var entityname = new SqlParameter("EntityName", "Transaction");
                        var ReferenceNo = _context.Database.SqlQueryRaw<string>("exec GenerateReferenceNoForTrans @EntityName", entityname).AsEnumerable().FirstOrDefault();
                        var accountTransactionList = new List<AccountTransaction>();
                        accountTransactionList.Add(new AccountTransaction
                        {
                            CustomerUniqueCode = "",
                            LoanAccountId = loanDetail.LoanAccountId,
                            DueDate = loanDetail.DueDate,
                            AnchorCompanyId = loanDetail.AnchorCompanyId,
                            ReferenceId = ReferenceNo,
                            TransactionTypeId = transactionType.Id,
                            TransactionStatusId = transactionStatus.Id,
                            CompanyProductId = loanDetail.ProductId,
                            GSTAmount = 0,
                            OrderAmount = loanDetail.EMIAmount,
                            TransactionAmount = loanDetail.EMIAmount,
                            PaidAmount = loanDetail.EMIAmount,
                            BounceCharge = loanDetail.BounceCharge,
                            InterestRate = loanDetail.InterestRate ?? 0,
                            InterestType = ValueTypeConstants.Percentage,
                            DelayPenaltyRate = loanDetail.MinPenalPercent,
                            GstRate = loanDetail.GST ?? 18,
                            ProcessingFeeRate = loanDetail.ProcessingFeeRate ?? 0,
                            ProcessingFeeType = loanDetail.PFType ?? "",
                            PayableBy = "Customer",
                            DisbursementDate = loanDetail.Disbursementdate,
                            IsActive = true,
                            IsDeleted = false,
                            InvoiceDate = currentDateTime,
                            Created = currentDateTime,
                            CreatedBy = "System",
                            AccountTransactionDetails = detailList
                        });

                        //Invoice
                        _context.Invoices.Add(new Invoice
                        {
                            LoanAccountId = loanDetail.LoanAccountId,
                            Status = AccountInvoiceStatus.Due.ToString(),
                            Comment = AccountInvoiceStatus.Initiate.ToString(),
                            OrderNo = "",
                            InvoiceNo = "",
                            InvoicePdfUrl = "",
                            InvoiceDate = loanDetail.DueDate,
                            OrderAmount = loanDetail.EMIAmount,
                            InvoiceAmount = loanDetail.EMIAmount,
                            TotalTransAmount = loanDetail.EMIAmount,
                            IsActive = true,
                            IsDeleted = false,
                            Created = currentDateTime,
                            CreatedBy = "System",
                            AccountTransactions = accountTransactionList
                        });
                    }
                }
                _context.SaveChanges();
            }
            return gRPCReply;
        }

        public async Task<GRPCReply<bool>> UploadBLInvoiceExcel(UploadBLInvoiceExcelReq request)
        {
            GRPCReply<bool> reply = new GRPCReply<bool>();
            List<BLInvoiceExcelData> dataList = new List<BLInvoiceExcelData>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(request.FileUrl);
                    response.EnsureSuccessStatusCode();

                    // Step 2: Read the content as a byte array
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                    // Step 3: Use ClosedXML to read the Excel file from the byte array
                    using (MemoryStream stream = new MemoryStream(fileBytes))
                    {
                        using (XLWorkbook workbook = new XLWorkbook(stream))
                        {
                            var worksheet = workbook.Worksheet(1); // Access the first worksheet
                            var headers = worksheet.Row(1).Cells(); // Get header row

                            // Find column indices by header name
                            int loanIdIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "loanid")?.Address.ColumnNumber ?? -1;
                            //int partnerIdIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "partnerid")?.Address.ColumnNumber ?? -1;
                            //int productIdIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "product_id")?.Address.ColumnNumber ?? -1;
                            int emiMonthIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "emi_month")?.Address.ColumnNumber ?? -1;
                            int repaymentAmountIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "repaymentamount")?.Address.ColumnNumber ?? -1;
                            int principalPaidIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "principal_paid")?.Address.ColumnNumber ?? -1;
                            int interestPaidIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "interest_paid")?.Address.ColumnNumber ?? -1;
                            int bouncePaidIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "bounce_paid")?.Address.ColumnNumber ?? -1;
                            int penalPaidIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "penal_paid")?.Address.ColumnNumber ?? -1;
                            int overduePaidIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "overdue_paid")?.Address.ColumnNumber ?? -1;
                            int lpiPaidIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "lpi_paid")?.Address.ColumnNumber ?? -1;
                            int loanAmountIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "loan_amount")?.Address.ColumnNumber ?? -1;
                            int paymentDateIndex = headers.FirstOrDefault(c => c.Value.ToString().ToLower() == "payment_date")?.Address.ColumnNumber ?? -1;

                            // Ensure the required columns exist
                            if (loanIdIndex == -1 || repaymentAmountIndex == -1 || principalPaidIndex == -1 || interestPaidIndex == -1 || bouncePaidIndex == -1 || penalPaidIndex == -1 || overduePaidIndex == -1 || loanAmountIndex == -1)
                            {
                                reply.Message = "Required Columns Not Found!!!";
                                return reply;
                            }

                            // Loop through data rows, skipping the header
                            foreach (var row in worksheet.RowsUsed().Skip(1))
                            {
                                var loanId = row.Cell(loanIdIndex).GetString();
                                if (!string.IsNullOrEmpty(loanId))
                                {
                                    //int? PartnerId = TryParseInt(row.Cell(partnerIdIndex).GetString());
                                    //if (PartnerId == null)
                                    //{
                                    //    reply.Message = "Partner Id is invalid!!!";
                                    //    return reply;
                                    //}
                                    //int? ProductId = TryParseInt(row.Cell(productIdIndex).GetString());
                                    //if (ProductId == null)
                                    //{
                                    //    reply.Message = "Product Id is invalid!!!";
                                    //    return reply;
                                    //}
                                    DateTime? EmiMonth = TryParseDate(row.Cell(emiMonthIndex).GetString());
                                    if (EmiMonth == null)
                                    {
                                        reply.Message = "Emi Month is invalid!!!";
                                        return reply;
                                    }
                                    double? RepaymentAmount = TryParseDouble(row.Cell(repaymentAmountIndex).GetString());
                                    if (RepaymentAmount == null)
                                    {
                                        reply.Message = "Repayment Amount is invalid!!!";
                                        return reply;
                                    }
                                    double? PrincipalPaid = TryParseDouble(row.Cell(principalPaidIndex).GetString());
                                    if (PrincipalPaid == null)
                                    {
                                        reply.Message = "Principal Paid is invalid!!!";
                                        return reply;
                                    }
                                    double? InterestPaid = TryParseDouble(row.Cell(interestPaidIndex).GetString());
                                    if (InterestPaid == null)
                                    {
                                        reply.Message = "Interest Paid is invalid!!!";
                                        return reply;
                                    }
                                    double? BouncePaid = TryParseDouble(row.Cell(bouncePaidIndex).GetString());
                                    if (BouncePaid == null)
                                    {
                                        reply.Message = "Bounce Paid is invalid!!!";
                                        return reply;
                                    }
                                    double? PenalPaid = TryParseDouble(row.Cell(penalPaidIndex).GetString());
                                    if (PenalPaid == null)
                                    {
                                        reply.Message = "Penal Paid is invalid!!!";
                                        return reply;
                                    }
                                    double? OverduePaid = TryParseDouble(row.Cell(overduePaidIndex).GetString());
                                    if (OverduePaid == null)
                                    {
                                        reply.Message = "Overdue Paid is invalid!!!";
                                        return reply;
                                    }
                                    double? LpiPaid = TryParseDouble(row.Cell(lpiPaidIndex).GetString());
                                    if (LpiPaid == null)
                                    {
                                        reply.Message = "Lpi Paid is invalid!!!";
                                        return reply;
                                    }
                                    double? LoanAmount = TryParseDouble(row.Cell(loanAmountIndex).GetString());
                                    if (LoanAmount == null)
                                    {
                                        reply.Message = "Loan Amount is invalid!!!";
                                        return reply;
                                    }
                                    DateTime? PaymentDate = TryParseDate(row.Cell(paymentDateIndex).GetString());
                                    //if (TxnDate == null)
                                    //{
                                    //    reply.Message = "Txn date is invalid!!!";
                                    //    return reply;
                                    //}
                                    var data = new BLInvoiceExcelData
                                    {
                                        LoanAccountId = 0,
                                        LoanId = loanId,
                                        EmiMonth = EmiMonth.Value,
                                        RepaymentAmount = RepaymentAmount.Value,
                                        PrincipalPaid = PrincipalPaid.Value,
                                        InterestPaid = InterestPaid.Value,
                                        BouncePaid = BouncePaid.Value,
                                        PenalPaid = PenalPaid.Value,
                                        OverduePaid = OverduePaid.Value,
                                        LpiPaid = LpiPaid.Value,
                                        LoanAmount = LoanAmount.Value,
                                        PaymentDate = PaymentDate,
                                        PaymentReqNo = "",
                                        IsProcess = false,
                                        FileUrl = request.FileUrl
                                    };
                                    dataList.Add(data);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                reply.Message = ex.Message;
                return reply;
            }
            if (dataList != null && dataList.Any())
            {
                var businessLoanDisbursementDetails = await _context.BusinessLoanDisbursementDetail.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
                var blPaymentUploads = (from data in dataList
                                        join bl in businessLoanDisbursementDetails on data.LoanId equals bl.LoanId
                                        select new BLPaymentUpload
                                        {
                                            LoanAccountId = bl.LoanAccountId,
                                            LoanId = data.LoanId,
                                            EmiMonth = data.EmiMonth,
                                            RepaymentAmount = data.RepaymentAmount,
                                            PrincipalPaid = data.PrincipalPaid,
                                            InterestPaid = data.InterestPaid,
                                            BouncePaid = data.BouncePaid,
                                            PenalPaid = data.PenalPaid,
                                            OverduePaid = data.OverduePaid,
                                            LpiPaid = data.LpiPaid,
                                            LoanAmount = data.LoanAmount,
                                            PaymentDate = data.PaymentDate,
                                            PaymentReqNo = data.PaymentReqNo,
                                            IsProcess = data.IsProcess,
                                            FileUrl = data.FileUrl,
                                            Status = "Pending",
                                            IsActive = true,
                                            IsDeleted = false
                                        }).ToList();
                if (blPaymentUploads != null && blPaymentUploads.Any())
                {
                    DateConvertHelper dateConvertHelper = new DateConvertHelper();
                    DateTime currentDate = dateConvertHelper.GetIndianStandardTime();

                    await _context.BLPaymentUploads.AddRangeAsync(blPaymentUploads);
                    await _context.SaveChangesAsync();

                    var accountTransactions = await _context.AccountTransactions.Where(x => x.IsActive && !x.IsDeleted).Include(x => x.TransactionType).ToListAsync();
                    var accountTransactionDetails = await _context.AccountTransactionDetails.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
                    var transactionTypes = await _context.TransactionTypes.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
                    var transactionStatuses = await _context.TransactionStatuses.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
                    foreach (var item in blPaymentUploads)
                    {
                        var loanDetail = businessLoanDisbursementDetails.FirstOrDefault(x => x.LoanAccountId == item.LoanAccountId);
                        var emiAmountTransaction = accountTransactions.Where(x => x.LoanAccountId == item.LoanAccountId && x.TransactionType.Code == TransactionTypesConstants.EMIAmount && x.DueDate.HasValue && x.DueDate.Value.Month == item.EmiMonth.Month && x.DueDate.Value.Year == item.EmiMonth.Year).FirstOrDefault();
                        //.OrderBy(x => x.DueDate).FirstOrDefault();
                        // && x.DueDate.HasValue && item.TxnDate.HasValue && x.DueDate.Value.Month == item.TxnDate.Value.Month && x.DueDate.Value.Year == item.TxnDate.Value.Year
                        if (emiAmountTransaction != null && loanDetail != null)
                        {
                            DateTime PaymentDate = item.PaymentDate ?? (loanDetail.FirstEMIDate.Year > 2000 ? loanDetail.FirstEMIDate : emiAmountTransaction.DueDate.Value);
                            var emiPaymentRequest = new EmiPaymentRequestDc
                            {
                                ParentAccountTransactionsID = emiAmountTransaction.Id,
                                PrincipalAmount = item.PrincipalPaid,
                                InterestAmount = item.InterestPaid,
                                PenalAmount = item.PenalPaid,
                                BounceAmount = item.BouncePaid,
                                OverdueAmount = item.OverduePaid,
                                PaymentDate = PaymentDate,
                                PaymentReqNo = item.PaymentReqNo ?? "",
                                TransactionTypeId = transactionTypes.First(x => x.Code == TransactionTypesConstants.EMIPayment).Id,
                                TransactionStatusId = transactionStatuses.First(x => x.Code == TransactionStatuseConstants.Paid).Id
                            };
                            var res = await InsertEmiPaymentTransaction(accountTransactions, accountTransactionDetails, emiPaymentRequest);

                            emiAmountTransaction.SettlementDate = currentDate;
                            _context.Entry(emiAmountTransaction).State = EntityState.Modified;

                            item.IsProcess = true;
                            _context.Entry(item).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                            //ScaleUp Share
                            if (res)
                            {
                                var emiScaleUpShareRequestDc = new EmiScaleUpShareRequestDc
                                {
                                    ParentAccountTransactionsID = emiAmountTransaction.Id,
                                    FinTechCompanyId = request.FinTechCompanyId,
                                    //PrincipalAmount = item.PrincipalPaid,
                                    InterestAmount = item.InterestPaid,
                                    NBFCInterestRate = loanDetail.NBFCInterest,
                                    CustomerInterestRate = loanDetail.InterestRate ?? 0,
                                    PenalAmount = item.PenalPaid,
                                    NBFCPenalRate = loanDetail.NBFCPenal ?? 0,
                                    CustomerPenalRate = loanDetail.Penal ?? 0,
                                    BounceAmount = item.BouncePaid,
                                    NBFCBounceCharge = loanDetail.NBFCBounce ?? 0,
                                    CustomerBounceCharge = loanDetail.Bounce ?? 0,
                                    OverdueAmount = item.OverduePaid,
                                    NBFCOverdueRate = loanDetail.NBFCInterest,
                                    CustomerOverdueRate = loanDetail.InterestRate ?? 0,
                                    PaymentDate = PaymentDate,
                                    PaymentReqNo = item.PaymentReqNo ?? "",
                                    TransactionTypeId = transactionTypes.First(x => x.Code == TransactionTypesConstants.ScaleupShareEMIAmount).Id,
                                    TransactionStatusId = transactionStatuses.First(x => x.Code == TransactionStatuseConstants.Pending).Id
                                };
                                res = await InsertEmiPaymentScaleUpShare(emiScaleUpShareRequestDc);

                            }
                        }
                    }

                }
            }
            return reply;
        }
        public async Task<bool> InsertEmiPaymentTransaction(List<AccountTransaction> accountTransactions, List<AccountTransactionDetail> accountTransactionDetails, EmiPaymentRequestDc request)
        {
            var transactionHeads = await _context.TransactionDetailHeads.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();

            long AccountTransactionID = 0;
            AccountTransaction? emiAmountTransaction = accountTransactions.FirstOrDefault(x => x.Id == request.ParentAccountTransactionsID && x.TransactionType.Code == TransactionTypesConstants.EMIAmount);
            if (emiAmountTransaction != null)
            {
                AccountTransaction? emiPaymentAccTrans = accountTransactions.FirstOrDefault(x => x.ParentAccountTransactionsID == emiAmountTransaction.Id && x.TransactionType.Code == TransactionTypesConstants.EMIPayment);

                if (emiPaymentAccTrans != null)
                {
                    AccountTransactionID = emiPaymentAccTrans.Id;
                }
                else
                {
                    emiPaymentAccTrans = new AccountTransaction
                    {
                        TransactionTypeId = request.TransactionTypeId,
                        TransactionStatusId = request.TransactionStatusId,
                        TransactionAmount = 0,
                        OrderAmount = 0,
                        PaidAmount = 0,
                        CustomerUniqueCode = emiAmountTransaction.CustomerUniqueCode,
                        LoanAccountId = emiAmountTransaction.LoanAccountId,
                        AnchorCompanyId = emiAmountTransaction.AnchorCompanyId,
                        CompanyProductId = emiAmountTransaction.CompanyProductId,
                        ParentAccountTransactionsID = emiAmountTransaction.Id,
                        ProcessingFeeType = emiAmountTransaction.ProcessingFeeType,
                        ProcessingFeeRate = emiAmountTransaction.ProcessingFeeRate,
                        GstRate = emiAmountTransaction.GstRate,
                        InterestType = emiAmountTransaction.InterestType,
                        InterestRate = emiAmountTransaction.InterestRate,
                        BounceCharge = emiAmountTransaction.BounceCharge,
                        DelayPenaltyRate = emiAmountTransaction.DelayPenaltyRate,
                        CreditDays = emiAmountTransaction.CreditDays,
                        PayableBy = emiAmountTransaction.PayableBy,
                        ReferenceId = emiAmountTransaction.ReferenceId,
                        DueDate = emiAmountTransaction.DueDate,
                        InvoiceDate = emiAmountTransaction.InvoiceDate,
                        InvoiceNo = emiAmountTransaction.InvoiceNo,
                        InvoiceId = emiAmountTransaction.InvoiceId,
                        InvoicePdfURL = emiAmountTransaction.InvoicePdfURL,
                        OrderDate = emiAmountTransaction.OrderDate,
                        DisbursementDate = emiAmountTransaction.DisbursementDate,

                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.AccountTransactions.Add(emiPaymentAccTrans);
                    _context.SaveChanges();
                    AccountTransactionID = emiPaymentAccTrans.Id;
                }

                List<AccountTransactionDetail> accountTransactionDetailList = new List<AccountTransactionDetail>();

                //EMIInterestPayment
                if (request.InterestAmount > 0)
                {
                    var EMIInterestPaymentHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.EMIInterestPayment).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == EMIInterestPaymentHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        emiPaymentAccTrans.TransactionAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.OrderAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.PaidAmount += existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = (-1) * request.InterestAmount,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = EMIInterestPaymentHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    emiPaymentAccTrans.TransactionAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.OrderAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.PaidAmount -= accountTransactionDtl.Amount;
                }

                //EMIPenalPayment
                if (request.PenalAmount > 0)
                {
                    var EMIPenalPaymentHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.EMIPenalPayment).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == EMIPenalPaymentHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        emiPaymentAccTrans.TransactionAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.OrderAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.PaidAmount += existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = (-1) * request.PenalAmount,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = EMIPenalPaymentHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    emiPaymentAccTrans.TransactionAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.OrderAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.PaidAmount -= accountTransactionDtl.Amount;
                }

                //EMIBouncePayment
                if (request.BounceAmount > 0)
                {
                    var EMIBouncePaymentHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.EMIBouncePayment).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == EMIBouncePaymentHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        emiPaymentAccTrans.TransactionAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.OrderAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.PaidAmount += existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = (-1) * request.BounceAmount,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = EMIBouncePaymentHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    emiPaymentAccTrans.TransactionAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.OrderAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.PaidAmount -= accountTransactionDtl.Amount;
                }

                //EMIOverduePayment
                if (request.OverdueAmount > 0)
                {
                    var EMIOverduePaymentHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.EMIOverduePayment).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == EMIOverduePaymentHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        emiPaymentAccTrans.TransactionAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.OrderAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.PaidAmount += existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = (-1) * request.OverdueAmount,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = EMIOverduePaymentHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    emiPaymentAccTrans.TransactionAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.OrderAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.PaidAmount -= accountTransactionDtl.Amount;
                }

                //EMIPrincipalPayment
                if (request.PrincipalAmount > 0)
                {
                    var EMIPrincipalPaymentHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.EMIPrincipalPayment).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == EMIPrincipalPaymentHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        emiPaymentAccTrans.TransactionAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.OrderAmount += existingTDetails.Amount;
                        emiPaymentAccTrans.PaidAmount += existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = (-1) * request.PrincipalAmount,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = EMIPrincipalPaymentHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    emiPaymentAccTrans.TransactionAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.OrderAmount -= accountTransactionDtl.Amount;
                    emiPaymentAccTrans.PaidAmount -= accountTransactionDtl.Amount;
                }

                _context.Entry(emiPaymentAccTrans).State = EntityState.Modified;
                _context.AccountTransactionDetails.AddRange(accountTransactionDetailList);
                if (_context.SaveChanges() > 0)
                    return true;
                else return false;
            }
            else return false;
        }
        public async Task<bool> InsertEmiPaymentScaleUpShare(EmiScaleUpShareRequestDc request)
        {
            var accountTransactions = await _context.AccountTransactions.Where(x => x.IsActive && !x.IsDeleted).Include(x => x.TransactionType).ToListAsync();
            var accountTransactionDetails = await _context.AccountTransactionDetails.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
            var transactionHeads = await _context.TransactionDetailHeads.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
            //ScaleUp Charge Rates
            var scaleUpInterestRate = request.CustomerInterestRate - request.NBFCInterestRate;
            var scaleUpPenalRate = request.CustomerBounceCharge - request.NBFCBounceCharge;
            var scaleUpBounceCharge = request.CustomerPenalRate - request.NBFCPenalRate;
            var scaleUpOverdueRate = request.CustomerOverdueRate - request.NBFCOverdueRate;
            //ScaleUp Shares
            var scaleUpInterestShare = Math.Round((request.InterestAmount / request.CustomerInterestRate) * scaleUpInterestRate, 2);
            var scaleUpPenalShare = Math.Round((request.PenalAmount / request.CustomerPenalRate) * scaleUpPenalRate, 2);
            var scaleUpBounceShare = Math.Round((request.BounceAmount / request.CustomerBounceCharge) * scaleUpBounceCharge, 2);
            var scaleUpOverdueShare = Math.Round((request.OverdueAmount / request.CustomerOverdueRate) * scaleUpOverdueRate, 2);

            long AccountTransactionID = 0;
            AccountTransaction? emiPaymentAccTrans = accountTransactions.FirstOrDefault(x => x.ParentAccountTransactionsID == request.ParentAccountTransactionsID && x.TransactionType.Code == TransactionTypesConstants.EMIPayment);
            if (emiPaymentAccTrans != null)
            {
                AccountTransaction? scaleupShareEMIAmountAccTrans = accountTransactions.FirstOrDefault(x => x.ParentAccountTransactionsID == request.ParentAccountTransactionsID && x.TransactionType.Code == TransactionTypesConstants.ScaleupShareEMIAmount);

                if (scaleupShareEMIAmountAccTrans != null)
                {
                    AccountTransactionID = scaleupShareEMIAmountAccTrans.Id;
                }
                else
                {
                    scaleupShareEMIAmountAccTrans = new AccountTransaction
                    {
                        TransactionTypeId = request.TransactionTypeId,
                        TransactionStatusId = request.TransactionStatusId,
                        AnchorCompanyId = request.FinTechCompanyId,
                        ParentAccountTransactionsID = request.ParentAccountTransactionsID,
                        InterestType = ValueTypeConstants.Percentage,
                        InterestRate = scaleUpInterestRate,
                        BounceCharge = scaleUpBounceCharge,
                        DelayPenaltyRate = scaleUpPenalRate,
                        TransactionAmount = 0,
                        OrderAmount = 0,
                        PaidAmount = 0,
                        ProcessingFeeType = emiPaymentAccTrans.ProcessingFeeType,
                        ProcessingFeeRate = emiPaymentAccTrans.ProcessingFeeRate,
                        CustomerUniqueCode = emiPaymentAccTrans.CustomerUniqueCode,
                        LoanAccountId = emiPaymentAccTrans.LoanAccountId,
                        CompanyProductId = emiPaymentAccTrans.CompanyProductId,
                        GstRate = emiPaymentAccTrans.GstRate,
                        CreditDays = emiPaymentAccTrans.CreditDays,
                        PayableBy = emiPaymentAccTrans.PayableBy,
                        ReferenceId = emiPaymentAccTrans.ReferenceId,
                        DueDate = emiPaymentAccTrans.DueDate,
                        InvoiceDate = emiPaymentAccTrans.InvoiceDate,
                        InvoiceNo = emiPaymentAccTrans.InvoiceNo,
                        InvoiceId = emiPaymentAccTrans.InvoiceId,
                        InvoicePdfURL = emiPaymentAccTrans.InvoicePdfURL,
                        OrderDate = emiPaymentAccTrans.OrderDate,
                        DisbursementDate = emiPaymentAccTrans.DisbursementDate,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.AccountTransactions.Add(scaleupShareEMIAmountAccTrans);
                    _context.SaveChanges();

                    AccountTransactionID = scaleupShareEMIAmountAccTrans.Id;
                }

                List<AccountTransactionDetail> accountTransactionDetailList = new List<AccountTransactionDetail>();
                DateConvertHelper dateConvertHelper = new DateConvertHelper();
                DateTime currentDate = dateConvertHelper.GetIndianStandardTime();

                //ScaleUpShareEMIInterestAmount Entry
                if (scaleUpInterestShare > 0)
                {
                    var ScaleEMIInterestAmountHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpShareEMIInterestAmount).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == ScaleEMIInterestAmountHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        scaleupShareEMIAmountAccTrans.TransactionAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.OrderAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.PaidAmount -= existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = scaleUpInterestShare,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = ScaleEMIInterestAmountHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    scaleupShareEMIAmountAccTrans.TransactionAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.OrderAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.PaidAmount += accountTransactionDtl.Amount;
                }

                //ScaleUpShareEMIPenalAmount Entry
                if (scaleUpPenalShare > 0)
                {
                    var ScaleEMIPenalAmountHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpShareEMIPenalAmount).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == ScaleEMIPenalAmountHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        scaleupShareEMIAmountAccTrans.TransactionAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.OrderAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.PaidAmount -= existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = scaleUpPenalShare,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = ScaleEMIPenalAmountHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    scaleupShareEMIAmountAccTrans.TransactionAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.OrderAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.PaidAmount += accountTransactionDtl.Amount;
                }

                //ScaleUpShareEMIBounceAmount Entry
                if (scaleUpBounceShare > 0)
                {
                    var ScaleEMIBounceAmountHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpShareEMIBounceAmount).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == ScaleEMIBounceAmountHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        scaleupShareEMIAmountAccTrans.TransactionAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.OrderAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.PaidAmount -= existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = scaleUpBounceShare,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = ScaleEMIBounceAmountHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    scaleupShareEMIAmountAccTrans.TransactionAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.OrderAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.PaidAmount += accountTransactionDtl.Amount;
                }

                //ScaleUpShareEMIOverdueAmount Entry
                if (scaleUpOverdueShare > 0)
                {
                    var ScaleEMIOverdueAmountHeadId = transactionHeads.First(x => x.Code == TransactionDetailHeadsConstants.ScaleUpShareEMIOverdueAmount).Id;
                    //Remove Existing transaction Detail of Same head
                    var existingTDetails = accountTransactionDetails.FirstOrDefault(x => x.AccountTransactionId == AccountTransactionID && x.TransactionDetailHeadId == ScaleEMIOverdueAmountHeadId);
                    if (existingTDetails != null)
                    {
                        existingTDetails.IsActive = false;
                        existingTDetails.IsDeleted = true;
                        _context.Entry(existingTDetails).State = EntityState.Modified;

                        scaleupShareEMIAmountAccTrans.TransactionAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.OrderAmount -= existingTDetails.Amount;
                        scaleupShareEMIAmountAccTrans.PaidAmount -= existingTDetails.Amount;
                    }
                    AccountTransactionDetail accountTransactionDtl = new AccountTransactionDetail
                    {
                        AccountTransactionId = AccountTransactionID,
                        Amount = scaleUpOverdueShare,
                        PaymentDate = request.PaymentDate,
                        TransactionDate = request.PaymentDate,
                        TransactionDetailHeadId = ScaleEMIOverdueAmountHeadId,
                        PaymentReqNo = request.PaymentReqNo,
                        Status = "success",
                        IsActive = true,
                        IsDeleted = false,
                        IsPayableBy = false
                    };
                    accountTransactionDetailList.Add(accountTransactionDtl);
                    scaleupShareEMIAmountAccTrans.TransactionAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.OrderAmount += accountTransactionDtl.Amount;
                    scaleupShareEMIAmountAccTrans.PaidAmount += accountTransactionDtl.Amount;
                }

                _context.Entry(scaleupShareEMIAmountAccTrans).State = EntityState.Modified;
                _context.AccountTransactionDetails.AddRange(accountTransactionDetailList);
                if (_context.SaveChanges() > 0)
                    return true;
                else return false;
            }
            else return false;
        }
        static DateTime? TryParseDate(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
            {
                return result;
            }
            return null; // Return null if parsing fails
        }
        static double? TryParseDouble(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result;
            }
            return null; // Return null if parsing fails
        }
        public async Task<GRPCReply<BLDetailsDc>> GetBLDetails(GRPCRequest<long> req)
        {
            GRPCReply<BLDetailsDc> reply = new GRPCReply<BLDetailsDc>() { Message = "Data Not Found!!!" };
            var loanAccountDetails = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.Id == req.Request && x.IsActive && !x.IsDeleted);
            var blDetail = await _context.BusinessLoanDisbursementDetail.FirstOrDefaultAsync(x => x.LoanAccountId == req.Request && x.IsActive && !x.IsDeleted);
            if (blDetail != null && loanAccountDetails != null)
            {
                double InsuranceAmount = blDetail.InsuranceAmount ?? 0;
                double otherCharges = blDetail.OtherCharges ?? 0;
                BLDetailsDc details = new BLDetailsDc
                {
                    LoanAccountId = blDetail.LoanAccountId
                };
                details.BLInfo = new BLInfoDc
                {
                    emi_amount = blDetail.MonthlyEMI,
                    gst_on_pf_amt = blDetail.ProcessingFeeTax,
                    gst_on_pf_perc = blDetail.GST,
                    loan_int_amt = blDetail.LoanInterestAmount,
                    loan_int_rate = blDetail.InterestRate.ToString(),
                    net_disbur_amt = blDetail.LoanAmount - ((blDetail.ProcessingFeeAmount - blDetail.PFDiscount) + blDetail.ProcessingFeeTax + InsuranceAmount + otherCharges),
                    processing_fees_amt = blDetail.ProcessingFeeAmount,
                    processing_fees_perc = blDetail.ProcessingFeeRate,
                    sanction_amount = blDetail.LoanAmount,
                    tenure = blDetail.Tenure.ToString(),
                    PfDiscount = blDetail.PFDiscount,
                    InsuranceAmount = InsuranceAmount,
                    Othercharges = otherCharges,
                    broken_period_int_amt = blDetail.brokenPeriodinterestAmount
                };
                details.ProfileDetail = new BLProfileDetailDc
                {
                    CityName = loanAccountDetails.CityName ?? "",
                    CustomerName = loanAccountDetails.CustomerName,
                    MobileNo = loanAccountDetails.MobileNo,
                    NBFCIdentificationCode = loanAccountDetails.NBFCIdentificationCode,
                    ProductType = loanAccountDetails.ProductType,
                    ShopName = loanAccountDetails.ShopName ?? "",
                    ThirdPartyLoanCode = loanAccountDetails.ThirdPartyLoanCode ?? "",
                    AccountCode = loanAccountDetails.AccountCode,
                    loan_id = blDetail.LoanId ?? "",
                    first_name = loanAccountDetails.CustomerName,
                };
                var LoanAccountId = new SqlParameter("LoanAccountId", req.Request);
                var LoanAccount = _context.Database.SqlQueryRaw<GetBLAccountDetailDTO>("exec GetBLAccountDetail @LoanAccountId", LoanAccountId).AsEnumerable().FirstOrDefault();
                if (LoanAccount != null)
                {
                    details.Repayment = new BLRepaymentDc
                    {
                        BounceCharges = Math.Round(LoanAccount.BounceRePaymentAmount, 2),
                        InterestAmount = Math.Round(LoanAccount.InterestRepaymentAmount, 2),
                        PenalCharges = Math.Round(LoanAccount.PenalRePaymentAmount, 2),
                        PrincipleAmount = Math.Round(LoanAccount.PrincipalRepaymentAmount, 2),
                        OverdueRePaymentAmount = Math.Round(LoanAccount.OverdueRePaymentAmount, 2),
                        TotalRepayment = Math.Round(LoanAccount.TotalRepayment, 2)
                    };
                    details.Outstanding = new BLOutstandingDc
                    {
                        BounceCharges = Math.Round(LoanAccount.BounceOutStanding, 2),
                        PenalCharges = Math.Round(LoanAccount.PenalOutStanding, 2),
                        InterestOutstanding = Math.Round(LoanAccount.InterestOutstanding, 2),
                        PrincipleOutstanding = Math.Round(LoanAccount.PrincipleOutstanding, 2),
                        OverdueInterestOutStanding = Math.Round(LoanAccount.OverdueInterestOutStanding, 2),
                        TotalOutstanding = Math.Round(LoanAccount.TotalOutStanding, 2)
                    };
                }
                reply.Response = details;
                reply.Status = true;
                reply.Message = "Data Found";
            }
            return reply;
        }

        public async Task<GRPCReply<List<GetBLPaymentUploadsDTO>>> GetBLPaymentUploads(int Skip,int Take)
        {
            GRPCReply<List<GetBLPaymentUploadsDTO>> reply = new GRPCReply<List<GetBLPaymentUploadsDTO>>() { Message = "Data not found!" };
            var totalRecordes = _context.BLPaymentUploads.Where(x => x.IsActive && !x.IsDeleted).Count();
            var blPaymentUploads = await _context.BLPaymentUploads.Where(x => x.IsActive && !x.IsDeleted).Select(x => new GetBLPaymentUploadsDTO
            {
                LoanAccountId = 0,
                BouncePaid = x.BouncePaid,
                Id = x.Id,
                InterestPaid = x.InterestPaid,
                LoanAmount = x.LoanAmount,
                LoanId = x.LoanId,
                OverduePaid = x.OverduePaid,
                LpiPaid = x.LpiPaid,
                PaymentDate = x.PaymentDate,
                PaymentReqNo = x.PaymentReqNo,
                PenalPaid = x.PenalPaid,
                PrincipalPaid = x.PrincipalPaid,
                IsProcess = x.IsProcess,
                RepaymentAmount = x.RepaymentAmount,
                Status = x.Status,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                Deleted = x.Deleted,
                DeletedBy = x.DeletedBy,
                LastModified = x.LastModified,
                LastModifiedBy = x.LastModifiedBy,
                FileUrl = x.FileUrl,
                EmiMonth = x.EmiMonth,
                TotalRecords = totalRecordes
            }).OrderBy(y=>y.Id).Skip(Skip).Take(Take).ToListAsync();

            if (blPaymentUploads != null && blPaymentUploads.Any())
            {
                reply.Status = true;
                reply.Message = "Data found";
                reply.Response = blPaymentUploads;
            }
            return reply;
        }

        public async Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> request)
        {
            GRPCReply<ApplyLoanResponseDC> response = new GRPCReply<ApplyLoanResponseDC> { Message = "Data Not Found!!!" };
            var nbfcService = _loanNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.ApplyLoan(request);
            }

            return response;
        }

        public async Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> request)
        {
            GRPCReply<CheckTotalAndAvailableLimitResponseDc> response = new GRPCReply<CheckTotalAndAvailableLimitResponseDc> { Message = "Data Not Found!!!" };
            var nbfcService = _loanNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.CheckTotalAndAvailableLimit(request);
            }

            return response;
        }
        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> request)
        {
            GRPCReply<DeliveryConfirmationResponseDC> response = new GRPCReply<DeliveryConfirmationResponseDC> { Message = "Data Not Found!!!" };
            var nbfcService = _loanNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.DeliveryConfirmation(request);
            }

            return response;
        }
        public async Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> request)
        {
            GRPCReply<CancellationResponseDC> response = new GRPCReply<CancellationResponseDC> { Message = "Data Not Found!!!" };
            var nbfcService = _loanNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.CancelTransaction(request);
            }

            return response;
        }


        public async Task<GRPCReply<long>> SaveLoanAccountDetails(GRPCRequest<SaveLoanAccountRequestDC> request)
        {
            GRPCReply<long> gRPCReply = new GRPCReply<long>();
            //var LoanAccountTbl = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted);
            var LoanData = await _context.LoanAccounts.FirstOrDefaultAsync(x => x.LeadId == request.Request.LeadId && x.IsActive && !x.IsDeleted);
            string accountcode = await GetCurrentNumber("AccountCode");

        
            if (LoanData != null)
            {
                gRPCReply.Status = false;
                gRPCReply.Message = "Loan already exists for this lead";
                gRPCReply.Response = 0;
            }
            else
            {
                LoanAccount loanAccount = new LoanAccount
                {
                    LeadId = request.Request.LeadId,
                    LeadCode = request.Request.LeadCode,
                    ProductId = request.Request.ProductId,
                    UserId = request.Request.UserId,
                    AccountCode = accountcode,
                    CustomerName = request.Request.CustomerName, //TransactionTypeId.Id,
                    MobileNo = request.Request.MobileNo,
                    NBFCCompanyId = request.Request.NBFCCompanyId,
                    AnchorCompanyId = request.Request.AnchorCompanyId,
                    ApplicationDate = Convert.ToDateTime(request.Request.ApplicationDate),
                    AgreementRenewalDate = Convert.ToDateTime(request.Request.AgreementRenewalDate),
                    IsDefaultNBFC = request.Request.IsDefaultNBFC,
                    CityName = request.Request.CityName,
                    AnchorName = request.Request.AnchorName,
                    ProductType = request.Request.ProductType,
                    IsAccountActive = request.Request.IsAccountActive ?? true,
                    IsBlock = request.Request.IsBlock ?? false,
                    ShopName = request.Request.ShopName,
                    CustomerImage = request.Request.CustomerImage,
                    IsBlockComment = "",
                    IsActive = true,
                    IsDeleted = false,
                    Created = DateTime.Now,
                    DisbursalDate = DateTime.Now,
                    ThirdPartyLoanCode = null,
                    IsBlockHideLimit = false,
                    NBFCIdentificationCode = LeadNBFCConstants.AyeFinanceSCF,
                    Type = LoanAccountUserTypeConstants.Customer
                };
                await _context.LoanAccounts.AddAsync(loanAccount);
                var rowData = await _context.SaveChangesAsync();
                var LoanAccountId = loanAccount.Id;
                if (rowData > 0)
                {
                    double DisbursedLimit = 0;
                    var Limit = await CheckTotalAndAvailableLimit(new GRPCRequest<AyeloanReq> { Request = new AyeloanReq { loanId = LoanAccountId, token = request.Request.token } });
                    if (Limit.Status)
                    {
                        DisbursedLimit = Limit.Response.data.totalLimit;
                    }

                    var LoanAccountCreditTbl = await _context.LoanAccountCredits.FirstOrDefaultAsync(x => x.LoanAccountId == LoanAccountId);

                    if (LoanAccountCreditTbl != null)
                    {
                        gRPCReply.Status = false;
                        gRPCReply.Message = "Loan Account Credit Already Exist";
                        gRPCReply.Response = 0;
                    }
                    else
                    {
                        LoanAccountCredit _loanaccountcredit = new LoanAccountCredit
                        {
                            LoanAccountId = LoanAccountId,
                            DisbursalAmount = DisbursedLimit,
                            IsActive = true,
                            IsDeleted = false,
                            Created = DateTime.Now,
                            CreditLimitAmount = DisbursedLimit,
                        };
                        await _context.LoanAccountCredits.AddAsync(_loanaccountcredit);
                        if (await _context.SaveChangesAsync() > 0)
                        {
                            gRPCReply.Status = true;
                            gRPCReply.Message = "Success";
                            gRPCReply.Response = _loanaccountcredit.Id;
                        }
                        else
                        {
                            gRPCReply.Status = false;
                            gRPCReply.Message = "Failed to save Loan Account Credit";
                            gRPCReply.Response = 0;
                        }
                    }
                }
                else
                {
                    gRPCReply.Status = false;
                    gRPCReply.Message = "Failed to save LoanAccount";
                    gRPCReply.Response = 0;
                }
            }

            return gRPCReply;
        }
        public async Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request)
        {
            GRPCReply<DeliveryConfirmationResponseDC> response = new GRPCReply<DeliveryConfirmationResponseDC> { Message = "Data Not Found!!!" };
            var nbfcService = _loanNBFCFactory.GetService(LeadNBFCConstants.AyeFinanceSCF.ToString());
            if (nbfcService != null)
            {
                response = await nbfcService.Repayment(request);
            }
            return response;
        }

        public async Task<GRPCReply<NBFCLoanAccountDetailResponseDTO>> GetNBFCLoanAccountDetail(GRPCRequest<GetNBFCLoanAccountDetailDTO> req)
        {
            GRPCReply<NBFCLoanAccountDetailResponseDTO> reply = new GRPCReply<NBFCLoanAccountDetailResponseDTO>();
            NBFCLoanAccountDetailResponseDTO loanAccountDetailResponse = new NBFCLoanAccountDetailResponseDTO();
            var LoanAccountId = new SqlParameter("LoanAccountId", req.Request.loanAccountId);
            var LoanAccount = _context.Database.SqlQueryRaw<GetLoanAccountDetailDTO>("exec GetLoanAccountDetail @LoanAccountId", LoanAccountId).AsEnumerable().FirstOrDefault();

            if (LoanAccount != null)
            {
                double CreditLimit = 0;
                var res = await _loanAccountHelper.GetAvailableCreditLimitByLoanId(req.Request.loanAccountId,req.Request.NBFCToken);

                CreditLimit = res.Response.CreditLimit;

                loanAccountDetailResponse.LoanAccountNumber = LoanAccount.AccountCode;
                loanAccountDetailResponse.ShopName = LoanAccount.ShopName;
                loanAccountDetailResponse.UserName = LoanAccount.CustomerName;
                loanAccountDetailResponse.PhoneNumber = LoanAccount.MobileNo;
                loanAccountDetailResponse.UserId = LoanAccount.AccountCode;
                loanAccountDetailResponse.MobileNumber = LoanAccount.MobileNo;
                loanAccountDetailResponse.CityName = LoanAccount.CityName;
                loanAccountDetailResponse.ProductType = LoanAccount.ProductType;
                loanAccountDetailResponse.LoanImage = LoanAccount.LoanImage;
                loanAccountDetailResponse.IsAccountActive = LoanAccount.IsAccountActive;
                loanAccountDetailResponse.IsBlock = LoanAccount.IsBlock;
                loanAccountDetailResponse.IsBlockComment = LoanAccount.IsBlockComment;
                loanAccountDetailResponse.NBFCIdentificationCode = LoanAccount.NBFCIdentificationCode;
                loanAccountDetailResponse.ThirdPartyLoanCode = LoanAccount.ThirdPartyLoanCode;
                loanAccountDetailResponse.CreditLineInfo = new CreditLineInfoDTO
                {
                    TotalSanctionedAmount = Math.Round(LoanAccount.TotalSanctionedAmount, 2),
                    TotalCreditLimit = Math.Round(LoanAccount.TotalSanctionedAmount, 2),// Math.Round(CreditLimit, 2),
                    UtilizedAmount = Math.Round(LoanAccount.UtilizedAmount, 2),
                    LTDUtilizedAmount = Math.Round(LoanAccount.LTDUtilizedAmount, 2),
                    AvailableLimit = Math.Round(CreditLimit, 2),
                    AvailableLimitPercentage = LoanAccount.TotalSanctionedAmount > 0 ? Math.Round((CreditLimit / LoanAccount.TotalSanctionedAmount) * 100, 2) : 0,
                    PenalAmount = Math.Round(LoanAccount.PenalAmount, 2),
                    ProcessingFee = LoanAccount.ProcessingFee
                };
                loanAccountDetailResponse.Repayments = new RepaymentsDTO
                {
                    InterestAmount = Math.Round(LoanAccount.InterestRepaymentAmount, 2),
                    OverdueInterestAmount = Math.Round(LoanAccount.OverdueInterestPaymentAmount, 2),
                    PenalInterestAmount = Math.Round(LoanAccount.PenalRePaymentAmount, 2),
                    PrincipalAmount = Math.Round(LoanAccount.PrincipalRepaymentAmount, 2),
                    TotalPaidAmount = Math.Round(LoanAccount.TotalRepayment, 2),
                    BounceRePaymentAmount = Math.Round(LoanAccount.BounceRePaymentAmount, 2),
                    ExtraPaymentAmount = Math.Round(LoanAccount.ExtraPaymentAmount, 2)
                };
                loanAccountDetailResponse.Outstanding = new OutstandingDTO
                {
                    InterestAmount = Math.Round(LoanAccount.InterestOutstanding, 2),
                    OverdueInterestAmount = Math.Round(LoanAccount.OverdueInterestOutStanding, 2),
                    PenalInterestAmount = Math.Round(LoanAccount.PenalOutStanding, 2),
                    PrincipalAmount = Math.Round(LoanAccount.PrincipleOutstanding, 2),
                    TotalOutstandingAmount = Math.Round(LoanAccount.TotalOutStanding, 2)
                };
                loanAccountDetailResponse.CreditLine = new CreditLineDTO
                {
                    TotalCreditLimit = Math.Round(CreditLimit, 2),
                    Percentage = Math.Round(100 - loanAccountDetailResponse.CreditLineInfo.AvailableLimitPercentage, 2),
                    UtilizedAmount = Math.Round(LoanAccount.UtilizedAmount, 2)
                };
                //loanAccountDetailResponse.Transactions = new List<Transaction>();

                loanAccountDetailResponse.Status = true;
                loanAccountDetailResponse.Message = "Data Found";
                reply.Response = loanAccountDetailResponse;
            }
            else
            {
                loanAccountDetailResponse.Status = false;
                loanAccountDetailResponse.Message = "Data Not Found";
            }
            return reply;

        }
    }
}

