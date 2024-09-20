using Google.Protobuf.WellKnownTypes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Global.Infrastructure.Constants.Lead;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Constant;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;
using ScaleUP.Services.LeadModels;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadModels.LeadNBFC;
using System.Data;


namespace ScaleUP.Services.LeadAPI.NBFCFactory.Implementation
{
    public class DefaultLeadNBFCService : ILeadNBFCService
    {
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;

        private readonly LeadApplicationDbContext _context;
        public DefaultLeadNBFCService(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
        }

        public Task<ICreateLeadNBFCResponse> AadhaarUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> AcceptOfferWithXMLAadharOTP(GRPCRequest<SecondAadharXMLDc> AadharObj)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BlackSoilCommonApplicationDetail(long leadid)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BusinessAddressUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> BusinessUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status)
        {
            throw new NotImplementedException();
        }

        

        public async Task<ICreateLeadNBFCResponse> GenerateOffer(long leadId, long nbfcid)
        {
            var result = new CreateLeadNBFCResponse();
            var Leadmasters = _context.Leads.Where(x => x.Id == leadId).FirstOrDefault();
            var leadoffer = await _context.LeadOffers.Where(x => x.IsActive && x.LeadId == leadId && x.NBFCCompanyId == nbfcid && x.Status == LeadOfferConstant.Initiated && !x.IsDeleted).FirstOrDefaultAsync();

            var subactvityList = _leadNBFCSubActivityManager.GetSubactivityData(leadId, nbfcid, ActivityConstants.GenerateOffer);
            LeadNBFCSubActivity subactvity = null;
            if (leadoffer != null && subactvityList != null && subactvityList.Any())
            {
                subactvity = subactvityList.First();

                _leadNBFCSubActivityManager.UpdateSubActivityStatus(subactvity.Id, LeadNBFCSubActivityConstants.Inprogress);

                var leadCompleteDetail = await _context.CompanyLead.Where(x => x.LeadId == leadId && x.CompanyId == nbfcid && x.IsActive && x.LeadProcessStatus == 2).FirstOrDefaultAsync();

                var NBFCCompanyIdlist = new DataTable();
                NBFCCompanyIdlist.Columns.Add("companyId");
                NBFCCompanyIdlist.Columns.Add("leadId");
                NBFCCompanyIdlist.Columns.Add("VintageDays");
                NBFCCompanyIdlist.Columns.Add("AvgMonthlyBuying");
                NBFCCompanyIdlist.Columns.Add("CibilScore");
                NBFCCompanyIdlist.Columns.Add("CustomerType");
                var dr = NBFCCompanyIdlist.NewRow();
                dr["companyId"] = nbfcid;
                dr["leadId"] = leadId;
                dr["VintageDays"] = leadCompleteDetail != null ? leadCompleteDetail.VintageDays : 0;
                dr["AvgMonthlyBuying"] = leadCompleteDetail != null ? leadCompleteDetail.MonthlyAvgBuying : 0;
                dr["CibilScore"] = Leadmasters != null ? Leadmasters.CreditScore : 0;
                dr["CustomerType"] = "Retailer";
                NBFCCompanyIdlist.Rows.Add(dr);

                var nbfccompany = new SqlParameter("NBFCCompanyOffers", NBFCCompanyIdlist);
                nbfccompany.SqlDbType = SqlDbType.Structured;
                nbfccompany.TypeName = "dbo.NBFCCompanyOffer";
                var Offer = _context.Database.SqlQueryRaw<NBFCSelfOfferReply>("exec GetQualifiedOffer @NBFCCompanyOffers", nbfccompany).AsEnumerable().FirstOrDefault();

                if (Offer != null && Offer.OfferAmount > 0)
                {
                    leadoffer.Status = LeadOfferConstant.OfferGenerated;
                    leadoffer.CreditLimit = Offer.OfferAmount;
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(subactvity.Id, LeadNBFCSubActivityConstants.Completed);
                }
                else
                {
                    leadoffer.Status = LeadOfferConstant.OfferError;
                    _leadNBFCSubActivityManager.UpdateSubActivityStatus(subactvity.Id, LeadNBFCSubActivityConstants.CompletedWithError);
                }
                _context.Entry(leadoffer).State = EntityState.Modified;
                _context.SaveChanges();



            }
            return result;
        }

        public Task<CommonResponseDc> GenerateOtpToAcceptOffer(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetLeadMasterByLeadId(long leadId)
        {
            throw new NotImplementedException();
        }

        public Task<List<LeadLoanDataDc>> GetLoan(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<List<OfferEmiDetailDC>>> GetOfferEmiDetails(long leadId, int tenure = 0)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<string>> GetOfferEmiDetailsDownloadPdf(long leadId)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> leadId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultViewModel<string>> GetPFCollection(long leadid, string MobileNo)
        {
            throw new NotImplementedException();
        }

        public Task<LoanInsuranceConfiguration> GetRateOfInterest(int tenure)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> InsertCeplrBanklist()
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> InsertLoanDataByArthmateTest(long leadid, string loanid)
        {
            throw new NotImplementedException();
        }

        public Task<LBABusinessLoanDc> LBABusinessLoan(long leadid,string AgreementURL, bool IsSubmit, ProductCompanyConfigDc loanconfig)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanDataSave(long leadid, long id)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> OfferRegenerate(int LeadId, int tenure, double sactionAmount)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PanUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PersonAddressUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public Task<ICreateLeadNBFCResponse> PersonUpdate(long leadid, long nbfcCompanyId)
        {
            throw new NotImplementedException();
        }

        public async Task<ICreateLeadNBFCResponse> PrpareAgreement(long leadid, long nbfcCompanyId)
        {
            return new CreateLeadNBFCResponse
            {
                IsSuccess = true,
                Message = "Success"
            };
        }

        public Task<CommonResponseDc> SaveAgreementESignDocument(long leadmasterid, string eSignDocumentURL)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> UpdateBeneficiaryBankDetail(BeneficiaryBankDetailDc Obj)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> eSignSessionAsync(string agreementPdfUrl, long leadid)
        {
            throw new NotImplementedException();
        }

        public Task<CommonResponseDc> eSignDocumentsAsync(long leadid, string DocumentId)
        {
            throw new NotImplementedException();
        }

        public Task<string> AgreementEsign(GRPCRequest<EsignAgreementRequest> req)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<string>> GenerateToken()
        {
            throw new NotImplementedException();
        }

        public async Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
        public async Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> ayeleadReq)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> ayeleadReq)
        {
            throw new NotImplementedException();
        }
      
    }


}
