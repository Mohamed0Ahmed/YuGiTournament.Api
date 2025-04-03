using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFinishedToLeagueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "LeagueId",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "LeagueId");
        }
    }
}
