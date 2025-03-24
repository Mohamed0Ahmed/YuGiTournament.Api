using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> ةشفؤا)
        {
            ةشفؤا.HasKey(m => m.MatchId);

            ةشفؤا.Property(m => m.Score1)
                .HasDefaultValue(0);

            ةشفؤا.Property(m => m.Score2)
                .HasDefaultValue(0);

            ةشفؤا.Property(m => m.IsCompleted)
                .HasDefaultValue(false);

            ةشفؤا.HasOne(m => m.Player1)
                .WithMany()
                .HasForeignKey(m => m.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            ةشفؤا.HasOne(m => m.Player2)
                .WithMany()
                .HasForeignKey(m => m.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
