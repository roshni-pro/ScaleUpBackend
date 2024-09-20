using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _21feb_ApplicantName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadNBFCApis_LeadNBFCSubActivitys_LeadNBFCSubActivityId",
                table: "LeadNBFCApis");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantName",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LeadNBFCSubActivityId",
                table: "LeadNBFCApis",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_LeadNBFCApis_LeadNBFCSubActivitys_LeadNBFCSubActivityId",
                table: "LeadNBFCApis",
                column: "LeadNBFCSubActivityId",
                principalTable: "LeadNBFCSubActivitys",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadNBFCApis_LeadNBFCSubActivitys_LeadNBFCSubActivityId",
                table: "LeadNBFCApis");

            migrationBuilder.DropColumn(
                name: "ApplicantName",
                table: "Leads");

            migrationBuilder.AlterColumn<long>(
                name: "LeadNBFCSubActivityId",
                table: "LeadNBFCApis",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeadNBFCApis_LeadNBFCSubActivitys_LeadNBFCSubActivityId",
                table: "LeadNBFCApis",
                column: "LeadNBFCSubActivityId",
                principalTable: "LeadNBFCSubActivitys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
