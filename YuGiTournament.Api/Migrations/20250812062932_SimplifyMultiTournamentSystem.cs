using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyMultiTournamentSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultiMatches_MultiFixtures_MultiFixtureId",
                table: "MultiMatches");

            migrationBuilder.DropTable(
                name: "MultiFixtures");

            migrationBuilder.DropTable(
                name: "MultiTeamMembers");

            migrationBuilder.DropTable(
                name: "MultiTieBreakAwards");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "MultiTournaments");

            migrationBuilder.DropColumn(
                name: "IsLiveNow",
                table: "MultiTournaments");

            migrationBuilder.RenameColumn(
                name: "IsStarted",
                table: "MultiTournaments",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "MultiFixtureId",
                table: "MultiMatches",
                newName: "Team2Id");

            migrationBuilder.RenameIndex(
                name: "IX_MultiMatches_MultiFixtureId",
                table: "MultiMatches",
                newName: "IX_MultiMatches_Team2Id");

            migrationBuilder.AlterColumn<string>(
                name: "SystemOfScoring",
                table: "MultiTournaments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ChampionTeamId",
                table: "MultiTournaments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedOn",
                table: "MultiTournaments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedOn",
                table: "MultiTournaments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MultiTournaments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "MultiTeams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<int>(
                name: "Draws",
                table: "MultiTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Losses",
                table: "MultiTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlayerIds",
                table: "MultiTeams",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TotalPoints",
                table: "MultiTeams",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Wins",
                table: "MultiTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedOn",
                table: "MultiMatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Team1Id",
                table: "MultiMatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MultiTournaments_ChampionTeamId",
                table: "MultiTournaments",
                column: "ChampionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiMatches_MultiTournamentId",
                table: "MultiMatches",
                column: "MultiTournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiMatches_Team1Id",
                table: "MultiMatches",
                column: "Team1Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MultiMatches_MultiTeams_Team1Id",
                table: "MultiMatches",
                column: "Team1Id",
                principalTable: "MultiTeams",
                principalColumn: "MultiTeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MultiMatches_MultiTeams_Team2Id",
                table: "MultiMatches",
                column: "Team2Id",
                principalTable: "MultiTeams",
                principalColumn: "MultiTeamId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MultiMatches_MultiTournaments_MultiTournamentId",
                table: "MultiMatches",
                column: "MultiTournamentId",
                principalTable: "MultiTournaments",
                principalColumn: "MultiTournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MultiTournaments_MultiTeams_ChampionTeamId",
                table: "MultiTournaments",
                column: "ChampionTeamId",
                principalTable: "MultiTeams",
                principalColumn: "MultiTeamId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultiMatches_MultiTeams_Team1Id",
                table: "MultiMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_MultiMatches_MultiTeams_Team2Id",
                table: "MultiMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_MultiMatches_MultiTournaments_MultiTournamentId",
                table: "MultiMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_MultiTournaments_MultiTeams_ChampionTeamId",
                table: "MultiTournaments");

            migrationBuilder.DropIndex(
                name: "IX_MultiTournaments_ChampionTeamId",
                table: "MultiTournaments");

            migrationBuilder.DropIndex(
                name: "IX_MultiMatches_MultiTournamentId",
                table: "MultiMatches");

            migrationBuilder.DropIndex(
                name: "IX_MultiMatches_Team1Id",
                table: "MultiMatches");

            migrationBuilder.DropColumn(
                name: "ChampionTeamId",
                table: "MultiTournaments");

            migrationBuilder.DropColumn(
                name: "FinishedOn",
                table: "MultiTournaments");

            migrationBuilder.DropColumn(
                name: "StartedOn",
                table: "MultiTournaments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MultiTournaments");

            migrationBuilder.DropColumn(
                name: "Draws",
                table: "MultiTeams");

            migrationBuilder.DropColumn(
                name: "Losses",
                table: "MultiTeams");

            migrationBuilder.DropColumn(
                name: "PlayerIds",
                table: "MultiTeams");

            migrationBuilder.DropColumn(
                name: "TotalPoints",
                table: "MultiTeams");

            migrationBuilder.DropColumn(
                name: "Wins",
                table: "MultiTeams");

            migrationBuilder.DropColumn(
                name: "CompletedOn",
                table: "MultiMatches");

            migrationBuilder.DropColumn(
                name: "Team1Id",
                table: "MultiMatches");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "MultiTournaments",
                newName: "IsStarted");

            migrationBuilder.RenameColumn(
                name: "Team2Id",
                table: "MultiMatches",
                newName: "MultiFixtureId");

            migrationBuilder.RenameIndex(
                name: "IX_MultiMatches_Team2Id",
                table: "MultiMatches",
                newName: "IX_MultiMatches_MultiFixtureId");

            migrationBuilder.AlterColumn<int>(
                name: "SystemOfScoring",
                table: "MultiTournaments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "MultiTournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLiveNow",
                table: "MultiTournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "MultiTeams",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

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
                    FriendlyPlayerId = table.Column<int>(type: "integer", nullable: false),
                    MultiTeamId = table.Column<int>(type: "integer", nullable: false),
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
                    AwardedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
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
                name: "IX_MultiTeamMembers_FriendlyPlayerId",
                table: "MultiTeamMembers",
                column: "FriendlyPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTeamMembers_MultiTeamId",
                table: "MultiTeamMembers",
                column: "MultiTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTieBreakAwards_MultiTournamentId",
                table: "MultiTieBreakAwards",
                column: "MultiTournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_MultiTieBreakAwards_WinnerTeamId",
                table: "MultiTieBreakAwards",
                column: "WinnerTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_MultiMatches_MultiFixtures_MultiFixtureId",
                table: "MultiMatches",
                column: "MultiFixtureId",
                principalTable: "MultiFixtures",
                principalColumn: "MultiFixtureId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
