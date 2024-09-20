using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _13May2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"


CREATE OR ALTER   Proc [dbo].[GetLoanData]
--declare
@leadmasterid bigint=57
as
Begin 
	select 
	IsNull(au.AgreementPdfURL,'') UrlSlaDocument,
	 IsNull(ld.UrlSlaUploadSignedDocument,'') UrlSlaUploadSignedDocument,
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
	from Leads k 
	inner join ArthMateUpdates au with(nolock) on au.leadid=k.Id
	Inner join (Select Top 1 * From CoLenderResponse Where LeadMasterId  = @leadmasterid and status ='success' and IsActive =1 and IsDeleted =0 Order by Id Desc) adm 
	ON k.Id = adm.LeadMasterId
	Left outer join leadloan ld ON k.Id=ld.LeadMasterId and ld.IsActive=1 and ld.IsDeleted=0
	where k.Id = @leadmasterid --and adm.IsActive =1 and adm.IsDeleted =0 and adm.status ='success'
End

GO


CREATE OR ALTER Proc [dbo].[GetOfferEmiDetails]
--declare
@leadId int =0,
@SanctionAmount float=0,
@EmiAmount float=0,
@RPIValue float=0,
@Tenure int=0,
@FirstEmiDate date =''
AS
Begin
Declare @RPI float = @RPIValue/100

;with sdata
as
(
   select  @FirstEmiDate DueDate 
		   ,Round(@SanctionAmount,2) OutStandingAmount
		   ,Round(@EmiAmount - @SanctionAmount*@RPI/12,2) as Prin
		   ,Round(@SanctionAmount*@RPI/12,2) as InterestAmount
		   ,Round(@EmiAmount,2) as EMIAmount,
			 1 AS RecursionLevel
   union All
   select  dateadd(month,1,DueDate) DueDate
           ,Round(OutStandingAmount - Prin,2) as OutStandingAmount
		   ,Round(@EmiAmount - ((OutStandingAmount - Prin)*@RPI/12),2) as Prin
		   ,Round((OutStandingAmount - Prin)*@RPI/12,2) as InterestAmount
		   ,Round(@EmiAmount,2) as EMIAmount,
		    RecursionLevel + 1
      from sdata  a --where a.vale > 250000
	  where RecursionLevel <@Tenure
)

select DueDate,OutStandingAmount,Prin,InterestAmount,EMIAmount,round(OutStandingAmount-Prin,2) PrincipalAmount from sdata OPTION (MAXRECURSION 0); 
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
