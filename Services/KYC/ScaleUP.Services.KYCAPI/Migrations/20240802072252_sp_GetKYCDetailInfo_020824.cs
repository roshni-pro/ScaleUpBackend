using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.KYCAPI.Migrations
{
    /// <inheritdoc />
    public partial class sp_GetKYCDetailInfo_020824 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @" 
CREATE OR ALTER Proc [dbo].[GetKYCDetailInfo] 
--declare
@Timestamp DATETIME ='2024-07-26 11:14:06.1764087',
@KYCMasterInfoId bigint=1510
As 
Begin
	Select  kycD.id As kycDetailID, kycD.FieldValue , d.Field as KYCDetails_Field, d.FieldInfoType , m.Code As KYCMasters_ActivityType , a.id, a.Changes, a.Action 
	from KYCDetailInfos kycD with(nolock) 
	Inner Join KYCDetails d  with(nolock)  ON d.Id=kycD.KYCDetailId and d.IsActive =1 and d.IsDeleted =0
	Inner join KYCMasters m  with(nolock)  ON m.id=d.KYCMasterId and m.IsActive =1 and m.IsDeleted =0
	Inner Join AuditLogs a   with(nolock)  ON a.EntityName='KYCDetailInfo' and a.EntityId = kycD.id
	Where kycD.IsActive=1 and kycD.IsDeleted =0 and a.Timestamp  > DATEADD(second, -30, GETDATE()) and kycD.KYCMasterInfoId=@KYCMasterInfoId 
	and a.Action IN ('Modified','Added')
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
