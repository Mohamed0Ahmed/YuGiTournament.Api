using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YuGiTournament.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendlyMatchAndMessageSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "FriendlyMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SenderFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SenderPhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsFromAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendlyMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FriendlyPlayers",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendlyPlayers", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "FriendlyMatches",
                columns: table => new
                {
                    MatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Player1Id = table.Column<int>(type: "integer", nullable: false),
                    Player2Id = table.Column<int>(type: "integer", nullable: false),
                    Player1Score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Player2Score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PlayedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendlyMatches", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_FriendlyMatches_FriendlyPlayers_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "FriendlyPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FriendlyMatches_FriendlyPlayers_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "FriendlyPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShutoutResults",
                columns: table => new
                {
                    ShutoutId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    WinnerId = table.Column<int>(type: "integer", nullable: false),
                    LoserId = table.Column<int>(type: "integer", nullable: false),
                    AchievedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShutoutResults", x => x.ShutoutId);
                    table.ForeignKey(
                        name: "FK_ShutoutResults_FriendlyMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "FriendlyMatches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShutoutResults_FriendlyPlayers_LoserId",
                        column: x => x.LoserId,
                        principalTable: "FriendlyPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShutoutResults_FriendlyPlayers_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "FriendlyPlayers",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMatches_IsDeleted",
                table: "FriendlyMatches",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMatches_PlayedOn",
                table: "FriendlyMatches",
                column: "PlayedOn");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMatches_Player1Id_Player2Id",
                table: "FriendlyMatches",
                columns: new[] { "Player1Id", "Player2Id" });

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMatches_Player2Id",
                table: "FriendlyMatches",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMessages_IsDeleted",
                table: "FriendlyMessages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMessages_IsFromAdmin",
                table: "FriendlyMessages",
                column: "IsFromAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMessages_IsRead",
                table: "FriendlyMessages",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMessages_SenderId",
                table: "FriendlyMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyMessages_SentAt",
                table: "FriendlyMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_FriendlyPlayers_FullName",
                table: "FriendlyPlayers",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_ShutoutResults_AchievedOn",
                table: "ShutoutResults",
                column: "AchievedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ShutoutResults_IsDeleted",
                table: "ShutoutResults",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ShutoutResults_LoserId",
                table: "ShutoutResults",
                column: "LoserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShutoutResults_MatchId",
                table: "ShutoutResults",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShutoutResults_WinnerId",
                table: "ShutoutResults",
                column: "WinnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendlyMessages");

            migrationBuilder.DropTable(
                name: "ShutoutResults");

            migrationBuilder.DropTable(
                name: "FriendlyMatches");

            migrationBuilder.DropTable(
                name: "FriendlyPlayers");
        }
    }
}
