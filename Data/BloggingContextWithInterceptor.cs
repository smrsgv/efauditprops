using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfAuditPropsPoC.Data;

/// <summary>
/// Alternative DbContext implementation using interceptor for audit properties.
/// This is a cleaner approach as it separates concerns.
///
/// Compare with BloggingContext which uses SaveChangesAsync override.
/// </summary>
public class BloggingContextWithInterceptor : DbContext
{
    public BloggingContextWithInterceptor(DbContextOptions<BloggingContextWithInterceptor> options)
        : base(options)
    {
    }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Blog entity
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasMany(e => e.Posts)
                  .WithOne(p => p.Blog)
                  .HasForeignKey(p => p.BlogId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Post entity
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Content).IsRequired();

            entity.HasMany(e => e.Comments)
                  .WithOne(c => c.Post)
                  .HasForeignKey(c => c.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Comment entity
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Author).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Text).HasMaxLength(2000).IsRequired();
        });

        // Apply audit property configuration automatically
        modelBuilder.ConfigureAuditableEntities();
    }

    // NOTE: No SaveChanges override needed!
    // The AuditableEntityInterceptor handles everything.
}

/// <summary>
/// Extension methods to register the interceptor with DbContext.
/// </summary>
public static class AuditInterceptorExtensions
{
    /// <summary>
    /// Adds the AuditableEntityInterceptor to the DbContext options.
    ///
    /// Usage:
    ///   services.AddDbContext&lt;BloggingContextWithInterceptor&gt;(options =>
    ///       options.UseSqlServer(connectionString)
    ///              .AddAuditInterceptor());
    /// </summary>
    public static DbContextOptionsBuilder AddAuditInterceptor(
        this DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.AddInterceptors(new AuditableEntityInterceptor());
    }
}
