using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _04Sep2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   Procedure [dbo].[GetOverDueTransactionsList]
--Declare
@TransactionNo varchar(20) ='2024673' --= 'HQDUD33EYW0Q3RP4'
, @LoanAccountId bigint =NULL --= 15
, @DateOfCalculation datetime= '2024-09-04' --='2024-02-18'
As 
Begin
	Declare @TransactionStatusesID_Overdue bigint
		Select @TransactionStatusesID_Overdue=Id From TransactionStatuses with(nolock)  Where Code='Overdue'
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 
	select    ats.Id AccountTransactionId
			, ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN ad.Amount ELSE 0 END), 0) PricipleAmount  
			,ats.InterestRate ,l.NBFCIdentificationCode,ats.LoanAccountId
	into #tempTransaction 
	from AccountTransactions ats  with(nolock) 
	inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id
	inner join TransactionStatuses ts with(nolock) on ats.TransactionStatusId=ts.Id
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
	where  t.Code = 'OrderPlacement'
	And (ats.ReferenceId=@TransactionNo or ISNULL(@TransactionNo,'')='')
	And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
	-- And ats.TransactionStatusId = @TransactionStatusesID_Overdue  and (cast(DATEADD(day, 1, ats.DueDate) as Date) <= Cast(GETDATE() as Date) or ats.DueDate is null)
	And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	and CAST(ats.DueDate as date) < CAST(@DateOfCalculation as DAte)
	and ts.Code in ('Pending','Due','Overdue','Delinquent') and ats.DisbursementDate is not null
	group by ats.Id,ats.InterestRate,l.NBFCIdentificationCode,ats.LoanAccountId
	select    tmp.*
			, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
			, tmp.PricipleAmount - abs(ISNULL(x.PaymentAmount, 0)) PrincipleOutstanding
	from #tempTransaction tmp 
	outer apply 
	(
		select   SUM(CASE WHEN h.Code = 'Payment' THEN ad.Amount ELSE 0 END ) PaymentAmount 
		from  AccountTransactions ats  with(nolock) 
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
			--and (h.Code = 'Payment' ) 
		where tmp.AccountTransactionId = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment' and cast(ad.TransactionDate as date) < cast(@DateOfCalculation as date)
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
	)x
End
GO


	CREATE OR ALTER proc [dbo].[DelayPenalityGetPerDayTransactions]
	--Declare
	  @TransactionNo varchar(100) ='2024673',
	  @LoanAccountId bigint =null,
	  @DateOfCalculation datetime= '2024-09-04' 
	As 
	Begin
		Declare @TransactionStatusesID_Overdue bigint
		Select @TransactionStatusesID_Overdue=Id From TransactionStatuses with(nolock)  Where Code='Overdue'
		IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
			DROP TABLE #tempTransaction 
		select  ats.Id AccountTransactionId
				, ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' OR h.Code='Interest' THEN ad.Amount ELSE 0 END), 0) PricipleAmount  
				--, ISNULL(SUM(CASE WHEN h.Code='Interest' OR h.Code='OverdueInterestAmount' 
				--			THEN ad.Amount ELSE 0 END), 0) PricipleAmount2  
				,ats.DelayPenaltyRate ,l.NBFCIdentificationCode,ats.LoanAccountId
		into #tempTransaction 
		from AccountTransactions ats  with(nolock) 
		inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id
		inner join TransactionStatuses ts with(nolock) on ats.TransactionStatusId=ts.Id
		inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
		inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
		inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
		where  t.Code = 'OrderPlacement'
		And (ats.ReferenceId=@TransactionNo or ISNULL(@TransactionNo,'')='')
		And (ats.LoanAccountId=@LoanAccountId  or ISNULL(@LoanAccountId,0)=0)
		-- And ats.TransactionStatusId = @TransactionStatusesID_Overdue  and (cast(DATEADD(day, 1, ats.DueDate) as Date) <= Cast(GETDATE() as Date) or ats.DueDate is null)
		And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		and CAST(ats.DueDate as date) < CAST(@DateOfCalculation as DAte)
		and ts.Code in ('Pending','Due','Overdue','Delinquent') and ats.DisbursementDate is not null
		group by ats.Id,ats.DelayPenaltyRate,l.NBFCIdentificationCode,ats.LoanAccountId
		select    tmp.*
				, abs(ISNULL(x.PaymentAmount, 0)) PaymentAmount
				, tmp.PricipleAmount - abs(ISNULL(x.PaymentAmount, 0)) PrincipleOutstanding
		from #tempTransaction tmp 
		outer apply 
		(
			select   SUM(CASE WHEN h.Code = 'Payment' or h.Code='InterestPaymentAmount' THEN ad.Amount ELSE 0 END ) PaymentAmount 
			from  AccountTransactions ats  with(nolock) 
			inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
			inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
			inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
				--and (h.Code = 'Payment' ) 
			where tmp.AccountTransactionId = ats.ParentAccountTransactionsID and t.Code = 'OrderPayment' and cast(ad.TransactionDate as date) < cast(@DateOfCalculation as date)
			And ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0
		)x
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
