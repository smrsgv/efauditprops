using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfAuditPropsPoC.Data;

/// <summary>
/// Extension methods for ModelBuilder to reduce configuration duplication.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Automatically configures audit properties (CreatedAt, UpdatedAt) for all
    /// entities implementing IAuditableEntity.
    ///
    /// This eliminates the need to manually configure these properties for each entity.
    /// </summary>
    public static ModelBuilder ConfigureAuditableEntities(this ModelBuilder modelBuilder)
    {
        // Find all entity types that implement IAuditableEntity
        var auditableEntityTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(e => typeof(IAuditableEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in auditableEntityTypes)
        {
            // Configure CreatedAt
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IAuditableEntity.CreatedAt))
                .IsRequired();

            // Configure UpdatedAt
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IAuditableEntity.UpdatedAt))
                .IsRequired();
        }

        return modelBuilder;
    }
}
