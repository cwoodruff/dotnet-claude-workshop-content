using BookTracker.Core.Services;
using BookTracker.Data;
using BookTracker.Mcp.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MCP server over Streamable HTTP; tools are discovered from [McpServerTool] methods in this assembly.
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Reuse the BookTracker stack against the same SQLite database. AddBookTrackerData registers the
// DbContext + repositories; the Core services are registered here (as the API host does).
var connectionString = builder.Configuration.GetConnectionString("BookTracker")
    ?? "Data Source=booktracker.db";
builder.Services.AddBookTrackerData(connectionString);
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IReadingProgressService, ReadingProgressService>();

builder.Services.AddScoped<AuditLogger>();

var app = builder.Build();

// Migrate/seed at startup so the server is runnable straight from a clone.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<BookTrackerDbContext>().Database.Migrate();
}

// Bearer-token gate (no-op unless Mcp:ApiKey is configured) — teaches the auth pattern.
app.UseMiddleware<McpAuth>();

app.MapMcp();

app.Run();
