using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _9sept2024_sp_GetHopDashboardData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            Create or ALTER Procedure[dbo].[GetHopDashboardData]
            As
            Begin
            declare @currentDate datetime = getdate();
            if (OBJECT_ID('tempdb..#TempAccountTransactionDetail') is not null)
                drop table #TempAccountTransactionDetail  
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
				into #TempAccountTransactionDetail
					from LoanAccounts l  with(nolock)
                    inner join AccountTransactions a with(nolock) on a.LoanAccountId = l.id and l.IsActive = 1 and l.IsDeleted = 0 and a.IsActive = 1 and a.IsDeleted = 0
                    inner join AccountTransactionDetails d with(nolock) on a.id = d.AccountTransactionId and d.IsActive = 1 and d.IsDeleted = 0
                    inner join TransactionTypes t with(nolock) on t.Id = a.TransactionTypeId and t.IsActive = 1 and t.IsDeleted = 0
                    inner join TransactionStatuses s with(nolock) on s.Id = a.TransactionStatusId
                    inner join TransactionDetailHeads h with(nolock) on h.Id = d.TransactionDetailHeadId and h.IsActive = 1 and h.IsDeleted = 0
					where((d.TransactionDetailHeadId != 1 and cast(d.TransactionDate as date) <= cast(@currentDate as date)) or(d.TransactionDetailHeadId = 1 and(cast(a.Created as date) <= cast(@currentDate as date)))) and DisbursementDate is not null
				if ((select COUNT(*) from HopDashboardData with(nolock) where cast(TransactionDate as date) = CAST(@currentDate as date) and IsActive = 1 and IsDeleted = 0) = 0)
				Begin
                    INSERT INTO HopDashboardData(ProductType, ProductId, AnchorCompanyId, AnchorName, CityName, LTDUtilizedAmount, PrincipleOutstanding, InterestOutstanding, PenalOutStanding, OverdueInterestOutStanding, ODPrincipleOutstanding, ODInterestOutstanding, ODPenalOutStanding, ODOverdueInterestOutStanding, TotalRepayment, PrincipalRepayment, InterestRepayment, OverdueInterestPayment, PenalRePayment, BounceRePayment, ExtraPayment,ScaleupShareAmount, TransactionDate, Created, CreatedBy, LastModified, LastModifiedBy, Deleted, DeletedBy, IsActive, IsDeleted)
				  
				  select ProductType, ProductId, AnchorCompanyId, isnull(AnchorName,'') AnchorName, CityName,
                    Sum(case when Head in ('Refund', 'Order') and Status not in ('Failed', 'Canceled')  then Amount else 0 end) as LTDUtilizedAmount
                    --total outstanding--
					,Sum(case when Head in ('Order', 'Payment', 'Refund') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent')  then Amount else 0 end) as PrincipleOutstanding
					,Sum(case when Head in ('Interest', 'InterestPaymentAmount') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent') then Amount else 0 end) as InterestOutstanding
					,Sum(case when Head in ('DelayPenalty', 'PenalPaymentAmount') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent')  then Amount else 0 end) as PenalOutStanding
					,Sum(case when Head in ('OverdueInterestAmount', 'OverduePaymentAmount') and Status in ('Pending', 'Due', 'Overdue', 'Delinquent')  then Amount else 0 end) as OverdueInterestOutStanding
                    --total outstanding--
                    --total overdue outstanding
					,Sum(case when Head in ('Order', 'Payment', 'Refund') and Status in ('Overdue')  then Amount else 0 end) as ODPrincipleOutstanding
					,Sum(case when Head in ('Interest', 'InterestPaymentAmount') and Status in ('Overdue') then Amount else 0 end) as ODInterestOutstanding
					,Sum(case when Head in ('DelayPenalty', 'PenalPaymentAmount') and Status in ('Overdue')  then Amount else 0 end) as ODPenalOutStanding
					,Sum(case when Head in ('OverdueInterestAmount', 'OverduePaymentAmount') and Status in ('Overdue')  then Amount else 0 end) as ODOverdueInterestOutStanding
                    --total overdue outstanding

					---Repayment--
					,Sum(case when Head in ('ExtraPaymentAmount','OverduePaymentAmount','PenalPaymentAmount','InterestPaymentAmount','BouncePaymentAmount','Payment') 
							  then Amount 
						else 0 end) as TotalRepayment
					,Sum(case when Head in ('Payment') then Amount else 0 end) as PrincipalRepayment
					,Sum(case when Head in ('InterestPaymentAmount')  then Amount else 0 end) as InterestRepayment
					,Sum(case when Head in ('OverduePaymentAmount')  then Amount else 0 end) as OverdueInterestPayment
					,Sum(case when Head in ('PenalPaymentAmount')  then Amount else 0 end) as PenalRePayment
					,Sum(case when Head in ('BouncePaymentAmount')  then Amount else 0 end) as BounceRePayment
					,Sum(case when Head in ('ExtraPaymentAmount')  then Amount else 0 end) as ExtraPayment
					---Repayment--

                    --scaleup share amount--
					,sum(case when Head in ('ScaleUpShareOverdueAmount', 'ScaleUpSharePenalAmount', 'ScaleUpShareBounceAmount', 'ScaleUpShareInterestAmount') then Amount else 0 end) as ScaleupShareAmount
                    --scaleup share amount--
					,@currentDate TransactionDate, @currentDate currentDate,'system',null,'system',null,'',1,0
                    from #TempAccountTransactionDetail ac
					
                    group by ProductType, ProductId, AnchorCompanyId, AnchorName, CityName
                end
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
