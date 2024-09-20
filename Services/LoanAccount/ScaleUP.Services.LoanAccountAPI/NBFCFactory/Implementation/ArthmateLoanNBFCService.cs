using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenHtmlToPdf;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Global.Infrastructure.Constants;
using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.Global.Infrastructure.Constants.Product;
using ScaleUP.Services.LoanAccountAPI.Helpers.NBFC;
using ScaleUP.Services.LoanAccountAPI.Managers;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using ScaleUP.Services.LoanAccountDTO.Loan;
using ScaleUP.Services.LoanAccountDTO.NBFC;
using ScaleUP.Services.LoanAccountDTO.NBFC.Arthmate;
using ScaleUP.Services.LoanAccountDTO.NBFC.BlackSoil;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using ScaleUP.Services.LoanAccountModels.Master.NBFC.ArthMate;
using ScaleUP.Services.LoanAccountModels.Transaction.NBFC;
using System.Collections.Generic;
using System.Data;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Lead.DataContracts.AyeFinSCF;

namespace ScaleUP.Services.LoanAccountAPI.NBFCFactory.Implementation
{
    public class ArthmateLoanNBFCService : BaseNBFCService, ILoanNBFCService
    {
        private readonly LoanAccountApplicationDbContext _context;
        private readonly ArthmateNBFCHelper _ArthMateNBFCHelper;
        public ArthmateLoanNBFCService(LoanAccountApplicationDbContext context) : base(context)
        {
            _context = context;
            _ArthMateNBFCHelper = new ArthmateNBFCHelper();
        }

        public async Task<ResultViewModel<List<LoanRepaymentScheduleDetailDc>>> LoanRepaymentScheduleDetails(long loanAccountId)
        {
            ResultViewModel<List<LoanRepaymentScheduleDetailDc>> result = new ResultViewModel<List<LoanRepaymentScheduleDetailDc>>();

            var repayment = _context.repaymentSchedules.Where(x => x.LoanAccountId == loanAccountId).ToList();
            if (repayment.Any())
            {
                result.Result = repayment.Select(x => new LoanRepaymentScheduleDetailDc
                {
                    company_id = x.CompanyId,
                    due_date = x.DueDate,
                    emi_amount = x.EMIAmount,
                    emi_no = x.EMINo,
                    int_amount = x.InterestAmount,
                    prin = x.Principal,
                    principal_bal = x.PrincipalBalance,
                    principal_outstanding = x.PrincipalOutstanding,
                    product_id = Convert.ToInt32(x.ProductId),
                    repay_schedule_id = x.RepayScheduleId
                }).ToList();
                result.IsSuccess = true;
                return result;
            }
            var Loan = await _context.BusinessLoans.Where(x => x.LoanAccountId == loanAccountId).FirstOrDefaultAsync();
            if (Loan != null)
            {
                var Postrepayment_schedule = new Postrepayment_scheduleDc
                {
                    loan_id = Loan.loan_id,
                    product_id = Loan.product_id,
                    company_id = Loan.company_id ?? 0,
                };
                var repaymentScheduleApi = await _context.NBFCCompanyAPIs.FirstOrDefaultAsync(x => x.Code == CompanyApiConstants.ArthmateRepaymentSchedule && x.IsActive && !x.IsDeleted);
                var Response = await _ArthMateNBFCHelper.repayment_schedule(Postrepayment_schedule, repaymentScheduleApi.APIUrl, repaymentScheduleApi.TAPIKey, repaymentScheduleApi.TAPISecretKey, repaymentScheduleApi.Id);
                _context.ArthMateCommonAPIRequestResponses.Add(Response);
                _context.SaveChanges();
                result.IsSuccess = Response.IsSuccess;
                if (Response.IsSuccess)
                {
                    var RepaymentAllResponse = JsonConvert.DeserializeObject<repay_scheduleDc>(Response.Response);
                    if (Loan.first_inst_date != null && RepaymentAllResponse != null && RepaymentAllResponse.data != null && RepaymentAllResponse.data.rows != null && RepaymentAllResponse.data.rows.Any())
                    {
                        List<RepaymentSchedule> repayments = new List<RepaymentSchedule>();
                        foreach (var item in RepaymentAllResponse.data.rows)
                        {
                            RepaymentSchedule Repay = new RepaymentSchedule
                            {
                                LoanAccountId = loanAccountId,
                                CompanyId = item.company_id,
                                loanId = Loan.loan_id,
                                ProductId = Loan.product_id,
                                EMIAmount = item.emi_amount,
                                EMINo = item.emi_no,
                                InterestAmount = item.int_amount,
                                DueDate = Loan.first_inst_date.Value.AddMonths(item.emi_no - 1),
                                Principal = item.prin,
                                PrincipalBalance = item.principal_bal,
                                PrincipalOutstanding = item.principal_outstanding,
                                RepayScheduleId = item.repay_schedule_id,
                                Created = DateTime.Now,
                                IsActive = true,
                                IsDeleted = false,
                            };
                            repayments.Add(Repay);
                        }
                        _context.repaymentSchedules.AddRange(repayments);
                        _context.SaveChanges();

                        result.Result = repayments.Select(x => new LoanRepaymentScheduleDetailDc
                        {
                            company_id = x.CompanyId,
                            due_date = x.DueDate,
                            emi_amount = x.EMIAmount,
                            emi_no = x.EMINo,
                            int_amount = x.InterestAmount,
                            prin = x.Principal,
                            principal_bal = x.PrincipalBalance,
                            principal_outstanding = x.PrincipalOutstanding,
                            product_id = Convert.ToInt32(x.ProductId),
                            repay_schedule_id = x.RepayScheduleId,
                        }).ToList();
                    }
                }
            }
            return result;
        }

        public Task<double> CalculatePerDayInterest(double interest)
        {
            throw new NotImplementedException();
        }

        public Task<double> GetAvailableCreditLimit(long accountId)
        {
            throw new NotImplementedException();
        }

        public Task<List<InvoiceNBFCReqRes>> GetInvoiceNBFCReqRes(List<long> ApiDetailIds)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> InsertSkippedPayments(long loanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInvoiceInitiated(long invoiceId)
        {
            throw new NotImplementedException();
        }
        public Task<NBFCFactoryResponse> OrderCaptured(long invoiceId, long accountId, double transAmount, bool status, string? OrderNo, string? ayeFinNBFCToken = "")
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> OrderInitiate(long invoiceId, long accountId, double transAmount)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveNBFCLoanData(long accountId, string? webhookresposne, NBFCDetailDTO nBFCDetailDTO = null)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> SettlePayment(long blackSoilRepaymentId, long blackSoilLoanAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> SettlePaymentLater(GRPCRequest<SettlePaymentJobRequest> request)
        {
            throw new NotImplementedException();
        }

        public async Task<RePaymentScheduleDataDc> GetDisbursedLoanDetail(long Leadmasterid)
        {
            RePaymentScheduleDataDc response = new RePaymentScheduleDataDc();

            var loanaccount = _context.LoanAccounts.FirstOrDefault(x => x.LeadId == Leadmasterid && x.IsActive && !x.IsDeleted);

            if (loanaccount != null)
            {
                if (loanaccount.NBFCIdentificationCode == CompanyIdentificationCodeConstants.ArthMate)
                {
                    var leadid = new SqlParameter("@leadid", Leadmasterid);
                    var result = _context.Database.SqlQueryRaw<DisbursedDetailDc>("exec GetDisbursedLoanDetail @leadid", leadid).AsEnumerable().FirstOrDefault();

                    if (result != null)
                    {
                        Postrepayment_scheduleDc reqdata = new Postrepayment_scheduleDc()
                        {
                            company_id = result.company_id,
                            loan_id = result.loan_id,
                            product_id = result.product_id
                        };

                        long loanAccountId = _context.LoanAccounts.FirstOrDefault(x => x.LeadId == Leadmasterid).Id;
                        var Response = await LoanRepaymentScheduleDetails(loanAccountId);

                        if (Response.IsSuccess)
                        {
                            // var res = JsonConvert.DeserializeObject<repay_scheduleDc>(Response.Response);
                            if (Response != null && Response.Result.Count > 0)
                            {
                                List<repay_scheduleDetails> list = new List<repay_scheduleDetails>();
                                foreach (var item in Response.Result.ToList())
                                {
                                    repay_scheduleDetails Repay = new repay_scheduleDetails
                                    {
                                        company_id = item.company_id,
                                        emi_amount = item.emi_amount,
                                        emi_no = item.emi_no,
                                        int_amount = item.int_amount,
                                        prin = item.prin,
                                        principal_bal = item.principal_bal,
                                        principal_outstanding = item.principal_outstanding,
                                        product_id = item.product_id,
                                        repay_schedule_id = item.repay_schedule_id,
                                        due_date = item.due_date,
                                    };
                                    list.Add(Repay);
                                }

                                response.rows = list;
                                response.MonthlyEMI = result.MonthlyEMI;
                                response.InsurancePremium = result.InsurancePremium;
                                response.sanction_amount = result.sanction_amount;
                                response.borro_bank_acc_num = result.borro_bank_acc_num;
                                response.loan_int_amt = result.loan_int_amt;
                                response.net_disbur_amt = result.net_disbur_amt;
                                response.loan_id = result.loan_id;
                                response.processing_fees_amt = result.processing_fees_amt;
                            }
                        }
                    }
                }
                else
                {
                    var bldisbursement = _context.BusinessLoanDisbursementDetail.Where(x => x.LoanAccountId == loanaccount.Id && x.NBFCCompanyId == loanaccount.NBFCCompanyId).FirstOrDefault();
                    if (bldisbursement != null)
                    {
                        List<repay_scheduleDetails> list = new List<repay_scheduleDetails>();
                        var repayment = _context.repaymentSchedules.Where(x => x.LoanAccountId == loanaccount.Id).ToList();
                        if (repayment.Any())
                        {
                            var result = repayment.Select(x => new LoanRepaymentScheduleDetailDc
                            {
                                company_id = x.CompanyId,
                                due_date = x.DueDate,
                                emi_amount = x.EMIAmount,
                                emi_no = x.EMINo,
                                int_amount = x.InterestAmount,
                                prin = x.Principal,
                                principal_bal = x.PrincipalBalance,
                                principal_outstanding = x.PrincipalOutstanding,
                                product_id = Convert.ToInt32(x.ProductId),
                                repay_schedule_id = x.RepayScheduleId
                            }).ToList();

                            foreach (var item in result)
                            {
                                list.Add(new repay_scheduleDetails
                                {
                                    company_id = item.company_id,
                                    product_id = item.product_id,
                                    due_date = item.due_date,
                                    emi_amount = item.emi_amount,
                                    emi_no = item.emi_no,
                                    int_amount = item.int_amount,
                                    prin = item.prin,
                                    principal_bal = item.principal_bal,
                                    principal_outstanding = item.principal_outstanding,
                                    repay_schedule_id = item.repay_schedule_id
                                });
                            }
                        }
                        double InsuranceAmount = bldisbursement.InsuranceAmount ?? 0;
                        double othercharges = bldisbursement.OtherCharges ?? 0;

                        response.rows = list;
                        response.MonthlyEMI = bldisbursement.MonthlyEMI ?? 0;
                        response.InsurancePremium = 0;
                        response.sanction_amount = bldisbursement.LoanAmount ?? 0;
                        response.borro_bank_acc_num = "";
                        response.loan_int_amt = bldisbursement.LoanInterestAmount ?? 0;
                        response.net_disbur_amt = (bldisbursement.LoanAmount - ((bldisbursement.ProcessingFeeAmount - bldisbursement.PFDiscount) + bldisbursement.ProcessingFeeTax + InsuranceAmount + othercharges)) ?? 0;
                        response.loan_id = bldisbursement.LoanId ?? "";
                        response.processing_fees_amt = ((bldisbursement.ProcessingFeeAmount - bldisbursement.PFDiscount) + bldisbursement.ProcessingFeeTax) ?? 0;
                        response.CompanyIdentificationCode = loanaccount.NBFCIdentificationCode;
                        response.InsuranceAmount = InsuranceAmount;
                        response.Othercharges = othercharges;
                    }
                }

            }

            return response;
        }

        public Task<GRPCReply<string>> BLoanEMIPdf(GRPCRequest<long> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<ApplyLoanResponseDC>> ApplyLoan(GRPCRequest<ApplyLoanreq> request)
        {
            throw new NotImplementedException();
        }
        public Task<GRPCReply<CheckTotalAndAvailableLimitResponseDc>> CheckTotalAndAvailableLimit(GRPCRequest<AyeloanReq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<DeliveryConfirmationResponseDC>> DeliveryConfirmation(GRPCRequest<DeliveryConfirmationreq> request)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<CancellationResponseDC>> CancelTransaction(GRPCRequest<CancelTxnReq> request)
        {
            throw new NotImplementedException();
        }

        public Task<NBFCFactoryResponse> AyeSCFCOrderInitiate(GRPCRequest<AyeSCFCOrderInitiateDTO> req)
        {
            throw new NotImplementedException();
        }

        public Task<GRPCReply<DeliveryConfirmationResponseDC>> Repayment(GRPCRequest<Repaymentreq> request)
        {
            throw new NotImplementedException();
        }
    }
}
