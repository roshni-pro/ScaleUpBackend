using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _20June2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   Proc [dbo].[GetLoanData]
--declare
@leadmasterid bigint
as
Begin 
	select 
	 IsNull(ag.DocUnSignUrl,'') UrlSlaDocument,
	 IsNull(ag.DocSignedUrl,'') UrlSlaUploadSignedDocument,
	 IsNull(ld.IsUpload,0) IsUpload,
	 IsNull(ld.UrlSlaUploadDocument_id,'') UrlSlaUploadDocument_id,
	 IsNull(adm.SanctionAmount,0)SanctionAmount,
	 IsNull(adm.loan_amount,0)loan_amount,
	 Lower(IsNull(ld.status,'')) LoanStatus,
	 IsNull(adm.pricing,0)pricing,
	 IsNull(cast(au.tenure as int),0) tenure,
	 IsNull(ld.loan_int_amt,0)loan_int_amt,
	 IsNull(cast(ld.loan_int_rate as float),0)loan_int_rate,
	 IsNull(ld.emi_amount,0)emi_amount,
	 IsNull(ld.emi_count,0)emi_count,
	 IsNull(ld.UMRN,'')UMRN
	,CAST( IsNull(ld.insurance_amount,0) as int) Loan_insurance_amount
	,isnull(ld.loan_id, '') as LoanId,
	es.eSignRemark
	from Leads k with(nolock)
	inner join ArthMateUpdates au with(nolock) on au.leadid=k.Id
	Inner join (Select Top 1 * From CoLenderResponse with(nolock) Where LeadMasterId  = @leadmasterid and status ='success' and IsActive =1 and IsDeleted =0 Order by Id Desc) adm 
	ON k.Id = adm.LeadMasterId
	Left outer join leadloan ld with(nolock) ON k.Id=ld.LeadMasterId and ld.IsActive=1 and ld.IsDeleted=0
	left outer join leadAgreements ag with(nolock) on ag.LeadId=k.Id and ag.IsActive=1 and ag.IsDeleted=0
	left outer join eSignResponseDocumentIds es with(nolock) on es.LeadId=k.Id and es.IsActive=1 and es.IsDeleted=0
	where k.Id = @leadmasterid --and adm.IsActive =1 and adm.IsDeleted =0 and adm.status ='success'
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
