using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScaleUP.Services.LeadAPI.Migrations
{
    /// <inheritdoc />
    public partial class _05June2024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "request",
                table: "eSignDocumentResponse");

            migrationBuilder.DropColumn(
                name: "webhookStatus",
                table: "eSignDocumentResponse");

            migrationBuilder.DropColumn(
                name: "webhookStatusCode",
                table: "eSignDocumentResponse");

            migrationBuilder.RenameColumn(
                name: "verification",
                table: "eSignDocumentResponse",
                newName: "users");

            migrationBuilder.RenameColumn(
                name: "signer",
                table: "eSignDocumentResponse",
                newName: "messages");

            migrationBuilder.RenameColumn(
                name: "mac",
                table: "eSignDocumentResponse",
                newName: "irn");

            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "eSignDocumentResponse",
                newName: "clientData");

            migrationBuilder.AlterColumn<string>(
                name: "documentId",
                table: "eSignDocumentResponse",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "eSignDocumentResponse",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "LeadId",
                table: "eSignDocumentResponse",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "File",
                table: "eSignDocumentResponse");

            migrationBuilder.DropColumn(
                name: "LeadId",
                table: "eSignDocumentResponse");

            migrationBuilder.RenameColumn(
                name: "users",
                table: "eSignDocumentResponse",
                newName: "verification");

            migrationBuilder.RenameColumn(
                name: "messages",
                table: "eSignDocumentResponse",
                newName: "signer");

            migrationBuilder.RenameColumn(
                name: "irn",
                table: "eSignDocumentResponse",
                newName: "mac");

            migrationBuilder.RenameColumn(
                name: "clientData",
                table: "eSignDocumentResponse",
                newName: "FileUrl");

            migrationBuilder.AlterColumn<string>(
                name: "documentId",
                table: "eSignDocumentResponse",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "request",
                table: "eSignDocumentResponse",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "webhookStatus",
                table: "eSignDocumentResponse",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "webhookStatusCode",
                table: "eSignDocumentResponse",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
