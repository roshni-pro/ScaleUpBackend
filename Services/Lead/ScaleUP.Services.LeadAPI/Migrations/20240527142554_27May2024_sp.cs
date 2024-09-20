using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _27May2024_sp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"

CREATE OR ALTER   PROCEDURE [dbo].[GetGenerateOfferStatus] 
--Declare
	@leadId bigint =109,
    @ActivityName nvarchar(100)='Generate Offer'
AS
BEGIN
select  sub.IdentificationCode ComapanyName,
		sub.ActivityMasterId,
		sub.SubActivityMasterId,
		sub.SubActivitySequence,
		sub.Code,
		sub.ActivityName,
		sub.Status as SubActivityStatus,
		o.Id as LeadOfferId,
		sub.Id as LeadNBFCSubActivityId,
		o.Status as LeadOfferStatus,
		Isnull(o.CreditLimit,0) CreditLimit,
		o.ErrorMessage,
		o.NBFCCompanyId,
		api.*
from LeadNBFCSubActivitys sub with(nolock)
inner join LeadOffers o with(nolock)  on o.LeadId = sub.LeadId and sub.NBFCCompanyId=o.NBFCCompanyId and o.IsActive=1 and o.IsDeleted=0
outer apply(
	select  api.Id as APIId, 
			api.Code as ApiCode, 
			api.Status as ApiStatus,
			api.APIUrl,
			api.Sequence as APISequence,
			CASE WHEN comnres.Request IS NULL THEN comnres2.Request ELSE comnres.Request END Request,
			CASE WHEN comnres.Response IS NULL THEN comnres2.Response ELSE comnres.Response END Response
	from LeadNBFCApis api  with(nolock)
	outer apply
	(
		select top 1 
		       res.Response
			   ,res.Request 
		from BlackSoilCommonAPIRequestResponses res with(nolock) 
		where  api.Id=res.LeadNBFCApiId and res.LeadId=@leadId 
		and sub.IdentificationCode = 'BlackSoil'
		order by res.id desc
	)comnres
	outer apply
	(
		select top 1 
		      RIGHT(res.Response, 5000) Response
			 ,RIGHT(res.Request, 5000) Request
			   --res.Response
			   --,res.Request
		from ArthMateCommonAPIRequestResponses res with(nolock) 
		where  api.Id=res.LeadNBFCApiId and res.LeadId=@leadId 
		and sub.IdentificationCode = 'ArthMate'
		order by res.id desc
	)comnres2
	where sub.Id = api.LeadNBFCSubActivityId
	and api.IsActive=1 and api.IsDeleted =0
)api
where sub.ActivityName =@ActivityName
and sub.IsActive =1 and sub.IsDeleted =0
and o.IsActive =1 and o.IsDeleted =0
and sub.LeadId =@leadId
order by sub.IdentificationCode, sub.SubActivitySequence, api.APISequence
END

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
