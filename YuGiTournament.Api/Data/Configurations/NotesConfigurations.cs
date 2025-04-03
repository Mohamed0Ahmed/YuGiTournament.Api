using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuGiTournament.Api.Models;

namespace YuGiTournament.Api.Data.Configurations
{
    public class NotesConfigurations : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> note)
        {

            note.HasKey(p => p.Id);

            note.Property(p => p.Id)
                  .ValueGeneratedOnAdd()
                  .UseIdentityColumn();

            note.Property(n => n.IsDeleted)
                .IsRequired();
            
            note.Property(n => n.IsHidden)
                .IsRequired();
            
            note.Property(n => n.Content)
                .IsRequired();

        }
    }
}
