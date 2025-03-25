using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchRounds",
                columns: table => new
                {
                    MatchRoundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    WinnerId = table.Column<int>(type: "int", nullable: true),
                    IsDraw = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchRounds", x => x.MatchRoundId);
                    table.ForeignKey(
                        name: "FK_MatchRounds_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchRounds_MatchId",
                table: "MatchRounds",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchRounds");
        }
    }
}
