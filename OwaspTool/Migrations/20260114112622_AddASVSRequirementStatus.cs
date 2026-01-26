using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OwaspTool.Migrations
{
    /// <inheritdoc />
    public partial class AddASVSRequirementStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ASVSRequirementStatus",
                columns: table => new
                {
                    ASVSRequirementStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserWebAppID = table.Column<int>(type: "int", nullable: false),
                    ASVSRequirementID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ASVSReqS__XXXXXXXXXXXXX", x => x.ASVSRequirementStatusID);
                    table.ForeignKey(
                        name: "FK__ASVSReqSt__ASVSR__XXXXXX",
                        column: x => x.ASVSRequirementID,
                        principalTable: "ASVSRequirement",
                        principalColumn: "ASVSRequirementID");
                    table.ForeignKey(
                        name: "FK__ASVSReqSt__UserW__XXXXXX",
                        column: x => x.UserWebAppID,
                        principalTable: "UserWebApp",
                        principalColumn: "UserWebAppID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ASVSRequirementStatus_ASVSRequirementID",
                table: "ASVSRequirementStatus",
                column: "ASVSRequirementID");

            migrationBuilder.CreateIndex(
                name: "IX_ASVSRequirementStatus_UserWebAppID",
                table: "ASVSRequirementStatus",
                column: "UserWebAppID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ASVSRequirementStatus");
        }
    }
}
