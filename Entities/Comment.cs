namespace EfAuditPropsPoC.Entities;

/// <summary>
/// Represents a comment on a blog post.
/// Demonstrates leaf-level entity in the nested hierarchy.
/// </summary>
public class Comment : AuditableEntity
{
    public string Author { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to parent Post.
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// Navigation property to parent Post.
    /// </summary>
    public Post Post { get; set; } = null!;
}
