using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfAuditPropsPoC.Data.Configurations;

/// <summary>
/// Base configuration for all auditable entities.
/// Entity configurations inherit from this to get audit property configuration.
/// </summary>
public abstract class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : AuditableEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        // Configure audit properties
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // Call derived class configuration
        ConfigureEntity(builder);
    }

    /// <summary>
    /// Override this in derived classes to configure entity-specific properties.
    /// </summary>
    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
}
