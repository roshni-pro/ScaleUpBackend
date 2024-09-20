using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _29april_OverdueMessageJob_SP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER procedure [dbo].[SendOverdueMessageJobData]
AS
BEGIN
select 
	ReferenceId
	, i.OrderNo
	, Round(sum(ad.Amount),2) Amount
	,s.Code Status
	,l.MobileNo
	,LEFT(l.CustomerName, CHARINDEX(' ', l.CustomerName + ' ') - 1) CustomerName
	,l.AnchorName
	from Invoices i with(nolock)
	inner join AccountTransactions a  with(nolock) on a.InvoiceId=i.Id
	inner join TransactionTypes t  with(nolock) on a.TransactionTypeId=t.Id 
	inner join AccountTransactionDetails ad  with(nolock) on ad.AccountTransactionId=a.Id 
	inner join TransactionDetailHeads h  with(nolock) on h.id=ad.TransactionDetailHeadId
	inner join LoanAccounts l with(nolock) on a.LoanAccountId=l.Id
	inner join TransactionStatuses s with(nolock)  on s.id=a.TransactionStatusId
	where a.IsActive=1 And a.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0 
	and s.Code = 'overdue'
	Group BY  i.OrderNo,s.Code,l.MobileNo,l.CustomerName,l.AnchorName,ReferenceId
END
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
