using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OwaspTool.Migrations
{
    /// <inheritdoc />
    public partial class AddAiNotesToASVSRequirementStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiNotes",
                table: "ASVSRequirementStatus",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiNotes",
                table: "ASVSRequirementStatus");
        }
    }
}
