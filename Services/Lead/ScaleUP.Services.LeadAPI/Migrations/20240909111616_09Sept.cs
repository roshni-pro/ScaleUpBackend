using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _09Sept : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArrangementType",
                table: "nbfcOfferUpdate",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Bounce",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCBounce",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCInterest",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NBFCPenal",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCProcessingFee",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NBFCProcessingFeeType",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Penal",
                table: "nbfcOfferUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArrangementType",
                table: "BusinessLoanNBFCUpdate",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Bounce",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCBounce",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCInterest",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NBFCPenal",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NBFCProcessingFee",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NBFCProcessingFeeType",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Penal",
                table: "BusinessLoanNBFCUpdate",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrangementType",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "Bounce",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCBounce",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCInterest",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCPenal",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCProcessingFee",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCProcessingFeeType",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "Penal",
                table: "nbfcOfferUpdate");

            migrationBuilder.DropColumn(
                name: "ArrangementType",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "Bounce",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCBounce",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCInterest",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCPenal",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCProcessingFee",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "NBFCProcessingFeeType",
                table: "BusinessLoanNBFCUpdate");

            migrationBuilder.DropColumn(
                name: "Penal",
                table: "BusinessLoanNBFCUpdate");
        }
    }
}
