using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class addNewAccountfiled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PenaltyCharge",
                table: "LoanAccountCredits",
                newName: "DelayPenaltyRate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDate",
                table: "LoanAccounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DisbursalDate",
                table: "LoanAccounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "CreditDays",
                table: "LoanAccountCredits",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationDate",
                table: "LoanAccounts");

            migrationBuilder.DropColumn(
                name: "DisbursalDate",
                table: "LoanAccounts");

            migrationBuilder.RenameColumn(
                name: "DelayPenaltyRate",
                table: "LoanAccountCredits",
                newName: "PenaltyCharge");

            migrationBuilder.AlterColumn<int>(
                name: "CreditDays",
                table: "LoanAccountCredits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
