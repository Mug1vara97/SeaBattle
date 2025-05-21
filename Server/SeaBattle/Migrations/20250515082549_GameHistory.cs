using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeaBattle.Migrations
{
    /// <inheritdoc />
    public partial class GameHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerUsername = table.Column<string>(type: "text", nullable: false),
                    GameId = table.Column<string>(type: "text", nullable: false),
                    OpponentUsername = table.Column<string>(type: "text", nullable: true),
                    GameFinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameHistories_GameFinishedAt",
                table: "GameHistories",
                column: "GameFinishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GameHistories_PlayerUsername",
                table: "GameHistories",
                column: "PlayerUsername");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameHistories");
        }
    }
}
