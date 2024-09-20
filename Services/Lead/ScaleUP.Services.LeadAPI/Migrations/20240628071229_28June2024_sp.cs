using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _28June2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
CREATE OR ALTER   Procedure [dbo].[GetSCAccountList]
	 --declare
	 @Fromdate datetime='2024-01-31'
	,@ToDate datetime='2024-05-30'
	,@CityName varchar(100)=null
	,@Keyword varchar(100)=null
	,@Skip int=0
	,@Take int=1000
	,@AnchorId bigint=0
	,@Status varchar(100)='All'
	,@CityId bigint=0
	As
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
	,CAST(l.CreditLimit as float) OfferAmount 
	,l.Created as CreatedDate
	,l.LastModified as ModifiedDate
	--,'' as Status
	,l.Status
	--,l.AnchorCompanyId
	,l.CityId
	,COUNT(*) over() as TotalCount
	from Leads l with(nolock)
	Inner join CompanyLead cl with(nolock) on l.Id=cl.LeadId and cl.IsActive=1 and cl.IsDeleted=0
	where l.Status in ('pending','lineinitiated','lineactivated','lineapproved','linerejected')
			and ((l.ApplicantName like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.MobileNo like '%'+@Keyword+'%' or ISNULL(@Keyword,'')='')
				  or (l.LeadCode like '%'+@Keyword+'%' or ISNULL(@Keyword,'')=''))
				  and (cl.CompanyId=@AnchorId or Isnull(@AnchorId,0)=0)
				  and CAST(l.Created as date)>=@FromDate and CAST(l.Created as date)<=@ToDate
				  and (@Status='All' or @Status=l.Status)
				  And (l.CityId=@CityId or Isnull(@CityId,0)=0)
	order by l.LastModified 
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
