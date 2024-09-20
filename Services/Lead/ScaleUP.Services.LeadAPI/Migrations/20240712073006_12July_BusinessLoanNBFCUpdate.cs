using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _12July_BusinessLoanNBFCUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "InterestRate",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LoanAmount",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LoanInterestAmount",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MonthlyEMI",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PFDiscount",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProcessingFeeAmount",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProcessingFeeRate",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "LoanAmount",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "LoanInterestAmount",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "MonthlyEMI",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "PFDiscount",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "ProcessingFeeAmount",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "ProcessingFeeRate",
                table: "BusinessLoanNBFCUpdate");
        }
    }
}
