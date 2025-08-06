using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
     public class FriendlyMatchConfiguration : IEntityTypeConfiguration<FriendlyMatch>
     {
          public void Configure(EntityTypeBuilder<FriendlyMatch> match)
          {
               match.HasKey(m => m.MatchId);

               match.Property(m => m.MatchId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn();

               match.Property(m => m.Player1Id)
                    .IsRequired();

               match.Property(m => m.Player2Id)
                    .IsRequired();

               match.Property(m => m.Player1Score)
                    .IsRequired()
                    .HasDefaultValue(0);

               match.Property(m => m.Player2Score)
                    .IsRequired()
                    .HasDefaultValue(0);

               match.Property(m => m.PlayedOn)
                    .IsRequired();

               match.Property(m => m.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

               // Foreign Key Relationships
               match.HasOne(m => m.Player1)
                    .WithMany(p => p.Player1Matches)
                    .HasForeignKey(m => m.Player1Id)
                    .OnDelete(DeleteBehavior.Restrict);

               match.HasOne(m => m.Player2)
                    .WithMany(p => p.Player2Matches)
                    .HasForeignKey(m => m.Player2Id)
                    .OnDelete(DeleteBehavior.Restrict);

               // Indexes for better performance
               match.HasIndex(m => new { m.Player1Id, m.Player2Id });
               match.HasIndex(m => m.PlayedOn);
               match.HasIndex(m => m.IsDeleted);
          }
     }
}