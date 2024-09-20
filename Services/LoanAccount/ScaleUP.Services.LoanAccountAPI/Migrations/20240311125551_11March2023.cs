using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _11March2023 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RemainingExtraPaymentAmount",
                table: "LoanAccountRepayments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RemainingInterestAmount",
                table: "LoanAccountRepayments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RemainingOverdueInterest",
                table: "LoanAccountRepayments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RemainingPenalInterest",
                table: "LoanAccountRepayments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RemainingPrincipalAmount",
                table: "LoanAccountRepayments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RemainingProcessingFees",
                table: "LoanAccountRepayments",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingExtraPaymentAmount",
                table: "LoanAccountRepayments");

            migrationBuilder.DropColumn(
                name: "RemainingInterestAmount",
                table: "LoanAccountRepayments");

            migrationBuilder.DropColumn(
                name: "RemainingOverdueInterest",
                table: "LoanAccountRepayments");

            migrationBuilder.DropColumn(
                name: "RemainingPenalInterest",
                table: "LoanAccountRepayments");

            migrationBuilder.DropColumn(
                name: "RemainingPrincipalAmount",
                table: "LoanAccountRepayments");

            migrationBuilder.DropColumn(
                name: "RemainingProcessingFees",
                table: "LoanAccountRepayments");
        }
    }
}
