using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MultiParticipations",
                table: "FriendlyPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MultiTitlesWon",
                table: "FriendlyPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MultiTournaments",
                columns: table => new
                {
                    MultiTournamentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SystemOfScoring = table.Column<int>(type: "integer", nullable: false),
                    TeamCount = table.Column<int>(type: "integer", nullable: false),
                    PlayersPerTeam = table.Column<int>(type: "integer", nullable: false),
                    IsLiveNow = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiTournaments", x => x.MultiTournamentId);
                });

            migrationBuilder.CreateTable(
                name: "MultiTeams",
                columns: table => new
                {
                    MultiTeamId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MultiTournamentId = table.Column<int>(type: "integer", nullable: false),
                    TeamName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiTeams", x => x.MultiTeamId);
                    table.ForeignKey(
                        name: "FK_MultiTeams_MultiTournaments_MultiTournamentId",
                        column: x => x.MultiTournamentId,
                        principalTable: "MultiTournaments",
                        principalColumn: "MultiTournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiFixtures",
                columns: table => new
                {
                    MultiFixtureId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MultiTournamentId = table.Column<int>(type: "integer", nullable: false),
                    TeamAId = table.Column<int>(type: "integer", nullable: false),
                    TeamBId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiFixtures", x => x.MultiFixtureId);
                    table.ForeignKey(
                        name: "FK_MultiFixtures_MultiTeams_TeamAId",
                        column: x => x.TeamAId,
                        principalTable: "MultiTeams",
                        principalColumn: "MultiTeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MultiFixtures_MultiTeams_TeamBId",
                        column: x => x.TeamBId,
                        principalTable: "MultiTeams",
                        principalColumn: "MultiTeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MultiFixtures_MultiTournaments_MultiTournamentId",
                        column: x => x.MultiTournamentId,
                        principalTable: "MultiTournaments",
                        principalColumn: "MultiTournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiTeamMembers",
                columns: table => new
                {
                    MultiTeamMemberId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MultiTeamId = table.Column<int>(type: "integer", nullable: false),
                    FriendlyPlayerId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiTeamMembers", x => x.MultiTeamMemberId);
                    table.ForeignKey(
                        name: "FK_MultiTeamMembers_FriendlyPlayers_FriendlyPlayerId",
                        column: x => x.FriendlyPlayerId,
                        principalTable: "FriendlyPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MultiTeamMembers_MultiTeams_MultiTeamId",
                        column: x => x.MultiTeamId,
                        principalTable: "MultiTeams",
                        principalColumn: "MultiTeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiTieBreakAwards",
                columns: table => new
                {
                    MultiTieBreakAwardId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MultiTournamentId = table.Column<int>(type: "integer", nullable: false),
                    WinnerTeamId = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiTieBreakAwards", x => x.MultiTieBreakAwardId);
                    table.ForeignKey(
                        name: "FK_MultiTieBreakAwards_MultiTeams_WinnerTeamId",
                        column: x => x.WinnerTeamId,
                        principalTable: "MultiTeams",
                        principalColumn: "MultiTeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MultiTieBreakAwards_MultiTournaments_MultiTournamentId",
                        column: x => x.MultiTournamentId,
                        principalTable: "MultiTournaments",
                        principalColumn: "MultiTournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultiMatches",
                columns: table => new
                {
                    MultiMatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MultiFixtureId = table.Column<int>(type: "integer", nullable: false),
                    MultiTournamentId = table.Column<int>(type: "integer", nullable: false),
                    Player1Id = table.Column<int>(type: "integer", nullable: false),
                    Player2Id = table.Column<int>(type: "integer", nullable: false),
                    Score1 = table.Column<int>(type: "integer", nullable: true),
                    Score2 = table.Column<int>(type: "integer", nullable: true),
                    TotalPoints1 = table.Column<double>(type: "double precision", nullable: true),
                    TotalPoints2 = table.Column<double>(type: "double precision", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiMatches", x => x.MultiMatchId);
                    table.ForeignKey(
                        name: "FK_MultiMatches_FriendlyPlayers_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "FriendlyPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MultiMatches_FriendlyPlayers_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "FriendlyPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MultiMatches_MultiFixtures_MultiFixtureId",
                        column: x => x.MultiFixtureId,
                        principalTable: "MultiFixtures",
                        principalColumn: "MultiFixtureId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MultiFixtures_MultiTournamentId",
                table: "MultiFixtures",
                column: "MultiTournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiFixtures_TeamAId",
                table: "MultiFixtures",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiFixtures_TeamBId",
                table: "MultiFixtures",
                column: "TeamBId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiMatches_MultiFixtureId",
                table: "MultiMatches",
                column: "MultiFixtureId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiMatches_Player1Id",
                table: "MultiMatches",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_MultiMatches_Player2Id",
                table: "MultiMatches",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTeamMembers_FriendlyPlayerId",
                table: "MultiTeamMembers",
                column: "FriendlyPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTeamMembers_MultiTeamId",
                table: "MultiTeamMembers",
                column: "MultiTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTeams_MultiTournamentId",
                table: "MultiTeams",
                column: "MultiTournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTieBreakAwards_MultiTournamentId",
                table: "MultiTieBreakAwards",
                column: "MultiTournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTieBreakAwards_WinnerTeamId",
                table: "MultiTieBreakAwards",
                column: "WinnerTeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MultiMatches");

            migrationBuilder.DropTable(
                name: "MultiTeamMembers");

            migrationBuilder.DropTable(
                name: "MultiTieBreakAwards");

            migrationBuilder.DropTable(
                name: "MultiFixtures");

            migrationBuilder.DropTable(
                name: "MultiTeams");

            migrationBuilder.DropTable(
                name: "MultiTournaments");

            migrationBuilder.DropColumn(
                name: "MultiParticipations",
                table: "FriendlyPlayers");

            migrationBuilder.DropColumn(
                name: "MultiTitlesWon",
                table: "FriendlyPlayers");
        }
    }
}
