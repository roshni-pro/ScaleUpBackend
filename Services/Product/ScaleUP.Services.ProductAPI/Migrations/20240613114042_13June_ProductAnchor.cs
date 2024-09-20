using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _13June_ProductAnchor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IseSignEnable",
                table: "ProductAnchorCompany",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxInterestRate",
                table: "ProductAnchorCompany",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IseSignEnable",
                table: "ProductAnchorCompany");

            migrationBuilder.DropColumn(
                name: "MaxInterestRate",
                table: "ProductAnchorCompany");
        }
    }
}
