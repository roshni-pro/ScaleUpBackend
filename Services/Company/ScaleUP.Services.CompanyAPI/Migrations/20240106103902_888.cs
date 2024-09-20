using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.CompanyAPI.Migrations
{
    /// <inheritdoc />
    public partial class _888 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GSTDocId",
                table: "Companies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GSTDocumentURL",
                table: "Companies",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MSMEDocId",
                table: "Companies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MSMEDocumentURL",
                table: "Companies",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GSTDocId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "GSTDocumentURL",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MSMEDocId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MSMEDocumentURL",
                table: "Companies");
        }
    }
}
