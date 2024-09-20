using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _06Aug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "InsuranceAmount",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OtherCharges",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceAmount",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "OtherCharges",
                table: "BusinessLoanDisbursementDetail");
        }
    }
}
