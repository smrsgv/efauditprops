namespace EfAuditPropsPoC.Entities;

/// <summary>
/// Represents a blog post with nested comments.
/// Demonstrates middle-level entity with both parent and child navigation properties.
/// </summary>
public class Post : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to parent Blog.
    /// </summary>
    public int BlogId { get; set; }

    /// <summary>
    /// Navigation property to parent Blog.
    /// </summary>
    public Blog Blog { get; set; } = null!;

    /// <summary>
    /// Navigation property to child comments.
    /// EF Core will track changes to this collection.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
