using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _26_Apr_LeadBorroBankDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Beneficiary_AccountNumber",
                table: "LeadBankDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Beneficiary_Accountholdername",
                table: "LeadBankDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Beneficiary_Bankname",
                table: "LeadBankDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Beneficiary_IFSCCode",
                table: "LeadBankDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Beneficiary_Typeofaccount",
                table: "LeadBankDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Beneficiary_AccountNumber",
                table: "LeadBankDetails");

            migrationBuilder.DropColumn(
                name: "Beneficiary_Accountholdername",
                table: "LeadBankDetails");

            migrationBuilder.DropColumn(
                name: "Beneficiary_Bankname",
                table: "LeadBankDetails");

            migrationBuilder.DropColumn(
                name: "Beneficiary_IFSCCode",
                table: "LeadBankDetails");

            migrationBuilder.DropColumn(
                name: "Beneficiary_Typeofaccount",
                table: "LeadBankDetails");
        }
    }
}
