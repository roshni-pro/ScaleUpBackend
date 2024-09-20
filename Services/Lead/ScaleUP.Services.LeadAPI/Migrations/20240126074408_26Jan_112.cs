using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _26Jan_112 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NBFCCompanyApiId",
                table: "LeadNBFCSubActivitys",
                newName: "NBFCCompanyId");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LeadNBFCApis",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "LeadNBFCApis");

            migrationBuilder.RenameColumn(
                name: "NBFCCompanyId",
                table: "LeadNBFCSubActivitys",
                newName: "NBFCCompanyApiId");
        }
    }
}
