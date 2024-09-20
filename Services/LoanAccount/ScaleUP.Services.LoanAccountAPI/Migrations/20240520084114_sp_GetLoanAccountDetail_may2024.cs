using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class sp_GetLoanAccountDetail_may2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
	CREATE OR ALTER   proc [dbo].[GetLoanAccountDetail] 
	--Declare
	   @LoanAccountId bigint =8
	as
	begin
		Select l.AccountCode,l.MobileNo,l.CustomerName,l.CityName,l.ProductType
		       , isnull(l.ShopName,'') ShopName,isnull(l.CustomerImage,'') LoanImage,l.IsAccountActive,l.IsBlock,l.IsBlockComment, l.ThirdPartyLoanCode
			  ,l.NBFCIdentificationCode,l.IsDefaultNBFC
			  --,b.CreditLimitAmount
			  ,b.DisbursalAmount CreditLimitAmount
			  ,b.DisbursalAmount TotalSanctionedAmount
			  ,y.UtilizedAmount
			  ,y.LTDUtilizedAmount
			   ,(round(PrincipleOutstanding,2,2)+round(InterestOutstanding,2,2)+round(PenalOutStanding,2,2)+round(OverdueInterestOutStanding,2,2)) TotalOutStanding
			   ,round(PrincipleOutstanding,2,2) PrincipleOutstanding
			   ,round(InterestOutstanding,2,2) InterestOutstanding
			   ,round(PenalOutStanding,2,2) PenalOutStanding
			   ,round(OverdueInterestOutStanding,2,2) OverdueInterestOutStanding
			   ,abs(round(TotalRepayment,2))TotalRepayment
			   ,abs(round(PrincipalRepaymentAmount,2,2))PrincipalRepaymentAmount
			   ,abs(round(InterestRepaymentAmount,2))InterestRepaymentAmount
			   ,abs(round(OverdueInterestPaymentAmount,2,2))OverdueInterestPaymentAmount
			   ,abs(round(PenalRePaymentAmount,2,2))PenalRePaymentAmount
			   ,abs(round(BounceRePaymentAmount,2,2))BounceRePaymentAmount
			   ,abs(round(ExtraPaymentAmount,2,2))ExtraPaymentAmount
			   ,abs(round(PenalAmount,2,2)) PenalAmount
			   ,abs(round(ProcessingFee,2,2)) ProcessingFee
			   --,tans.*
		from LoanAccounts l	
		inner join LoanAccountCredits b on l.id=b.LoanAccountId and b.IsActive=1 and b.IsDeleted=0
		outer apply
		(
			select	
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizedAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit,
				Sum(case when h.Code in ('Refund','Order') and s.Code not in ('Failed','Canceled')  then d.Amount else 0 end) as LTDUtilizedAmount
				,Sum(case when h.Code in ('Order','Payment','Refund') and s.Code in ('Pending','Due','Overdue','Delinquent')  then d.Amount else 0 end) as PrincipleOutstanding
				,Sum(case when h.Code in ('Interest','InterestPaymentAmount') and s.Code in ('Pending','Due','Overdue','Delinquent')  and cast(TransactionDate as date)<=cast(GETDATE() as date) then d.Amount else 0 end) as InterestOutstanding
				,Sum(case when h.Code in ('DelayPenalty','PenalPaymentAmount') and s.Code in ('Pending','Due','Overdue','Delinquent')  then d.Amount else 0 end) as PenalOutStanding
				,Sum(case when h.Code in ('OverdueInterestAmount','OverduePaymentAmount') and s.Code in ('Pending','Due','Overdue','Delinquent')  then d.Amount else 0 end) as OverdueInterestOutStanding
				,Sum(case when h.Code in ('ExtraPaymentAmount'
										,'OverduePaymentAmount'
										,'PenalPaymentAmount'
										,'InterestPaymentAmount'
										,'BouncePaymentAmount'
										,'Payment'
										,'Refund') 
						then d.Amount else 0 end) as TotalRepayment
				,Sum(case when h.Code in ('Payment') then d.Amount else 0 end) as PrincipalRepaymentAmount
				,Sum(case when h.Code in ('InterestPaymentAmount')  then d.Amount else 0 end) as InterestRepaymentAmount
				,Sum(case when h.Code in ('OverduePaymentAmount')  then d.Amount else 0 end) as OverdueInterestPaymentAmount
				,Sum(case when h.Code in ('PenalPaymentAmount')  then d.Amount else 0 end) as PenalRePaymentAmount
				,Sum(case when h.Code in ('BouncePaymentAmount')  then d.Amount else 0 end) as BounceRePaymentAmount
				,Sum(case when h.Code in ('ExtraPaymentAmount')  then d.Amount else 0 end) as ExtraPaymentAmount
				,Sum(case when h.Code in ('DelayPenalty')   then d.Amount else 0 end) as PenalAmount
				,Sum(case when h.Code in ('ProcessingFee','Gst')   then d.Amount else 0 end) as ProcessingFee
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id and h.IsActive=1 and h.IsDeleted=0
		) y	
	  where  l.id=@LoanAccountId
	end";
			migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
