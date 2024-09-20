using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.BuildingBlocks.GRPC.Contracts.KYC.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.Interfaces.NBFC;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Product.DataContracts;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Request;
using ScaleUP.Services.LeadDTO.NBFC.ArthMate.Response;
using ScaleUP.Services.LeadDTO.NBFC.AyeFinanceSCF.Request;
using ScaleUP.Services.LeadModels.ArthMate;

namespace ScaleUP.Services.LeadAPI.NBFCFactory
{
    public interface ILeadNBFCService
    {
        Task<ICreateLeadNBFCResponse> GenerateOffer(long leadid, long nbfcCompanyId);

        Task<ICreateLeadNBFCResponse> PrpareAgreement(long leadid, long nbfcCompanyId);

        Task<ICreateLeadNBFCResponse> PanUpdate(long leadid, long nbfcCompanyId);

        Task<ICreateLeadNBFCResponse> AadhaarUpdate(long leadid, long nbfcCompanyId);
        Task<ICreateLeadNBFCResponse> BusinessUpdate(long leadid, long nbfcCompanyId);
        Task<ICreateLeadNBFCResponse> PersonUpdate(long leadid, long nbfcCompanyId);
        Task<ICreateLeadNBFCResponse> PersonAddressUpdate(long leadid, long nbfcCompanyId);
        Task<ICreateLeadNBFCResponse> BusinessAddressUpdate(long leadid, long nbfcCompanyId);
        Task<ICreateLeadNBFCResponse> BlackSoilCommonApplicationDetail(long leadid);
        Task<LBABusinessLoanDc> LBABusinessLoan(long leadid, string AgreementURL, bool IsSubmit, ProductCompanyConfigDc loanconfig);
        //new for Esign
        Task<string> AgreementEsign(GRPCRequest<EsignAgreementRequest> req);
        Task<ResultViewModel<string>> GetPFCollection(long leadid, string MobileNo);

        #region Arthmate
        Task<CommonResponseDc> GenerateOtpToAcceptOffer(long LeadMasterId);
        Task<GRPCReply<string>> AcceptOfferWithXMLAadharOTP(GRPCRequest<SecondAadharXMLDc> AadharObj);
        Task<CommonResponseDc> GetLeadMasterByLeadId(long leadId);
        Task<LoanInsuranceConfiguration> GetRateOfInterest(int tenure);
        Task<CommonResponseDc> OfferRegenerate(int LeadId, int tenure, double sactionAmount);
        Task<CommonResponseDc> GetLoanByLoanId(long Leadmasterid);
        Task<CommonResponseDc> GetDisbursementAPI(long Leadmasterid);
        Task<List<LeadLoanDataDc>> GetLoan(long LeadMasterId);
        Task<CommonResponseDc> LoanRepaymentScheduleDetails(long LeadMasterId);
        Task<CommonResponseDc> LoanNach(string UMRN, long Leadmasterid);
        Task<CommonResponseDc> ChangeLoanStatus(long LeadMasterId, string Status);
        Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid);
        Task<CommonResponseDc> SaveAgreementESignDocument(long leadmasterid, string eSignDocumentURL);
        Task<CommonResponseDc> LoanDataSave(long leadid, long id);
        Task<CommonResponseDc> UpdateBeneficiaryBankDetail(BeneficiaryBankDetailDc Obj);
        Task<ResultViewModel<List<OfferEmiDetailDC>>> GetOfferEmiDetails(long leadId, int tenure = 0);
        Task<GRPCReply<string>> GetOfferEmiDetailsDownloadPdf(GRPCRequest<EmiDetailReqDc> leadId);
        Task<CommonResponseDc> InsertCeplrBanklist();
        Task<CommonResponseDc> InsertLoanDataByArthmateTest(long leadid, string loanid);
        Task<CommonResponseDc> eSignSessionAsync(string agreementPdfUrl, long leadid);
        Task<CommonResponseDc> eSignDocumentsAsync(long leadid, string DocumentId);
        #endregion
        #region AyeFinanceSCF
        Task<GRPCReply<string>> GenerateToken();

        Task<GRPCReply<string>> AddLead(GRPCRequest<AyeleadReq> ayeleadReq);
        Task<GRPCReply<CheckCreditLineData>> CheckCreditLine(GRPCRequest<AyeleadReq> ayeleadReq);
        Task<GRPCReply<string>> GetWebUrl(GRPCRequest<AyeleadReq> ayeleadReq); 
        Task<GRPCReply<string>> TransactionSendOtp(GRPCRequest<AyeleadReq> ayeleadReq);
        Task<GRPCReply<string>> TransactionVerifyOtp(GRPCRequest<TransactionVerifyOtpReqDc> txnVerifyOtpn);
       

        #endregion
    }
}
