using System.Text.Json;
using BookTracker.Api.Services;
using BookTracker.Api.Tools;
using BookTracker.Core.Services;

namespace BookTracker.Tests.Agent;

/// <summary>Test doubles + helpers for exercising the AgentService loop without the SDK.</summary>
internal static class AgentTestHarness
{
    /// <summary>Build a tool-request argument dictionary from an anonymous object.</summary>
    public static IReadOnlyDictionary<string, JsonElement> Args(object values)
        => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(values))!;

    public static AgentToolRequest ToolRequest(string name, object args, string? id = null)
        => new(id ?? Guid.NewGuid().ToString(), name, Args(args));

    public static AgentTurn Final(string text) => new(text, []);

    public static AgentTurn ToolTurn(params AgentToolRequest[] requests) => new(string.Empty, requests);

    public static AgentService Service(IAgentLlm llm, BookTrackerTools tools) => new(llm, tools);
}

/// <summary>An IAgentLlm whose single conversation replays a scripted list of turns.</summary>
internal sealed class ScriptedLlm : IAgentLlm
{
    private readonly ScriptedConversation _conversation;

    public ScriptedLlm(params AgentTurn[] turns) => _conversation = new ScriptedConversation(turns);

    public string? LastUserMessage { get; private set; }

    public ScriptedConversation Conversation => _conversation;

    public IAgentConversation StartConversation(string userMessage)
    {
        LastUserMessage = userMessage;
        return _conversation;
    }
}

internal sealed class ScriptedConversation : IAgentConversation
{
    private readonly Queue<AgentTurn> _turns;

    public ScriptedConversation(IEnumerable<AgentTurn> turns) => _turns = new Queue<AgentTurn>(turns);

    /// <summary>The tool outcomes submitted on each round, in order — for assertions.</summary>
    public List<IReadOnlyList<AgentToolOutcome>> SubmittedOutcomes { get; } = [];

    public Task<AgentTurn> SendUserMessageAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_turns.Dequeue());
    }

    public Task<AgentTurn> SubmitToolOutcomesAsync(
        IReadOnlyList<AgentToolOutcome> outcomes,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        SubmittedOutcomes.Add(outcomes);
        return Task.FromResult(_turns.Dequeue());
    }
}

/// <summary>An IAgentLlm that always asks for the same tool — used to exercise the iteration cap.</summary>
internal sealed class AlwaysCallsToolLlm : IAgentLlm
{
    private readonly AgentToolRequest _request;

    public AlwaysCallsToolLlm(AgentToolRequest request) => _request = request;

    public IAgentConversation StartConversation(string userMessage) => new Loop(_request);

    private sealed class Loop : IAgentConversation
    {
        private readonly AgentToolRequest _request;

        public Loop(AgentToolRequest request) => _request = request;

        public Task<AgentTurn> SendUserMessageAsync(CancellationToken ct = default)
            => Task.FromResult(new AgentTurn(string.Empty, [_request]));

        public Task<AgentTurn> SubmitToolOutcomesAsync(
            IReadOnlyList<AgentToolOutcome> outcomes, CancellationToken ct = default)
            => Task.FromResult(new AgentTurn(string.Empty, [_request]));
    }
}
