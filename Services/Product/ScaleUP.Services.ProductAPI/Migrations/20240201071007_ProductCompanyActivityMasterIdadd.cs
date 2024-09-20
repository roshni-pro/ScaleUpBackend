using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class ProductCompanyActivityMasterIdadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProductCompanyActivityMasterId",
                table: "NBFCSubActivityApis",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCompanyActivityMasterId",
                table: "NBFCSubActivityApis");
        }
    }
}
