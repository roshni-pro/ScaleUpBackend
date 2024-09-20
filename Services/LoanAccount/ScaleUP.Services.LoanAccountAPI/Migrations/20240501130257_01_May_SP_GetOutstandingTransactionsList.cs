using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _01_May_SP_GetOutstandingTransactionsList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"



CREATE OR ALTER Procedure [dbo].[GetOutstandingTransactionsList]
--Declare
@TransactionNo varchar(20) = NULL --= 'HQDUD33EYW0Q3RP4'
, @LoanAccountId bigint =41 --= 15
, @InvoiceDate datetime ='2024-03-06'
As 
Begin
	Declare @TransactionStatusesID_Overdue bigint
		Select @TransactionStatusesID_Overdue=Id From TransactionStatuses with(nolock)  Where Code='Overdue'
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 
	select    ats.Id
			, ats.ReferenceId As transactionReqNo
			, ats.DelayPenaltyRate
			, ats.GstRate
			, ats.PayableBy
			, IsNull(ats.InvoiceNo,'') As InvoiceNo
			, Isnull(b.WithdrawalId,0) as WithdrawlId
			, ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0),2) PricipleAmount  
			--, ISNULL(SUM(CASE WHEN h.Code = 'Interest' THEN ad.Amount ELSE 0 END), 0) InterestAmount  
			, ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Interest' And (Cast(ad.TransactionDate as Date)<= Cast(@InvoiceDate as Date) or @InvoiceDate is null) THEN ad.Amount ELSE 0 END), 0),2) InterestAmount  
			--, 0 as PenaltyChargesAmount
			,ats.InvoiceId 
			,ats.DisbursementDate
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join TransactionStatuses ts  with(nolock) on ats.TransactionStatusId = ts.Id
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	Left Outer Join  BlackSoilAccountTransactions b with(nolock) ON b.LoanInvoiceId= ats.InvoiceId
		--and (h.Code = 'Order' OR h.Code = 'Refund') 
	where  t.Code = 'OrderPlacement'
	and ts.IsActive =1 and ts.IsDeleted =0
	and (ts.Code = 'Pending' OR ts.Code = 'Due' OR ts.Code = 'Overdue'  OR ts.Code = 'Delinquent')
	And (ats.ReferenceId=@TransactionNo or ISNULL(@TransactionNo,'')='')
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
	and ats.DisbursementDate IS NOT NULL
	-- And ats.TransactionStatusId = @TransactionStatusesID_Overdue  and (cast(DATEADD(day, 1, ats.DueDate) as Date) <= Cast(GETDATE() as Date) or ats.DueDate is null)
	And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	group by ats.Id, ats.ReferenceId, ats.DelayPenaltyRate, ats.GstRate ,ats.PayableBy, ats.InvoiceNo,b.WithdrawalId,ats.InvoiceId, ats.DisbursementDate 
	select    tmp.*
			, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
			, abs(ISNULL(x.ExtraPaymentAmount, 0)) ExtraPaymentAmount
			, abs(ISNULL(x.InterestPaymentAmount, 0)) InterestPaymentAmount
			, abs(ISNULL(x.BouncePaymentAmount, 0)) BouncePaymentAmount
			, abs(ISNULL(x.PenalPaymentAmount, 0)) PenalPaymentAmount
			, abs(ISNULL(x.OverduePaymentAmount, 0)) OverduePaymentAmount
			, abs(ISNULL(tmp.PricipleAmount, 0) + ISNULL( x.PaymentAmount, 0)) as Outstanding
			, abs(ISNULL(pats.Id, 0)) PaneltyTxnId 
			, ROUND(IsNull(patsAmt.PenaltyChargesAmount,0),2)  as PenaltyChargesAmount
			, ROUND(IsNull(ovrAmt.OverdueCharges,0) , 2) as OverdueInterestAmount			
			, (tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30) as DelayPenalityAmount
			, ((tmp.PricipleAmount + IsNull(x.PaymentAmount,0)) * ((tmp.DelayPenaltyRate/100)/30)) * (tmp.GstRate/100) as DelayPenalityGstAmount			
	from #tempTransaction tmp 
	outer apply 
	(
		select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
				,SUM(CASE WHEN h.Code = 'InterestPaymentAmount' THEN ad.Amount ELSE 0 END ) InterestPaymentAmount
				,SUM(CASE WHEN h.Code = 'ExtraPaymentAmount' THEN ad.Amount ELSE 0 END ) ExtraPaymentAmount
				,SUM(CASE WHEN h.Code = 'BouncePaymentAmount' THEN ad.Amount ELSE 0 END ) BouncePaymentAmount
				,SUM(CASE WHEN h.Code = 'PenalPaymentAmount' THEN ad.Amount ELSE 0 END ) PenalPaymentAmount
				,SUM(CASE WHEN h.Code = 'OverduePaymentAmount' THEN ad.Amount ELSE 0 END ) OverduePaymentAmount
		from  AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.Id = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment'
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)x
	outer apply(	
		select pats.Id from   
		AccountTransactions pats  with(nolock) 
		left join TransactionTypes pt  with(nolock)  on pats.TransactionTypeId = pt.Id
		where  pt.Code = 'PenaltyCharges' and tmp.Id = pats.ParentAccountTransactionsID
	)pats
	outer apply(
		select IsNull(sum(ad.Amount),0) PenaltyChargesAmount from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
		where tmp.Id = pats.ParentAccountTransactionsID  and t.Code ='PenaltyCharges' and ph.Code = 'DelayPenalty' 
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)patsAmt
	outer apply(
		select IsNull(sum(ad.Amount),0) OverdueCharges from   
		AccountTransactions pats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on pats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on pats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads ph  with(nolock) on ad.TransactionDetailHeadId  = ph.Id 
		where tmp.Id = pats.ParentAccountTransactionsID and  t.Code = 'OverdueInterest' and ph.Code = 'OverdueInterestAmount'
		And pats.IsActive=1 And pats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)ovrAmt
	order by tmp.DisbursementDate 
End

GO


";

			migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
