using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfAuditPropsPoC.Data;

/// <summary>
/// DbContext with automatic audit property population.
/// Overrides SaveChanges/SaveChangesAsync to set CreatedAt and UpdatedAt
/// for all entities implementing IAuditableEntity.
/// </summary>
public class BloggingContext : DbContext
{
    public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
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

            // Configure audit properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

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

            // Configure audit properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

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

            // Configure audit properties
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }

    /// <summary>
    /// Synchronous SaveChanges override - calls the async version.
    /// </summary>
    public override int SaveChanges()
    {
        SetAuditProperties();
        return base.SaveChanges();
    }

    /// <summary>
    /// Synchronous SaveChanges with acceptAllChangesOnSuccess override.
    /// </summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditProperties();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// Async SaveChangesAsync override - sets audit properties before saving.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Async SaveChangesAsync with acceptAllChangesOnSuccess override.
    /// </summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Sets CreatedAt and UpdatedAt properties for all tracked entities
    /// that implement IAuditableEntity.
    ///
    /// Key behaviors:
    /// - For Added entities: Sets both CreatedAt and UpdatedAt to current UTC time
    /// - For Modified entities: Only updates UpdatedAt
    /// - Handles nested/related entities automatically via change tracker
    /// </summary>
    private void SetAuditProperties()
    {
        // Ensure change detection has run
        ChangeTracker.DetectChanges();

        var utcNow = DateTime.UtcNow;

        // Get all tracked entities that implement IAuditableEntity
        var auditableEntries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in auditableEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // New entity - set both timestamps
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    // Existing entity being modified - only update UpdatedAt
                    // Ensure CreatedAt is not modified (protect against accidental changes)
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }
    }
}
