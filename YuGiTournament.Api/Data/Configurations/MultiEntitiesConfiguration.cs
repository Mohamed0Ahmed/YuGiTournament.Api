using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class MultiTournamentConfiguration : IEntityTypeConfiguration<MultiTournament>
    {
        public void Configure(EntityTypeBuilder<MultiTournament> builder)
        {
            builder.ToTable("MultiTournaments");
            builder.HasKey(t => t.MultiTournamentId);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
            builder.Property(t => t.SystemOfScoring).HasConversion<string>();
            builder.Property(t => t.Status).HasConversion<string>();

            // Champion Team relationship
            builder.HasOne(t => t.ChampionTeam)
                .WithMany()
                .HasForeignKey(t => t.ChampionTeamId)
                .OnDelete(DeleteBehavior.SetNull);

            // Teams relationship
            builder.HasMany(t => t.Teams)
                .WithOne(tm => tm.Tournament!)
                .HasForeignKey(tm => tm.MultiTournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Matches relationship
            builder.HasMany(t => t.Matches)
                .WithOne(m => m.Tournament!)
                .HasForeignKey(m => m.MultiTournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MultiTeamConfiguration : IEntityTypeConfiguration<MultiTeam>
    {
        public void Configure(EntityTypeBuilder<MultiTeam> builder)
        {
            builder.ToTable("MultiTeams");
            builder.HasKey(t => t.MultiTeamId);
            builder.Property(t => t.TeamName).IsRequired().HasMaxLength(100);
            builder.Property(t => t.PlayerIds).IsRequired().HasMaxLength(500); // JSON array

            builder.HasOne(t => t.Tournament)
                .WithMany(tn => tn.Teams)
                .HasForeignKey(t => t.MultiTournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class MultiMatchConfiguration : IEntityTypeConfiguration<MultiMatch>
    {
        public void Configure(EntityTypeBuilder<MultiMatch> builder)
        {
            builder.ToTable("MultiMatches");
            builder.HasKey(m => m.MultiMatchId);

            builder.HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.MultiTournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Team1)
                .WithMany(t => t.HomeMatches)
                .HasForeignKey(m => m.Team1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Team2)
                .WithMany(t => t.AwayMatches)
                .HasForeignKey(m => m.Team2Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Player1)
                .WithMany()
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Player2)
                .WithMany()
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}