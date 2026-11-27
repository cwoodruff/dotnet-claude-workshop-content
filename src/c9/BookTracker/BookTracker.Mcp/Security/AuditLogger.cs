namespace BookTracker.Mcp.Security;

/// <summary>
/// Records every MCP tool invocation so you can answer "what did the agent do?". A teaching stub —
/// here it writes to ILogger; in production point it at a durable audit sink (this is formalized as
/// the AiAuditLog at C10).
/// </summary>
public class AuditLogger
{
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    public void ToolInvoked(string toolName, string argumentsSummary)
        => _logger.LogInformation("MCP tool invoked: {Tool} args=({Args})", toolName, argumentsSummary);

    public void WriteConfirmed(string toolName, string argumentsSummary)
        => _logger.LogInformation("MCP write confirmed by user: {Tool} args=({Args})", toolName, argumentsSummary);

    public void WriteDeclined(string toolName, string argumentsSummary)
        => _logger.LogWarning("MCP write declined, nothing written: {Tool} args=({Args})", toolName, argumentsSummary);
}
