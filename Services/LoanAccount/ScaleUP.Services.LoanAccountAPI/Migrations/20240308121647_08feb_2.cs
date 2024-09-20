using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _08feb_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			var sp1 = @"

ALTER Procedure [dbo].[InvoiceDetail]
--declare
	@InvoiceId bigint=274
AS
Begin
	if(@InvoiceId != 0)
	begin
		select    x.AccountTransactionId
				, x.TransactionType 
				, x.SequenceNo
				, MAX(TransactionDate) TransactionDate
				, (ROUND(Sum(ISNULL(x.TxnAmount,0)),2))  TxnAmount
				, x.ReferenceId
				, x.InvoiceId
		from
		(
			select		 ats.Id AccountTransactionId
						,case when h.Code = 'Refund' then 'Order' ELSE h.Code END TransactionType
						,case when cast(Max(ad.TransactionDate) as date) IS NULL then MAX(ats.Created) ELSE cast(Max(ad.TransactionDate) as date) END TransactionDate,
						-- ROUND(ROUND(Sum(ISNULL(ad.Amount,0)),2,2),2) TxnAmount
						Sum(ISNULL(ad.Amount,0)) TxnAmount
						,ats.ReferenceId
						,case when h.Code = 'Refund' then  2 else h.SequenceNo	END SequenceNo	
						,ats.InvoiceId
						from AccountTransactions ats  with(nolock) 
						inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id
						inner join TransactionStatuses s with(nolock) on s.id=ats.TransactionStatusId
						inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
						inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id 
						where (ats.InvoiceId=@InvoiceId or ats.ParentAccountTransactionsID=@InvoiceId)
						and (ad.TransactionDate is null or Cast(ad.TransactionDate as Date)<= Cast(GETDATE() as Date))
						and ad.IsActive=1 
						group by ats.Id,s.Code,h.Code,SequenceNo, ats.Id, ats.ReferenceId,ats.InvoiceId
		)X
		group by x.AccountTransactionId, x.TransactionType, SequenceNo, x.ReferenceId,x.InvoiceId
		order by ReferenceId, SequenceNo
	end
	else
	begin
	select 'ReferenceId is null'
	end
End
";
            migrationBuilder.Sql(sp1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
