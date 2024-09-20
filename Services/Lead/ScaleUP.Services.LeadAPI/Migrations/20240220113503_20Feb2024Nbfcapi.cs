using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _20Feb2024Nbfcapi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadNBFCApis_LeadNBFCSubActivitys_LeadNBFCSubActivityId",
                table: "LeadNBFCApis");

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
