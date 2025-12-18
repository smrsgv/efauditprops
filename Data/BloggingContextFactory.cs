using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfAuditPropsPoC.Data;

/// <summary>
/// Design-time factory for creating BloggingContext instances.
/// Used by EF Core tools for migrations.
/// </summary>
public class BloggingContextFactory : IDesignTimeDbContextFactory<BloggingContext>
{
    public BloggingContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BloggingContext>();

        // Use LocalDB for development - adjust connection string as needed
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=EfAuditPropsPoC;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new BloggingContext(optionsBuilder.Options);
    }
}
