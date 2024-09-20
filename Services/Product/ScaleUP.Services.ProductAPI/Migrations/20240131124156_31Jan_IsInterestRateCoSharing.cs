using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _31Jan_IsInterestRateCoSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsProcessingFeeCoSharing",
                table: "ProductNBFCCompany",
                newName: "IsInterestRateCoSharing");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsInterestRateCoSharing",
                table: "ProductNBFCCompany",
                newName: "IsProcessingFeeCoSharing");
        }
    }
}
