using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _24May2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER Proc [dbo].[GetLoanData]
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
	,isnull(ld.loan_id, '') as LoanId
	from Leads k 
	inner join ArthMateUpdates au with(nolock) on au.leadid=k.Id
	Inner join (Select Top 1 * From CoLenderResponse Where LeadMasterId  = @leadmasterid and status ='success' and IsActive =1 and IsDeleted =0 Order by Id Desc) adm 
	ON k.Id = adm.LeadMasterId
	Left outer join leadloan ld ON k.Id=ld.LeadMasterId and ld.IsActive=1 and ld.IsDeleted=0
	where k.Id = @leadmasterid --and adm.IsActive =1 and adm.IsDeleted =0 and adm.status ='success'
End

GO


	ALTER Procedure [dbo].[BLAccountList]
	--declare
	 @Fromdate datetime='2024-01-31'
	,@ToDate datetime='2024-05-30'
	,@CityName varchar(100)=null
	,@Keyword varchar(100)=null
	,@Skip int=0
	,@Take int=100
	,@AnchorId bigint=0
	,@Status varchar(100)='ALL'
	,@CityId bigint=0
	As
	Begin
	select 
	l.id as leadId
	,l.UserName as userId
	 ,l.LeadCode
	,l.ApplicantName
	,l.MobileNo
	,l.Created as CreatedDate
	,cl.AnchorName
	,l.CityId
	,'' as CityName
	,l.LastModified as ModifiedDate
	,cast(isnull(bl.sanction_amount ,0) as float) as OfferAmount
	,cl.UserUniqueCode as AnchorCode
	,ad.LoanAppId
	,ad.PartnerloanAppId
	,l.Status
	,COUNT(*) over() as TotalCount
	,isnull(bl.loan_id, '') as LoanId
	from Leads l with(nolock)  
	inner join ArthMateUpdates ad with(nolock) on l.Id= ad.LeadId
	join CompanyLead cl with(nolock) on cl.LeadId = l.Id
	Left join LeadLoan bl with(nolock) on bl.LeadMasterId = l.Id
	where l.Status in ('Pending','LoanApproved','LoanInitiated','LoanActivated','LoanRejected','LoanAvailed')
			and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  and (cl.CompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and (@Status='All' or @Status=l.Status)
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
	order by l.LastModified desc
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	End
Go

";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
