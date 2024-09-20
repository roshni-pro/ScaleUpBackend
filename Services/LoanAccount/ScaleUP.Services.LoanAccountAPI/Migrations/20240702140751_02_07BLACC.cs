using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _02_07BLACC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"


CREATE OR ALTER     procedure [dbo].[GetBusinessLoanAccountList]
	--declare
		@ProductType varchar(50)='BusinessLoan',
		@Status int=-1,
		@FromDate datetime='2024-01-01',
		@ToDate datetime='2024-05-30',
		@CityName  varchar(200)='',
		@Keyword varchar(200)='',
		--@Min int=0,
		--@Max int=0,
		@Skip int=0,
		@Take int=100,
		@AnchorId dbo.Intvalues readonly,
		@leadIds dbo.Intvalues readonly,
		@isDSA bit =0
	As
	Begin
	SET @skip= iif(@Skip>0, @Skip-1, @Skip) * @Take;
	select 
	la.LeadId as LeadId
	,la.ProductId
	,la.UserId as UserId
	,la.CustomerName as CustomerName
	,la.LeadCode
	,la.MobileNo
	,la.NBFCCompanyId as NBFCCompanyId
	,la.AnchorCompanyId as AnchorCompanyId
	,la.CreditScore
	,la.Id as LoanAccountId 
	,la.AnchorName
	,bl.loan_app_id as Loan_app_id
	,bl.partner_loan_app_id as Partner_loan_app_id
	,la.ShopName as business_name
	,case when la.IsAccountActive=1 and la.IsBlock=0 then 'Active' 
				  when la.IsAccountActive=0 and la.IsBlock=0 then 'InActive'
				  when la.IsBlock=1 then 'Block'
			end AccountStatus
	,bl.sanction_amount as loan_amount
	,bl.bureau_outstanding_loan_amt  as OutStandingAmount
	,la.CityName as CityName
	,COUNT(*) over() as TotalCount
	--,cast(0 as float) as TotalActive
	--,cast(0 as float) as TotalInActive
	--,cast(0 as float) as TotalDisbursal
	,cast(0 as bit) as IsBlock
	,CAST(0 as bit) as Status
	,la.DisbursalDate as DisbursalDate --must be disbursal date
	--,CAST(0 as int) CityId
	from 
	LoanAccounts LA with(nolock)
	join BusinessLoans bl with(nolock) on bl.LeadMasterId = LA.LeadId and la.IsActive=1 and la.IsDeleted=0 and bl.IsActive=1 and bl.IsDeleted=0
		join @AnchorId a on a.IntValue = la.AnchorCompanyId
	and ((@isDSA=1 and exists(select 1 from @leadIds c where LA.LeadId = c.intValue)) or 
	(@isDSA=0 and (exists(select 1 from @leadIds c where LA.LeadId<> c.intValue) or  not exists(select 1 from @leadIds)) ) )
	where 
	((la.CustomerName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (la.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (la.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  --and (la.AnchorCompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and CAST(la.Created as date)>=@FromDate and CAST(la.Created as date)<=@ToDate
				  and (la.CityName=@CityName or Isnull(@CityName,'')='')
				  and (la.ProductType=@ProductType or Isnull(@ProductType,'')='')
				  --and (@Status=-1 or (@Status<>2 and (l.IsAccountActive =@Status and l.IsBlock=0)) or (@Status=2 and (l.IsAccountActive=0 and l.IsBlock=1))
	order by la.LastModified 
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
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
