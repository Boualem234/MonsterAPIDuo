using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLittleRPG_ElGuendouz.Migrations
{
    /// <inheritdoc />
    public partial class AddInstanceMonster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstanceMonstre",
                columns: table => new
                {
                    PositionX = table.Column<int>(type: "int", nullable: false),
                    PositionY = table.Column<int>(type: "int", nullable: false),
                    monstreID = table.Column<int>(type: "int", nullable: false),
                    niveau = table.Column<int>(type: "int", nullable: false),
                    pointsVieMax = table.Column<int>(type: "int", nullable: false),
                    pointsVieActuels = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstanceMonstre", x => new { x.PositionX, x.PositionY });
                    table.ForeignKey(
                        name: "FK_InstanceMonstre_Monsters_monstreID",
                        column: x => x.monstreID,
                        principalTable: "Monsters",
                        principalColumn: "idMonster",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InstanceMonstre_monstreID",
                table: "InstanceMonstre",
                column: "monstreID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstanceMonstre");
        }
    }
}
