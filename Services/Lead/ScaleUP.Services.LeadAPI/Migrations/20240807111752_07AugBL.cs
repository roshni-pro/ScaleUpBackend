using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _07AugBL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

	CREATE OR ALTER   Procedure [dbo].[BLAccountList]
	--declare
	 @Fromdate datetime='2024-01-31'
	,@ToDate datetime='2024-08-30'
	,@CityName varchar(100)=null
	,@Keyword varchar(100)=null
	,@Skip int=0
	,@Take int=0
	,@AnchorId dbo.Intvalues readonly
	,@Status varchar(100)='ALL'
	,@CityId bigint=0
	,@leadIds dbo.Intvalues readonly
	,@isDSA bit =1
	As
	Begin
	--insert into @leadIds values (508)
	--insert into @AnchorId values (2)
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
	--,isnull(case when b.OfferStatus=1 and l.OfferCompanyId=b.NBFCCompanyId then l.Status else 'NA' end,0) Status
	,COUNT(*) over() as TotalCount
	,isnull(b.LoanId, '') as LoanId,
	Isnull(b.CompanyIdentificationCode,'') NBFCname,
	b.NBFCCompanyId 
	from Leads l with(nolock) 
	join @leadIds c on l.id = c.intValue  
	join CompanyLead cl with(nolock) on cl.LeadId = l.Id
	left join ArthMateUpdates ad with(nolock) on l.Id= ad.LeadId
	left join BusinessLoanNBFCUpdate b with(nolock) on b.LeadId=l.Id  and b.IsActive=1 and b.IsDeleted=0
	join @AnchorId a on a.IntValue = cl.CompanyId
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
	,b.LoanId
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
	--,isnull(case when b.OfferStatus=1 and l.OfferCompanyId=b.NBFCCompanyId then l.Status else 'NA' end,0) Status
	,l.Status
	,COUNT(*) over() as TotalCount
	,isnull(b.LoanId, '') as LoanId
	,Isnull(b.CompanyIdentificationCode,'') NBFCname,
	b.NBFCCompanyId 
	from Leads l with(nolock) 
	join @leadIds c on l.id = c.intValue  
	join CompanyLead cl with(nolock) on cl.LeadId = l.Id
	left join ArthMateUpdates ad with(nolock) on l.Id= ad.LeadId
	--left join nbfcofferupdate b with(nolock) on b.LeadId=l.Id
	left join BusinessLoanNBFCUpdate b with(nolock) on b.LeadId=l.Id  and b.IsActive=1 and b.IsDeleted=0
	join @AnchorId a on a.IntValue = cl.CompanyId
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
	,b.LoanId
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

	CREATE OR ALTER     Procedure [dbo].[NBFCBLAccountList]
	--declare
	 @Fromdate datetime='2024-01-31'
	,@ToDate datetime='2024-08-05'
	,@CityName varchar(100)=null
	,@Keyword varchar(100)=null
	,@Skip int=0
	,@Take int=0
	,@AnchorId dbo.Intvalues readonly
	,@Status varchar(100)='ALL'
	,@CityId bigint=0
	--,@role varchar(100) = 'AYEFIN' --'MASFIN'
	,@NbfcCompanyId bigint=134
	As
	Begin
	--insert into @AnchorId values(2)
	--insert into @AnchorId values(139)
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
	,cast(isnull(b.LoanAmount ,0) as float) as OfferAmount
	,cl.UserUniqueCode as AnchorCode
	,ad.LoanAppId
	,ad.PartnerloanAppId
	,case when l.OfferCompanyId=b.NBFCCompanyId then l.Status else 'NA' end Status
	,COUNT(*) over() as TotalCount
	,isnull(b.LoanId, '') as LoanId,
	Isnull(b.CompanyIdentificationCode,'') NBFCname,
	b.NBFCCompanyId 
	from Leads l with(nolock) 
	join CompanyLead cl with(nolock) on cl.LeadId = l.Id
	inner join LeadOffers o with(nolock) on o.LeadId= l.Id and o.IsActive=1 and o.IsDeleted=0 
	left join ArthMateUpdates ad with(nolock) on l.Id= ad.LeadId
	left join nbfcOfferUpdate b with(nolock) on b.LeadId= l.Id and b.IsActive=1 and b.IsDeleted=0 and b.NBFCCompanyId=@NbfcCompanyId
	--left join BusinessLoanNBFCUpdate b with(nolock) on b.LeadId=l.Id 
	join @AnchorId a on a.IntValue = cl.CompanyId
	Left join LeadLoan bl with(nolock) on bl.LeadMasterId = l.Id
	where l.Status in ('Pending','LoanApproved','LoanInitiated','LoanActivated','LoanRejected','LoanAvailed')
			and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  and (@Status='All' or @Status=l.Status)
				  --and b.CompanyIdentificationCode = @role 
				  and o.NBFCCompanyId=@NbfcCompanyId
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
				  and l.ProductCode='BusinessLoan'
				  --and b.IsActive=1 and b.IsDeleted=0
	group by  l.id 
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
	,b.LoanId
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
	,cast(isnull(b.LoanAmount ,0) as float) as OfferAmount
	,cl.UserUniqueCode as AnchorCode
	,ad.LoanAppId
	,ad.PartnerloanAppId
	,case when l.OfferCompanyId=b.NBFCCompanyId then l.Status else 'NA' end Status
	,COUNT(*) over() as TotalCount
	,isnull(b.LoanId, '') as LoanId
	,Isnull(b.CompanyIdentificationCode,'') NBFCname,
	b.NBFCCompanyId 
	from Leads l with(nolock) 
	join CompanyLead cl with(nolock) on cl.LeadId = l.Id
	inner join LeadOffers o with(nolock) on o.LeadId= l.Id and o.IsActive=1 and o.IsDeleted=0
	left join ArthMateUpdates ad with(nolock) on l.Id= ad.LeadId
	left join nbfcOfferUpdate b with(nolock) on b.LeadId= l.Id and b.IsActive=1 and b.IsDeleted=0 and b.NBFCCompanyId=@NbfcCompanyId
	--left join BusinessLoanNBFCUpdate b with(nolock) on b.LeadId=l.Id
	join @AnchorId a on a.IntValue = cl.CompanyId
	Left join LeadLoan bl with(nolock) on bl.LeadMasterId = l.Id
	where l.Status in ('Pending','LoanApproved','LoanInitiated','LoanActivated','LoanRejected','LoanAvailed')
			and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  and (@Status='All' or @Status=l.Status)
				  --and b.CompanyIdentificationCode = @role 
				  and o.NBFCCompanyId=@NbfcCompanyId
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
	,b.LoanId
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

";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
