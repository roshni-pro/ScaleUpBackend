using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class sp_GetLeadUpdateHistorie_GetLeadDetailInfoForBank_06082024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @" 

CREATE OR ALTER   Proc [dbo].[GetLeadUpdateHistorie] 
--Declare
@LeadID bigint = 432
, @EventName varchar(100) =''
,@Skip int=0
,@Take int=10
As
Begin
	select Id ,LeadId ,UserId ,UserName ,EventName ,Narration ,NarrationHTML , Created as CreateDate 
	from LeadUpdateHistories
	Where LeadId =@LeadID
	And (@EventName='' OR EventName=@EventName)
	order by Id Desc 
	OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
End

GO


CREATE OR ALTER   Proc [dbo].[GetLeadDetailInfoForBank] 
--declare
@Timestamp DATETIME ='2024-08-05 11:52:02.251'
, @leadId bigint=499
As 
Begin
	select a.EntityId, a.EntityName, a.Changes, a.Action 
	From AuditLogs a  with(nolock)
	Inner Join (
		select Id, LeadId, DocumentType as Type from LeadDocumentDetails with(nolock) Where IsActive =1 and IsDeleted =0 and LeadId =@leadId 
		UNION
		select Id, LeadId, Type  from LeadBankDetails  with(nolock) Where IsActive =1 and IsDeleted =0 and  LeadId =@leadId 
		)  B ON  b.Id = a.EntityId
	Where a.EntityName in ('LeadBankDetail', 'LeadDocumentDetail') 
	and b.LeadId=@leadId
	And a.Timestamp  > DATEADD(second, -30, @Timestamp)  
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
