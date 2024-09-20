﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCeplrPdfReportsApitoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "CeplrPdfReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "CeplrPdfReports");
        }
    }
}
