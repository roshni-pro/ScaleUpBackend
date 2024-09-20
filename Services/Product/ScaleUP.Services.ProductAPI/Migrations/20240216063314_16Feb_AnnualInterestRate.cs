using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _16Feb_AnnualInterestRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionFeePayableBy",
                table: "ProductAnchorCompany");

            migrationBuilder.DropColumn(
                name: "TransactionFeeRate",
                table: "ProductAnchorCompany");

            migrationBuilder.RenameColumn(
                name: "InterestRate",
                table: "ProductNBFCCompany",
                newName: "AnnualInterestRate");

            migrationBuilder.RenameColumn(
                name: "TransactionFeeType",
                table: "ProductAnchorCompany",
                newName: "AnnualInterestPayableBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnnualInterestRate",
                table: "ProductNBFCCompany",
                newName: "InterestRate");

            migrationBuilder.RenameColumn(
                name: "AnnualInterestPayableBy",
                table: "ProductAnchorCompany",
                newName: "TransactionFeeType");

            migrationBuilder.AddColumn<string>(
                name: "TransactionFeePayableBy",
                table: "ProductAnchorCompany",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TransactionFeeRate",
                table: "ProductAnchorCompany",
                type: "float",
                nullable: true);
        }
    }
}
