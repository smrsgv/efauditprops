using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfAuditPropsPoC.Data.Configurations;

public class BlogConfiguration : AuditableEntityConfiguration<Blog>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Blog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasMany(e => e.Posts)
               .WithOne(p => p.Blog)
               .HasForeignKey(p => p.BlogId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
