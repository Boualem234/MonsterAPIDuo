using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLittleRPG_ElGuendouz.Migrations
{
    /// <inheritdoc />
    public partial class VilleCoordonnees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "villeX",
                table: "Character",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "villeY",
                table: "Character",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "villeX",
                table: "Character");

            migrationBuilder.DropColumn(
                name: "villeY",
                table: "Character");
        }
    }
}
