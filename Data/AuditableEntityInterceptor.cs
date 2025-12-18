using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfAuditPropsPoC.Data;

/// <summary>
/// Interceptor that automatically sets audit properties (CreatedAt, UpdatedAt)
/// for entities implementing IAuditableEntity.
///
/// Benefits over SaveChangesAsync override:
/// 1. Single Responsibility - audit logic is separated from DbContext
/// 2. Reusable - can be shared across multiple DbContexts
/// 3. Testable - easier to unit test in isolation
/// 4. Configurable - can be conditionally registered
/// 5. Composable - multiple interceptors can be chained
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Called synchronously before SaveChanges executes.
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetAuditProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Called asynchronously before SaveChangesAsync executes.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetAuditProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Sets CreatedAt and UpdatedAt properties for all tracked auditable entities.
    /// </summary>
    private static void SetAuditProperties(DbContext? context)
    {
        if (context is null)
            return;

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    // Protect CreatedAt from being modified
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }
    }
}
