using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using YuGiTournament.Api.Identities;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<Match> Matches { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<MatchRound> MatchRounds { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<LeagueId> LeagueId { get; set; }
        public DbSet<FriendlyPlayer> FriendlyPlayers { get; set; }
        public DbSet<FriendlyMatch> FriendlyMatches { get; set; }
        public DbSet<ShutoutResult> ShutoutResults { get; set; }
        public DbSet<FriendlyMessage> FriendlyMessages { get; set; }
        public DbSet<GameRulesConfig> GameRulesConfigs { get; set; }
        public DbSet<MultiTournament> MultiTournaments { get; set; }
        public DbSet<MultiTeam> MultiTeams { get; set; }
        public DbSet<MultiMatch> MultiMatches { get; set; }
    }
}
