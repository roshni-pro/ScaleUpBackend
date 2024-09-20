using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class AccountTransac_15022024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConvenienceFeeType",
                table: "AccountTransactions",
                newName: "InterestType");

            migrationBuilder.RenameColumn(
                name: "ConvenienceFeeRate",
                table: "AccountTransactions",
                newName: "InterestRate");

            migrationBuilder.RenameColumn(
                name: "ConvenienceFeeAmount",
                table: "AccountTransactions",
                newName: "InterestAmountCalculated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InterestType",
                table: "AccountTransactions",
                newName: "ConvenienceFeeType");

            migrationBuilder.RenameColumn(
                name: "InterestRate",
                table: "AccountTransactions",
                newName: "ConvenienceFeeRate");

            migrationBuilder.RenameColumn(
                name: "InterestAmountCalculated",
                table: "AccountTransactions",
                newName: "ConvenienceFeeAmount");
        }
    }
}
