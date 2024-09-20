using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _27July_InvoiceDisbursalProcesseds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TDSAmount",
                table: "InvoiceDisbursalProcesseds",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UTRNumber",
                table: "InvoiceDisbursalProcesseds",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TDSAmount",
                table: "InvoiceDisbursalProcesseds");

            migrationBuilder.DropColumn(
                name: "UTRNumber",
                table: "InvoiceDisbursalProcesseds");
        }
    }
}
