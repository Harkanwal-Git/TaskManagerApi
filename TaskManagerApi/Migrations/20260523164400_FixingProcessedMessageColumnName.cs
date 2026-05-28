using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class FixingProcessedMessageColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "processedAt",
                table: "ProcessedMessages",
                newName: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "ProcessedMessages",
                newName: "processedAt");
        }
    }
}
