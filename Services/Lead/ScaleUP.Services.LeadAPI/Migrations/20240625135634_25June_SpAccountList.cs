using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _25June_SpAccountList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

	CREATE OR ALTER Procedure [dbo].[BLAccountList]
	--declare
	 @Fromdate datetime='2024-01-31'
	,@ToDate datetime='2024-06-30'
	,@CityName varchar(100)=null
	,@Keyword varchar(100)=null
	,@Skip int=0
	,@Take int=100
	,@AnchorId bigint=0
	,@Status varchar(100)='ALL'
	,@CityId bigint=0,
	@leadIds dbo.Intvalues readonly,
	@isDSA bit =0
	As
	Begin
	--insert into @leadIds values(384)
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

	and ((@isDSA=1 and exists(select 1 from @leadIds c where l.id = c.intValue)) or 
	(@isDSA=0 and (exists(select 1 from @leadIds c where l.id<> c.intValue) or  not exists(select 1 from @leadIds)) ) )

	Left join LeadLoan bl with(nolock) on bl.LeadMasterId = l.Id
	where l.Status in ('Pending','LoanApproved','LoanInitiated','LoanActivated','LoanRejected','LoanAvailed')
			and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  and (cl.CompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and (@Status='All' or @Status=l.Status)
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
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
	order by l.LastModified desc
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
