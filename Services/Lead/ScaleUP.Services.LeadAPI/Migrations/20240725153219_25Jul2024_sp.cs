using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _25Jul2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
CREATE OR ALTER   Proc [dbo].[GetAcceptedLoanDetail]
--declare
@leadmasterid bigint,
@NBFCCompanyId bigint
as
Begin 
	select 
	Isnull(b.LoanId, '') as LoanId
	,Isnull(b.LoanAmount, 0) as LoanAmount
	,Isnull(b.InterestRate, 0) as InterestRate
	,Isnull(b.Tenure, 0) as Tenure
	,Isnull(b.MonthlyEMI, 0) as MonthlyEMI
	,Isnull(b.LoanInterestAmount, 0) as LoanInterestAmount
	,Isnull(b.ProcessingFeeRate, 0) as ProcessingFeeRate
	,Isnull(b.ProcessingFeeAmount, 0) as ProcessingFeeAmount
	,Isnull(b.ProcessingFeeTax, 0) as ProcessingFeeTax
	,Isnull(b.PFDiscount, 0) as PFDiscount
	,b.CompanyIdentificationCode
	 ,IsNull(b.OfferStatus,0) as OfferStatus
	from Leads k with(nolock)
	inner join BusinessLoanNBFCUpdate b with(nolock) on b.LeadId=k.Id and b.NBFCCompanyId=@NBFCCompanyId
	where k.Id = @leadmasterid  and k.IsActive =1 and k.IsDeleted =0 --and k.status ='success'
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
