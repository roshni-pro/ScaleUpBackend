using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class sp_GetDisbursementDashboardData_12Aug2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE or ALTER proc [dbo].[GetDisbursementDashboardData] 
	--Declare
	   	@ProductType varchar(200) ='CreditLine',
		@CityName dbo.stringValues readonly,        
		@AnchorId dbo.Intvalues readonly,
		@FromDate datetime ='2024-01-01',        
		@ToDate datetime ='2024-08-06'	
	as
	begin
		--insert into @AnchorId values(2)
		--insert into @CityName values('')
	select 
		 Sum(case when hcode in ('Refund','Order','Payment') and sCode not in ('Paid','Canceled','Failed') and tcode not in('%scaleup%')  then z.Amount else 0 end) as UtilizedAmount
		,Sum(case when hCode in ('DisbursementAmount') and sCode not in ('Canceled','Failed') and tcode not in('%scaleup%')  then z.Amount else 0 end) as DisbursementAmount
		,Sum(case when sCode in ('Overdue') and tcode not in('%scaleup%')  then z.Amount else 0 end) as OverDueAmount
		,Sum(case when hCode in ('Order','Payment','Refund') and sCode in ('Pending','Due','Overdue','Delinquent') and tcode not in('%scaleup%')  then z.Amount else 0 end) --as PrincipleOutstanding
		+ Sum(case when hCode in ('Interest','InterestPaymentAmount') and sCode in ('Pending','Due','Overdue','Delinquent') and cast(Created as date)<=cast(GETDATE() as date) and tcode not in('%scaleup%') then z.Amount else 0 end) --as InterestOutstanding
		+ Sum(case when hCode in ('DelayPenalty','PenalPaymentAmount') and sCode in ('Pending','Due','Overdue','Delinquent') and tcode not in('%scaleup%')  then z.Amount else 0 end) --as PenalOutStanding
		+ Sum(case when hCode in ('OverdueInterestAmount','OverduePaymentAmount') and sCode in ('Pending','Due','Overdue','Delinquent')  and tcode not in('%scaleup%') then z.Amount else 0 end)  OutStanding--as OverdueInterestOutStanding
		,cast(min(z.Created) as date) as Created
		,sum(case when tCode = 'ScaleupShareAmount' then z.Amount else 0 end) as ScaleupShareAmount
	from 
		(	
			select	
			d.id,
			t.Code as tcode,
			d.Amount,
			h.Code as hcode,
			s.Code as scode,
			cast(d.TransactionDate as date) as Created
				from AccountTransactions a with(nolock)
				inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.id
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				inner join @CityName c on l.CityName= (case when c.stringValue='' or c.stringValue=null then l.CityName else c.stringValue end)
			where l.ProductType = @ProductType 
			and h.IsActive=1 and h.IsDeleted=0 and d.IsActive=1 and d.IsDeleted=0 
			and a.IsActive=1 and a.IsDeleted=0 and t.IsActive=1 and t.IsDeleted=0  
			--and (( exists(select 1 from @CityName c where l.CityName = c.stringValue)) or ( not exists(select 1 from @CityName)))
			and (( exists(select 1 from @AnchorId  c where l.AnchorCompanyId = c.IntValue)) or ( not exists(select 1 from @AnchorId )))
			and (cast(d.TransactionDate as date) between cast(@FromDate as date) and  cast(@ToDate as date))
		)z 
	group by cast(z.Created as date)

end

            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
