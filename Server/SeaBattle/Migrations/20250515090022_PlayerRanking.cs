using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaBattle.Migrations
{
    /// <inheritdoc />
    public partial class PlayerRanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerRankings",
                columns: table => new
                {
                    PlayerUsername = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    Losses = table.Column<int>(type: "integer", nullable: false),
                    TotalGames = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRankings", x => x.PlayerUsername);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRankings_Rating",
                table: "PlayerRankings",
                column: "Rating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerRankings");
        }
    }
}
