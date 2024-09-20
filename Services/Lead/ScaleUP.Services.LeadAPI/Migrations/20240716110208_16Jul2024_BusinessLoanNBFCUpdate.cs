using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _16Jul2024_BusinessLoanNBFCUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOfferRejected",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.AddColumn<int>(
                name: "OfferStatus",
                table: "BusinessLoanNBFCUpdate",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferStatus",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.AddColumn<bool>(
                name: "IsOfferRejected",
                table: "BusinessLoanNBFCUpdate",
                type: "bit",
                nullable: true);
        }
    }
}
