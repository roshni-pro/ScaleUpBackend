using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _2sept2024_sp_GetRetentionPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

	Create or ALTER Procedure [dbo].[GetRetentionPercentage]
	As	
	Begin
	declare 
			@PreviousmonthCount int,
			@CurrentMonthCount int,
			@MOMOrderRepeatedCustomer int,
			@PreviousQuarterCount int,
			@CurrentQuarterCount int,
			@QOQOrderRepeatedCustomer int

		if(OBJECT_ID('tempdb..#TransactionData') is not null) 
		drop table #TransactionData
		select l.id LoanId,ac.Created Created,tt.Code into #TransactionData 
		from LoanAccounts  l with(nolock) 
		join AccountTransactions ac with(nolock) on l.id=ac.LoanAccountId
		join TransactionTypes tt with(nolock) on tt.Id = ac.TransactionTypeId and l.IsActive=1 and l.IsDeleted=0 and ac.IsActive=1 and ac.IsDeleted=0 AND l.IsAccountActive=1 and tt.IsActive = 1 and tt.IsDeleted=0
		and cast(ac.Created as date) <=  cast(DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()) - 1, 0) as date) and cast(ac.Created as date) >= cast(DATEADD(SECOND, -1, DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()) + 1, 0)) as date)
		--select * from #TransactionData
---------------------------------------------------------------------MOM-----------------------------------------------------------------------------
		if(OBJECT_ID('tempdb..#PreviousMonthLoan') is not null) 
				drop table #PreviousMonthLoan  
		select 
		t.LoanId,COUNT(t.Created) OrderCount
		into #PreviousMonthLoan 
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= cast(DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0) as date) and cast(t.Created as date) >= cast(DATEADD(SECOND, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)) as date)
		group by t.LoanId
		having count(t.Created) > 0

		select @PreviousMonthCount=count(*) from #PreviousMonthLoan

		if(OBJECT_ID('tempdb..#CurrentMonthLoan') is not null) 
				drop table #CurrentMonthLoan  
		select 
		t.LoanId,COUNT(t.Created) OrderCount
		into #CurrentMonthLoan
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= cast(DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0) as date) and cast(t.Created as date) >=  cast(DATEADD(SECOND, -1, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)) as date)
		group by t.LoanId
		having count(t.Created) > 0
		select @CurrentMonthCount = count(*) from #CurrentMonthLoan
		select @MOMOrderRepeatedCustomer = count(*)  from #PreviousMonthLoan pm join #CurrentMonthLoan cm on pm.LoanId=cm.LoanId
-----------------------------------------------------QOQ----------------------------------------------------------
		if(OBJECT_ID('tempdb..#PreviousQuarterLoan') is not null) 
				drop table #PreviousQuarterLoan  
		select
		t.LoanId,COUNT(t.Created) OrderCount 
		into #PreviousQuarterLoan 
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= cast(DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()) - 1, 0) as date) and cast(t.Created as date) >= cast(DATEADD(SECOND, -1, DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()), 0)) as date)
		group by t.LoanId
		having count(t.Created) > 0
		select @PreviousQuarterCount=count(*) from #PreviousQuarterLoan
		if(OBJECT_ID('tempdb..#CurrentQuarterLoan') is not null) 
				drop table #CurrentQuarterLoan  
		select 
		t.LoanId,COUNT(t.Created) OrderCount
		into #CurrentQuarterLoan
		from #TransactionData t
		where t.Code = 'OrderPlacement' 
		and cast(t.Created as date) <= cast(DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()), 0) as date) and cast(t.Created as date) >= cast(DATEADD(SECOND, -1, DATEADD(QUARTER, DATEDIFF(QUARTER, 0, GETDATE()) + 1, 0)) as date)
		group by t.LoanId
		having count(t.Created) > 0

		select @CurrentQuarterCount = count(*) from #CurrentMonthLoan
		select @QOQOrderRepeatedCustomer = count(*)  from #PreviousQuarterLoan pm join #CurrentQuarterLoan cm on pm.LoanId=cm.LoanId

		select 
		Case When @PreviousMonthCount=0 Then 0 Else isnull(convert(float,((@MOMOrderRepeatedCustomer/@PreviousMonthCount) * 100)),0)  End as MOMRetentionPercentage
		,Case When @PreviousQuarterCount=0 Then 0 Else isnull(convert(float,((@QOQOrderRepeatedCustomer/@PreviousQuarterCount) * 100)),0) End As  QOQRetentionPercentage

	End ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
