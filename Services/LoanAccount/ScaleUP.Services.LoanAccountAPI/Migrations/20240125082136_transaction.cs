using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BounceCharge",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "ConvenienceFeeRate",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "CreditDays",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "DelayPenaltyRate",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "GstRate",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "ProcessingFeeRate",
                table: "LoanAccountCredits");

            migrationBuilder.AddColumn<double>(
                name: "BounceCharge",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ConvenienceFeeRate",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "CreditDays",
                table: "AccountTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DelayPenaltyRate",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GstRate",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ProcessingFeeRate",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingFeeType",
                table: "AccountTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPayableBy",
                table: "AccountTransactionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BounceCharge",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "ConvenienceFeeRate",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "CreditDays",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "DelayPenaltyRate",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "GstRate",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "ProcessingFeeRate",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "ProcessingFeeType",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "IsPayableBy",
                table: "AccountTransactionDetails");

            migrationBuilder.AddColumn<double>(
                name: "BounceCharge",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ConvenienceFeeRate",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "CreditDays",
                table: "LoanAccountCredits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DelayPenaltyRate",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GstRate",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ProcessingFeeRate",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
