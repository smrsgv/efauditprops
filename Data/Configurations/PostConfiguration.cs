using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfAuditPropsPoC.Data.Configurations;

public class PostConfiguration : AuditableEntityConfiguration<Post>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Content).IsRequired();

        builder.HasMany(e => e.Comments)
               .WithOne(c => c.Post)
               .HasForeignKey(c => c.PostId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
