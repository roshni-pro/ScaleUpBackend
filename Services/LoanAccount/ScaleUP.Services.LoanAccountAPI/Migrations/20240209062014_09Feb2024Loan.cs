using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _09Feb2024Loan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NBFCCompanyAPIFlows_NBFCComapnyAPIMasters_NBFCCompanyAPIMasterId",
                table: "NBFCCompanyAPIFlows");

            migrationBuilder.DropIndex(
                name: "IX_NBFCCompanyAPIFlows_NBFCCompanyAPIMasterId",
                table: "NBFCCompanyAPIFlows");

            migrationBuilder.DropColumn(
                name: "NBFCCompanyAPIMasterId",
                table: "NBFCCompanyAPIFlows");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceUrl",
                table: "BlackSoilAccountTransactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceUrl",
                table: "BlackSoilAccountTransactions");

            migrationBuilder.AddColumn<long>(
                name: "NBFCCompanyAPIMasterId",
                table: "NBFCCompanyAPIFlows",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_NBFCCompanyAPIFlows_NBFCCompanyAPIMasterId",
                table: "NBFCCompanyAPIFlows",
                column: "NBFCCompanyAPIMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_NBFCCompanyAPIFlows_NBFCComapnyAPIMasters_NBFCCompanyAPIMasterId",
                table: "NBFCCompanyAPIFlows",
                column: "NBFCCompanyAPIMasterId",
                principalTable: "NBFCComapnyAPIMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
