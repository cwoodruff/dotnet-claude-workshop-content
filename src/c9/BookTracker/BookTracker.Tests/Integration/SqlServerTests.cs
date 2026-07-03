using BookTracker.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace BookTracker.Tests.Integration;

/// <summary>
/// Stretch integration test: the data layer against real SQL Server in Docker (Testcontainers).
/// Gated behind RUN_MSSQL_TESTS=1 because the SQL Server image is large.
///
/// Caveat (build sheet §5): the EF migrations are SQLite-specific, so schema is created with
/// EnsureCreated() (model-driven, provider-agnostic) rather than Migrate(). EnsureCreated also applies
/// the HasData seed.
/// </summary>
public class SqlServerTests
{
    [DockerFact("RUN_MSSQL_TESTS")]
    public async Task DataLayer_OnSqlServer_AppliesSchemaAndSeed()
    {
        await using var container = new MsSqlBuilder().Build();
        await container.StartAsync();

        var options = new DbContextOptionsBuilder<BookTrackerDbContext>()
            .UseSqlServer(container.GetConnectionString())
            .Options;

        await using var db = new BookTrackerDbContext(options);
        await db.Database.EnsureCreatedAsync();

        // The HasData seed (5 books) is applied by EnsureCreated.
        Assert.True(await db.Books.CountAsync() >= 5);
        Assert.True(await db.Authors.CountAsync() >= 5);
    }
}
