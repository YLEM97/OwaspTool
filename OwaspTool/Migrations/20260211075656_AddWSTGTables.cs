using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OwaspTool.Migrations
{
    /// <inheritdoc />
    public partial class AddWSTGTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WSTGChapter",
                columns: table => new
                {
                    WSTGChapterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WSTGChap__WSTGChapterID", x => x.WSTGChapterID);
                });

            migrationBuilder.CreateTable(
                name: "WSTGTest",
                columns: table => new
                {
                    WSTGTestID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WSTGChapterID = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumberWSTG = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WSTGTest__WSTGTestID", x => x.WSTGTestID);
                    table.ForeignKey(
                        name: "FK__WSTGTest__WSTGChap__01A3B4A6",
                        column: x => x.WSTGChapterID,
                        principalTable: "WSTGChapter",
                        principalColumn: "WSTGChapterID");
                });

            migrationBuilder.CreateTable(
                name: "WSTGTestAnswer",
                columns: table => new
                {
                    WSTGTestAnswerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WSTGTestID = table.Column<int>(type: "int", nullable: false),
                    AnswerOptionID = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WSTGTest__WSTGTestAnswerID", x => x.WSTGTestAnswerID);
                    table.ForeignKey(
                        name: "FK__WSTGTestAn__Answe__03B69164",
                        column: x => x.AnswerOptionID,
                        principalTable: "AnswerOption",
                        principalColumn: "AnswerOptionID");
                    table.ForeignKey(
                        name: "FK__WSTGTestAn__WSTGTe__02FC7413",
                        column: x => x.WSTGTestID,
                        principalTable: "WSTGTest",
                        principalColumn: "WSTGTestID");
                });

            migrationBuilder.CreateTable(
                name: "WSTGTestStatus",
                columns: table => new
                {
                    WSTGTestStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserWebAppID = table.Column<int>(type: "int", nullable: false),
                    WSTGTestID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WSTGTest__WSTGTestStatusID", x => x.WSTGTestStatusID);
                    table.ForeignKey(
                        name: "FK__WSTGTestSt__UserW__04E4BC85",
                        column: x => x.UserWebAppID,
                        principalTable: "UserWebApp",
                        principalColumn: "UserWebAppID");
                    table.ForeignKey(
                        name: "FK__WSTGTestSt__WSTGTe__05D8E0BE",
                        column: x => x.WSTGTestID,
                        principalTable: "WSTGTest",
                        principalColumn: "WSTGTestID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WSTGTest_WSTGChapterID",
                table: "WSTGTest",
                column: "WSTGChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_WSTGTestAnswer_AnswerOptionID",
                table: "WSTGTestAnswer",
                column: "AnswerOptionID");

            migrationBuilder.CreateIndex(
                name: "IX_WSTGTestAnswer_WSTGTestID",
                table: "WSTGTestAnswer",
                column: "WSTGTestID");

            migrationBuilder.CreateIndex(
                name: "IX_WSTGTestStatus_UserWebAppID",
                table: "WSTGTestStatus",
                column: "UserWebAppID");

            migrationBuilder.CreateIndex(
                name: "IX_WSTGTestStatus_WSTGTestID",
                table: "WSTGTestStatus",
                column: "WSTGTestID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WSTGTestAnswer");

            migrationBuilder.DropTable(
                name: "WSTGTestStatus");

            migrationBuilder.DropTable(
                name: "WSTGTest");

            migrationBuilder.DropTable(
                name: "WSTGChapter");
        }
    }
}
