using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfAuditPropsPoC.Data;

/// <summary>
/// Alternative DbContext using interceptor for audit properties.
/// Uses IEntityTypeConfiguration classes (with base class) for configuration.
/// </summary>
public class BloggingContextWithInterceptor(DbContextOptions<BloggingContextWithInterceptor> options)
    : DbContext(options)
{
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloggingContextWithInterceptor).Assembly);
    }

    // No SaveChanges override - AuditableEntityInterceptor handles everything
}

/// <summary>
/// Extension to register the audit interceptor.
/// </summary>
public static class AuditInterceptorExtensions
{
    public static DbContextOptionsBuilder AddAuditInterceptor(this DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.AddInterceptors(new AuditableEntityInterceptor());
    }
}
