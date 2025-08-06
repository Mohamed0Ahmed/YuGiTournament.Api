using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class FriendlyPlayerConfiguration : IEntityTypeConfiguration<FriendlyPlayer>
    {
        public void Configure(EntityTypeBuilder<FriendlyPlayer> player)
        {
            player.HasKey(p => p.PlayerId);

            player.Property(p => p.PlayerId)
                  .ValueGeneratedOnAdd()
                  .UseIdentityColumn();

            player.Property(p => p.FullName)
                  .IsRequired()
                  .HasMaxLength(100);

            player.Property(p => p.CreatedOn)
                  .IsRequired();

            player.Property(p => p.IsActive)
                  .IsRequired()
                  .HasDefaultValue(true);

            // Index for better performance
            player.HasIndex(p => p.FullName);
        }
    }
} 