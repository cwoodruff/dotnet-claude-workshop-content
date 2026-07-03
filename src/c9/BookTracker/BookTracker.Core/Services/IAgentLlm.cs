using System.Text.Json;

namespace BookTracker.Core.Services;

/// <summary>A tool the model asked to call, with its id and parsed arguments.</summary>
public record AgentToolRequest(string Id, string Name, IReadOnlyDictionary<string, JsonElement> Input);

/// <summary>The result of running one tool, fed back to the model.</summary>
public record AgentToolOutcome(string Id, string Result, bool IsError);

/// <summary>
/// One model turn: either tool requests to execute, or (when none) a final answer in <see cref="FinalText"/>.
/// </summary>
public record AgentTurn(string FinalText, IReadOnlyList<AgentToolRequest> ToolRequests)
{
    public bool WantsTools => ToolRequests.Count > 0;
}

/// <summary>
/// A live, stateful exchange with the model for one agent run. Implemented in Api over the Anthropic
/// SDK; this neutral seam lets <c>AgentService</c>'s loop be unit-tested without the SDK (C9).
/// </summary>
public interface IAgentConversation
{
    /// <summary>Send the initial user message and get the model's first turn.</summary>
    Task<AgentTurn> SendUserMessageAsync(CancellationToken ct = default);

    /// <summary>Submit the results of the requested tools and get the next turn.</summary>
    Task<AgentTurn> SubmitToolOutcomesAsync(IReadOnlyList<AgentToolOutcome> outcomes, CancellationToken ct = default);
}

/// <summary>Starts a conversation for an agent run. The mockable seam over the LLM.</summary>
public interface IAgentLlm
{
    IAgentConversation StartConversation(string userMessage);
}
