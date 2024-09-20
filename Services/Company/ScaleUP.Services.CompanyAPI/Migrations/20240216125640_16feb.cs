using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.CompanyAPI.Migrations
{
    /// <inheritdoc />
    public partial class _16feb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccountType",
                table: "Companies",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<long>(
                name: "CustomerAgreementDocId",
                table: "Companies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerAgreementURL",
                table: "Companies",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PanDocId",
                table: "Companies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanURL",
                table: "Companies",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAgreementDocId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CustomerAgreementURL",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PanDocId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PanURL",
                table: "Companies");

            migrationBuilder.AlterColumn<string>(
                name: "AccountType",
                table: "Companies",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);
        }
    }
}
