using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _25Jan2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TAPIKey",
                table: "ProductNBFCCompany",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TAPISecretKey",
                table: "ProductNBFCCompany",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NBFCCompanyApiTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_NBFCCompanyApiTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NBFCCompanyApis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NBFCCompanyId = table.Column<long>(type: "bigint", nullable: false),
                    NBFCCompanyApiTypeId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_NBFCCompanyApis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NBFCCompanyApis_NBFCCompanyApiTypes_NBFCCompanyApiTypeId",
                        column: x => x.NBFCCompanyApiTypeId,
                        principalTable: "NBFCCompanyApiTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NBFCSubActivityApis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NBFCCompanyApiId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityMasterId = table.Column<long>(type: "bigint", nullable: false),
                    SubActivityMasterId = table.Column<long>(type: "bigint", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_NBFCSubActivityApis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NBFCSubActivityApis_ActivityMasters_ActivityMasterId",
                        column: x => x.ActivityMasterId,
                        principalTable: "ActivityMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NBFCSubActivityApis_NBFCCompanyApis_NBFCCompanyApiId",
                        column: x => x.NBFCCompanyApiId,
                        principalTable: "NBFCCompanyApis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NBFCSubActivityApis_SubActivityMasters_SubActivityMasterId",
                        column: x => x.SubActivityMasterId,
                        principalTable: "SubActivityMasters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NBFCCompanyApis_NBFCCompanyApiTypeId",
                table: "NBFCCompanyApis",
                column: "NBFCCompanyApiTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NBFCSubActivityApis_ActivityMasterId",
                table: "NBFCSubActivityApis",
                column: "ActivityMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_NBFCSubActivityApis_NBFCCompanyApiId",
                table: "NBFCSubActivityApis",
                column: "NBFCCompanyApiId");

            migrationBuilder.CreateIndex(
                name: "IX_NBFCSubActivityApis_SubActivityMasterId",
                table: "NBFCSubActivityApis",
                column: "SubActivityMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NBFCSubActivityApis");

            migrationBuilder.DropTable(
                name: "NBFCCompanyApis");

            migrationBuilder.DropTable(
                name: "NBFCCompanyApiTypes");

            migrationBuilder.DropColumn(
                name: "TAPIKey",
                table: "ProductNBFCCompany");

            migrationBuilder.DropColumn(
                name: "TAPISecretKey",
                table: "ProductNBFCCompany");
        }
    }
}
