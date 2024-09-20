using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class SP_GetCompanyInvoicesList_7Aug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"


CREATE OR ALTER   Procedure [dbo].[GetCompanyInvoicesList]
	--declare
	@NBFCId bigint=3,
	@AnchorId dbo.Intvalues readonly,
	@FromDate datetime='2024-05-01',
	@ToDate datetime='2024-05-31',
	@Skip int=0,
	@Take int=10,
	@Status int=100
	As
	Begin
	--insert into @AnchorId values(2);
	select '' as NBFCName
		,ci.InvoiceNo
		,ci.InvoiceDate,ci.Status
		,ci.PaymentDate
		,ci.PaymentReferenceNo as PaymentRefNo
		,ci.CompanyId NBFCCompanyId
		,Round(isnull(cid.Amount,0),2) Amount
		,cid.ReferenceNo
		,cid.AnchorCompanyId
		,isnull(ci.InvoiceUrl,'') InvoiceUrl
		,'' StatusName
		,'' CompanyEmail
		,ci.Created CreatedDate
		,'' UserType
		,ci.Id CompanyInvoiceId
		,COUNT(*) over() as TotalCount
	from CompanyInvoices ci with(nolock)	
	outer apply (select act.AnchorCompanyId,max(act.ReferenceId) ReferenceNo,
				Round(SUM(cd.ScaleupShare)*1.18,2) Amount
				 from CompanyInvoiceDetails cd with(nolock)
				inner join AccountTransactions act with(nolock) on cd.AccountTransactionId=act.Id
				inner join @AnchorId a on act.AnchorCompanyId=a.IntValue
				 where ci.Id = cd.CompanyInvoiceId and cd.IsActive=1 and cd.IsDeleted=0 
				group by act.AnchorCompanyId--,act.ReferenceId 
	) cid
	where ci.CompanyId = @NBFCId and ci.IsActive=1 and ci.IsDeleted=0 
	and (@Status=ci.Status or @Status =100)
	and CAST(ci.InvoiceDate as date)>=@FromDate and CAST(ci.InvoiceDate as date)<=@ToDate
	--group by ci.Id,ci.CompanyId,ci.InvoiceNo,ci.InvoiceDate,ci.Status,ci.PaymentDate,ci.PaymentReferenceNo
	order by ci.InvoiceDate 
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
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
