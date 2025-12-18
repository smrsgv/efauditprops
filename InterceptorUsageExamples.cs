using EfAuditPropsPoC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EfAuditPropsPoC;

/// <summary>
/// Examples showing how to register the AuditableEntityInterceptor
/// in different scenarios.
/// </summary>
public static class InterceptorUsageExamples
{
    /// <summary>
    /// Example 1: Console application or direct usage
    /// </summary>
    public static BloggingContextWithInterceptor CreateContextDirectly()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BloggingContextWithInterceptor>();

        optionsBuilder
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EfAuditPropsPoC;Trusted_Connection=True;")
            .AddAuditInterceptor(); // <-- Register interceptor here

        return new BloggingContextWithInterceptor(optionsBuilder.Options);
    }

    /// <summary>
    /// Example 2: ASP.NET Core with Dependency Injection
    /// Add this to Program.cs or Startup.cs
    /// </summary>
    public static void ConfigureServicesExample(IServiceCollection services)
    {
        // Option A: Using the extension method
        services.AddDbContext<BloggingContextWithInterceptor>(options =>
            options.UseSqlServer("YourConnectionString")
                   .AddAuditInterceptor());

        // Option B: Register interceptor as a service (useful for injecting dependencies)
        // services.AddSingleton<AuditableEntityInterceptor>();
        // services.AddDbContext<BloggingContextWithInterceptor>((sp, options) =>
        //     options.UseSqlServer("YourConnectionString")
        //            .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));
    }

    /// <summary>
    /// Example 3: Interceptor with user context (e.g., for CreatedBy/UpdatedBy)
    /// </summary>
    public static void ConfigureWithUserContext(IServiceCollection services)
    {
        // For scenarios where you need CreatedBy/UpdatedBy,
        // you can inject IHttpContextAccessor or a user service:
        //
        // services.AddScoped<AuditableEntityInterceptorWithUser>();
        // services.AddDbContext<BloggingContextWithInterceptor>((sp, options) =>
        //     options.UseSqlServer("YourConnectionString")
        //            .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptorWithUser>()));
    }
}

/*
 * =============================================================================
 * COMPARISON: SaveChangesAsync Override vs Interceptor
 * =============================================================================
 *
 * | Aspect                  | Override Approach        | Interceptor Approach     |
 * |-------------------------|--------------------------|--------------------------|
 * | Code Location           | Inside DbContext         | Separate class           |
 * | Reusability             | Per DbContext            | Shared across contexts   |
 * | Testability             | Harder to isolate        | Easy to unit test        |
 * | Dependency Injection    | Limited                  | Full DI support          |
 * | Multiple Interceptors   | Manual composition       | Built-in chaining        |
 * | Performance             | Slightly faster          | Minimal overhead         |
 * | Complexity              | Simpler for single use   | Better for enterprise    |
 *
 * RECOMMENDATION:
 * - Small projects / PoCs: SaveChangesAsync override is fine
 * - Production / Enterprise: Use Interceptor for better separation of concerns
 * - Need CreatedBy/UpdatedBy: Use Interceptor with DI for user context access
 *
 * =============================================================================
 */
