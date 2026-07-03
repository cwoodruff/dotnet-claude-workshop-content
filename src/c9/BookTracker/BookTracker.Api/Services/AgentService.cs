using System.Text.Json;
using BookTracker.Api.Tools;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;

namespace BookTracker.Api.Services;

/// <summary>
/// The tool-calling agent loop, now provider-neutral: it drives an <see cref="IAgentConversation"/>
/// (the LLM seam) and dispatches tools through <see cref="BookTrackerTools"/>. Guarded by a
/// max-iteration cap and per-tool try/catch. Free of SDK types, so the loop is unit-testable (C9).
/// </summary>
public class AgentService : IAgentService
{
    private const int MaxIterations = 5;

    private readonly IAgentLlm _llm;
    private readonly BookTrackerTools _tools;

    public AgentService(IAgentLlm llm, BookTrackerTools tools)
    {
        _llm = llm;
        _tools = tools;
    }

    public async Task<AgentResult> RunAsync(string message, CancellationToken ct = default)
    {
        var conversation = _llm.StartConversation(message);
        var toolCalls = new List<AgentToolCall>();

        var turn = await conversation.SendUserMessageAsync(ct);

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            if (!turn.WantsTools)
            {
                return new AgentResult(turn.FinalText, toolCalls);
            }

            var outcomes = new List<AgentToolOutcome>();
            foreach (var request in turn.ToolRequests)
            {
                string result;
                var isError = false;
                try
                {
                    result = await _tools.ExecuteAsync(request.Name, request.Input, ct);
                }
                catch (OperationCanceledException)
                {
                    throw; // cancellation propagates — not a tool error
                }
                catch (Exception ex)
                {
                    // Surface rule violations (e.g. the C4 state machine) as a tool error, don't crash.
                    result = $"Error: {ex.Message}";
                    isError = true;
                }

                toolCalls.Add(new AgentToolCall(
                    request.Name, JsonSerializer.Serialize(request.Input), result, isError));
                outcomes.Add(new AgentToolOutcome(request.Id, result, isError));
            }

            turn = await conversation.SubmitToolOutcomesAsync(outcomes, ct);
        }

        return new AgentResult(
            "Stopped after reaching the maximum number of tool iterations.", toolCalls);
    }
}
