using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.DataBase.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionIdInPlaylistEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameSessionId",
                table: "Playlists",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameSessionId",
                table: "Playlists");
        }
    }
}
