using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class MatchRoundConfiguration : IEntityTypeConfiguration<MatchRound>
    {
        public void Configure(EntityTypeBuilder<MatchRound> builder)
        {
            builder
                .Property(r => r.WinnerId)
                .IsRequired(false);
        }
    }
}
