using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _30jul2024_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "nbfcInterestRate",
                table: "BusinessLoanNBFCUpdate",
                newName: "Commission");

            migrationBuilder.AddColumn<double>(
                name: "Commission",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommissionType",
                table: "nbfcOfferUpdate",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommissionType",
                table: "BusinessLoanNBFCUpdate",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commission",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "CommissionType",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "CommissionType",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.RenameColumn(
                name: "Commission",
                table: "BusinessLoanNBFCUpdate",
                newName: "nbfcInterestRate");
        }
    }
}
