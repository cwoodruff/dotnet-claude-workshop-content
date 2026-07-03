using BookTracker.Core.Dtos;

namespace BookTracker.Core.Services;

/// <summary>
/// Runs the tool-calling agent loop: Claude plans, calls BookTracker tools against real data, and
/// returns a final answer. Pure interface — SDK impl lives in BookTracker.Api.
/// </summary>
public interface IAgentService
{
    Task<AgentResult> RunAsync(string message, CancellationToken ct = default);
}
