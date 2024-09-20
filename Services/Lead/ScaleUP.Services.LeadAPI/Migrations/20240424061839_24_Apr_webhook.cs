using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _24_Apr_webhook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompositeDisbursementWebhookResponse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMasterId = table.Column<long>(type: "bigint", nullable: false),
                    status_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    partner_loan_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    net_disbur_amt = table.Column<double>(type: "float", nullable: false),
                    utr_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    utr_date_time = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    txn_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_CompositeDisbursementWebhookResponse", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompositeDisbursementWebhookResponse");
        }
    }
}
