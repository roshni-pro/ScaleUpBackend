using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LoanAccountAPI.Migrations
{
    /// <inheritdoc />
    public partial class _08jan20241 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "CommonAPIRequestResponses",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "NBFCCompanyApiDetailId",
                table: "CommonAPIRequestResponses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlackSoilAccountDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanAccountId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    BusinessId = table.Column<long>(type: "bigint", nullable: false),
                    UpdateLimitURL = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                    table.PrimaryKey("PK_BlackSoilAccountDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlackSoilAccountTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("PK_BlackSoilAccountTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NBFCComapnyAPIMasters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentificationCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TransactionTypeCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TransactionStatuCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AccountTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("PK_NBFCComapnyAPIMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NBFCCompanyAPIs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    APIUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TAPIKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TAPISecretKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TReferralCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_NBFCCompanyAPIs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NBFCComapnyApiDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NBFCComapnyApiMasterId = table.Column<long>(type: "bigint", nullable: false),
                    NBFCCompanyAPIId = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("PK_NBFCComapnyApiDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NBFCComapnyApiDetails_NBFCComapnyAPIMasters_NBFCComapnyApiMasterId",
                        column: x => x.NBFCComapnyApiMasterId,
                        principalTable: "NBFCComapnyAPIMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NBFCCompanyAPIFlows",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NBFCCompanyAPIId = table.Column<long>(type: "bigint", nullable: false),
                    NBFCCompanyAPIMasterId = table.Column<long>(type: "bigint", nullable: false),
                    NBFCIdentificationCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TransactionStatusCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TransactionTypeCode = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
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
                    table.PrimaryKey("PK_NBFCCompanyAPIFlows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NBFCCompanyAPIFlows_NBFCComapnyAPIMasters_NBFCCompanyAPIMasterId",
                        column: x => x.NBFCCompanyAPIMasterId,
                        principalTable: "NBFCComapnyAPIMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NBFCCompanyAPIFlows_NBFCCompanyAPIs_NBFCCompanyAPIId",
                        column: x => x.NBFCCompanyAPIId,
                        principalTable: "NBFCCompanyAPIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NBFCComapnyApiDetails_NBFCComapnyApiMasterId",
                table: "NBFCComapnyApiDetails",
                column: "NBFCComapnyApiMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_NBFCCompanyAPIFlows_NBFCCompanyAPIId",
                table: "NBFCCompanyAPIFlows",
                column: "NBFCCompanyAPIId");

            migrationBuilder.CreateIndex(
                name: "IX_NBFCCompanyAPIFlows_NBFCCompanyAPIMasterId",
                table: "NBFCCompanyAPIFlows",
                column: "NBFCCompanyAPIMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackSoilAccountDetails");

            migrationBuilder.DropTable(
                name: "BlackSoilAccountTransactions");

            migrationBuilder.DropTable(
                name: "NBFCComapnyApiDetails");

            migrationBuilder.DropTable(
                name: "NBFCCompanyAPIFlows");

            migrationBuilder.DropTable(
                name: "NBFCComapnyAPIMasters");

            migrationBuilder.DropTable(
                name: "NBFCCompanyAPIs");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "CommonAPIRequestResponses");

            migrationBuilder.DropColumn(
                name: "NBFCCompanyApiDetailId",
                table: "CommonAPIRequestResponses");
        }
    }
}
