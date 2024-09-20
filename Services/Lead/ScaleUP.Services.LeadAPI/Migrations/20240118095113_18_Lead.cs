using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _18_Lead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectMessage",
                table: "LeadActivityMasterProgresses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeadCompanyBuyingHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyLeadId = table.Column<long>(type: "bigint", nullable: false),
                    MonthFirstBuyingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalMonthInvoice = table.Column<int>(type: "int", nullable: false),
                    MonthTotalAmount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LeadCompanyBuyingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadCompanyBuyingHistory_CompanyLead_CompanyLeadId",
                        column: x => x.CompanyLeadId,
                        principalTable: "CompanyLead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeadCompanyBuyingHistory_CompanyLeadId",
                table: "LeadCompanyBuyingHistory",
                column: "CompanyLeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeadCompanyBuyingHistory");

            migrationBuilder.DropColumn(
                name: "RejectMessage",
                table: "LeadActivityMasterProgresses");
        }
    }
}
