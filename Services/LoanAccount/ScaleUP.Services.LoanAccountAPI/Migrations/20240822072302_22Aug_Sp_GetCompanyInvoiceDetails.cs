using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _22Aug_Sp_GetCompanyInvoiceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"


CREATE OR ALTER         Procedure [dbo].[GetCompanyInvoiceDetails]
--declare
@InvoiceNo varchar(100)='AC/2024/9',
@RoleName varchar(100)='MakerUser'
AS
Begin	
		select
		cd.AccountTransactionId
		 ,ac.ReferenceId ReferenceNo
		, ci.Id CompanyInvoiceId
		,ac.CustomerUniqueCode as AnchorCode
		,ac.AnchorCompanyId
		,'' AnchorName
		,'' as NBFCName
		,ac.InvoiceNo
		,ac.InvoiceDate
		,ci.Status
		,Round(SUM(case when cd.InvoiceTransactionType = 1 then PayableAmount else 0 end),2) as ProcessingFeeTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 2 then PayableAmount else 0 end),2) as InterestTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 3 then PayableAmount else 0 end),2) as OverDueInterestTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 4 then PayableAmount else 0 end),2) as PenalTotal
		,Round(SUM(case when cd.InvoiceTransactionType = 5 then PayableAmount else 0 end),2) as BounceTotal
		,Round(inv.InvoiceAmount,2) InvoiceAmount
		,Round(sum(cd.ScaleupShare),2) as ScaleupShare
		,Round(sum(case when cd.InvoiceTransactionType = 1 then cd.ScaleupShare else 0 end),2) as ProcessingFeeScaleupShare
		,Round(SUM(case when cd.InvoiceTransactionType = 2 then cd.ScaleupShare else 0 end),2) as InterestScaleupShare
		,Round(SUM(case when cd.InvoiceTransactionType = 3 then cd.ScaleupShare else 0 end),2) as OverDueInterestScaleupShare
		,Round(SUM(case when cd.InvoiceTransactionType = 4 then cd.ScaleupShare else 0 end),2) as PenalScaleupShare
		,Round(SUM(case when cd.InvoiceTransactionType = 5 then cd.ScaleupShare else 0 end),2) as BounceScaleupShare
		,la.NBFCCompanyId
		,Round(sum(cd.PayableAmount),2) as TotalAmount
		,cd.IsActive IsActive
		,'' StatusName
		,cast(0 as bit) IsCheckboxVisible
		,Round((sum(cd.ScaleupShare)/1.09)*0.18,2) GST
		--,ac.GSTAmount GST
		,Isnull(ci.InvoiceUrl,'') InvoiceUrl
		from CompanyInvoices ci  with(nolock)
		inner join CompanyInvoiceDetails cd with(nolock) on ci.Id = cd.CompanyInvoiceId and ci.IsActive=1 and ci.IsDeleted=0 
		inner join AccountTransactions ac with(nolock) on ac.Id = cd.AccountTransactionId
		inner join Invoices inv with(nolock) on inv.Id=ac.InvoiceId
		inner join LoanAccounts la with(nolock) on la.Id = ac.LoanAccountId and la.IsActive=1 and la.IsDeleted=0
		where ci.InvoiceNo = @InvoiceNo
		 --AND (
   --           (@RoleName = 'MakerUser' AND 1 = 1)
   --           OR
   --           (@RoleName = 'checkerUser' and ci.status=1 and ci.status!=3 AND cd.IsActive = 1 AND cd.IsDeleted = 0))
		and cd.IsActive = 1 AND cd.IsDeleted = 0
		group by  cd.AccountTransactionId,ac.CustomerUniqueCode
		,la.AnchorName
		,ac.InvoiceNo
		,ac.InvoiceDate
		,inv.InvoiceAmount
		,la.NBFCCompanyId
		,ci.Id
		,ci.status
		,ac.AnchorCompanyId
		,cd.AccountTransactionId
		 ,ac.ReferenceId
		 ,cd.IsActive
		 ,ci.InvoiceUrl
		 ,ac.GSTAmount
End;
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
