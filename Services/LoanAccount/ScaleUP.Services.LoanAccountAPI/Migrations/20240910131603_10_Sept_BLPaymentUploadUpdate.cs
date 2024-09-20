using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _10_Sept_BLPaymentUploadUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "BLPaymentUploads");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "BLPaymentUploads");

            migrationBuilder.AddColumn<bool>(
                name: "IsProcess",
                table: "BLPaymentUploads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "LpiPaid",
                table: "BLPaymentUploads",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcess",
                table: "BLPaymentUploads");

            migrationBuilder.DropColumn(
                name: "LpiPaid",
                table: "BLPaymentUploads");

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "BLPaymentUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "BLPaymentUploads",
                type: "int",
                nullable: true);
        }
    }
}
