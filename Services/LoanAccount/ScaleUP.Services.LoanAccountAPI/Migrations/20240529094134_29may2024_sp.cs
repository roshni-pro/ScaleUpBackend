using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _29may2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

	CREATE OR ALTER Proc [dbo].[GetLoanAccountListExport]
	--declare 
	@ProductType varchar(200)='',
	@Status int=-1,
	@FromDate datetime='2024-01-31',
	@ToDate datetime='2024-05-29',
	@CityName  varchar(200)='',
	@Keyward varchar(200)='',
	@Min int=0,
	@Max int=0,
	@Skip int=0,
	@Take int=10,
	@AnchorId bigint=0
	 As
	 Begin
		DECLARE @StartDate date,
				@EndDate date
		IF((@FromDate is null or @FromDate='') and (@ToDate is null or @ToDate=''))
			Begin
				SELECT @StartDate = MIN(c.Created),  @EndDate = max(c.Created) FROM LoanAccounts c with(nolock) where c.IsActive=1 and c.IsDeleted=0
			End
		IF(@Max=0)
			Begin
				select @Max =max(c.DisbursalAmount)
				from LoanAccounts l with(nolock)
				inner join LoanAccountCredits c with(nolock) on l.Id=c.LoanAccountId  and l.IsActive=1 and l.IsDeleted=0
			End
		select 
			l.Id LoanAccountId,
			case when l.IsAccountActive=1 and l.IsBlock=0 then 'Active' 
				  when l.IsAccountActive=0 and l.IsBlock=0 then 'InActive'
				  when l.IsBlock=1 then 'Block'
			end AccountStatus,
			LeadId, 
			ProductId,
			l.ProductType,
			UserId, 
			CustomerName, 
			l.AccountCode LeadCode, 
			MobileNo,	
			NBFCCompanyId,	
			AnchorCompanyId,
			AgreementRenewalDate,	
			CreditScore,	
			ApplicationDate,	
			DisbursalDate,	
			IsDefaultNBFC,	
			isnull(CityName,'') CityName,
			isnull(AnchorName,'')  AnchorName,
			--c.CreditLimitAmount AvailableCreditLimit,
			case when c.CreditLimitAmount>Intransitlimit then c.CreditLimitAmount-Intransitlimit else 0 end AvailableCreditLimit,
			c.DisbursalAmount,
			--isNull(x.UtilizeAmount,0) UtilizeAmount,
			isNull((x.UtilizeAmount),0) UtilizeAmount,
		    isNull(case when c.DisbursalAmount>0 then	((x.UtilizeAmount)/c.DisbursalAmount)*100 else 0 end,0) as UtilizePercent,
			l.IsAccountActive status,
			dense_rank() over (order by l.id) + dense_rank() over (order by l.id desc) -1 TotalCount
			,sum(case when l.IsAccountActive=1 then ISNULL(c.CreditLimitAmount,0)  else 0 end) over() as TotalActive,
			sum(case when l.IsAccountActive=0 then ISNULL(c.CreditLimitAmount,0) else 0 end) over() as TotalInActive,
			sum(ISNULL(c.CreditLimitAmount,0)) over() as TotalDisbursal,
			l.IsBlock,l.NBFCIdentificationCode
			, cast(0 as bigint) ParentID
		from LoanAccounts l with(nolock)
		inner join LoanAccountCredits c with(nolock) on l.Id=c.LoanAccountId
		outer apply (
				select 
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizeAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id
		) x
		--select * from TransactionStatuses
			where ((l.CustomerName like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.MobileNo like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.AccountCode like '%'+@Keyward+'%' or ISNULL(@Keyward,'')='')
			  or (l.LeadCode like '%'+@Keyward+'%' or ISNULL(@Keyward,'')=''))
		      and (l.CityName=@CityName or Isnull(@CityName,'')='')
		      and (l.AnchorCompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
			  and (l.ProductType=@ProductType or Isnull(@ProductType,'')='')
			  and CAST(l.DisbursalDate as date)>=@FromDate and CAST(l.DisbursalDate as date)<=@ToDate
			  and l.IsActive=1 and l.IsDeleted=0
			  and (@Status=-1 or (@Status<>2 and (l.IsAccountActive =@Status and l.IsBlock=0)) or (@Status=2 and (l.IsAccountActive=0 and l.IsBlock=1)))
		order by l.LastModified 
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
