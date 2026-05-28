using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsStalledForOutboxMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStalled",
                table: "OutboxMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStalled",
                table: "OutboxMessages");
        }
    }
}
