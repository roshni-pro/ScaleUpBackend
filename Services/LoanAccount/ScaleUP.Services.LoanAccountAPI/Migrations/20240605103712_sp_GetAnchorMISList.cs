using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class sp_GetAnchorMISList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"
            CREATE OR ALTER Procedure [dbo].[GetAnchorMISList]
	@AnchorId  bigint=2
	,@Status varchar(200)='ALL'
	,@FromDate datetime='2023-01-31'
	,@ToDate datetime='2024-05-31'
As
Begin

		DECLARE @StartDate date,
				@EndDate date
		IF((@FromDate is null or @FromDate='') and (@ToDate is null or @ToDate=''))
			Begin
				SELECT @StartDate = MIN(c.Created),  @EndDate = max(c.Created) FROM LoanAccounts c with(nolock) where c.IsActive=1 and c.IsDeleted=0
			End

Select distinct
t.ReferenceId, LA.AccountCode as LoanID, LA.AnchorName
, i.OrderNo , i.Status InvoiceStatus, i.InvoiceNo, i.InvoiceDate, i.InvoiceAmount   
, LA.ShopName BusinessName
, lc.UserUniqueCode as AnchorCode  
,'?' NBFCName
, t.DisbursementDate ,tp.paymentAmount DisbursalAmount 
, cast(0 as float) ServiceFee, cast(0 as float)*(18/100) GST, '?'BeneficiaryName, '?' BeneficiaryAccountNumber	, i.NBFCUTR as UTR, i.NBFCStatus as Status
, LA.LeadId, LA.NBFCCompanyId, LA.ProductId, LA.AnchorCompanyId 
from LoanAccounts LA with(nolock)
Inner join LoanAccountCompanyLead lc with(nolock) on LA.Id=lc.LoanAccountId and lc.IsActive=1 and lc.IsDeleted=0
Inner Join AccountTransactions t with(nolock) on LA.Id=t.LoanAccountId and t.IsActive=1 and t.IsDeleted=0
Inner Join Invoices i with(nolock) ON LA.Id=i.LoanAccountId and i.Id =t.InvoiceId  and i.IsActive=1 and i.IsDeleted=0
inner join TransactionTypes ty on ty.Id = t.TransactionTypeId
outer apply(select st.amount paymentAmount from BlackSoilAccountTransactions tp with(nolock)
Inner join InvoiceDisbursalProcesseds st with(nolock) on tp.id=st.AccountTransactionId 
 where tp.LoanInvoiceId=i.Id) tp
Where LA.IsActive=1 and LA.IsDeleted=0
and ty.Code = 'OrderPlacement' --and i.invoiceNo in ('MP24253465')
and (i.Status=@Status or @Status='All')
and (t.AnchorCompanyId = @AnchorId)
and (CAST(t.DisbursementDate as date)>=@FromDate and CAST(t.DisbursementDate as date)<=@ToDate)
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
