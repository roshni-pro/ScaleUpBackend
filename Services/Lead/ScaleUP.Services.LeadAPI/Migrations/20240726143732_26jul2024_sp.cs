using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _26jul2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER Procedure [dbo].[BLAccountList]
	--declare
	 @Fromdate datetime='2024-01-31'
	,@ToDate datetime='2024-07-30'
	,@CityName varchar(100)=null
	,@Keyword varchar(100)=null
	,@Skip int=0
	,@Take int=0
	,@AnchorId dbo.Intvalues readonly
	,@Status varchar(100)='ALL'
	,@CityId bigint=0,
	@leadIds dbo.Intvalues readonly
	,@isDSA bit =0
	As
	Begin
	--insert into @leadIds values(463)
	--insert into @AnchorId values(1)
	--insert into @AnchorId values(125)
	if (@Skip = 0 and @Take = 0)
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
	--,cast(isnull(bl.sanction_amount ,0) as float) as OfferAmount
	,isnull(case when b.OfferStatus=1 and l.OfferCompanyId=b.NBFCCompanyId then b.LoanAmount else bl.sanction_amount end,0) OfferAmount
	,cl.UserUniqueCode as AnchorCode
	,ad.LoanAppId
	,ad.PartnerloanAppId
	,l.Status
	,COUNT(*) over() as TotalCount
	,isnull(bl.loan_id, '') as LoanId,
	Isnull(b.CompanyIdentificationCode,'') NBFCname
	from Leads l with(nolock) 
	join @leadIds c on l.id = c.intValue  
	join CompanyLead cl with(nolock) on cl.LeadId = l.Id
	left join ArthMateUpdates ad with(nolock) on l.Id= ad.LeadId
	left join BusinessLoanNBFCUpdate b with(nolock) on b.LeadId=l.Id
	join @AnchorId a on a.IntValue = cl.CompanyId
	--and ((@isDSA=1 and exists(select 1 from @leadIds c where l.id = c.intValue)) or 
	--(@isDSA=0 and not exists(select 1 from @leadIds c where l.id = c.intValue))) -- or  not exists(select 1 from @leadIds)
	Left join LeadLoan bl with(nolock) on bl.LeadMasterId = l.Id
	where l.Status in ('Pending','LoanApproved','LoanInitiated','LoanActivated','LoanRejected','LoanAvailed')
			and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  --and (cl.CompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and (@Status='All' or @Status=l.Status)
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
				  and l.ProductCode='BusinessLoan'
	group by l.id 
	,l.UserName 
	 ,l.LeadCode
	,l.ApplicantName
	,l.MobileNo
	,l.Created 
	,cl.AnchorName
	,l.CityId
	,l.LastModified 
	,bl.sanction_amount
	,cl.UserUniqueCode 
	,ad.LoanAppId
	,ad.PartnerloanAppId
	,l.Status
	,bl.loan_id
	,b.CompanyIdentificationCode
	,b.OfferStatus
	,l.OfferCompanyId
	,b.NBFCCompanyId
	,b.LoanAmount
	order by l.LastModified desc
	end
	else
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
	--,cast(isnull(bl.sanction_amount ,0) as float) as OfferAmount
	,isnull(case when b.OfferStatus=1 and l.OfferCompanyId=b.NBFCCompanyId then b.LoanAmount else bl.sanction_amount end,0) OfferAmount
	,cl.UserUniqueCode as AnchorCode
	,ad.LoanAppId
	,ad.PartnerloanAppId
	,l.Status
	,COUNT(*) over() as TotalCount
	,isnull(bl.loan_id, '') as LoanId
	,Isnull(b.CompanyIdentificationCode,'') NBFCname
	from Leads l with(nolock) 
	join @leadIds c on l.id = c.intValue  
	join CompanyLead cl with(nolock) on cl.LeadId = l.Id
	left join ArthMateUpdates ad with(nolock) on l.Id= ad.LeadId
	left join BusinessLoanNBFCUpdate b with(nolock) on b.LeadId=l.Id
	join @AnchorId a on a.IntValue = cl.CompanyId
	--and ((@isDSA=1 and exists(select 1 from @leadIds c where l.id = c.intValue)) or 
	--(@isDSA=0 and not exists(select 1 from @leadIds c where l.id = c.intValue))) -- or  not exists(select 1 from @leadIds)
	Left join LeadLoan bl with(nolock) on bl.LeadMasterId = l.Id
	where l.Status in ('Pending','LoanApproved','LoanInitiated','LoanActivated','LoanRejected','LoanAvailed')
			and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  --and (cl.CompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and (@Status='All' or @Status=l.Status)
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
				  and l.ProductCode='BusinessLoan'
	group by l.id 
	,l.UserName 
	 ,l.LeadCode
	,l.ApplicantName
	,l.MobileNo
	,l.Created 
	,cl.AnchorName
	,l.CityId
	,l.LastModified 
	,bl.sanction_amount
	,cl.UserUniqueCode 
	,ad.LoanAppId
	,ad.PartnerloanAppId
	,l.Status
	,bl.loan_id
	,b.CompanyIdentificationCode
	,b.OfferStatus
	,l.OfferCompanyId
	,b.NBFCCompanyId
	,b.LoanAmount
	order by l.LastModified desc
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	end
	End

GO

CREATE OR ALTER Proc [dbo].[GetAcceptedLoanDetail]
--declare
@leadmasterid bigint,
@NBFCCompanyId bigint
as
Begin 
	select 
	Isnull(b.LoanId, '') as LoanId
	,Isnull(b.LoanAmount, 0) as LoanAmount
	--,Isnull(b.InterestRate, 0) as InterestRate
	,Isnull(case when Isnull(k.InterestRate,0)>0 then k.InterestRate else b.InterestRate end,0) InterestRate
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
