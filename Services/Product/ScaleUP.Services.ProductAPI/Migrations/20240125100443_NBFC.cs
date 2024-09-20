using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class NBFC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NBFCCompanyApis_NBFCCompanyApiTypes_NBFCCompanyApiTypeId",
                table: "NBFCCompanyApis");

            migrationBuilder.DropIndex(
                name: "IX_NBFCCompanyApis_NBFCCompanyApiTypeId",
                table: "NBFCCompanyApis");

            migrationBuilder.DropColumn(
                name: "NBFCCompanyApiTypeId",
                table: "NBFCCompanyApis");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NBFCCompanyApiTypeId",
                table: "NBFCCompanyApis",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_NBFCCompanyApis_NBFCCompanyApiTypeId",
                table: "NBFCCompanyApis",
                column: "NBFCCompanyApiTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_NBFCCompanyApis_NBFCCompanyApiTypes_NBFCCompanyApiTypeId",
                table: "NBFCCompanyApis",
                column: "NBFCCompanyApiTypeId",
                principalTable: "NBFCCompanyApiTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
