using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnimalShelter.Migrations
{
    /// <inheritdoc />
    public partial class AddShelterRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShelterId",
                table: "Animals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Shelter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelter", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Animals_ShelterId",
                table: "Animals",
                column: "ShelterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_Shelter_ShelterId",
                table: "Animals",
                column: "ShelterId",
                principalTable: "Shelter",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_Shelter_ShelterId",
                table: "Animals");

            migrationBuilder.DropTable(
                name: "Shelter");

            migrationBuilder.DropIndex(
                name: "IX_Animals_ShelterId",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "ShelterId",
                table: "Animals");
        }
    }
}
