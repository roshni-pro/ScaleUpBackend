using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _31Jul2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Commission",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommissionType",
                table: "BusinessLoanDisbursementDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PFType",
                table: "BusinessLoanDisbursementDetail",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commission",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "CommissionType",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "PFType",
                table: "BusinessLoanDisbursementDetail");
        }
    }
}
