using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _15Jul2024_BusinessLoanNBFCUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "GST",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProcessingFeeTax",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GST",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "ProcessingFeeTax",
                table: "BusinessLoanNBFCUpdate");
        }
    }
}
