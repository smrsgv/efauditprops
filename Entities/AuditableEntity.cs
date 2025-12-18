namespace EfAuditPropsPoC.Entities;

/// <summary>
/// Base class for entities that require audit tracking.
/// Provides common Id property and audit timestamps.
/// </summary>
public abstract class AuditableEntity : IAuditableEntity
{
    public int Id { get; set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }
}
