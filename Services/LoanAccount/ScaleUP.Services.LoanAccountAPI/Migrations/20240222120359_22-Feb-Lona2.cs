using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _22FebLona2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LoanAccountId",
                table: "Invoices",
                column: "LoanAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_LoanAccounts_LoanAccountId",
                table: "Invoices",
                column: "LoanAccountId",
                principalTable: "LoanAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_LoanAccounts_LoanAccountId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_LoanAccountId",
                table: "Invoices");
        }
    }
}
