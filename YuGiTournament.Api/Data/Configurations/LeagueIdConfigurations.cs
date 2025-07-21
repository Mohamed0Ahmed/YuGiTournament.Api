using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class LeagueIdConfigurations : IEntityTypeConfiguration<LeagueId>
    {
        public void Configure(EntityTypeBuilder<LeagueId> league)
        {

            league.HasKey(p => p.Id);

            league.Property(p => p.Id)
                  .ValueGeneratedOnAdd()
                  .UseIdentityColumn();

            league.Property(p => p.CreatedOn).IsRequired();
            league.Property(p => p.Description).IsRequired();
            league.Property(p => p.Name).IsRequired();
            league.Property(p => p.TypeOfLeague).IsRequired();
            league.Property(l => l.SystemOfLeague).IsRequired();

        }
    }
}
