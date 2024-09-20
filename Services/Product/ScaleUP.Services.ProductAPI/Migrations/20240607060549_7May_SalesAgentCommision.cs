using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class _7May_SalesAgentCommision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentCommissionConfig");

            migrationBuilder.CreateTable(
                name: "ProductCommissionConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayOutMasterId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommisionValue = table.Column<double>(type: "float", nullable: false),
                    DSAAgreement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectorAgreement = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ProductCommissionConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCommissionConfig_PayOutMaster_PayOutMasterId",
                        column: x => x.PayOutMasterId,
                        principalTable: "PayOutMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCommissionConfig_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesAgentCommision",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesAgentProductId = table.Column<long>(type: "bigint", nullable: false),
                    PayOutMasterId = table.Column<long>(type: "bigint", nullable: false),
                    CommisionPercentage = table.Column<double>(type: "float", nullable: false),
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
                    table.PrimaryKey("PK_SalesAgentCommision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesAgentCommision_PayOutMaster_PayOutMasterId",
                        column: x => x.PayOutMasterId,
                        principalTable: "PayOutMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesAgentCommision_SalesAgentProduct_SalesAgentProductId",
                        column: x => x.SalesAgentProductId,
                        principalTable: "SalesAgentProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCommissionConfig_PayOutMasterId",
                table: "ProductCommissionConfig",
                column: "PayOutMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCommissionConfig_ProductId",
                table: "ProductCommissionConfig",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgentCommision_PayOutMasterId",
                table: "SalesAgentCommision",
                column: "PayOutMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgentCommision_SalesAgentProductId",
                table: "SalesAgentCommision",
                column: "SalesAgentProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCommissionConfig");

            migrationBuilder.DropTable(
                name: "SalesAgentCommision");

            migrationBuilder.CreateTable(
                name: "AgentCommissionConfig",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayOutMasterId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    CommisionValue = table.Column<double>(type: "float", nullable: false),
                    ConnectorAgreement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DSAAgreement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommissionConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCommissionConfig_PayOutMaster_PayOutMasterId",
                        column: x => x.PayOutMasterId,
                        principalTable: "PayOutMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentCommissionConfig_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_PayOutMasterId",
                table: "AgentCommissionConfig",
                column: "PayOutMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_ProductId",
                table: "AgentCommissionConfig",
                column: "ProductId");
        }
    }
}
