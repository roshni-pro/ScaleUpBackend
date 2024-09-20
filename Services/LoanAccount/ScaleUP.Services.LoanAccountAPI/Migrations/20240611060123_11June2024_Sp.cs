using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _11June2024_Sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

	CREATE OR ALTER Proc [dbo].[TransactionDetailByInvoiceId]
			--declare
	@InvoiceId bigint=47,
	@Head varchar(50)='Payment'
	As 
	Begin
		IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
			DROP TABLE #tempTransaction 
		SELECT ID INTO #tempTransaction FROM AccountTransactions with(nolock)
		where InvoiceId = @InvoiceId and IsActive =1 and IsDeleted =0
		select s.Code, 
		--d.PaymentReqNo as ReferenceId, 
		case when @Head='Payment' or @Head='InterestPaymentAmount' or @Head='PenalPaymentAmount' then d.PaymentReqNo else a.ReferenceId end ReferenceId
		,h.Code Head, Round(d.Amount,3) Amount
		--,case when d.TransactionDate IS NOT null then d.TransactionDate else d.Created end TransactionDate
		,case when @Head='Order' OR @Head='Refund'  then a.DisbursementDate
		when d.TransactionDate IS NOT null then d.TransactionDate else d.Created end TransactionDate
		from AccountTransactions a with(nolock) 
		inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
		inner join AccountTransactionDetails d with(nolock) on a.Id=d.AccountTransactionId
		inner join TransactionDetailHeads h with(nolock) on d.TransactionDetailHeadId=h.Id
		where h.Code=@Head and s.Code not in ('Failed', 'Canceled', 'Initiate')
		and (a.Id in  (select Id from #tempTransaction) OR a.ParentAccountTransactionsID in  (select Id from #tempTransaction) )
		and d.IsActive=1 and d.IsDeleted=0 and Amount!=0
		and (d.TransactionDate IS NULL OR cast(d.TransactionDate as date) <= cast(getdate() as date))
		order by d.TransactionDate desc
	End

GO
	CREATE OR ALTER   Proc [dbo].[GetLoanAccountDetailByTxnId]
	--declare
	@TransactionReqNo varchar(50)=''
	As 
	Begin
		declare @LoanId bigint=0
	select top 1 @LoanId=LoanAccountId from PaymentRequest with(nolock) where TransactionReqNo=@TransactionReqNo
	select l.Id LoanAccountId, l.LeadId,  l.ProductId,p.AnchorCompanyId,l.MobileNo,ac.CreditLimitAmount,l.NBFCCompanyId
	,isnull((x.UtilizeAmount),0) UtilizateLimit,l.AnchorName,p.TransactionAmount InvoiceAmount,l.NBFCIdentificationCode
	,p.OrderNo,l.CustomerName, isnull(l.CustomerImage,'') ImageUrl, cast(0 as bigint) creditday
	,case when Overdue.Status is not null then Overdue.Status else '' end TransactionStatus
	,l.IsAccountActive,l.IsBlock,l.IsBlockComment
	from PaymentRequest p with(nolock)
	inner join LoanAccounts l with(nolock)  on l.Id=p.LoanAccountId
	inner join LoanAccountCredits ac with(nolock) on ac.LoanAccountId=l.Id
	outer apply (
			select 
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizeAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id and h.IsActive=1 and h.IsDeleted=0
		) x
	outer apply(
			select top 1 s.code Status 
			from AccountTransactions a with(nolock) 
			inner join TransactionStatuses s with(nolock) on a.TransactionStatusId=s.Id
			where LoanAccountId=@LoanId and s.Code='Overdue' and a.IsActive=1 and a.IsDeleted=0
		) Overdue
		where p.TransactionReqNo=@TransactionReqNo
	End

GO

	CREATE OR ALTER proc [dbo].[GetLoanAccountDetail] 
	--Declare
	   @LoanAccountId bigint =13
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
										) 
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
				and a.IsActive=1 and a.IsDeleted=0
		) y	
	  where  l.id=@LoanAccountId
	end

GO


CREATE OR ALTER     proc [dbo].[GetLoanAccountDashboardDetails] 
	--Declare
	   	@ProductType varchar(200) ='CreditLine',
		@CityName dbo.stringValues readonly,        
		@AnchorId dbo.Intvalues readonly,
		@FromDate datetime ='2023-12-01',        
		@ToDate datetime ='2024-5-17'	
	as
	begin
		--insert into @cityName values('Indore'),('Akodiya')
		--,('Deepdi'),('Deoguradia'),('Shajapur'),('Burhanpur'),('Gadhwa Kalan')
		--insert into @AnchorId values(2)
		--insert into @AnchorId values(14)
		Select 
			  b.DisbursalAmount CreditLimitAmount
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
			 , l.Created
		from LoanAccounts l	
		inner join LoanAccountCredits b on l.id=b.LoanAccountId and b.IsActive=1 and b.IsDeleted=0
			and ((exists(select 1 from @CityName) and exists(select 1 from @CityName c where l.CityName = c.stringValue)) or 
		  ( not exists(select 1 from @CityName) and b.Id=b.Id))
		and ((exists(select 1 from @AnchorId ) and exists(select 1 from @AnchorId  c where l.AnchorCompanyId = c.IntValue)) or 
		  ( not exists(select 1 from @AnchorId ) and b.Id=b.Id))
	    and  l.ProductType = @ProductType 
		and (cast(l.Created as date) between cast(@FromDate as date) and  cast(@ToDate as date))
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
										) 
						then d.Amount else 0 end) as TotalRepayment
				,Sum(case when h.Code in ('Payment') then d.Amount else 0 end) as PrincipalRepaymentAmount
				,Sum(case when h.Code in ('InterestPaymentAmount')  then d.Amount else 0 end) as InterestRepaymentAmount
				,Sum(case when h.Code in ('OverduePaymentAmount')  then d.Amount else 0 end) as OverdueInterestPaymentAmount
				,Sum(case when h.Code in ('PenalPaymentAmount')  then d.Amount else 0 end) as PenalRePaymentAmount
				,Sum(case when h.Code in ('BouncePaymentAmount')  then d.Amount else 0 end) as BounceRePaymentAmount
				,Sum(case when h.Code in ('ExtraPaymentAmount')  then d.Amount else 0 end) as ExtraPaymentAmount
				,Sum(case when h.Code in ('DelayPenalty')   then d.Amount else 0 end) as PenalAmount
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id and h.IsActive=1 and h.IsDeleted=0
				and a.IsActive=1 and a.IsDeleted=0
		) y	
	end

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
