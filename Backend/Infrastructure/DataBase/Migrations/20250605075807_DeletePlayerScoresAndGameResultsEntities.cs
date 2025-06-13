using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class DeletePlayerScoresAndGameResultsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerScores");

            migrationBuilder.DropTable(
                name: "GameResults");

            migrationBuilder.DropColumn(
                name: "EndedAt",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "GameSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndedAt",
                table: "GameSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "GameSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "GameResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaylistId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Genre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameResults_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameResults_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GameResults_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameResults_Users_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerScores_GameResults_GameResultId",
                        column: x => x.GameResultId,
                        principalTable: "GameResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerScores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_GameSessionId",
                table: "GameResults",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_PlaylistId",
                table: "GameResults",
                column: "PlaylistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_RoomId",
                table: "GameResults",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_GameResults_WinnerId",
                table: "GameResults",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerScores_GameResultId",
                table: "PlayerScores",
                column: "GameResultId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerScores_UserId",
                table: "PlayerScores",
                column: "UserId");
        }
    }
}
