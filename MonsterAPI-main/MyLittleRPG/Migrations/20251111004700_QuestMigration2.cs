using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLittleRPG_ElGuendouz.Migrations
{
    /// <inheritdoc />
    public partial class QuestMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NvRequis = table.Column<int>(type: "int", nullable: true),
                    NbMonstresATuer = table.Column<int>(type: "int", nullable: true),
                    NbMonstresTues = table.Column<int>(type: "int", nullable: true),
                    TypeMonstre = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TuileASeRendreX = table.Column<int>(type: "int", nullable: true),
                    TuileASeRendreY = table.Column<int>(type: "int", nullable: true),
                    Termine = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    idPersonnage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quest_Character_idPersonnage",
                        column: x => x.idPersonnage,
                        principalTable: "Character",
                        principalColumn: "idPersonnage",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Quest_idPersonnage",
                table: "Quest",
                column: "idPersonnage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Quest");
        }
    }
}
