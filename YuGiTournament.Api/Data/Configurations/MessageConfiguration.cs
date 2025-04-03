using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.SenderId).IsRequired();
            builder.Property(m => m.SenderFullName).IsRequired();
            builder.Property(m => m.SenderPhoneNumber).IsRequired();
            builder.Property(m => m.Content).IsRequired();
            builder.Property(m => m.IsRead).IsRequired();
            builder.Property(m => m.SentAt).IsRequired();
            builder.Property(m => m.IsDeleted).IsRequired();
        }
    }
}