using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _10feb2024BlackSoilDe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackSoilWebhookResponses_LoanAccounts_LoanAccountId",
                table: "BlackSoilWebhookResponses");

            migrationBuilder.AlterColumn<long>(
                name: "LoanAccountId",
                table: "BlackSoilWebhookResponses",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BlackSoilAccountTransactions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceUrl",
                table: "BlackSoilAccountTransactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<long>(
                name: "WithdrawalId",
                table: "BlackSoilAccountTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WithdrawalUrl",
                table: "BlackSoilAccountTransactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackSoilWebhookResponses_LoanAccounts_LoanAccountId",
                table: "BlackSoilWebhookResponses",
                column: "LoanAccountId",
                principalTable: "LoanAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlackSoilWebhookResponses_LoanAccounts_LoanAccountId",
                table: "BlackSoilWebhookResponses");

            migrationBuilder.DropColumn(
                name: "WithdrawalId",
                table: "BlackSoilAccountTransactions");

            migrationBuilder.DropColumn(
                name: "WithdrawalUrl",
                table: "BlackSoilAccountTransactions");

            migrationBuilder.AlterColumn<long>(
                name: "LoanAccountId",
                table: "BlackSoilWebhookResponses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "BlackSoilAccountTransactions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceUrl",
                table: "BlackSoilAccountTransactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlackSoilWebhookResponses_LoanAccounts_LoanAccountId",
                table: "BlackSoilWebhookResponses",
                column: "LoanAccountId",
                principalTable: "LoanAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
