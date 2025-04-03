using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnToLeagueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeOfLeague",
                table: "LeagueId",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeOfLeague",
                table: "LeagueId");
        }
    }
}
