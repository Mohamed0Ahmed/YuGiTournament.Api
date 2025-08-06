using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class ShutoutResultConfiguration : IEntityTypeConfiguration<ShutoutResult>
    {
        public void Configure(EntityTypeBuilder<ShutoutResult> shutout)
        {
            shutout.HasKey(s => s.ShutoutId);

            shutout.Property(s => s.ShutoutId)
                   .ValueGeneratedOnAdd()
                   .UseIdentityColumn();

            shutout.Property(s => s.MatchId)
                   .IsRequired();

            shutout.Property(s => s.WinnerId)
                   .IsRequired();

            shutout.Property(s => s.LoserId)
                   .IsRequired();

            shutout.Property(s => s.AchievedOn)
                   .IsRequired();

            shutout.Property(s => s.IsDeleted)
                   .IsRequired()
                   .HasDefaultValue(false);

            // Foreign Key Relationships
            shutout.HasOne(s => s.Match)
                   .WithMany()
                   .HasForeignKey(s => s.MatchId)
                   .OnDelete(DeleteBehavior.Restrict);

            shutout.HasOne(s => s.Winner)
                   .WithMany()
                   .HasForeignKey(s => s.WinnerId)
                   .OnDelete(DeleteBehavior.Restrict);

            shutout.HasOne(s => s.Loser)
                   .WithMany()
                   .HasForeignKey(s => s.LoserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better performance
            shutout.HasIndex(s => s.MatchId);
            shutout.HasIndex(s => s.WinnerId);
            shutout.HasIndex(s => s.LoserId);
            shutout.HasIndex(s => s.AchievedOn);
            shutout.HasIndex(s => s.IsDeleted);
        }
    }
}