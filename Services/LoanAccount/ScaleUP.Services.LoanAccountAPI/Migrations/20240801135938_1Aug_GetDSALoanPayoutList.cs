using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _1Aug_GetDSALoanPayoutList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   Proc [dbo].[GetDSALoanPayoutList]
 --declare 
 @leadIds intvalues readonly,
 @agentUserId nvarchar(500)='88e2653a-a8bf-4ecf-8546-674afdbb59ac'
--insert into @leadIds values(349)
AS
BEGIN
;WITH LeadData AS (
    SELECT
		isnull(bd.LoanId,isnull(bl.loan_id,'')) LoanId,
		l.DisbursalDate DisbursmentDate,
		l.CustomerName FullName,
		'' Status,
		l.MobileNo,
		sum(atd.Amount) DisbursmentAmount,
		l.CustomerImage ProfileImage,
        l.LeadId,
        at.Id AS TransactionId
    FROM LoanAccounts l with(nolock)
    INNER JOIN @leadIds AS leadIds ON l.LeadId = leadIds.IntValue
    INNER JOIN AccountTransactions at with(nolock) ON l.Id = at.LoanAccountId
	INNER JOIN AccountTransactionDetails atd with(nolock) ON at.Id = atd.AccountTransactionId
	INNER JOIN TransactionDetailHeads tdh with(nolock) ON tdh.Id = atd.TransactionDetailHeadId
    LEFT JOIN BusinessLoans bl with(nolock) ON bl.LoanAccountId = l.Id
    LEFT JOIN BusinessLoanDisbursementDetail bd with(nolock) ON bd.LoanAccountId = l.Id
    WHERE l.IsActive = 1 AND l.IsDeleted = 0 AND at.IsActive = 1 AND at.IsDeleted = 0 and atd.IsActive=1 and atd.IsDeleted=0 and tdh.Code='DisbursementAmount'
	group by bl.loan_id,l.AccountCode,l.DisbursalDate,l.CustomerName,l.MobileNo,l.CustomerImage,l.LeadId,at.Id,bd.LoanId
)
SELECT l.UserId,
		LoanId,
		DisbursmentDate,
		FullName,
		'' Status,
		ld.MobileNo,
		DisbursmentAmount,
		ProfileImage,
        ld.LeadId,
		sum(atd.Amount) AS PayoutAmount
FROM LoanAccounts l
INNER JOIN AccountTransactions at with(nolock) ON l.Id = at.LoanAccountId
INNER JOIN LeadData ld with(nolock) ON at.ParentAccountTransactionsID = ld.TransactionId
INNER JOIN AccountTransactionDetails atd with(nolock) ON at.Id = atd.AccountTransactionId
WHERE at.IsActive = 1 AND at.IsDeleted = 0 and atd.IsActive = 1 AND atd.IsDeleted = 0 and l.UserId = @agentUserId
group by l.UserId,LoanId,DisbursmentDate,FullName,ld.Status,ld.MobileNo,DisbursmentAmount,ProfileImage,ld.LeadId
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
