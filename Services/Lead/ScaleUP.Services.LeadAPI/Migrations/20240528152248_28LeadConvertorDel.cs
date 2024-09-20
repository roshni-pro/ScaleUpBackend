using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _28LeadConvertorDel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "LeadConverter",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "LeadGenerator",
                table: "Leads");

            //migrationBuilder.AlterColumn<string>(
            //    name: "ApiToken",
            //    table: "CeplrPdfReports",
            //    type: "nvarchar(100)",
            //    maxLength: 100,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(500)",
            //    oldMaxLength: 500);
            migrationBuilder.DropColumn(
            name: "IsOfferRejected",
            table: "ArthMateUpdates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeadConverter",
                table: "Leads",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeadGenerator",
                table: "Leads",
                type: "nvarchar(500)",
                nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ApiToken",
            //    table: "CeplrPdfReports",
            //    type: "nvarchar(500)",
            //    maxLength: 500,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(100)",
            //    oldMaxLength: 100);
            migrationBuilder.AddColumn<bool>(
              name: "IsOfferRejected",
              table: "ArthMateUpdates",
              type: "bit",
              nullable: true);
        }
    }
}
