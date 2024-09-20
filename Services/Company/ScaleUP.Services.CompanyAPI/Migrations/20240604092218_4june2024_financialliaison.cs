using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.CompanyAPI.Migrations
{
    /// <inheritdoc />
    public partial class _4june2024_financialliaison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "first_Name",
                table: "FinancialLiaisonDetails",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "email_Address",
                table: "FinancialLiaisonDetails",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "contact_No",
                table: "FinancialLiaisonDetails",
                newName: "ContactNo");

            migrationBuilder.RenameColumn(
                name: "Last_Name",
                table: "FinancialLiaisonDetails",
                newName: "EmailAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "FinancialLiaisonDetails",
                newName: "first_Name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "FinancialLiaisonDetails",
                newName: "email_Address");

            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                table: "FinancialLiaisonDetails",
                newName: "Last_Name");

            migrationBuilder.RenameColumn(
                name: "ContactNo",
                table: "FinancialLiaisonDetails",
                newName: "contact_No");
        }
    }
}
