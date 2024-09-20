using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.CompanyAPI.Migrations
{
    /// <inheritdoc />
    public partial class LoanCompanyInvoiceUrl_14May : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NBFCInvoiceTemplateDocId",
                table: "Companies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NBFCInvoiceTemplateURL",
                table: "Companies",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NBFCInvoiceTemplateDocId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "NBFCInvoiceTemplateURL",
                table: "Companies");
        }
    }
}
