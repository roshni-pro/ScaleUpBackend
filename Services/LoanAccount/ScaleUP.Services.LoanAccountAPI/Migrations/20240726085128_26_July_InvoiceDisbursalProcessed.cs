using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _26_July_InvoiceDisbursalProcessed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TDSAmount",
                table: "InvoiceDisbursalProcesseds");

            migrationBuilder.RenameColumn(
                name: "UTRNumber",
                table: "InvoiceDisbursalProcesseds",
                newName: "TopUpNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TopUpNumber",
                table: "InvoiceDisbursalProcesseds",
                newName: "UTRNumber");

            migrationBuilder.AddColumn<double>(
                name: "TDSAmount",
                table: "InvoiceDisbursalProcesseds",
                type: "float",
                nullable: true);
        }
    }
}
