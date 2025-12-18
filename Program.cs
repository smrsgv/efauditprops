using EfAuditPropsPoC.Data;
using EfAuditPropsPoC.Entities;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine("EF CORE AUDIT PROPERTIES - PROOF OF CONCEPT");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine();

// Configure the DbContext
var optionsBuilder = new DbContextOptionsBuilder<BloggingContext>();
optionsBuilder.UseSqlServer(
    "Server=(localdb)\\mssqllocaldb;Database=EfAuditPropsPoC;Trusted_Connection=True;MultipleActiveResultSets=true");

// Enable detailed logging to see what EF Core is doing
optionsBuilder.LogTo(
    msg => Console.WriteLine($"  [EF] {msg}"),
    new[] { DbLoggerCategory.Database.Command.Name },
    Microsoft.Extensions.Logging.LogLevel.Information);

await using var context = new BloggingContext(optionsBuilder.Options);

// Ensure database is created and migrations are applied
Console.WriteLine("Setting up database...");
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

// Note: For proper trigger support, run migrations instead:
// dotnet ef database update
Console.WriteLine("Database ready!");
Console.WriteLine();

// ============================================================
// DEMO 1: Creating nested entities
// ============================================================
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine("DEMO 1: Creating nested entities");
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine();

var blog = new Blog
{
    Name = "Tech Blog",
    Description = "A blog about technology",
    Posts = new List<Post>
    {
        new Post
        {
            Title = "Introduction to EF Core",
            Content = "Entity Framework Core is an ORM...",
            Comments = new List<Comment>
            {
                new Comment { Author = "Alice", Text = "Great article!" },
                new Comment { Author = "Bob", Text = "Very helpful, thanks!" }
            }
        },
        new Post
        {
            Title = "Advanced EF Core Features",
            Content = "Let's explore some advanced features...",
            Comments = new List<Comment>
            {
                new Comment { Author = "Charlie", Text = "Looking forward to more!" }
            }
        }
    }
};

Console.WriteLine("Before SaveChangesAsync - checking audit properties:");
Console.WriteLine($"  Blog.CreatedAt: {blog.CreatedAt} (should be default: 01/01/0001)");
Console.WriteLine($"  Blog.UpdatedAt: {blog.UpdatedAt} (should be default: 01/01/0001)");
Console.WriteLine($"  Post[0].CreatedAt: {blog.Posts.First().CreatedAt}");
Console.WriteLine($"  Comment[0].CreatedAt: {blog.Posts.First().Comments.First().CreatedAt}");
Console.WriteLine();

context.Blogs.Add(blog);
await context.SaveChangesAsync();

Console.WriteLine("After SaveChangesAsync - audit properties should be set:");
Console.WriteLine($"  Blog.CreatedAt: {blog.CreatedAt:O}");
Console.WriteLine($"  Blog.UpdatedAt: {blog.UpdatedAt:O}");
Console.WriteLine($"  Post[0].CreatedAt: {blog.Posts.First().CreatedAt:O}");
Console.WriteLine($"  Post[0].UpdatedAt: {blog.Posts.First().UpdatedAt:O}");
Console.WriteLine($"  Comment[0].CreatedAt: {blog.Posts.First().Comments.First().CreatedAt:O}");
Console.WriteLine($"  Comment[0].UpdatedAt: {blog.Posts.First().Comments.First().UpdatedAt:O}");
Console.WriteLine();

var createdAtAfterCreate = blog.CreatedAt;
var updatedAtAfterCreate = blog.UpdatedAt;

// ============================================================
// DEMO 2: Modifying parent entity only
// ============================================================
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine("DEMO 2: Modifying parent entity only");
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine();

await Task.Delay(1000); // Wait to ensure different timestamp

blog.Description = "Updated: A comprehensive blog about technology";
Console.WriteLine("Modified Blog.Description...");

await context.SaveChangesAsync();

Console.WriteLine("After modifying Blog only:");
Console.WriteLine($"  Blog.CreatedAt: {blog.CreatedAt:O} (should be unchanged)");
Console.WriteLine($"  Blog.UpdatedAt: {blog.UpdatedAt:O} (should be updated)");
Console.WriteLine($"  Post[0].UpdatedAt: {blog.Posts.First().UpdatedAt:O} (should be unchanged)");
Console.WriteLine($"  CreatedAt unchanged: {blog.CreatedAt == createdAtAfterCreate}");
Console.WriteLine($"  UpdatedAt changed: {blog.UpdatedAt > updatedAtAfterCreate}");
Console.WriteLine();

// ============================================================
// DEMO 3: Modifying nested child entity only
// ============================================================
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine("DEMO 3: Modifying nested child entity (Comment) only");
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine();

await Task.Delay(1000);

var parentUpdatedAtBefore = blog.UpdatedAt;
var postUpdatedAtBefore = blog.Posts.First().UpdatedAt;

var comment = blog.Posts.First().Comments.First();
var commentCreatedAtBefore = comment.CreatedAt;
comment.Text = "Great article! Updated my comment.";

Console.WriteLine("Modified Comment.Text (nested grandchild entity)...");

await context.SaveChangesAsync();

Console.WriteLine("After modifying Comment only:");
Console.WriteLine($"  Blog.UpdatedAt: {blog.UpdatedAt:O} (unchanged: {blog.UpdatedAt == parentUpdatedAtBefore})");
Console.WriteLine($"  Post[0].UpdatedAt: {blog.Posts.First().UpdatedAt:O} (unchanged: {blog.Posts.First().UpdatedAt == postUpdatedAtBefore})");
Console.WriteLine($"  Comment.CreatedAt: {comment.CreatedAt:O} (unchanged: {comment.CreatedAt == commentCreatedAtBefore})");
Console.WriteLine($"  Comment.UpdatedAt: {comment.UpdatedAt:O} (should be updated)");
Console.WriteLine();
Console.WriteLine("KEY INSIGHT: Only the modified entity's UpdatedAt changed!");
Console.WriteLine("Parent entities (Blog, Post) were NOT affected.");
Console.WriteLine();

// ============================================================
// DEMO 4: Adding new nested entity to existing parent
// ============================================================
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine("DEMO 4: Adding new nested entity to existing parent");
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine();

await Task.Delay(1000);

var existingPost = blog.Posts.First();
var postUpdatedAtBeforeAddingComment = existingPost.UpdatedAt;

existingPost.Comments.Add(new Comment
{
    Author = "Diana",
    Text = "Just discovered this blog!"
});

Console.WriteLine("Added new Comment to existing Post...");

await context.SaveChangesAsync();

var newComment = existingPost.Comments.Last();
Console.WriteLine("After adding new Comment:");
Console.WriteLine($"  New Comment.CreatedAt: {newComment.CreatedAt:O}");
Console.WriteLine($"  New Comment.UpdatedAt: {newComment.UpdatedAt:O}");
Console.WriteLine($"  Parent Post.UpdatedAt: {existingPost.UpdatedAt:O} (unchanged: {existingPost.UpdatedAt == postUpdatedAtBeforeAddingComment})");
Console.WriteLine();
Console.WriteLine("KEY INSIGHT: Adding a child does NOT update parent's UpdatedAt.");
Console.WriteLine("This is intentional - parent wasn't modified, only had a child added.");
Console.WriteLine();

// ============================================================
// DEMO 5: Change tracker inspection
// ============================================================
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine("DEMO 5: Change tracker inspection - what gets tracked?");
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine();

await Task.Delay(1000);

// Make changes to multiple entities
blog.Name = "Tech Blog v2";
blog.Posts.First().Title = "Updated: Introduction to EF Core";

Console.WriteLine("Modified Blog.Name and Post[0].Title...");
Console.WriteLine();

// Inspect change tracker before save
Console.WriteLine("Change Tracker state BEFORE SaveChangesAsync:");
context.ChangeTracker.DetectChanges();

foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
{
    if (entry.State == EntityState.Modified)
    {
        Console.WriteLine($"  [{entry.State}] {entry.Entity.GetType().Name} (Id: {entry.Entity.GetType().GetProperty("Id")?.GetValue(entry.Entity)})");

        var modifiedProps = entry.Properties
            .Where(p => p.IsModified)
            .Select(p => p.Metadata.Name);

        Console.WriteLine($"    Modified properties: {string.Join(", ", modifiedProps)}");
    }
}
Console.WriteLine();

await context.SaveChangesAsync();

Console.WriteLine("After SaveChangesAsync - both entities updated:");
Console.WriteLine($"  Blog.UpdatedAt: {blog.UpdatedAt:O}");
Console.WriteLine($"  Post[0].UpdatedAt: {blog.Posts.First().UpdatedAt:O}");
Console.WriteLine();

// ============================================================
// DEMO 6: Protection against CreatedAt modification
// ============================================================
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine("DEMO 6: CreatedAt is protected from accidental modification");
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine();

var originalCreatedAt = blog.CreatedAt;
blog.CreatedAt = DateTime.UtcNow.AddYears(-10); // Try to change CreatedAt
Console.WriteLine("Attempted to change Blog.CreatedAt to 10 years ago...");

await context.SaveChangesAsync();

// Reload from database to verify
await context.Entry(blog).ReloadAsync();

Console.WriteLine($"  Original CreatedAt: {originalCreatedAt:O}");
Console.WriteLine($"  Current CreatedAt:  {blog.CreatedAt:O}");
Console.WriteLine($"  Protected: {blog.CreatedAt == originalCreatedAt}");
Console.WriteLine();

// ============================================================
// Summary
// ============================================================
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine("SUMMARY");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine();
Console.WriteLine("This PoC demonstrates:");
Console.WriteLine();
Console.WriteLine("1. AUTOMATIC TIMESTAMPS via SaveChangesAsync override:");
Console.WriteLine("   - CreatedAt is set once on entity creation");
Console.WriteLine("   - UpdatedAt is updated on every modification");
Console.WriteLine();
Console.WriteLine("2. NESTED ENTITY TRACKING:");
Console.WriteLine("   - EF Core tracks changes to nested entities independently");
Console.WriteLine("   - Modifying a child does NOT affect parent's timestamps");
Console.WriteLine("   - Each entity's audit properties are managed separately");
Console.WriteLine();
Console.WriteLine("3. DATABASE TRIGGERS (for manual DB modifications):");
Console.WriteLine("   - INSERT triggers: Set timestamps if not provided");
Console.WriteLine("   - UPDATE triggers: Always update UpdatedAt");
Console.WriteLine("   - Protects against direct SQL modifications");
Console.WriteLine();
Console.WriteLine("4. PROTECTION:");
Console.WriteLine("   - CreatedAt is protected from accidental modification");
Console.WriteLine("   - Use entry.Property(e => e.CreatedAt).IsModified = false");
Console.WriteLine();

// Cleanup
Console.WriteLine("-".PadRight(70, '-'));
Console.WriteLine("Demo complete! Database can be inspected at:");
Console.WriteLine("  (localdb)\\mssqllocaldb - Database: EfAuditPropsPoC");
Console.WriteLine();
Console.WriteLine("To apply migrations with triggers, run:");
Console.WriteLine("  dotnet ef database update");
Console.WriteLine("-".PadRight(70, '-'));
