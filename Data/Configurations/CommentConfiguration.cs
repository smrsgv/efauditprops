using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfAuditPropsPoC.Data.Configurations;

public class CommentConfiguration : AuditableEntityConfiguration<Comment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Author).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Text).HasMaxLength(2000).IsRequired();
    }
}
