using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> match)
        {
            match.HasKey(m => m.MatchId);

            match.Property(m => m.Score1)
                .HasDefaultValue(0);

            match.Property(m => m.Score2)
                .HasDefaultValue(0);

            match.Property(m => m.IsCompleted)
                .HasDefaultValue(false);

            match.HasOne(m => m.Player1)
                .WithMany()
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            match.HasOne(m => m.Player2)
                .WithMany()
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
