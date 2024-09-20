using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class SP_GetSCAccountList_16july2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

    CREATE or ALTER Procedure [dbo].[GetSCAccountList]
	--declare
	 @Fromdate datetime='2024-01-31'
	,@ToDate datetime='2024-07-09'
	,@CityName varchar(100)=null
	,@Keyword varchar(100)=null
	,@Skip int=0
	,@Take int=0
	,@AnchorId bigint=2
	,@Status varchar(100)='All'
	,@CityId bigint=0
	As
	Begin
	if (@Skip = 0 and @Take = 0)
	Begin
	select 
	l.id as leadId
	,l.UserName as userId
	,l.LeadCode
	,l.ApplicantName
	,l.MobileNo
	,cl.AnchorName
	,'' as CityName
	,cl.UserUniqueCode as AnchorCode
	,Isnull(CAST(l.CreditLimit as float),0) OfferAmount 
	,l.Created as CreatedDate
	,l.LastModified as ModifiedDate
	--,'' as Status
	,l.Status
	--,l.AnchorCompanyId
	,l.CityId
	,COUNT(*) over() as TotalCount
	from Leads l with(nolock)
	Inner join CompanyLead cl with(nolock) on l.Id=cl.LeadId and cl.IsActive=1 and cl.IsDeleted=0
	where l.Status in ('pending','lineinitiated','lineactivated','lineapproved','linerejected','LoanAvailed')
			 and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  and (cl.CompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
				  and (@Status='All' or @Status=l.Status)
				  And (l.CityId=@CityId or Isnull(@CityId,0)=0)
				  and l.ProductCode='CreditLine'
	order by l.LastModified 
	end
	else 
	Begin
	select 
	l.id as leadId
	,l.UserName as userId
	,l.LeadCode
	,l.ApplicantName
	,l.MobileNo
	,cl.AnchorName
	,'' as CityName
	,cl.UserUniqueCode as AnchorCode
	,Isnull(CAST(l.CreditLimit as float),0) OfferAmount 
	,l.Created as CreatedDate
	,l.LastModified as ModifiedDate
	--,'' as Status
	,l.Status
	--,l.AnchorCompanyId
	,l.CityId
	,COUNT(*) over() as TotalCount
	from Leads l with(nolock)
	Inner join CompanyLead cl with(nolock) on l.Id=cl.LeadId and cl.IsActive=1 and cl.IsDeleted=0
	where l.Status in ('pending','lineinitiated','lineactivated','lineapproved','linerejected','LoanAvailed')
			 and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  and (cl.CompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
				  and (@Status='All' or @Status=l.Status)
				  And (l.CityId=@CityId or Isnull(@CityId,0)=0)
				  and l.ProductCode='CreditLine'
	order by l.LastModified 
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	end
	End

            ";
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
