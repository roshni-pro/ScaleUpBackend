using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class VirtualBankDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VirtualAccountNumber",
                table: "LoanBankDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VirtualBankName",
                table: "LoanBankDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VirtualIFSCCode",
                table: "LoanBankDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VirtualUPIId",
                table: "LoanBankDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VirtualAccountNumber",
                table: "LoanBankDetails");

            migrationBuilder.DropColumn(
                name: "VirtualBankName",
                table: "LoanBankDetails");

            migrationBuilder.DropColumn(
                name: "VirtualIFSCCode",
                table: "LoanBankDetails");

            migrationBuilder.DropColumn(
                name: "VirtualUPIId",
                table: "LoanBankDetails");
        }
    }
}
