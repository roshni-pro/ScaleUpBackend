using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _09Feb2024Loan2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateLimitURL",
                table: "BlackSoilAccountDetails");

            migrationBuilder.AddColumn<string>(
                name: "NBFCIdentificationCode",
                table: "LoanAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "BlackSoilLoanId",
                table: "BlackSoilAccountDetails",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NBFCIdentificationCode",
                table: "LoanAccounts");

            migrationBuilder.DropColumn(
                name: "BlackSoilLoanId",
                table: "BlackSoilAccountDetails");

            migrationBuilder.AddColumn<string>(
                name: "UpdateLimitURL",
                table: "BlackSoilAccountDetails",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
