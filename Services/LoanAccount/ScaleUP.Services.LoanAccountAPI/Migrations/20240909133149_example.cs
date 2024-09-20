using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class example : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArrangementType",
                table: "BusinessLoanDisbursementDetail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Bounce",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCBounce",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCInterest",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NBFCPenal",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCProcessingFee",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NBFCProcessingFeeType",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Penal",
                table: "BusinessLoanDisbursementDetail",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrangementType",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "Bounce",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "NBFCBounce",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "NBFCInterest",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "NBFCPenal",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "NBFCProcessingFee",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "NBFCProcessingFeeType",
                table: "BusinessLoanDisbursementDetail");

            migrationBuilder.DropColumn(
                name: "Penal",
                table: "BusinessLoanDisbursementDetail");
        }
    }
}
