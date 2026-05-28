using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedMessagedForKafkaIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConsumerGroup = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    processedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => new { x.TaskId, x.ConsumerGroup });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedMessages");
        }
    }
}
