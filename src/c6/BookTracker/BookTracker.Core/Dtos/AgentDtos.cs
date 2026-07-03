namespace BookTracker.Core.Dtos;

/// <summary>Agent request — a single natural-language instruction.</summary>
public record AgentRequest(string Message);

/// <summary>One tool the agent invoked during a run (surfaced so the multi-step loop is visible).</summary>
public record AgentToolCall(string Name, string Input, string Result, bool IsError);

/// <summary>The agent's final reply plus the tool calls it made to get there.</summary>
public record AgentResult(string Reply, IReadOnlyList<AgentToolCall> ToolCalls);
