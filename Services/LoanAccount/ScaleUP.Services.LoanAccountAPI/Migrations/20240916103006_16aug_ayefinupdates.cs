using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _16aug_ayefinupdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "AyeFinanceUpdates");

            migrationBuilder.RenameColumn(
                name: "totalLimit",
                table: "AyeFinanceUpdates",
                newName: "totallimit");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "AyeFinanceUpdates",
                newName: "orderId");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "AyeFinanceUpdates",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoiceNo",
                table: "AyeFinanceUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoiceNo",
                table: "AyeFinanceUpdates");

            migrationBuilder.RenameColumn(
                name: "totallimit",
                table: "AyeFinanceUpdates",
                newName: "totalLimit");

            migrationBuilder.RenameColumn(
                name: "orderId",
                table: "AyeFinanceUpdates",
                newName: "code");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "AyeFinanceUpdates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                table: "AyeFinanceUpdates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
