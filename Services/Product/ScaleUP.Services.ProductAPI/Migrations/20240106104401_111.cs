using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _111 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SanctionLetterDocId",
                table: "ProductNBFCCompany",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SanctionLetterURL",
                table: "ProductNBFCCompany",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OfferMaxRate",
                table: "ProductAnchorCompany",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SanctionLetterDocId",
                table: "ProductNBFCCompany");

            migrationBuilder.DropColumn(
                name: "SanctionLetterURL",
                table: "ProductNBFCCompany");

            migrationBuilder.DropColumn(
                name: "OfferMaxRate",
                table: "ProductAnchorCompany");
        }
    }
}
