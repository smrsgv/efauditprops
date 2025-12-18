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

        // Apply audit property configuration to ALL entities implementing IAuditableEntity
        // This single line replaces repeated configuration for each entity
        modelBuilder.ConfigureAuditableEntities();
    }

    /// <summary>
    /// Synchronous SaveChanges override.
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
    /// </summary>
    private void SetAuditProperties()
    {
        ChangeTracker.DetectChanges();

        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }
    }
}
