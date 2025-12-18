namespace EfAuditPropsPoC.Entities;

/// <summary>
/// Represents a blog with nested posts.
/// Demonstrates parent entity with collection navigation property.
/// </summary>
public class Blog : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to child posts.
    /// EF Core will track changes to this collection.
    /// </summary>
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
