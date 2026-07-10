using Demo6.McpServer.Services;

var builder = WebApplication.CreateBuilder(args);

// MCP server over Streamable HTTP; tools are discovered from [McpServerTool] methods in this assembly.
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// In-memory catalog stands in for the BookTracker database so this demo is fully self-contained.
// The shape is the same as the real BookTracker.Mcp server: tools call a service, never raw data.
builder.Services.AddSingleton<IBookCatalog, BookCatalog>();

var app = builder.Build();

// Maps the Streamable HTTP MCP endpoint at the root — clients connect to http://localhost:5100
app.MapMcp();

app.Run();
