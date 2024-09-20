using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class BlackSoilUpdateAddFeild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AgreementDocId",
                table: "BlackSoilUpdates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationCode",
                table: "BlackSoilUpdates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ApplicationId",
                table: "BlackSoilUpdates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BussinessCode",
                table: "BlackSoilUpdates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SanctionDocId",
                table: "BlackSoilUpdates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SingingUrl",
                table: "BlackSoilUpdates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StampId",
                table: "BlackSoilUpdates",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgreementDocId",
                table: "BlackSoilUpdates");

            migrationBuilder.DropColumn(
                name: "ApplicationCode",
                table: "BlackSoilUpdates");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "BlackSoilUpdates");

            migrationBuilder.DropColumn(
                name: "BussinessCode",
                table: "BlackSoilUpdates");

            migrationBuilder.DropColumn(
                name: "SanctionDocId",
                table: "BlackSoilUpdates");

            migrationBuilder.DropColumn(
                name: "SingingUrl",
                table: "BlackSoilUpdates");

            migrationBuilder.DropColumn(
                name: "StampId",
                table: "BlackSoilUpdates");
        }
    }
}
