using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class sp_GetRetentionPercentage_12Aug2024 : Migration
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


				if(OBJECT_ID('tempdb..#TransactionData') is not null) 
				drop table #TransactionData

				select l.id LoanId,ac.Created Created,tt.Code,ac.TransactionAmount into #TransactionData 
				from LoanAccounts  l with(nolock) 
				join AccountTransactions ac with(nolock) on l.id=ac.LoanAccountId
				join TransactionTypes tt with(nolock) on tt.Id = ac.TransactionTypeId and l.IsActive=1 and l.IsDeleted=0 and ac.IsActive=1 and ac.IsDeleted=0 AND l.IsAccountActive=1 and tt.IsActive = 1 and tt.IsDeleted=0

---------------------------------------------------------------------MOM-----------------------------------------------------------------------------
		if(OBJECT_ID('tempdb..#PreviousMonthLoan') is not null) 
				drop table #PreviousMonthLoan  
		select 
		t.LoanId,COUNT(t.Created) OrderCount,sum(t.TransactionAmount) TotalOrderAmount
		--l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount 
		into #PreviousMonthLoan 
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= @previousMonthFromDate and cast(t.Created as date) <= @previousMonthToDate
		group by t.LoanId
		having count(t.Created) > 0
		select @PreviousMonthCount=count(*) from #PreviousMonthLoan

		if(OBJECT_ID('tempdb..#CurrentMonthLoan') is not null) 
				drop table #CurrentMonthLoan  
		select 
		t.LoanId,COUNT(t.Created) OrderCount,sum(t.TransactionAmount) TotalOrderAmount
		--l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount
		into #CurrentMonthLoan
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= @currentMonthFromDate and cast(t.Created as date) <= @currentMonthToDate
		group by t.LoanId
		having count(t.Created) > 0
		select @CurrentMonthCount = count(*) from #CurrentMonthLoan

		--select @PreviousMonthCount as previousMonthCount,@CurrentMonthCount as currentMonthCount
		select @MOMOrderRepeatedCustomer = count(*)  from #PreviousMonthLoan pm join #CurrentMonthLoan cm on pm.LoanId=cm.LoanId
		--select ((@MOMOrderRepeatedCustomer/@PreviousMonthCount) * 100)  MOMRetentionPercentage

-----------------------------------------------------QOQ----------------------------------------------------------


		if(OBJECT_ID('tempdb..#PreviousQuarterLoan') is not null) 
				drop table #PreviousQuarterLoan  
		select
		t.LoanId,COUNT(t.Created) OrderCount,sum(t.TransactionAmount) TotalOrderAmount
		--l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount 
		into #PreviousQuarterLoan 
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= @PreviousQuarterStartDate and cast(t.Created as date) <= @PreviousQuarterEndDate
		group by t.LoanId
		having count(t.Created) > 0
		select @PreviousQuarterCount=count(*) from #PreviousQuarterLoan

		if(OBJECT_ID('tempdb..#CurrentQuarterLoan') is not null) 
				drop table #CurrentQuarterLoan  
		select 
		t.LoanId,COUNT(t.Created) OrderCount,sum(t.TransactionAmount) TotalOrderAmount
		--l.id LoanId,count(ac.Created) OrderCount,sum(ac.TransactionAmount) TotalOrderAmount
		into #CurrentQuarterLoan
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= @CurrentQuarterStartDate and cast(t.Created as date) <= @CurrentQuarterEndDate
		group by t.LoanId
		having count(t.Created) > 0
		select @CurrentQuarterCount = count(*) from #CurrentMonthLoan

		--select @PreviousQuarterCount as previousQuarterCount,@CurrentQuarterCount as currentQuarterCount
		select @QOQOrderRepeatedCustomer = count(*)  from #PreviousQuarterLoan pm join #CurrentQuarterLoan cm on pm.LoanId=cm.LoanId
		--select ((@QOQOrderRepeatedCustomer/@PreviousQuarterCount) * 100)  QOQRetentionPercentage

		select convert(float,((@MOMOrderRepeatedCustomer/@PreviousMonthCount) * 100))  MOMRetentionPercentage,convert(float,((@QOQOrderRepeatedCustomer/@PreviousQuarterCount) * 100))  QOQRetentionPercentage
	End
            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
