using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Models;
using System.Text.RegularExpressions;

namespace YuGiTournament.Api.Data.Configurations
{
    public class MatchRoundConfiguration : IEntityTypeConfiguration<MatchRound>
    {
        public void Configure(EntityTypeBuilder<MatchRound> builder)
        {
            builder
                .Property(r => r.WinnerId)
            .IsRequired(false);

            builder.Property(m => m.IsDeleted)
                .IsRequired();
            builder.Property(m => m.LeagueNumber)
                 .IsRequired();
        }
    }
}
