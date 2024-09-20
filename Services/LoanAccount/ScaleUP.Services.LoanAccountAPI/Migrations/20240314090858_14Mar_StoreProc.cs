using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _14Mar_StoreProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"
GO
/****** Object:  StoredProcedure [dbo].[GetDueDisbursmentDetails]    Script Date: 3/14/2024 4:01:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[GetDueDisbursmentDetails]
AS
BEGIN
declare @messageBeforeDays int = 4;
select ReferenceId
	, i.OrderNo
	, Round(sum(ad.Amount),2,2) amount
	,s.Code Status
	,l.MobileNo
	,LEFT(l.CustomerName, CHARINDEX(' ', l.CustomerName + ' ') - 1) UserName
	,l.AnchorName
	,cast(ats.DueDate as date) DueDate
	,'..XX'+RIGHT(lb.AccountNumber, 4) BankAccountNo

from Invoices i with(nolock)
inner join AccountTransactions ats with(nolock) on i.Id = ats.InvoiceId
inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id
inner join TransactionStatuses s with(nolock)  on s.id=ats.TransactionStatusId
inner join TransactionTypes t with(nolock) on ats.TransactionTypeId = t.Id
inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
inner join LoanBankDetails lb  with(nolock) on ats.LoanAccountId  = lb.LoanAccountId 
where  ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0 and ats.DisbursementDate!='' 
and s.Code = 'Pending' 
and cast(ats.DueDate as date)=DATEADD(day,@messageBeforeDays,cast(getdate() as date))
Group BY  i.OrderNo,s.Code,l.MobileNo,l.CustomerName,l.AnchorName,ReferenceId,cast(ats.DueDate as date),lb.AccountNumber
order by DueDate desc
END

";
            migrationBuilder.Sql(sp1);

            var sp2 = @"

GO
/****** Object:  StoredProcedure [dbo].[SendOverdueMessageJobData]    Script Date: 3/14/2024 4:03:03 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER Procedure [dbo].[SendOverdueMessageJobData]
AS
BEGIN

select ReferenceId
	, i.OrderNo
	, Round(sum(ad.Amount),2,2) Amount
	,s.Code Status
	,l.MobileNo
	,LEFT(l.CustomerName, CHARINDEX(' ', l.CustomerName + ' ') - 1) CustomerName
	,l.AnchorName
	
from Invoices i with(nolock)
inner join AccountTransactions ats with(nolock) on i.Id = ats.InvoiceId
inner join LoanAccounts l with(nolock) on ats.LoanAccountId=l.Id
inner join TransactionStatuses s with(nolock)  on s.id=ats.TransactionStatusId
inner join TransactionTypes t with(nolock) on ats.TransactionTypeId = t.Id
inner join AccountTransactionDetails ad  with(nolock) on ats.Id = ad.AccountTransactionId 
inner join TransactionDetailHeads h  with(nolock) on ad.TransactionDetailHeadId  = h.Id 
where  ats.IsActive=1 And ats.IsDeleted=0 and ad.IsActive=1 and ad.IsDeleted=0 and ats.DisbursementDate!='' and s.Code = 'Overdue'
Group BY  i.OrderNo,s.Code,l.MobileNo,l.CustomerName,l.AnchorName,ReferenceId
order by Amount desc
END
";
            migrationBuilder.Sql(sp2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
