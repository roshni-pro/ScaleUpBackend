using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _05feb_loanaccount2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "LoanAccounts");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccountActive",
                table: "LoanAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlock",
                table: "LoanAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccountActive",
                table: "LoanAccounts");

            migrationBuilder.DropColumn(
                name: "IsBlock",
                table: "LoanAccounts");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "LoanAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
