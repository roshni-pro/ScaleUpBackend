using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _01feb_paymentrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NBFCCompanyId",
                table: "PaymentRequest");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "PaymentRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "IsOrderPlace",
                table: "PaymentRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOrderPlace",
                table: "PaymentRequest");

            migrationBuilder.AlterColumn<long>(
                name: "OrderId",
                table: "PaymentRequest",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "NBFCCompanyId",
                table: "PaymentRequest",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
