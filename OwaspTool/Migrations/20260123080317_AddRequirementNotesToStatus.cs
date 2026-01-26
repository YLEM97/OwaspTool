using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OwaspTool.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementNotesToStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ASVSRequirementStatus",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ASVSRequirementStatus");
        }
    }
}
