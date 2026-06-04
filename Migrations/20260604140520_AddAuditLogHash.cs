using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalSuppliesCatalog.Lab06.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "AuditLogs");
        }
    }
}
