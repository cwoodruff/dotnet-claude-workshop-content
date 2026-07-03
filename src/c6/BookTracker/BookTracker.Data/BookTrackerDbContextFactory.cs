using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookTracker.Data;

/// <summary>
/// Lets the EF Core tools (dotnet ef migrations / database update) build the context at design time.
/// </summary>
public class BookTrackerDbContextFactory : IDesignTimeDbContextFactory<BookTrackerDbContext>
{
    public BookTrackerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BookTrackerDbContext>()
            .UseSqlite("Data Source=booktracker.db")
            .Options;

        return new BookTrackerDbContext(options);
    }
}
