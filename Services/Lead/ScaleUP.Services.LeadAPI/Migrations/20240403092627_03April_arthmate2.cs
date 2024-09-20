using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _03April_arthmate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "partner_loan_app_id",
                table: "ArthMateUpdates",
                newName: "borrowerId");

            migrationBuilder.RenameColumn(
                name: "partner_borrower_id",
                table: "ArthMateUpdates",
                newName: "PartnerloanAppId");

            migrationBuilder.RenameColumn(
                name: "loan_app_id",
                table: "ArthMateUpdates",
                newName: "PartnerborrowerId");

            migrationBuilder.RenameColumn(
                name: "borrower_id",
                table: "ArthMateUpdates",
                newName: "LoanAppId");

            migrationBuilder.AddColumn<string>(
                name: "AScoreRequestId",
                table: "ArthMateUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CeplerCustomerId",
                table: "ArthMateUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CeplerRequestId",
                table: "ArthMateUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ColenderAssignmentId",
                table: "ArthMateUpdates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ColenderCreatedDate",
                table: "ArthMateUpdates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "ColenderLoanAmount",
                table: "ArthMateUpdates",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ColenderPricing",
                table: "ArthMateUpdates",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ColenderProgramType",
                table: "ArthMateUpdates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColenderRequestId",
                table: "ArthMateUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColenderStatus",
                table: "ArthMateUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AScoreRequestId",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "CeplerCustomerId",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "CeplerRequestId",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "ColenderAssignmentId",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "ColenderCreatedDate",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "ColenderLoanAmount",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "ColenderPricing",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "ColenderProgramType",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "ColenderRequestId",
                table: "ArthMateUpdates");

            migrationBuilder.DropColumn(
                name: "ColenderStatus",
                table: "ArthMateUpdates");

            migrationBuilder.RenameColumn(
                name: "borrowerId",
                table: "ArthMateUpdates",
                newName: "partner_loan_app_id");

            migrationBuilder.RenameColumn(
                name: "PartnerloanAppId",
                table: "ArthMateUpdates",
                newName: "partner_borrower_id");

            migrationBuilder.RenameColumn(
                name: "PartnerborrowerId",
                table: "ArthMateUpdates",
                newName: "loan_app_id");

            migrationBuilder.RenameColumn(
                name: "LoanAppId",
                table: "ArthMateUpdates",
                newName: "borrower_id");
        }
    }
}
