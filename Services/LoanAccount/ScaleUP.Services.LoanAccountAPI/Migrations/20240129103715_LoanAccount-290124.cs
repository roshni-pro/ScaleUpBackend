using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class LoanAccount290124 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead",
                column: "LoanAccountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead",
                column: "LoanAccountId");
        }
    }
}
