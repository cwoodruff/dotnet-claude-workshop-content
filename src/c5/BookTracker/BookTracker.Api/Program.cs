using Anthropic;
using BookTracker.Api.Endpoints;
using BookTracker.Api.Options;
using BookTracker.Api.Services;
using BookTracker.Core.Services;
using BookTracker.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BookTracker")
    ?? "Data Source=booktracker.db";

builder.Services.AddBookTrackerData(connectionString);
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReadingProgressService, ReadingProgressService>();

// --- Day 2: Claude SDK chat ---
builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection("Anthropic"));

// AnthropicClient is a Singleton — it holds an HttpClient; Scoped/Transient risks socket exhaustion.
// (The SDK already retries 429/5xx with backoff, so no extra resilience pipeline is wired here.)
builder.Services.AddSingleton(sp =>
    new AnthropicClient { ApiKey = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value.ApiKey });
builder.Services.AddScoped<IClaudeService, ClaudeService>();

// In-memory IDistributedCache for chat history — zero infra. Swap AddStackExchangeRedisCache for prod.
builder.Services.AddDistributedMemoryCache();

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
app.MapReadingProgressEndpoints();
app.MapChatEndpoints();

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
