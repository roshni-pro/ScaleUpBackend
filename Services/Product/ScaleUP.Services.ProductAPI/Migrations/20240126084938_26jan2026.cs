using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _26jan2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NBFCSubActivityApis_NBFCCompanyApis_NBFCCompanyApiId",
                table: "NBFCSubActivityApis");

            migrationBuilder.DropTable(
                name: "NBFCCompanyApis");

            migrationBuilder.CreateTable(
                name: "CompanyApis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
                    IsWebhook = table.Column<bool>(type: "bit", nullable: false),
                    APIUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("PK_CompanyApis", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_NBFCSubActivityApis_CompanyApis_NBFCCompanyApiId",
                table: "NBFCSubActivityApis",
                column: "NBFCCompanyApiId",
                principalTable: "CompanyApis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NBFCSubActivityApis_CompanyApis_NBFCCompanyApiId",
                table: "NBFCSubActivityApis");

            migrationBuilder.DropTable(
                name: "CompanyApis");

            migrationBuilder.CreateTable(
                name: "NBFCCompanyApis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    APIUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NBFCCompanyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NBFCCompanyApis", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_NBFCSubActivityApis_NBFCCompanyApis_NBFCCompanyApiId",
                table: "NBFCSubActivityApis",
                column: "NBFCCompanyApiId",
                principalTable: "NBFCCompanyApis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
