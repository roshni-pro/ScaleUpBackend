using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _24May2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

	CREATE OR ALTER Proc [dbo].[GetLoanAccountDetailByTxnId]
	--declare
	@TransactionReqNo varchar(50)=''
	As 
	Begin
		declare @LoanId bigint=0
	select top 1 @LoanId=LoanAccountId from PaymentRequest with(nolock) where TransactionReqNo=@TransactionReqNo
	select l.Id LoanAccountId, l.LeadId,  l.ProductId,p.AnchorCompanyId,l.MobileNo,ac.CreditLimitAmount,l.NBFCCompanyId
	,isnull((x.UtilizeAmount),0) UtilizateLimit,l.AnchorName,p.TransactionAmount InvoiceAmount,l.NBFCIdentificationCode
	,p.OrderNo,l.CustomerName, isnull(l.CustomerImage,'') ImageUrl, cast(0 as bigint) creditday
	,case when Overdue.Status is not null then Overdue.Status else '' end TransactionStatus
	,l.IsAccountActive,l.IsBlock,l.IsBlockComment
	from PaymentRequest p with(nolock)
	inner join LoanAccounts l with(nolock)  on l.Id=p.LoanAccountId
	inner join LoanAccountCredits ac with(nolock) on ac.LoanAccountId=l.Id
	outer apply (
			select 
				Sum(case when h.Code in ('Refund','Order','Payment') and s.Code not in ('Paid','Canceled','Failed')  then d.Amount else 0 end) as UtilizeAmount,
				Sum(case when t.Code='OrderPlacement' and s.Code in ('Initiate','Intransit') then a.TransactionAmount else 0 end) Intransitlimit
				from AccountTransactions a
				inner join AccountTransactionDetails d with(nolock) on a.id=d.AccountTransactionId
				inner join TransactionTypes t with(nolock) on t.Id=a.TransactionTypeId
				inner join TransactionStatuses s with(nolock) on s.Id=a.TransactionStatusId
				inner join TransactionDetailHeads h with(nolock) on h.Id=d.TransactionDetailHeadId
				where d.IsActive=1 and d.IsDeleted=0 and a.LoanAccountId=l.Id and h.IsActive=1 and h.IsDeleted=0
		) x
	outer apply(
			select top 1 s.code Status 
			from AccountTransactions a with(nolock) 
			inner join TransactionStatuses s with(nolock) on a.TransactionStatusId=s.Id
			where LoanAccountId=@LoanId and s.Code='Overdue' and a.IsActive=1 and a.IsDeleted=0
		) Overdue
		where p.TransactionReqNo=@TransactionReqNo
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
