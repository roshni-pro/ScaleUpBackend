﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _17_may_lead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isEnach",
                table: "LeadBankDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isEnach",
                table: "LeadBankDetails");
        }
    }
}