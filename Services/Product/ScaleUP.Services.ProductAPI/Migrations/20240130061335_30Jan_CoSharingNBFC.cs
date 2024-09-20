using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _30Jan_CoSharingNBFC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBounceChargeCoSharing",
                table: "ProductNBFCCompany",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPenaltyChargeCoSharing",
                table: "ProductNBFCCompany",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPlatformFeeCoSharing",
                table: "ProductNBFCCompany",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessingFeeCoSharing",
                table: "ProductNBFCCompany",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBounceChargeCoSharing",
                table: "ProductNBFCCompany");

            migrationBuilder.DropColumn(
                name: "IsPenaltyChargeCoSharing",
                table: "ProductNBFCCompany");

            migrationBuilder.DropColumn(
                name: "IsPlatformFeeCoSharing",
                table: "ProductNBFCCompany");

            migrationBuilder.DropColumn(
                name: "IsProcessingFeeCoSharing",
                table: "ProductNBFCCompany");
        }
    }
}
