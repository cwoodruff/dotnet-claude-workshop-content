using BookTracker.Core.Interfaces;
using BookTracker.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookTracker.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddBookTrackerData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BookTrackerDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        return services;
    }
}
