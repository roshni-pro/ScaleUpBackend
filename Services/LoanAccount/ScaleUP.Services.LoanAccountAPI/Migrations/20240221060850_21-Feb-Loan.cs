using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _21FebLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead");

            migrationBuilder.DropColumn(
                name: "IsOrderPlace",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                table: "PaymentRequest");

            migrationBuilder.RenameColumn(
                name: "AccountTransactionId",
                table: "NBFCComapnyAPIMasters",
                newName: "InvoiceId");

            migrationBuilder.RenameColumn(
                name: "AccountTransactionId",
                table: "BlackSoilAccountTransactions",
                newName: "LoanInvoiceId");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReqNo",
                table: "PaymentRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "PaymentRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "PaymentRequest",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "OrderNo",
                table: "PaymentRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "InvoiceId",
                table: "AccountTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: true),
                    InvoicePdfUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrderAmount = table.Column<double>(type: "float", nullable: false),
                    InvoiceAmount = table.Column<double>(type: "float", nullable: false),
                    TotalTransAmount = table.Column<double>(type: "float", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead",
                column: "LoanAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransactions_InvoiceId",
                table: "AccountTransactions",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountTransactions_Invoices_InvoiceId",
                table: "AccountTransactions",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountTransactions_Invoices_InvoiceId",
                table: "AccountTransactions");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead");

            migrationBuilder.DropIndex(
                name: "IX_AccountTransactions_InvoiceId",
                table: "AccountTransactions");

            migrationBuilder.DropColumn(
                name: "OrderNo",
                table: "PaymentRequest");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "AccountTransactions");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "NBFCComapnyAPIMasters",
                newName: "AccountTransactionId");

            migrationBuilder.RenameColumn(
                name: "LoanInvoiceId",
                table: "BlackSoilAccountTransactions",
                newName: "AccountTransactionId");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReqNo",
                table: "PaymentRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "PaymentRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "PaymentRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<bool>(
                name: "IsOrderPlace",
                table: "PaymentRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "PaymentRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMode",
                table: "PaymentRequest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccountCompanyLead_LoanAccountId",
                table: "LoanAccountCompanyLead",
                column: "LoanAccountId",
                unique: true);
        }
    }
}
