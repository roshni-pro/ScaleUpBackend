using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _10sep2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
CREATE OR ALTER   Procedure [dbo].[GetCustomerTransactionList_Two]
--declare
	@LeadId bigint=182, --= 15
	@Skip int = 0,
	@Take int = 5000
As 
Begin
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 
	SELECT   max(t.Id) as TransactionId
			,max(ts.Code) as Status	
	        ,sum(Isnull(atd.Amount,0)) as TotalAmount
			,max(CASE WHEN t.DisbursementDate IS NOT NULL THEN t.DueDate ELSE NULL END) DueDate
			,i.OrderNo as OrderId 
			,max(la.AnchorName) as AnchorName  
			,ROUND(ISNULL(SUM(CASE WHEN h.Code = 'Order' OR h.Code = 'Refund' THEN atd.Amount ELSE 0 END), 0),2) PricipleAmount  
			,t.InvoiceId
			,i.InvoiceNo 
	into #tempTransaction 
	FROM AccountTransactions t
	inner join Invoices i on t.InvoiceId = i.Id
	inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	inner join TransactionTypes tt  with(nolock) on t.TransactionTypeId = tt.Id  and tt.IsActive=1 and tt.IsDeleted=0
	inner join AccountTransactionDetails atd with(nolock) on t.Id = atd.AccountTransactionId  and atd.IsActive=1 and atd.IsDeleted=0
	inner join TransactionDetailHeads h with(nolock) on h.Id=atd.TransactionDetailHeadId and h.IsActive=1 and h.IsDeleted=0
	inner join LoanAccounts la with(nolock) on t.LoanAccountId = la.Id and la.IsActive=1 and la.IsDeleted=0
	inner join dbo.GetTransactionTypeId() fn on fn.id =t.TransactionTypeId
	WHERE tt.Code = 'OrderPlacement'
		and (ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
		and t.IsActive=1 and t.IsDeleted=0 and (atd.TransactionDate is null or cast(atd.TransactionDate as date) <= cast(getdate() as date))
		--and cast(t.DueDate as date) <=  cast(getdate() as date)
		and t.IsActive =1 and t.IsDeleted =0
		and la.LeadId = @LeadId
		group by  t.InvoiceId, i.InvoiceNo, i.OrderNo
		order by max(t.Created) desc
		OFFSET @Skip ROWS
		FETCH NEXT @Take ROWS ONLY
	select   tt.AnchorName
		    ,tt.DueDate
			,tt.OrderId
			,tt.Status
			,tt.TransactionId
	        ,ROUND(CASE WHEN tt.Status != 'Paid' THEN ISNULL(tt.TotalAmount, 0) + IsNULL(x.Amount, 0) ELSE ABS(IsNULL(x.Amount, 0)) END,2) as PaidAmount
			,ROUND(tt.PricipleAmount,2) as Amount
			,tt.InvoiceId
			,tt.InvoiceNo
	from #tempTransaction tt
	outer apply(
             select
			       SUM(ISNULL(atd.Amount, 0)) Amount
			 from AccountTransactions at with(nolock) 
			 inner join TransactionTypes xtt  with(nolock) on at.TransactionTypeId = xtt.Id  and xtt.IsActive=1 and xtt.IsDeleted=0
			 inner join AccountTransactionDetails atd with(nolock) on at.Id = atd.AccountTransactionId
			 inner join dbo.GetTransactionTypeId() fn on fn.id =at.TransactionTypeId
			 where tt.TransactionId = at.ParentAccountTransactionsID  and (atd.TransactionDate  IS NULL OR CAST(atd.TransactionDate as DATE) <= GETDATE())
	         and at.IsActive =1 and at.IsDeleted =0 and atd.IsActive =1 and atd.IsDeleted =0
			 and(tt.Status != 'Paid' OR xtt.Code = 'OrderPayment')
			 --group by at.ParentAccountTransactionsID 
	)x	
End

GO


CREATE OR ALTER Procedure [dbo].[GetCustomerTransactionBreakup]
--declare
	@InvoiceId bigint =51--=179
As 
Begin
	IF OBJECT_ID('tempdb..#tempTransaction') IS NOT NULL 
		DROP TABLE #tempTransaction 
   select 
         t.Id
    Into #tempTransaction 
	from AccountTransactions t
	inner join TransactionStatuses ts on t.TransactionStatusId = ts.Id  and ts.IsActive=1 and ts.IsDeleted=0
	where t.InvoiceId = @InvoiceId  and (ts.Code != 'Failed'  AND ts.Code != 'Initiate'  AND ts.Code != 'Canceled')
	select   h.Code TransactionType
			,ROUND(ISNULL(SUM(ISNULL(ad.Amount, 0)), 0),2) Amount ,
			max(ats.Id) as Id
	from AccountTransactions ats  with(nolock) 
	inner join TransactionTypes t  with(nolock) on ats.TransactionTypeId = t.Id  and t.IsActive =1 and t.IsDeleted =0
	inner join AccountTransactionDetails ad  with(nolock)  on ats.Id = ad.AccountTransactionId 
	inner join TransactionDetailHeads h  with(nolock)  on ad.TransactionDetailHeadId  = h.Id  and h.IsActive =1 and h.IsDeleted =0
	inner join dbo.GetTransactionTypeId() fn on fn.id =ats.TransactionTypeId
	where --( ats.Id = @TransactionId or ats.ParentAccountTransactionsId = @TransactionId)
	( ats.Id in( select * from #tempTransaction  ) or ats.ParentAccountTransactionsId  in( select * from #tempTransaction)) and
	 (ad.TransactionDate  IS NULL OR CAST(ad.TransactionDate as DATE) <= GETDATE())
	and ats.IsActive =1 and ats.IsDeleted =0 
	and ad.IsActive =1 and ad.IsDeleted =0
	group by  h.Code,h.SequenceNo
	order by h.SequenceNo
	 --MAX(ats.Created)
			--,ats.OrderId
			--,ats.Status
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
