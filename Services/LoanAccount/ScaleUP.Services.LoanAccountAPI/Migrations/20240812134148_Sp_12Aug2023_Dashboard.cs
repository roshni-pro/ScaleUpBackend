using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class Sp_12Aug2023_Dashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

	Create or ALTER Procedure [dbo].[GetRetentionPercentage]
	As
	Begin
	declare @previousMonthFromDate datetime,
			@previousMonthToDate datetime,
			@currentMonthFromDate datetime,
			@currentMonthToDate datetime,
			@PreviousmonthCount int,
			@CurrentMonthCount int,
			@MOMOrderRepeatedCustomer int,
			@PreviousQuarterStartDate datetime,
			@PreviousQuarterEndDate	  datetime,
			@CurrentQuarterStartDate  datetime,
			@CurrentQuarterEndDate  datetime,
			@PreviousQuarterCount int,
			@CurrentQuarterCount int,
			@QOQOrderRepeatedCustomer int
			select @previousMonthFromDate = DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE())-1, 0) 
			select @previousMonthToDate = DATEADD(MONTH, DATEDIFF(MONTH, -1, GETDATE())-1, -1)
			select @currentMonthFromDate = CONVERT(varchar,dateadd(d,-(day(getdate()-1)),getdate()),106)
			select @currentMonthToDate = CONVERT(varchar,dateadd(d,-(day(dateadd(m,1,getdate()))),dateadd(m,1,getdate())),106)

			SELECT @PreviousQuarterStartDate = DATEFROMPARTS(YEAR(DATEADD(MONTH, -6, GETDATE())),  MONTH(DATEADD(MONTH, -6, GETDATE())), 1) 
				  ,@PreviousQuarterEndDate = EOMONTH(DATEADD(MONTH, -4, GETDATE()))

			SELECT @CurrentQuarterStartDate = DATEFROMPARTS(YEAR(DATEADD(MONTH, -3, GETDATE())), MONTH(DATEADD(MONTH, -3, GETDATE())), 1),
				   @CurrentQuarterEndDate = EOMONTH(GETDATE())

---------------------------------------------------------------------MOM-----------------------------------------------------------------------------
		if(OBJECT_ID('tempdb..#PreviousMonthLoan') is not null) 
				drop table #PreviousMonthLoan  
		select l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount into #PreviousMonthLoan 
		from LoanAccounts  l with(nolock) 
		join AccountTransactions ac with(nolock) on l.id=ac.LoanAccountId
		join TransactionTypes tt with(nolock) on tt.Id = ac.TransactionTypeId and l.IsActive=1 and l.IsDeleted=0 and ac.IsActive=1 and ac.IsDeleted=0 AND l.IsAccountActive=1 and tt.IsActive = 1 and tt.IsDeleted=0
		where tt.Code = 'OrderPlacement' 
		and cast(ac.Created as date) <= @previousMonthFromDate and cast(ac.Created as date) <= @previousMonthToDate
		group by l.id
		having count(ac.Created) > 0
		select @PreviousMonthCount=count(*) from #PreviousMonthLoan

		if(OBJECT_ID('tempdb..#CurrentMonthLoan') is not null) 
				drop table #CurrentMonthLoan  
		select l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount
		into #CurrentMonthLoan
		from LoanAccounts  l with(nolock) 
		join AccountTransactions ac with(nolock) on l.id=ac.LoanAccountId
		join TransactionTypes tt with(nolock) on tt.Id = ac.TransactionTypeId and l.IsActive=1 and l.IsDeleted=0 and ac.IsActive=1 and ac.IsDeleted=0 AND l.IsAccountActive=1 and tt.IsActive = 1 and tt.IsDeleted=0
		where tt.Code = 'OrderPlacement' 
		and cast(ac.Created as date) <= @currentMonthFromDate and cast(ac.Created as date) <= @currentMonthToDate
		group by l.id
		having count(ac.Created) > 0
		select @CurrentMonthCount = count(*) from #CurrentMonthLoan

		--select @PreviousMonthCount as previousMonthCount,@CurrentMonthCount as currentMonthCount
		select @MOMOrderRepeatedCustomer = count(*)  from #PreviousMonthLoan pm join #CurrentMonthLoan cm on pm.LoanId=cm.LoanId
		--select ((@MOMOrderRepeatedCustomer/@PreviousMonthCount) * 100)  MOMRetentionPercentage

-----------------------------------------------------QOQ----------------------------------------------------------


		if(OBJECT_ID('tempdb..#PreviousQuarterLoan') is not null) 
				drop table #PreviousQuarterLoan  
		select l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount into #PreviousQuarterLoan 
		from LoanAccounts  l with(nolock) 
		join AccountTransactions ac with(nolock) on l.id=ac.LoanAccountId
		join TransactionTypes tt with(nolock) on tt.Id = ac.TransactionTypeId and l.IsActive=1 and l.IsDeleted=0 and ac.IsActive=1 and ac.IsDeleted=0 AND l.IsAccountActive=1 and tt.IsActive = 1 and tt.IsDeleted=0
		where tt.Code = 'OrderPlacement' 
		and cast(ac.Created as date) <= @PreviousQuarterStartDate and cast(ac.Created as date) <= @PreviousQuarterEndDate
		group by l.id
		having count(ac.Created) > 0
		select @PreviousQuarterCount=count(*) from #PreviousQuarterLoan

		if(OBJECT_ID('tempdb..#CurrentQuarterLoan') is not null) 
				drop table #CurrentQuarterLoan  
		select l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount
		into #CurrentQuarterLoan
		from LoanAccounts  l with(nolock) 
		join AccountTransactions ac with(nolock) on l.id=ac.LoanAccountId
		join TransactionTypes tt with(nolock) on tt.Id = ac.TransactionTypeId and l.IsActive=1 and l.IsDeleted=0 and ac.IsActive=1 and ac.IsDeleted=0 AND l.IsAccountActive=1 and tt.IsActive = 1 and tt.IsDeleted=0
		where tt.Code = 'OrderPlacement' 
		and cast(ac.Created as date) <= @CurrentQuarterStartDate and cast(ac.Created as date) <= @CurrentQuarterEndDate
		group by l.id
		having count(ac.Created) > 0
		select @CurrentQuarterCount = count(*) from #CurrentMonthLoan

		--select @PreviousQuarterCount as previousQuarterCount,@CurrentQuarterCount as currentQuarterCount
		select @QOQOrderRepeatedCustomer = count(*)  from #PreviousQuarterLoan pm join #CurrentQuarterLoan cm on pm.LoanId=cm.LoanId
		--select ((@QOQOrderRepeatedCustomer/@PreviousQuarterCount) * 100)  QOQRetentionPercentage

		select convert(float,((@MOMOrderRepeatedCustomer/@PreviousMonthCount) * 100))  MOMRetentionPercentage,convert(float,((@QOQOrderRepeatedCustomer/@PreviousQuarterCount) * 100))  QOQRetentionPercentage
	End


Go


Create Or ALTER proc [dbo].[GetDisbursementDashboardData] 
	--Declare
	   	@ProductType varchar(200) ='CreditLine',
		@CityName dbo.stringValues readonly,        
		@AnchorId dbo.Intvalues readonly,
		@FromDate datetime ='2024-01-01',        
		@ToDate datetime ='2024-08-06'	
	as
	begin
		--insert into @AnchorId values(2)
		--insert into @CityName values('null')
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
