using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Company.DataContracts;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.Services.LeadAPI.Manager;
using ScaleUP.Services.LeadAPI.NBFCFactory.Implementation;
using ScaleUP.Services.LeadAPI.Persistence;
using ScaleUP.Services.LeadDTO.Lead;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadModels.ArthMate;
using ScaleUP.Services.LeadModels.BusinessLoan;

namespace ScaleUP.Services.LeadAPI.NBFCFactory
{
    public class ArthMateNBFCSchedular
    {
        private readonly LeadApplicationDbContext _context;
        private readonly LeadNBFCSubActivityManager _leadNBFCSubActivityManager;
        private readonly LeadNBFCFactory _leadNBFCFactory;
        public ArthMateNBFCSchedular(LeadApplicationDbContext context, LeadNBFCSubActivityManager leadNBFCSubActivityManager, LeadNBFCFactory leadNBFCFactory)
        {
            _context = context;
            _leadNBFCSubActivityManager = leadNBFCSubActivityManager;
            _leadNBFCFactory = leadNBFCFactory;
        }

        public async Task<CommonResponseDc> AadhaarOtpGenerate(long leadid)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.GenerateOtpToAcceptOffer(leadid);
            }
            return res;
        }

        //public async Task<CommonResponseDc> AadhaarOtpVerify(SecondAadharXMLDc AadharObj)
        //{

        //    CommonResponseDc res = new CommonResponseDc();
        //    var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
        //    if (nbfcService != null)
        //    {
        //        res = await nbfcService.AcceptOfferWithXMLAadharOTP(AadharObj);
        //    }
        //    return res;
        //}

        public async Task<CommonResponseDc> GetLeadMasterByLeadId(long leadid)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.GetLeadMasterByLeadId(leadid);
            }
            return res;
        }


        public async Task<LoanInsuranceConfiguration> GetRateOfInterest(int tenure)
        {
            LoanInsuranceConfiguration res = new LoanInsuranceConfiguration();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.GetRateOfInterest(tenure);
            }
            return res;

        }


        public async Task<CommonResponseDc> OfferRegenerate(int leadId, int tenure, double sactionAmount)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.OfferRegenerate(leadId, tenure, sactionAmount);
            }
            return res;
        }


        public async Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.GetLoanByLoanId(Leadmasterid);
            }
            return res;
        }


        public async Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.GetDisbursementAPI(Leadmasterid);
            }
            return res;
        }


        public async Task<List<LeadLoanDataDc>> GetLoan(long LeadMasterId)
        {
            List<LeadLoanDataDc> res = new List<LeadLoanDataDc>();
            var lead = await _context.Leads.FirstOrDefaultAsync(x => x.Id == LeadMasterId && x.IsActive && !x.IsDeleted);

            var leadoffer = _context.LeadOffers.FirstOrDefault(x => x.LeadId == LeadMasterId && x.NBFCCompanyId == lead.OfferCompanyId);

            if (leadoffer != null)
            {
                if (leadoffer.CompanyIdentificationCode == CompanyIdentificationCodeConstants.ArthMate)
                {

                    var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
                    if (nbfcService != null)
                    {
                        res = await nbfcService.GetLoan(LeadMasterId);
                    }
                }
                else 
                {
                    //var Leadmasterid = new SqlParameter("leadmasterid", LeadMasterId);
                    //var CompanyId = new SqlParameter("NBFCCompanyId", lead.OfferCompanyId);
                    //var acceptedLoanDetail = _context.Database.SqlQueryRaw<GetAcceptedLoanDetailDC>("exec GetAcceptedLoanDetail @leadmasterid,@NBFCCompanyId", Leadmasterid, CompanyId).AsEnumerable().FirstOrDefault();

                    var mbfcOffer = await _context.nbfcOfferUpdate.FirstOrDefaultAsync(x => x.LeadId == LeadMasterId && x.NBFCCompanyId == lead.OfferCompanyId && x.IsActive && !x.IsDeleted);

                    if (mbfcOffer != null)
                    {
                        LeadLoanDataDc obj = new LeadLoanDataDc();

                        double? processing_fee = 0;
                        double? ProcessingfeeTax = 0;
                        double LoanAmount = 0;
                        double RateOfInterest = 0;
                        int Tenure = 0;
                        double monthlyPayment = 0;
                        double loanIntAmt = 0;
                        double PFPer = 0;
                        double GST = 0;

                        LoanAmount = mbfcOffer.LoanAmount ?? 0;
                        var PF = mbfcOffer.ProcessingFeeRate;
                        RateOfInterest = lead.InterestRate ?? 0;
                        Tenure = Convert.ToInt32(mbfcOffer.Tenure);
                        monthlyPayment = Math.Round(Convert.ToDouble(CalculateMonthlyPayment(RateOfInterest, Tenure, LoanAmount)), 2);
                        loanIntAmt = Math.Round(((Convert.ToDouble(monthlyPayment) * Tenure) - LoanAmount), 2);
                        PFPer = PF ?? 0;
                        GST = mbfcOffer.GST ?? 0;
                        processing_fee = ((mbfcOffer.ProcessingFeeAmount??0) >0) ? (mbfcOffer.ProcessingFeeAmount??0)- (mbfcOffer.PFDiscount??0):0; //PF != null && PF.ValueType == "Percentage" ? Math.Round((LoanAmount * PFPer) / 100, 2) : PFPer;
                        ProcessingfeeTax = mbfcOffer.ProcessingFeeTax;

                        obj.tenure = Tenure;
                        obj.loan_amount = LoanAmount;
                        obj.LoanId = mbfcOffer.LoanId;
                        obj.SanctionAmount = LoanAmount;
                        obj.loan_int_amt = loanIntAmt;
                        obj.loan_int_rate = RateOfInterest;
                        obj.emi_amount = monthlyPayment;
                        obj.LastModified = lead.LastModified;
                        obj.LoanStatus = mbfcOffer.OfferStatus;// mbfcOffer.OfferStatus == 1 ? "Accepted" : "NA";
                        obj.CompanyIdentificationCode = mbfcOffer.CompanyIdentificationCode;
                        obj.processingfee = processing_fee ?? 0;
                        obj.NbfcCompanyId = lead.OfferCompanyId;
                        res.Add(obj);
                    }
                }
            }
            return res;
        }
        public static decimal CalculateMonthlyPayment(double yearlyinterestrate, int totalnumberofmonths, double loanamount)
        {
            if (yearlyinterestrate > 0)
            {
                var rate = (double)yearlyinterestrate / 100 / 12;
                var denominator = Math.Pow((1 + rate), totalnumberofmonths) - 1;
                return new decimal((rate + (rate / denominator)) * loanamount);
            }
            return totalnumberofmonths > 0 ? new decimal(loanamount / totalnumberofmonths) : 0;
        }

        public async Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.LoanRepaymentScheduleDetails(LeadMasterId);
            }
            return res;

        }


        public async Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.LoanNach(UMRN, Leadmasterid);
            }
            return res;
        }


        public async Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.ChangeLoanStatus(LeadMasterId, Status);
            }
            return res;
        }


        public async Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            RePaymentScheduleDataDc res = new RePaymentScheduleDataDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.GetDisbursedLoanDetail(Leadmasterid);
            }
            return res;

        }

        public async Task<CommonResponseDc> UploadSignedSla(long leadmasterid, string eSignDocumentURL)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.SaveAgreementESignDocument(leadmasterid, eSignDocumentURL);
            }
            return res;

        }



        public async Task<CommonResponseDc> LoanDataSave(loandc obj)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.LoanDataSave(obj.leadid, obj.id);
            }

            return res;
        }
        public async Task<CommonResponseDc> UpdateBeneficiaryBankDetail(BeneficiaryBankDetailDc Obj)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.UpdateBeneficiaryBankDetail(Obj);
            }
            return res;

        }

        public async Task<ResultViewModel<List<OfferEmiDetailDC>>> GetOfferEmiDetails(long leadId, int ReqTenure = 0)
        {
            ResultViewModel<List<OfferEmiDetailDC>> res = new ResultViewModel<List<OfferEmiDetailDC>>();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.GetOfferEmiDetails(leadId, ReqTenure);
            }
            return res;

        }
        //public async Task<ResultViewModel<string>> GetOfferEmiDetailsDownloadPdf(long leadId)
        //{
        //    ResultViewModel<string> res = new ResultViewModel<string>();
        //    var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
        //    if (nbfcService != null)
        //    {
        //        res = await nbfcService.GetOfferEmiDetailsDownloadPdf(leadId);
        //    }
        //    return res;

        //}

        public async Task<CommonResponseDc> InsertCeplrBanklist()
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.InsertCeplrBanklist();
            }
            return res;
        }
        public async Task<CommonResponseDc> InsertLoanDataByArthmateTest(long leadid, string loanid)
        {
            CommonResponseDc res = new CommonResponseDc();
            var nbfcService = _leadNBFCFactory.GetService(CompanyIdentificationCodeConstants.ArthMate);
            if (nbfcService != null)
            {
                res = await nbfcService.InsertLoanDataByArthmateTest(leadid, loanid);
            }

            return res;
        }
    }
}
