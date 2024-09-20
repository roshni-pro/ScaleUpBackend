using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class addAccountfiled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BounceCharge",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "CreditDays",
                table: "LoanAccountCredits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "CreditLimitAmount",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PenaltyCharge",
                table: "LoanAccountCredits",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GSTAmount",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OrderAmount",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PaidAmount",
                table: "AccountTransactions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BounceCharge",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "CreditDays",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "CreditLimitAmount",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "PenaltyCharge",
                table: "LoanAccountCredits");

            migrationBuilder.DropColumn(
                name: "GSTAmount",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "OrderAmount",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "AccountTransactions");
        }
    }
}
