using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _24_Apr_ArthmateSP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			var sp = @"
CREATE OR ALTER Proc [dbo].[GetDisbursedLoanDetail]
--declare
@LeadMasterId bigint=279
As 
Begin
select l.loan_id, LeadMasterId, sanction_amount,isnull(l.insurance_amount,0) InsurancePremium,emi_amount MonthlyEMI
,ll.AccountNumber as borro_bank_acc_num,l.loan_int_amt
,l.company_id,l.product_id
from LeadLoan l with(nolock)
inner join Leads a with(nolock) on a.Id=l.LeadMasterId
inner join LeadBankDetails ll on ll.LeadId=a.Id

where a.IsActive=1 and a.IsDeleted=0 and a.Id=@LeadMasterId and ll.IsActive=1 and ll.IsDeleted=0
End


GO


CREATE OR ALTER Proc [dbo].[GetLoanData]
--declare
@leadmasterid bigint=279
as
Begin 
	select 
	IsNull(au.AgreementPdfURL,'') UrlSlaDocument,
	 IsNull(ld.UrlSlaUploadSignedDocument,'') UrlSlaUploadSignedDocument,
	 IsNull(ld.IsUpload,0) IsUpload,
	 IsNull(ld.UrlSlaUploadDocument_id,'') UrlSlaUploadDocument_id,
	 IsNull(adm.SanctionAmount,'')SanctionAmount,
	 IsNull(adm.loan_amount,'')loan_amount,
	 Lower(IsNull(ld.status,'')) LoanStatus,
	 IsNull(adm.pricing,0)pricing,
	 IsNull(cast(au.tenure as int),0) tenure,
	 IsNull(ld.loan_int_amt,0)loan_int_amt,
	 IsNull(ld.loan_int_rate,0)loan_int_rate,
	 IsNull(ld.emi_amount,0)emi_amount,
	 IsNull(ld.emi_count,0)emi_count,
	 IsNull(ld.UMRN,'')UMRN
	,CAST( IsNull(ld.insurance_amount,0) as int) Loan_insurance_amount
	from Leads k 
	inner join ArthMateUpdates au with(nolock) on au.leadid=k.Id
	Inner join (Select Top 1 * From CoLenderResponse Where LeadMasterId  = @leadmasterid and status ='success' and IsActive =1 and IsDeleted =0 Order by Id Desc) adm 
	ON k.Id = adm.LeadMasterId
	Left outer join leadloan ld ON k.Id=ld.LeadMasterId and ld.IsActive=1 and ld.IsDeleted=0

	where k.Id = @leadmasterid --and adm.IsActive =1 and adm.IsDeleted =0 and adm.status ='success'
End
GO";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
