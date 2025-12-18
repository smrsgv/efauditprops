using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EfAuditPropsPoC.Migrations
{
    /// <summary>
    /// Migration that adds database-level triggers for audit properties.
    /// These triggers ensure UpdatedAt is properly set even when data is
    /// modified directly in the database (bypassing EF Core).
    ///
    /// This addresses the concern: "What happens if someone manually modifies data?"
    ///
    /// Key behaviors:
    /// - INSERT: Sets CreatedAt and UpdatedAt if they are '0001-01-01' (default DateTime)
    /// - UPDATE: Always updates UpdatedAt to current UTC time
    /// </summary>
    public partial class AddAuditTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create triggers for each auditable table

            // ==================== BLOGS TABLE ====================

            // Trigger for INSERT - handles manual inserts without timestamps
            migrationBuilder.Sql(@"
                CREATE TRIGGER [dbo].[TR_Blogs_Insert_Audit]
                ON [dbo].[Blogs]
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- If CreatedAt was not set (is default/1753-01-01 or 0001-01-01),
                    -- update it to current UTC time
                    UPDATE b
                    SET
                        b.CreatedAt = CASE
                            WHEN i.CreatedAt < '1900-01-01' THEN GETUTCDATE()
                            ELSE i.CreatedAt
                        END,
                        b.UpdatedAt = CASE
                            WHEN i.UpdatedAt < '1900-01-01' THEN GETUTCDATE()
                            ELSE i.UpdatedAt
                        END
                    FROM [dbo].[Blogs] b
                    INNER JOIN inserted i ON b.Id = i.Id
                    WHERE i.CreatedAt < '1900-01-01' OR i.UpdatedAt < '1900-01-01';
                END
            ");

            // Trigger for UPDATE - always update UpdatedAt on modification
            migrationBuilder.Sql(@"
                CREATE TRIGGER [dbo].[TR_Blogs_Update_Audit]
                ON [dbo].[Blogs]
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- Skip if only UpdatedAt was changed (avoid infinite loop)
                    IF NOT UPDATE(Name) AND NOT UPDATE(Description)
                        RETURN;

                    UPDATE b
                    SET b.UpdatedAt = GETUTCDATE()
                    FROM [dbo].[Blogs] b
                    INNER JOIN inserted i ON b.Id = i.Id;
                END
            ");

            // ==================== POSTS TABLE ====================

            migrationBuilder.Sql(@"
                CREATE TRIGGER [dbo].[TR_Posts_Insert_Audit]
                ON [dbo].[Posts]
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE p
                    SET
                        p.CreatedAt = CASE
                            WHEN i.CreatedAt < '1900-01-01' THEN GETUTCDATE()
                            ELSE i.CreatedAt
                        END,
                        p.UpdatedAt = CASE
                            WHEN i.UpdatedAt < '1900-01-01' THEN GETUTCDATE()
                            ELSE i.UpdatedAt
                        END
                    FROM [dbo].[Posts] p
                    INNER JOIN inserted i ON p.Id = i.Id
                    WHERE i.CreatedAt < '1900-01-01' OR i.UpdatedAt < '1900-01-01';
                END
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER [dbo].[TR_Posts_Update_Audit]
                ON [dbo].[Posts]
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- Skip if only UpdatedAt was changed (avoid infinite loop)
                    IF NOT UPDATE(Title) AND NOT UPDATE(Content) AND NOT UPDATE(BlogId)
                        RETURN;

                    UPDATE p
                    SET p.UpdatedAt = GETUTCDATE()
                    FROM [dbo].[Posts] p
                    INNER JOIN inserted i ON p.Id = i.Id;
                END
            ");

            // ==================== COMMENTS TABLE ====================

            migrationBuilder.Sql(@"
                CREATE TRIGGER [dbo].[TR_Comments_Insert_Audit]
                ON [dbo].[Comments]
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    UPDATE c
                    SET
                        c.CreatedAt = CASE
                            WHEN i.CreatedAt < '1900-01-01' THEN GETUTCDATE()
                            ELSE i.CreatedAt
                        END,
                        c.UpdatedAt = CASE
                            WHEN i.UpdatedAt < '1900-01-01' THEN GETUTCDATE()
                            ELSE i.UpdatedAt
                        END
                    FROM [dbo].[Comments] c
                    INNER JOIN inserted i ON c.Id = i.Id
                    WHERE i.CreatedAt < '1900-01-01' OR i.UpdatedAt < '1900-01-01';
                END
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER [dbo].[TR_Comments_Update_Audit]
                ON [dbo].[Comments]
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- Skip if only UpdatedAt was changed (avoid infinite loop)
                    IF NOT UPDATE(Author) AND NOT UPDATE([Text]) AND NOT UPDATE(PostId)
                        RETURN;

                    UPDATE c
                    SET c.UpdatedAt = GETUTCDATE()
                    FROM [dbo].[Comments] c
                    INNER JOIN inserted i ON c.Id = i.Id;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all audit triggers
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_Blogs_Insert_Audit]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_Blogs_Update_Audit]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_Posts_Insert_Audit]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_Posts_Update_Audit]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_Comments_Insert_Audit]");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_Comments_Update_Audit]");
        }
    }
}
