using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _16July_ProductSlab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ProductSlabConfigurations",
                newName: "MinValue");

            migrationBuilder.RenameColumn(
                name: "Min",
                table: "ProductSlabConfigurations",
                newName: "MinLoanAmount");

            migrationBuilder.RenameColumn(
                name: "Max",
                table: "ProductSlabConfigurations",
                newName: "MaxValue");

            migrationBuilder.AddColumn<double>(
                name: "MaxLoanAmount",
                table: "ProductSlabConfigurations",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SharePercentage",
                table: "ProductSlabConfigurations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MaxBounceCharges",
                table: "ProductNBFCCompany",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MaxPenaltyCharges",
                table: "ProductNBFCCompany",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxLoanAmount",
                table: "ProductSlabConfigurations");

            migrationBuilder.DropColumn(
                name: "SharePercentage",
                table: "ProductSlabConfigurations");

            migrationBuilder.DropColumn(
                name: "MaxBounceCharges",
                table: "ProductNBFCCompany");

            migrationBuilder.DropColumn(
                name: "MaxPenaltyCharges",
                table: "ProductNBFCCompany");

            migrationBuilder.RenameColumn(
                name: "MinValue",
                table: "ProductSlabConfigurations",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "MinLoanAmount",
                table: "ProductSlabConfigurations",
                newName: "Min");

            migrationBuilder.RenameColumn(
                name: "MaxValue",
                table: "ProductSlabConfigurations",
                newName: "Max");
        }
    }
}
