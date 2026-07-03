using BookTracker.Api.Endpoints;
using BookTracker.Core.Services;
using BookTracker.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookTracker")
    ?? "Data Source=booktracker.db";

builder.Services.AddBookTrackerData(connectionString);
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var app = builder.Build();

// Apply migrations and seed at startup so the app is runnable straight from a clone.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookTrackerDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => "BookTracker API");
app.MapBookEndpoints();
app.MapAuthorEndpoints();
app.MapReviewsEndpoints();

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
