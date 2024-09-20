using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _07Aug2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "brokenPeriodinterestAmount",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "brokenPeriodinterestAmount",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "brokenPeriodinterestAmount",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "brokenPeriodinterestAmount",
                table: "BusinessLoanNBFCUpdate");
        }
    }
}
