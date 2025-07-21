using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemOfLeague : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SystemOfLeague",
                table: "LeagueId",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemOfLeague",
                table: "LeagueId");
        }
    }
}
