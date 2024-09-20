using ScaleUP.BuildingBlocks.GRPC.Contracts.LoanAccount.DataContracts;
using ScaleUP.BuildingBlocks.GRPC.Contracts;
using ScaleUP.Services.LoanAccountAPI.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using ScaleUP.Services.LoanAccountModels.Master;
using ScaleUP.Services.LoanAccountModels.Transaction;
using Microsoft.Data.SqlClient;
using ScaleUP.BuildingBlocks.GRPC.Contracts.NBFC.DataContracts;
using System.Data;
using ScaleUP.Services.LoanAccountDTO.Loan;
using MassTransit;
using ScaleUP.Global.Infrastructure.Constants.AccountTransaction;
using Microsoft.VisualBasic;
using System.Transactions;
using MassTransit.Futures.Contracts;
using ScaleUP.Services.LoanAccountDTO;
using ScaleUP.Services.LoanAccountAPI.NBFCFactory;
using ScaleUP.Services.LoanAccountDTO.Transaction;
using ScaleUP.Global.Infrastructure.Enum;

namespace ScaleUP.Services.LoanAccountAPI.Managers
{
    public class DelayPenalityOnDuePerDayJobManager
    {
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        private DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

        private readonly LoanAccountApplicationDbContext _context;
        private readonly LoanNBFCFactory _loanNBFCFactory;
        public DelayPenalityOnDuePerDayJobManager(LoanAccountApplicationDbContext context, LoanNBFCFactory loanNBFCFactory)
        {
            _context = context;
            _loanNBFCFactory = loanNBFCFactory;
        }

        //public async Task<CommonResponse> InsertDailyPenaltyCharges(string TransactionId = "")
        //{
        //    CommonResponse res = new CommonResponse();
        //    AccountTransactionManager objAccounttransManager = new AccountTransactionManager(_context);
        //    try
        //    {

        //    var RefId = new SqlParameter("TransactionId", TransactionId);
        //    var DelayPenalityOnDuePerDay = await _context.Database.SqlQueryRaw<GetDelayPenalityOnDuePerDayJobDTO>("exec DelayPenalityGetPerDayTransactions @TransactionId", RefId).ToListAsync();
        //    if (DelayPenalityOnDuePerDay != null && DelayPenalityOnDuePerDay.Any())
        //    {
        //        //var TransactionStatusesIDsOverdue = _context.TransactionStatuses.Where(x => x.Code == TransactionStatuseConstants.Overdue).FirstOrDefault();
        //        //var TransactionTypeIdOrderPlacement = _context.TransactionTypes.Where(x => x.Code == TransactionTypesConstants.OrderPlacement).FirstOrDefault();
        //        //var TransactionTypeId = _context.TransactionTypes.Where(x => x.Code == TransactionTypesConstants.PenaltyCharges).FirstOrDefault();
        //        //var TransactionDetailHeadIds = _context.TransactionDetailHeads.Where(x => x.Code == TransactionDetailHeadsConstants.DelayPenalty).FirstOrDefault();
        //        long TransactionStatusesID_Overdue = 0;
        //        long TransactionTypeId_OrderPlacement = 0;
        //        long TransactionTypeId_PenaltyCharges = 0;
        //        long TransactionDetailHeadId_DelayPenalty = 0;

        //        foreach (var item in DelayPenalityOnDuePerDay)
        //        {
        //            int rowChanged = 0;
        //            long AccountTransactionId = 0;
        //            double PenaltyAmount = 0;
        //            double PenaltyGSTAmount = 0;
        //            double PreviousPenaltyGSTAmount = 0;
        //            double TotalAmount = 0;

        //            bool PayableBy = false;
        //            string sPayableBY = item.PayableBy ?? "";
        //            if (sPayableBY == "Customer")
        //            { PayableBy = true; }

        //            TransactionStatusesID_Overdue = item.TransactionStatusesID_Overdue;
        //            TransactionTypeId_OrderPlacement = item.TransactionTypeId_OrderPlacement;
        //            TransactionTypeId_PenaltyCharges = item.TransactionTypeId_PenaltyCharges;
        //            TransactionDetailHeadId_DelayPenalty = item.TransactionDetailHeadId_DelayPenalty;

        //            string GSTHeadCode = "GST" + Convert.ToString(item.GstRate);
        //            var TransactionDetailHeadId_GST = _context.TransactionDetailHeads.Where(x => x.Code == GSTHeadCode).FirstOrDefault();

        //            var accountTransactionExist = await _context.AccountTransactions.Where(x => x.Id == item.AccountTransactionId || x.ParentAccountTransactionsID == item.AccountTransactionId).ToListAsync();
        //            if (accountTransactionExist != null && accountTransactionExist.Any())
        //            {
        //                var OrderPlacementTransExist = accountTransactionExist.Where(x => x.TransactionTypeId == TransactionTypeId_OrderPlacement).FirstOrDefault();
        //                if (OrderPlacementTransExist != null)
        //                {
        //                    //long TransactionStatusId = TransactionStatusesID_Overdue; // res.TransactionStatusId ;
        //                    long LoanAccountId = OrderPlacementTransExist.LoanAccountId;

        //                    var PenalityExist = accountTransactionExist.Where(x => x.TransactionTypeId == TransactionTypeId_PenaltyCharges).FirstOrDefault();
        //                    if (PenalityExist == null)
        //                    {
        //                        //TotalAmount = (PayableBy == true) ? (item.DelayPenalityCharge + item.PenalityGstCharge ?? 0) : item.DelayPenalityCharge ?? 0;
        //                        TotalAmount = (PayableBy == true) ? (item.DelayPenalityCharge + item.PenalityGstCharge ?? 0) : 0;

        //                        LoanAccountTransactionRequest AccountTransaction = new LoanAccountTransactionRequest()
        //                        {
        //                            CustomerUniqueCode = OrderPlacementTransExist.CustomerUniqueCode ?? "",
        //                            LoanAccountId = LoanAccountId,
        //                            AnchorCompanyId = OrderPlacementTransExist.AnchorCompanyId,
        //                            ReferenceId = OrderPlacementTransExist.ReferenceId ?? "",
        //                            TransactionTypeCode = TransactionTypesConstants.PenaltyCharges, // TransactionTypesConstants.Disbursement,
        //                            TransactionAmount = 0,
        //                            ParentAccountTransactionsID = OrderPlacementTransExist.Id,
        //                            OrderAmount = item.DelayPenalityCharge ?? 0,
        //                            GSTAmount = item.PenalityGstCharge ?? 0,
        //                            PaidAmount = TotalAmount,

        //                            ProcessingFeeType = "",
        //                            ProcessingFeeRate = 0,
        //                            GstRate = OrderPlacementTransExist.GstRate,
        //                            InterestType = "", //ConvenienceFeeType
        //                            InterestRate = 0, //ConvenienceFeeRate
        //                            CreditDays = OrderPlacementTransExist.CreditDays,
        //                            BounceCharge = 0,
        //                            DelayPenaltyRate = OrderPlacementTransExist.DelayPenaltyRate,
        //                            PayableBy = sPayableBY,
        //                            PaymentRefNo = "",
        //                        };
        //                        GRPCRequest<LoanAccountTransactionRequest> requestAccountTransaction = new GRPCRequest<LoanAccountTransactionRequest> { Request = AccountTransaction };
        //                        var replyAccountTransaction = objAccounttransManager.SaveTransaction(requestAccountTransaction, OrderPlacementTransExist.CompanyProductId, OrderPlacementTransExist.SettlementDate, TransactionStatusesID_Overdue, Convert.ToDateTime(OrderPlacementTransExist.DueDate));

        //                        if (replyAccountTransaction.Result.Response > 0 && replyAccountTransaction.Result.Status == true)
        //                        {
        //                            AccountTransactionId = replyAccountTransaction.Result.Response;
        //                            PenaltyAmount = item.DelayPenalityCharge ?? 0;
        //                            PenaltyGSTAmount = item.PenalityGstCharge ?? 0;
        //                            PreviousPenaltyGSTAmount = 0;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        AccountTransactionId = PenalityExist.Id;

        //                        var accountTransactionDtl = _context.AccountTransactionDetails.Where(x => x.AccountTransactionId == AccountTransactionId).ToList();
        //                        //Double TotalPenaltyAmount = accountTransactionDtl.Where(x => x.TransactionDetailHeadId == TransactionDetailHeadIds.Id).Sum(z => z.Amount);
        //                        var PenaltyGSTData = accountTransactionDtl.Where(x => x.TransactionDetailHeadId == TransactionDetailHeadId_GST.Id && x.Amount > 0).OrderByDescending(x => x.Id).FirstOrDefault();
        //                        PreviousPenaltyGSTAmount = PenaltyGSTData.Amount;

        //                        PenaltyAmount = item.DelayPenalityCharge ?? 0;
        //                        PenaltyGSTAmount = PreviousPenaltyGSTAmount + item.PenalityGstCharge ?? 0;

        //                        //TotalAmount = (PayableBy == true) ? (PenaltyAmount + (PenaltyGSTAmount - PreviousPenaltyGSTAmount)) : PenaltyAmount;
        //                        TotalAmount = (PayableBy == true) ? (PenaltyAmount + (PenaltyGSTAmount - PreviousPenaltyGSTAmount)) : 0;
        //                    }

        //                    ///--------------
        //                    if (AccountTransactionId > 0)
        //                    {
        //                        List<LoanAccountTransactionDetailRequest> transactionDetails = new List<LoanAccountTransactionDetailRequest>();

        //                        transactionDetails.Add(new LoanAccountTransactionDetailRequest()
        //                        {
        //                            AccountTransactionId = AccountTransactionId,
        //                            Amount = PenaltyAmount,
        //                            TransactionDetailHeadCode = TransactionDetailHeadsConstants.DelayPenalty,
        //                            IsPayableBy = PayableBy,
        //                        });

        //                        if (PreviousPenaltyGSTAmount > 0)
        //                        {
        //                            transactionDetails.Add(new LoanAccountTransactionDetailRequest()
        //                            {
        //                                AccountTransactionId = AccountTransactionId,
        //                                Amount = (-1) * PreviousPenaltyGSTAmount,
        //                                TransactionDetailHeadCode = GSTHeadCode,
        //                                IsPayableBy = PayableBy,
        //                            });
        //                        }

        //                        transactionDetails.Add(new LoanAccountTransactionDetailRequest()
        //                        {
        //                            AccountTransactionId = AccountTransactionId,
        //                            Amount = PenaltyGSTAmount,
        //                            TransactionDetailHeadCode = GSTHeadCode,
        //                            IsPayableBy = PayableBy,
        //                        });

        //                        foreach (var itemData in transactionDetails)
        //                        {
        //                            GRPCRequest<LoanAccountTransactionDetailRequest> requestAccountTransactionDetail = new GRPCRequest<LoanAccountTransactionDetailRequest> { Request = itemData };
        //                            var replyAccountTransactionDetail = objAccounttransManager.SaveTransactionDetail(requestAccountTransactionDetail);

        //                            rowChanged++;
        //                        }
        //                    }

        //                    //if (replyAccountTransaction.Id > 0)
        //                    if (rowChanged > 0)
        //                    {
        //                        var loanAccountcredit = _context.LoanAccountCredits.Where(x => x.LoanAccountId == LoanAccountId).FirstOrDefault();
        //                        loanAccountcredit.CreditLimitAmount = loanAccountcredit.CreditLimitAmount - TotalAmount;
        //                        _context.Entry(loanAccountcredit).State = EntityState.Modified;

        //                        if (PreviousPenaltyGSTAmount > 0)
        //                        {
        //                            PenalityExist.OrderAmount = PenalityExist.OrderAmount + PenaltyAmount;
        //                            PenalityExist.GSTAmount = PenalityExist.GSTAmount + (PenaltyGSTAmount - PreviousPenaltyGSTAmount);
        //                            PenalityExist.PaidAmount = PenalityExist.PaidAmount + TotalAmount;
        //                            _context.Entry(PenalityExist).State = EntityState.Modified;
        //                        }

        //                        foreach (var ACT in accountTransactionExist)
        //                        {
        //                            ACT.TransactionStatusId = TransactionStatusesID_Overdue;
        //                            _context.Entry(ACT).State = EntityState.Modified;
        //                        }

        //                        _context.SaveChanges();
        //                        //dbContextTransaction.Commit();

        //                        res.status = true;
        //                        res.Message = "Success";
        //                    }
        //                    else
        //                    {
        //                        //dbContextTransaction.Rollback();

        //                        res.status = false;
        //                        res.Message = "Fail";
        //                    }
        //                    ///--------------
        //                }
        //            }
        //        }
        //    }

        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //    return res;
        //}


        public async Task<List<GetOutstandingTransactionsDTO>> GetOutstandingTransactionsList(string TransactionNo, long LoanAccountId)
        {
            List<GetOutstandingTransactionsDTO> result = new List<GetOutstandingTransactionsDTO>();

            try
            {
                var Transaction_No = new SqlParameter("TransactionNo", TransactionNo);
                var LoanAccount_Id = new SqlParameter("LoanAccountId", LoanAccountId);
                result = _context.Database.SqlQueryRaw<GetOutstandingTransactionsDTO>("exec GetOutstandingTransactionsList @TransactionNo, @LoanAccountId", Transaction_No, LoanAccount_Id).AsEnumerable().ToList();

            }
            catch (Exception ex)
            {

                throw;
            }



            return result;
        }

        public async Task<bool> UpdateTransactionStatusJob()
        {
            CommonResponse res = new CommonResponse();

            List<AccountTransaction> Transactions = new List<AccountTransaction>();

            var transactionStatuse = await _context.TransactionStatuses.Where(x => x.IsActive).ToListAsync();
            var transactionTypeIds = await _context.Database.SqlQueryRaw<long>($"SELECT Id FROM dbo.GetTransactionTypeId()").ToListAsync();


            var PendingStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Pending)?.Id ?? 0;
            long DueStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Due)?.Id ?? 0;
            long OverDueStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Overdue)?.Id ?? 0;
            long DelinquentStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Delinquent)?.Id ?? 0;
            long PaidStatusId = transactionStatuse.FirstOrDefault(x => x.Code == TransactionStatuseConstants.Paid)?.Id ?? 0;

            long TxnTypeId = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement)?.Id ?? 0;

            int days = 121;
            DateTime currentdate = indianTime;

            //ParentAccountTransactionId
            var transactiolist = Transactions = await _context.AccountTransactions.Where(x => ((x.DueDate.Value.Date <= currentdate.Date && (x.TransactionStatusId == PendingStatusId || x.TransactionStatusId == DueStatusId))
                               || (x.DueDate.Value.Date.AddDays(days) <= currentdate.Date && (x.TransactionStatusId == OverDueStatusId))) && x.DisbursementDate.HasValue && x.TransactionStatusId != PaidStatusId && x.TransactionTypeId == TxnTypeId && x.IsActive && !x.IsDeleted).ToListAsync();

            var ReferenceIds = transactiolist.Select(x => x.ReferenceId).Distinct().ToList();
           
            if (Transactions.Any())
            {
                foreach (var transaction in transactiolist)
                {
                    if (transaction.DueDate.Value.Date == currentdate.Date)
                    {
                        var txns = await _context.AccountTransactions.Where(x => x.ReferenceId == transaction.ReferenceId && transactionTypeIds.Contains(x.TransactionTypeId) && x.IsActive && !x.IsDeleted).ToListAsync();

                        foreach (var tx in txns)
                        {
                            tx.TransactionStatusId = DueStatusId; //change status from pendig to due
                            tx.LastModified = currentdate;
                            _context.Entry(tx).State = EntityState.Modified;
                        }
                    }
                    if (transaction.TransactionStatusId == DueStatusId && transaction.DueDate.Value.AddDays(1).Date <= currentdate.Date)
                    {
                        var txns = await _context.AccountTransactions.Where(x => x.ReferenceId == transaction.ReferenceId && transactionTypeIds.Contains(x.TransactionTypeId) && x.IsActive && !x.IsDeleted).ToListAsync();
                        foreach (var tx in txns)
                        {
                            tx.TransactionStatusId = OverDueStatusId; //change status from pendig to due
                            tx.LastModified = currentdate;
                            _context.Entry(tx).State = EntityState.Modified;
                        }
                    }
                    if (transaction.TransactionStatusId == OverDueStatusId && transaction.DueDate.Value.AddDays(days).Date <= currentdate.Date)
                    {
                        var txns = await _context.AccountTransactions.Where(x => x.ReferenceId == transaction.ReferenceId && transactionTypeIds.Contains(x.TransactionTypeId) && x.IsActive && !x.IsDeleted).ToListAsync();
                        foreach (var tx in txns)
                        {
                            tx.TransactionStatusId = DelinquentStatusId; //change status from pendig to due
                            tx.LastModified = currentdate;
                            _context.Entry(tx).State = EntityState.Modified;
                        }
                    }
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
                    res.status = true;
                    res.Message = "Success";
                }
                else
                {
                    res.status = false;
                    res.Message = "Failed";
                }
            }
            return res.status;
        }

        public async Task<bool> OverDueInterestCharge(DateTime? CalculationDate = null, string? TransactionNo = null, long? LoanAccountId = null)
        {
            CommonResponse res = new CommonResponse();

            if (!CalculationDate.HasValue)
                CalculationDate = indianTime;

            var date = CalculationDate.Value.ToString("yyyy-MM-dd");

            CalculationDate = Convert.ToDateTime(date);

            AccountTransactionManager objAccounttransManager = new AccountTransactionManager(_context, _loanNBFCFactory);
            var transactionNo = new SqlParameter("TransactionNo", TransactionNo == null ? DBNull.Value : TransactionNo);
            var LoanId = new SqlParameter("LoanAccountId", LoanAccountId == null ? DBNull.Value : LoanAccountId);
            var dateofcalculation = new SqlParameter("DateOfCalculation", CalculationDate.Value.ToString("yyyy-MM-dd"));
            var OverDueInterestCharges = await _context.Database.SqlQueryRaw<OverDueInterestCharge>("exec GetOverDueTransactionsList @TransactionNo,@LoanAccountId,@DateOfCalculation", transactionNo, LoanId, dateofcalculation).ToListAsync();
            if (OverDueInterestCharges != null && OverDueInterestCharges.Any())
            {
                var transactionTypes = _context.TransactionTypes.Where(x => x.IsActive && !x.IsDeleted).ToList();
                var Head = _context.TransactionDetailHeads.Where(x => x.IsActive && !x.IsDeleted).ToList();
                long? OverdueInterestHeadId = Head.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.OverdueInterestAmount)?.Id;
                long? OverdueInterest_Id = transactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OverdueInterest)?.Id;
                var OrderPlacement_Id = transactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement)?.Id;
                var transIds = OverDueInterestCharges.Select(x => x.AccountTransactionId).Distinct().ToList();
                var AllTrans = await _context.AccountTransactions.Where(x => transIds.Contains(x.Id) && (x.TransactionTypeId == OrderPlacement_Id || x.TransactionTypeId == OverdueInterest_Id) && x.IsActive && !x.IsDeleted).ToListAsync();
                var referenceids = AllTrans.Select(x => x.ReferenceId).Distinct().ToList();
                var AllOverdueInterestTrans = await _context.AccountTransactions.Where(x => referenceids.Contains(x.ReferenceId) && x.TransactionTypeId == OverdueInterest_Id && x.IsActive && !x.IsDeleted).ToListAsync();

                foreach (var item in OverDueInterestCharges)
                {
                    var OrderPlacementTrans = AllTrans.Where(x => x.Id == item.AccountTransactionId && x.TransactionTypeId == OrderPlacement_Id && x.IsActive && !x.IsDeleted).ToList();

                    if (OrderPlacementTrans != null && OrderPlacementTrans.Any())
                    {
                        string referenceid = OrderPlacementTrans.FirstOrDefault(x => x.Id == item.AccountTransactionId).ReferenceId;
                        bool PayableBy = false;
                        //string sPayableBY = OrderPlacementTrans.FirstOrDefault().PayableBy ?? "";
                        string sPayableBY = "Customer"; //As discussed with Deepak as on 15/03/2024.
                        if (sPayableBY == "Customer")
                        { PayableBy = true; }

                        var OverdueInterestExist = AllOverdueInterestTrans.Where(x => x.ReferenceId == referenceid && x.TransactionTypeId == OverdueInterest_Id).FirstOrDefault();

                        var result = _loanNBFCFactory.GetService(item.NBFCIdentificationCode);
                        double InterestRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(item.InterestRate));

                        var perDayInterest = OrderPlacementTrans.FirstOrDefault().InterestType == "Percentage" ? (item.PrincipleOutstanding * InterestRatePerDay / 100.0) : InterestRatePerDay;

                        double perDayInterestAmount = 0;

                        if (Math.Round(perDayInterest, 2) > 0)
                        {
                            perDayInterestAmount = perDayInterest;
                        }
                        else
                        {
                            perDayInterestAmount = 0;
                        }

                        //if (perDayInterestAmount > 0)
                        {
                            if (OverdueInterestExist == null)
                            {
                                AccountTransaction AccountTransaction = new AccountTransaction()
                                {
                                    CustomerUniqueCode = OrderPlacementTrans.FirstOrDefault().CustomerUniqueCode ?? "",
                                    LoanAccountId = item.LoanAccountId,
                                    AnchorCompanyId = OrderPlacementTrans.FirstOrDefault().AnchorCompanyId,
                                    ReferenceId = OrderPlacementTrans.FirstOrDefault().ReferenceId ?? "",
                                    TransactionTypeId = Convert.ToInt64(OverdueInterest_Id),
                                    TransactionAmount = OrderPlacementTrans.FirstOrDefault().TransactionAmount,
                                    ParentAccountTransactionsID = OrderPlacementTrans.FirstOrDefault().Id,
                                    OrderAmount = perDayInterestAmount,
                                    GSTAmount = 0,
                                    PaidAmount = perDayInterestAmount,
                                    ProcessingFeeType = "",
                                    ProcessingFeeRate = 0,
                                    GstRate = OrderPlacementTrans.FirstOrDefault().GstRate,
                                    InterestType = OrderPlacementTrans.FirstOrDefault().InterestType,
                                    InterestRate = OrderPlacementTrans.FirstOrDefault().InterestRate,
                                    CreditDays = OrderPlacementTrans.FirstOrDefault().CreditDays,
                                    BounceCharge = OrderPlacementTrans.FirstOrDefault().BounceCharge,
                                    DelayPenaltyRate = OrderPlacementTrans.FirstOrDefault().DelayPenaltyRate,
                                    PayableBy = sPayableBY,// OrderPlacementTrans.FirstOrDefault().PayableBy,
                                    CompanyProductId = OrderPlacementTrans.FirstOrDefault().CompanyProductId,
                                    TransactionStatusId = OrderPlacementTrans.FirstOrDefault().TransactionStatusId,
                                    AccountTransactionDetails = new List<AccountTransactionDetail>(),
                                    IsActive = true,
                                    IsDeleted = false,
                                    DueDate = OrderPlacementTrans.FirstOrDefault().DueDate,
                                    InvoiceDate = OrderPlacementTrans.FirstOrDefault().InvoiceDate,
                                    InvoiceId = OrderPlacementTrans.FirstOrDefault().InvoiceId,
                                    InvoiceNo = OrderPlacementTrans.FirstOrDefault().InvoiceNo,
                                    OrderDate = OrderPlacementTrans.FirstOrDefault().OrderDate,
                                    TransactionStatus = OrderPlacementTrans.FirstOrDefault().TransactionStatus,
                                    SettlementDate = OrderPlacementTrans.FirstOrDefault().SettlementDate,
                                    InterestAmountCalculated = OrderPlacementTrans.FirstOrDefault().InterestAmountCalculated,
                                    InvoicePdfURL = OrderPlacementTrans.FirstOrDefault().InvoicePdfURL,
                                    DisbursementDate = OrderPlacementTrans.FirstOrDefault().DisbursementDate
                                };

                                if (item.AccountTransactionId > 0)
                                {
                                    AccountTransactionDetail transactionDetail = new AccountTransactionDetail()
                                    {
                                        AccountTransactionId = AccountTransaction.Id,
                                        Amount = perDayInterestAmount,
                                        TransactionDetailHeadId = Convert.ToInt64(OverdueInterestHeadId),
                                        IsPayableBy = PayableBy,
                                        Status = "success",
                                        Created = indianTime,
                                        IsActive = true,
                                        IsDeleted = false,
                                        TransactionDate = CalculationDate
                                    };
                                    AccountTransaction.AccountTransactionDetails.Add(transactionDetail);
                                }
                                _context.AccountTransactions.Add(AccountTransaction);

                            }
                            else
                            {
                                var transactionDetail = _context.AccountTransactionDetails.FirstOrDefault(x => x.TransactionDetailHeadId == OverdueInterestHeadId && x.AccountTransactionId == OverdueInterestExist.Id && x.TransactionDate.Value.Date == CalculationDate.Value.Date && x.IsActive && !x.IsDeleted);
                                if (transactionDetail != null)
                                {
                                    transactionDetail.Amount = perDayInterestAmount;
                                    transactionDetail.LastModified = indianTime;
                                    _context.Entry(OverdueInterestExist).State = EntityState.Modified;
                                }
                                else if (transactionDetail == null)
                                {
                                    OverdueInterestExist.OrderAmount += perDayInterestAmount;
                                    OverdueInterestExist.PaidAmount += perDayInterestAmount;
                                    OverdueInterestExist.LastModified = indianTime;
                                    _context.Entry(OverdueInterestExist).State = EntityState.Modified;


                                    //if (transactionDetail != null)
                                    //{
                                    //    transactionDetail.Amount = perDayInterestAmount;
                                    //    transactionDetail.LastModified = indianTime;
                                    //    _context.Entry(OverdueInterestExist).State = EntityState.Modified;
                                    //}
                                    //else
                                    //{
                                    //AccountTransaction AccountTransaction = new AccountTransaction();
                                    OverdueInterestExist.AccountTransactionDetails = new List<AccountTransactionDetail>();

                                    AccountTransactionDetail Detail = new AccountTransactionDetail()
                                    {
                                        AccountTransactionId = OverdueInterestExist.Id,
                                        Amount = perDayInterestAmount,
                                        TransactionDetailHeadId = Convert.ToInt64(OverdueInterestHeadId),
                                        IsPayableBy = PayableBy,
                                        Status = "success",
                                        Created = indianTime,
                                        IsActive = true,
                                        IsDeleted = false,
                                        TransactionDate = CalculationDate,
                                    };
                                    //OverdueInterestExist.AccountTransactionDetails.Add(Detail);
                                    //_context.AccountTransactions.Add(OverdueInterestExist);
                                    OverdueInterestExist.AccountTransactionDetails.Add(Detail);
                                    _context.Entry(OverdueInterestExist).State = EntityState.Modified;
                                    //}
                                }
                            }
                        }
                    }
                }
                if (_context.SaveChanges() > 0)
                {
                    res.status = true;
                    res.Message = "Success";
                }
                else
                {
                    res.status = false;
                    res.Message = "Failed";
                }
            }

            return res.status;
        }

        public async Task<bool> InsertDailyPenaltyCharges(DateTime? CalculationDate = null, string? TransactionNo = null, long? LoanAccountId = null)
        {
            CommonResponse res = new CommonResponse();

            if (!CalculationDate.HasValue)
                CalculationDate = indianTime;

            //var date = CalculationDate.Value.ToString("dd-MM-yyyy");
            var date = CalculationDate.Value.ToString("yyyy-MM-dd");

            CalculationDate = Convert.ToDateTime(date);

            AccountTransactionManager objAccounttransManager = new AccountTransactionManager(_context, _loanNBFCFactory);

            var RefId = new SqlParameter("TransactionNo", TransactionNo == null ? DBNull.Value : TransactionNo);
            var LoanId = new SqlParameter("LoanAccountId", LoanAccountId == null ? DBNull.Value : LoanAccountId);
            var dateofcalculation = new SqlParameter("DateOfCalculation", CalculationDate.Value.ToString("yyyy-MM-dd"));
            var OverdueDelayPenaltyList = await _context.Database.SqlQueryRaw<OverDueDelayPenaltyRateDc>("exec DelayPenalityGetPerDayTransactions @TransactionNo,@LoanAccountId,@DateOfCalculation", RefId, LoanId, dateofcalculation).ToListAsync();
            if (OverdueDelayPenaltyList != null && OverdueDelayPenaltyList.Any())
            {
                var transactionTypes = _context.TransactionTypes.Where(x => x.IsActive && !x.IsDeleted).ToList();
                var Head = _context.TransactionDetailHeads.Where(x => x.IsActive && !x.IsDeleted).ToList();
                long? DelayPenaltyHeadId = Head.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.DelayPenalty)?.Id;
                long? PenaltyCharges_Id = transactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.PenaltyCharges)?.Id;
                var OrderPlacement_Id = transactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.OrderPlacement)?.Id;
                var transIds = OverdueDelayPenaltyList.Select(x => x.AccountTransactionId).Distinct().ToList();
                var AllTrans = await _context.AccountTransactions.Where(x => transIds.Contains(x.Id) && (x.TransactionTypeId == OrderPlacement_Id || x.TransactionTypeId == PenaltyCharges_Id) && x.IsActive && !x.IsDeleted).ToListAsync();
                var referenceids = AllTrans.Select(x => x.ReferenceId).Distinct().ToList();
                var AllPenaltyChargesTrans = await _context.AccountTransactions.Where(x => referenceids.Contains(x.ReferenceId) && x.TransactionTypeId == PenaltyCharges_Id && x.IsActive && !x.IsDeleted).ToListAsync();

                bool PayableBy = false;

                foreach (var item in OverdueDelayPenaltyList)
                {
                    var OrderPlacementTrans = AllTrans.Where(x => x.Id == item.AccountTransactionId && x.TransactionTypeId == OrderPlacement_Id && x.IsActive && !x.IsDeleted).ToList();

                    if (OrderPlacementTrans != null && OrderPlacementTrans.Any())
                    {
                        //string sPayableBY = OrderPlacementTrans.FirstOrDefault().PayableBy ?? "";
                        string sPayableBY = "Customer"; //As discussed with Deepak as on 15/03/2024.
                        if (sPayableBY == "Customer")
                        { PayableBy = true; }

                        string referenceid = OrderPlacementTrans.FirstOrDefault(x => x.Id == item.AccountTransactionId).ReferenceId;
                        var PenaltyChargesExist = AllPenaltyChargesTrans.Where(x => x.ReferenceId == referenceid && x.TransactionTypeId == PenaltyCharges_Id).FirstOrDefault();

                        var result = _loanNBFCFactory.GetService(item.NBFCIdentificationCode);
                        double perDayPenaltyRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(item.DelayPenaltyRate));

                        var perDayPenalty = OrderPlacementTrans.FirstOrDefault().InterestType == "Percentage" ? (item.PrincipleOutstanding * perDayPenaltyRatePerDay / 100.0) : perDayPenaltyRatePerDay;

                        double perDayPenaltyAmount = 0;

                        if (Math.Round(perDayPenalty, 2) > 0)
                        {
                            perDayPenaltyAmount = perDayPenalty;
                        }
                        else
                        {
                            perDayPenaltyAmount = 0;
                        }

                        //if (perDayPenaltyAmount > 0)
                        {
                            if (PenaltyChargesExist == null)
                            {
                                AccountTransaction AccountTransaction = new AccountTransaction()
                                {
                                    CustomerUniqueCode = OrderPlacementTrans.FirstOrDefault().CustomerUniqueCode ?? "",
                                    LoanAccountId = item.LoanAccountId,
                                    AnchorCompanyId = OrderPlacementTrans.FirstOrDefault().AnchorCompanyId,
                                    ReferenceId = OrderPlacementTrans.FirstOrDefault().ReferenceId ?? "",
                                    TransactionTypeId = Convert.ToInt64(PenaltyCharges_Id),
                                    TransactionAmount = OrderPlacementTrans.FirstOrDefault().TransactionAmount,
                                    ParentAccountTransactionsID = OrderPlacementTrans.FirstOrDefault().Id,
                                    OrderAmount = perDayPenaltyAmount,
                                    GSTAmount = 0,
                                    PaidAmount = perDayPenaltyAmount,
                                    ProcessingFeeType = "",
                                    ProcessingFeeRate = 0,
                                    GstRate = OrderPlacementTrans.FirstOrDefault().GstRate,
                                    InterestType = OrderPlacementTrans.FirstOrDefault().InterestType,
                                    InterestRate = OrderPlacementTrans.FirstOrDefault().InterestRate,
                                    CreditDays = OrderPlacementTrans.FirstOrDefault().CreditDays,
                                    BounceCharge = OrderPlacementTrans.FirstOrDefault().BounceCharge,
                                    DelayPenaltyRate = OrderPlacementTrans.FirstOrDefault().DelayPenaltyRate,
                                    PayableBy = sPayableBY,// OrderPlacementTrans.FirstOrDefault().PayableBy,
                                    CompanyProductId = OrderPlacementTrans.FirstOrDefault().CompanyProductId,
                                    TransactionStatusId = OrderPlacementTrans.FirstOrDefault().TransactionStatusId,
                                    AccountTransactionDetails = new List<AccountTransactionDetail>(),
                                    IsActive = true,
                                    IsDeleted = false,
                                    DueDate = OrderPlacementTrans.FirstOrDefault().DueDate,
                                    InvoiceDate = OrderPlacementTrans.FirstOrDefault().InvoiceDate,
                                    InvoiceId = OrderPlacementTrans.FirstOrDefault().InvoiceId,
                                    InvoiceNo = OrderPlacementTrans.FirstOrDefault().InvoiceNo,
                                    OrderDate = OrderPlacementTrans.FirstOrDefault().OrderDate,
                                    TransactionStatus = OrderPlacementTrans.FirstOrDefault().TransactionStatus,
                                    SettlementDate = OrderPlacementTrans.FirstOrDefault().SettlementDate,
                                    InterestAmountCalculated = OrderPlacementTrans.FirstOrDefault().InterestAmountCalculated,
                                    InvoicePdfURL = OrderPlacementTrans.FirstOrDefault().InvoicePdfURL,
                                    DisbursementDate = OrderPlacementTrans.FirstOrDefault().DisbursementDate
                                };



                                if (item.AccountTransactionId > 0)
                                {
                                    AccountTransactionDetail transactionDetail = new AccountTransactionDetail()
                                    {
                                        AccountTransactionId = AccountTransaction.Id,
                                        Amount = perDayPenaltyAmount,
                                        TransactionDetailHeadId = DelayPenaltyHeadId ?? 0,
                                        IsPayableBy = PayableBy,
                                        Status = "success",
                                        Created = indianTime,
                                        IsActive = true,
                                        IsDeleted = false,
                                        TransactionDate = CalculationDate
                                    };
                                    AccountTransaction.AccountTransactionDetails.Add(transactionDetail);
                                }
                                _context.AccountTransactions.Add(AccountTransaction);

                            }
                            else
                            {
                                var transactionDetail = _context.AccountTransactionDetails.FirstOrDefault(x => x.TransactionDetailHeadId == DelayPenaltyHeadId && x.AccountTransactionId == PenaltyChargesExist.Id && x.TransactionDate.Value.Date == CalculationDate.Value.Date && x.IsActive && !x.IsDeleted);
                                if (transactionDetail != null)
                                {
                                    transactionDetail.Amount = perDayPenaltyAmount;
                                    transactionDetail.LastModified = indianTime;
                                    _context.Entry(PenaltyChargesExist).State = EntityState.Modified;
                                }
                                else if (transactionDetail == null)
                                {
                                    PenaltyChargesExist.OrderAmount += perDayPenaltyAmount;
                                    PenaltyChargesExist.PaidAmount += perDayPenaltyAmount;
                                    PenaltyChargesExist.LastModified = indianTime;
                                    _context.Entry(PenaltyChargesExist).State = EntityState.Modified;

                                    //if (transactionDetail != null)
                                    //{
                                    //    transactionDetail.Amount = perDayPenaltyAmount;
                                    //    transactionDetail.LastModified = indianTime;
                                    //    _context.Entry(PenaltyChargesExist).State = EntityState.Modified;
                                    //}
                                    //else
                                    //{
                                    PenaltyChargesExist.AccountTransactionDetails = new List<AccountTransactionDetail>();

                                    AccountTransactionDetail Detail = new AccountTransactionDetail()
                                    {
                                        AccountTransactionId = PenaltyChargesExist.Id,
                                        Amount = perDayPenaltyAmount,
                                        TransactionDetailHeadId = DelayPenaltyHeadId ?? 0,
                                        IsPayableBy = PayableBy,
                                        Status = "success",
                                        Created = indianTime,
                                        IsActive = true,
                                        IsDeleted = false,
                                        TransactionDate = CalculationDate,
                                    };
                                    PenaltyChargesExist.AccountTransactionDetails.Add(Detail);
                                    _context.Entry(PenaltyChargesExist).State = EntityState.Modified;
                                    //}
                                }
                            }
                        }
                    }
                }
                if (_context.SaveChanges() > 0)
                {
                    res.status = true;
                    res.Message = "Success";
                }
                else
                {
                    res.status = false;
                    res.Message = "Failed";
                }
            }
            return res.status;
        }

        public async Task<bool> EMIPenaltyJob(DateTime? CalculationDate = null, string? TransactionNo = null, long? LoanAccountId = null)
        {
            CommonResponse res = new CommonResponse();
            if (!CalculationDate.HasValue)
                CalculationDate = indianTime;
            CalculationDate = Convert.ToDateTime(CalculationDate.Value.ToString("yyyy-MM-dd"));

            string sPayableBY = "Customer";
            var PayableBy = (sPayableBY == "Customer") ? true : false;

            var RefId = new SqlParameter("TransactionNo", TransactionNo == null ? DBNull.Value : TransactionNo);
            var LoanId = new SqlParameter("LoanAccountId", LoanAccountId == null ? DBNull.Value : LoanAccountId);
            var calculateDate = new SqlParameter("DateOfCalculation", CalculationDate.Value);

            var EMIDelayPenaltyList = await _context.Database.SqlQueryRaw<OverDueDelayPenaltyRateDc>("exec EMIPenaltyPerDayTransactions @TransactionNo,@LoanAccountId,@DateOfCalculation", RefId, LoanId, calculateDate).ToListAsync();
            if (EMIDelayPenaltyList != null && EMIDelayPenaltyList.Any())
            {
                var transIds = EMIDelayPenaltyList.Select(x => x.AccountTransactionId).Distinct().ToList();
                var EMIAccountTrans = await _context.AccountTransactions.Where(x => transIds.Contains(x.Id) && x.IsActive && !x.IsDeleted 
                && x.TransactionStatus.Code == TransactionStatuseConstants.Overdue && x.TransactionType.Code == TransactionTypesConstants.EMIAmount).ToListAsync();

                long? PenalHeadId = _context.TransactionDetailHeads.FirstOrDefault(x => x.Code == TransactionDetailHeadsConstants.EMIPenalAmount)?.Id;
                long? EMIPenalAmountId = _context.TransactionTypes.FirstOrDefault(x => x.Code == TransactionTypesConstants.EMIPenalAmount)?.Id;

                var referenceIds = EMIAccountTrans.Select(x => x.ReferenceId).Distinct().ToList();
                var AllPenaltyChargesTrans = await _context.AccountTransactions.Where(x => referenceIds.Contains(x.ReferenceId) && x.TransactionTypeId == EMIPenalAmountId && x.IsActive && !x.IsDeleted).ToListAsync();

                foreach (var item in EMIDelayPenaltyList)
                {
                    var transaction =   EMIAccountTrans.FirstOrDefault(x=>x.Id == item.AccountTransactionId);
                    if (transaction != null)
                    {
                        var result = _loanNBFCFactory.GetService(item.NBFCIdentificationCode);
                        double perDayPenaltyRatePerDay = await result.CalculatePerDayInterest(Convert.ToDouble(item.DelayPenaltyRate));

                        var perDayPenalty = item.PrincipleOutstanding * perDayPenaltyRatePerDay / 100.0 ;
                        double perDayPenaltyAmount = Math.Round(perDayPenalty, 2) > 0 ?  perDayPenalty :  0;
                  
                        var PenaltyChargesExist = AllPenaltyChargesTrans.Where(x => x.ReferenceId == transaction.ReferenceId && x.TransactionTypeId == EMIPenalAmountId).FirstOrDefault();
                        if (PenaltyChargesExist == null )
                        {
                            AccountTransaction AccountTransaction = new AccountTransaction()
                            {
                                CustomerUniqueCode = transaction.CustomerUniqueCode ?? "",
                                LoanAccountId = item.LoanAccountId,
                                AnchorCompanyId = transaction.AnchorCompanyId,
                                ReferenceId = transaction.ReferenceId ?? "",
                                TransactionTypeId = Convert.ToInt64(EMIPenalAmountId),
                                TransactionAmount = transaction.TransactionAmount,
                                ParentAccountTransactionsID = transaction.Id,
                                OrderAmount = perDayPenaltyAmount,
                                GSTAmount = 0,
                                PaidAmount = perDayPenaltyAmount,
                                ProcessingFeeType = "",
                                ProcessingFeeRate = 0,
                                GstRate = transaction.GstRate,
                                InterestType = transaction.InterestType,
                                InterestRate = transaction.InterestRate,
                                CreditDays = transaction.CreditDays,
                                BounceCharge = transaction.BounceCharge,
                                DelayPenaltyRate = transaction.DelayPenaltyRate,
                                PayableBy = sPayableBY,
                                CompanyProductId = transaction.CompanyProductId,
                                TransactionStatusId = transaction.TransactionStatusId,
                                AccountTransactionDetails = new List<AccountTransactionDetail>(),
                                IsActive = true,
                                IsDeleted = false,
                                DueDate = transaction.DueDate,
                                InvoiceDate = transaction.InvoiceDate,
                                InvoiceId = transaction.InvoiceId,
                                InvoiceNo = transaction.InvoiceNo,
                                OrderDate = transaction.OrderDate,
                                TransactionStatus = transaction.TransactionStatus,
                                SettlementDate = transaction.SettlementDate,
                                InterestAmountCalculated = transaction.InterestAmountCalculated,
                                InvoicePdfURL = transaction.InvoicePdfURL,
                                DisbursementDate = transaction.DisbursementDate
                            };

                            if (item.AccountTransactionId > 0)
                            {
                                AccountTransactionDetail transactionDetail = new AccountTransactionDetail()
                                {
                                    AccountTransactionId = AccountTransaction.Id,
                                    Amount = perDayPenaltyAmount,
                                    TransactionDetailHeadId = PenalHeadId ?? 0,
                                    IsPayableBy = PayableBy,
                                    Status = "success",
                                    Created = indianTime,
                                    IsActive = true,
                                    IsDeleted = false,
                                    TransactionDate = CalculationDate
                                };
                                AccountTransaction.AccountTransactionDetails.Add(transactionDetail);
                            }
                            _context.AccountTransactions.Add(AccountTransaction);

                        }
                        else
                        {
                            var transactionDetail = _context.AccountTransactionDetails.FirstOrDefault(x => x.TransactionDetailHeadId == PenalHeadId && x.AccountTransactionId == PenaltyChargesExist.Id && x.TransactionDate!=null && x.TransactionDate.Value.Date == CalculationDate.Value.Date && x.IsActive && !x.IsDeleted);
                            if (transactionDetail != null)
                            {
                                transactionDetail.Amount = perDayPenaltyAmount;
                                transactionDetail.LastModified = indianTime;
                                _context.Entry(PenaltyChargesExist).State = EntityState.Modified;
                            }
                            else 
                            {
                                PenaltyChargesExist.OrderAmount += perDayPenaltyAmount;
                                PenaltyChargesExist.PaidAmount += perDayPenaltyAmount;
                                PenaltyChargesExist.LastModified = indianTime;
                                _context.Entry(PenaltyChargesExist).State = EntityState.Modified;

                                PenaltyChargesExist.AccountTransactionDetails = new List<AccountTransactionDetail>();

                                AccountTransactionDetail Detail = new AccountTransactionDetail()
                                {
                                    AccountTransactionId = PenaltyChargesExist.Id,
                                    Amount = perDayPenaltyAmount,
                                    TransactionDetailHeadId = PenalHeadId ?? 0,
                                    IsPayableBy = PayableBy,
                                    Status = "success",
                                    Created = indianTime,
                                    IsActive = true,
                                    IsDeleted = false,
                                    TransactionDate = CalculationDate,
                                };
                                PenaltyChargesExist.AccountTransactionDetails.Add(Detail);
                                _context.Entry(PenaltyChargesExist).State = EntityState.Modified;
                            }
                        }
                    }
                }
                if (_context.SaveChanges() > 0)
                {
                    res.status = true;
                    res.Message = "Success";
                }
                else
                {
                    res.status = false;
                    res.Message = "Failed";
                }
            }
            return res.status;
        }
    }
}
