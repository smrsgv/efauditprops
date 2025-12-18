namespace EfAuditPropsPoC.Entities;

/// <summary>
/// Interface for entities that track creation and modification timestamps.
/// Entities implementing this interface will have their audit properties
/// automatically populated by the DbContext.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// The UTC timestamp when the entity was first created.
    /// This value is set once during initial insert and never modified by the application.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// The UTC timestamp when the entity was last modified.
    /// This value is updated on every modification.
    /// </summary>
    DateTime UpdatedAt { get; set; }
}
