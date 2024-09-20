using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _13May2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuccess",
                table: "BusinessLoans");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "BusinessLoans");

            migrationBuilder.DropColumn(
                name: "ReponseId",
                table: "BusinessLoans");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "BusinessLoans");

            migrationBuilder.AddColumn<long>(
                name: "LoanAccountId",
                table: "BusinessLoans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoanAccountId",
                table: "BusinessLoans");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuccess",
                table: "BusinessLoans",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "BusinessLoans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "ReponseId",
                table: "BusinessLoans",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RequestId",
                table: "BusinessLoans",
                type: "bigint",
                nullable: true);
        }
    }
}
