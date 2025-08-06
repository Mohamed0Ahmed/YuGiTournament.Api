using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
       public class FriendlyMessageConfiguration : IEntityTypeConfiguration<FriendlyMessage>
       {
              public void Configure(EntityTypeBuilder<FriendlyMessage> message)
              {
                     // Primary Key
                     message.HasKey(m => m.Id);

                     // Auto-increment Identity
                     message.Property(m => m.Id)
                            .ValueGeneratedOnAdd()
                            .UseIdentityColumn();

                     // Required Properties
                     message.Property(m => m.SenderId)
                            .IsRequired()
                            .HasMaxLength(450); // Same as ApplicationUser Id

                     message.Property(m => m.SenderFullName)
                            .IsRequired()
                            .HasMaxLength(200);

                     message.Property(m => m.SenderPhoneNumber)
                            .IsRequired()
                            .HasMaxLength(20);

                     message.Property(m => m.Content)
                            .IsRequired()
                            .HasMaxLength(2000);

                     message.Property(m => m.SentAt)
                            .IsRequired()
                            .HasDefaultValueSql("NOW()");

                     message.Property(m => m.IsRead)
                            .IsRequired()
                            .HasDefaultValue(false);

                     message.Property(m => m.IsDeleted)
                            .IsRequired()
                            .HasDefaultValue(false);

                     message.Property(m => m.IsFromAdmin)
                            .IsRequired()
                            .HasDefaultValue(false);

                     // Indexes for better performance
                     message.HasIndex(m => m.SenderId);
                     message.HasIndex(m => m.SentAt);
                     message.HasIndex(m => m.IsRead);
                     message.HasIndex(m => m.IsDeleted);
                     message.HasIndex(m => m.IsFromAdmin);
              }
       }
}