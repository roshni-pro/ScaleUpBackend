using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _11sept2024_sp_GetDashboardAllData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            Create or ALTER Procedure [dbo].[GetDashboardAllData]
  --declare 
			@ProductType varchar(200) ='CreditLine',
		    @CityName dbo.stringValues readonly,        
		    @AnchorId dbo.Intvalues readonly,
			@FromDate datetime = '2024-09-01',
			@ToDate datetime ='2024-09-10'
   AS
  Begin
			--Declare @StartDate datetime=DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
			--insert into @AnchorId values(2)
			--insert into @CityName values('')
            if (OBJECT_ID('tempdb..#TemporaryAccountTransactionDetail') is not null)
                drop table #TemporaryAccountTransactionDetail  
                select
                l.Id as LoanAccountId,
				a.id as AccountTransactionId,
				d.id,
				t.Code as TxnType,
				d.Amount,
				h.Code as Head,
				s.Code as Status,
				cast(d.TransactionDate as date) as TransactionDate
				,l.ProductType,ProductId,l.AnchorCompanyId,l.AnchorName,l.CityName
				,cast(a.Created as date) as AccountTransactionCreatedDate
				into #TemporaryAccountTransactionDetail
					from LoanAccounts l  with(nolock)
                    inner join AccountTransactions a with(nolock) on a.LoanAccountId = l.id and l.IsActive = 1 and l.IsDeleted = 0 and a.IsActive = 1 and a.IsDeleted = 0
                    inner join AccountTransactionDetails d with(nolock) on a.id = d.AccountTransactionId and d.IsActive = 1 and d.IsDeleted = 0
                    inner join TransactionTypes t with(nolock) on t.Id = a.TransactionTypeId and t.IsActive = 1 and t.IsDeleted = 0
                    inner join TransactionStatuses s with(nolock) on s.Id = a.TransactionStatusId
                    inner join TransactionDetailHeads h with(nolock) on h.Id = d.TransactionDetailHeadId and h.IsActive = 1 and h.IsDeleted = 0
					inner join @CityName c on l.CityName= (case when c.stringValue='' or c.stringValue=null then l.CityName else c.stringValue end)
					where  l.ProductType = @ProductType and (d.TransactionDetailHeadId != 1 and cast(d.TransactionDate as date) >= cast(@FromDate as date) and cast(d.TransactionDate as date) <= cast(@ToDate as date)) --and DisbursementDate is not null
					and (( exists(select 1 from @AnchorId  c where l.AnchorCompanyId = c.IntValue)) or ( not exists(select 1 from @AnchorId )))
				  
				  select --ProductType, ProductId, AnchorCompanyId, isnull(AnchorName,'') AnchorName, CityName,
                    Sum(case when Head in ('Refund', 'Order') and Status not in ('Failed', 'Canceled')  then Amount else 0 end) as LTDUtilizedAmount
					--total outstanding--
					,Sum(case when Head in ('Order', 'Payment', 'Refund') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent')   then Amount else 0 end) --as PrincipleOutstanding
					+Sum(case when Head in ('Interest', 'InterestPaymentAmount') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent')    then Amount else 0 end) --as InterestOutstanding
					+Sum(case when Head in ('DelayPenalty', 'PenalPaymentAmount') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent')  then Amount else 0 end) --as PenalOutStanding
					+Sum(case when Head in ('OverdueInterestAmount', 'OverduePaymentAmount') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent')  then Amount else 0 end) as OutStanding--as OverdueInterestOutStanding
                    --total outstanding--
					,Sum(case when Head in ('Order', 'Payment', 'Refund') and Status in ('Overdue','Delinquent')  then Amount else 0 end) --as ODPrincipleOutstanding
					+Sum(case when Head in ('Interest', 'InterestPaymentAmount') and Status in ('Overdue','Delinquent') then Amount else 0 end) --as ODInterestOutstanding
					+Sum(case when Head in ('DelayPenalty', 'PenalPaymentAmount') and Status in ('Overdue','Delinquent')  then Amount else 0 end) --as ODPenalOutStanding
					+Sum(case when Head in ('OverdueInterestAmount', 'OverduePaymentAmount') and Status in ('Overdue','Delinquent') then Amount else 0 end) as ODOutStanding--as ODOverdueInterestOutStanding
					,sum(case when Head in ('ScaleUpShareOverdueAmount', 'ScaleUpSharePenalAmount', 'ScaleUpShareBounceAmount', 'ScaleUpShareInterestAmount') then Amount else 0 end) as ScaleupShareAmount
                    ,cast(ac.TransactionDate as date) TransactionDate
					from #TemporaryAccountTransactionDetail ac
                    group by cast(ac.TransactionDate as date)--ProductType, ProductId, AnchorCompanyId, AnchorName, CityName
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
