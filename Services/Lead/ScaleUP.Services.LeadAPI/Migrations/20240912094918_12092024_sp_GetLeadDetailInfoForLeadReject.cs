using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _12092024_sp_GetLeadDetailInfoForLeadReject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @" '
CREATE OR ALTER   Proc [dbo].[GetLeadDetailInfoForLeadReject] 
--declare
@Timestamp DATETIME ='2024-09-10 13:30:28.405'
, @leadId bigint=613
As 
Begin
	select a.EntityId, Case When a.EntityName ='LeadActivityMasterProgresses' then a.EntityName + '-'+ b.Type else  a.EntityName end EntityName
	, a.Changes, a.Action 
	From AuditLogs a  with(nolock)
	Left Outer Join (
		select Id, leadmasterid, ActivityMasterName +space(1)+ SubActivityMasterName as Type from LeadActivityMasterProgresses with(nolock) Where --IsActive =1 and IsDeleted =0 and 
		leadmasterid =@leadId 
		UNION
		select Id, Id as LeadId, Status as Type  from Leads  with(nolock) Where IsActive =1 and IsDeleted =0 and  Id =@leadId 
		)  B ON  b.Id = a.EntityId
	Where --a.EntityName in ('LeadActivityMasterProgresses', 'Leads') 
	 b.leadmasterid=@leadId
	and a.Timestamp  > DATEADD(second, -30, @Timestamp)  
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
