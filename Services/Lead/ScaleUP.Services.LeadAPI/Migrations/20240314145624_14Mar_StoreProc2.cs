using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _14Mar_StoreProc2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp1 = @"

GO
CREATE OR ALTER Procedure [dbo].[GetLeadActivityHistory]
@LeadId bigint
As
Begin
select  a.ActivityMasterName,a.SubActivityMasterName,b.UserId,b.Timestamp,b.Changes,b.Action,'' as UserName from LeadActivityMasterProgresses a
left join AuditLogs b on a.Id=b.EntityId and b.EntityName='LeadActivityMasterProgresses'
where LeadMasterId=@LeadId and a.ActivityMasterName not in('MobileOtp','MyAccount') and b.Changes is not null and b.Changes!=''
order by a.Sequence,Timestamp
End

";

            migrationBuilder.Sql(sp1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
