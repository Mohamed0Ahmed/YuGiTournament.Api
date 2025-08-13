using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeamsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPoints1",
                table: "MultiMatches");

            migrationBuilder.DropColumn(
                name: "TotalPoints2",
                table: "MultiMatches");

            migrationBuilder.AddColumn<int>(
                name: "WinnerId",
                table: "MultiMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MultiMatches_WinnerId",
                table: "MultiMatches",
                column: "WinnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MultiMatches_FriendlyPlayers_WinnerId",
                table: "MultiMatches",
                column: "WinnerId",
                principalTable: "FriendlyPlayers",
                principalColumn: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultiMatches_FriendlyPlayers_WinnerId",
                table: "MultiMatches");

            migrationBuilder.DropIndex(
                name: "IX_MultiMatches_WinnerId",
                table: "MultiMatches");

            migrationBuilder.DropColumn(
                name: "WinnerId",
                table: "MultiMatches");

            migrationBuilder.AddColumn<double>(
                name: "TotalPoints1",
                table: "MultiMatches",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalPoints2",
                table: "MultiMatches",
                type: "double precision",
                nullable: true);
        }
    }
}
