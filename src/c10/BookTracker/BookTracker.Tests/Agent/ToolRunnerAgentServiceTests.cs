using System.Net;
using System.Text;
using Anthropic;
using BookTracker.Api.Options;
using BookTracker.Api.Services;
using BookTracker.Api.Tools;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Tests.Agent;

/// <summary>
/// Drives ToolRunnerAgentService against a scripted HTTP handler (no API key, no network): the model
/// asks for find_book, then answers. Proves the runner runs the tool, records the call, and sends the
/// thinking block back unchanged, which the hand-written loop has to do itself.
/// </summary>
public class ToolRunnerAgentServiceTests
{
    private const string ToolUseTurn = """
        {"id":"msg_1","type":"message","role":"assistant","model":"claude-opus-5-5",
         "content":[
           {"type":"thinking","thinking":"I should look the book up.","signature":"sig-abc"},
           {"type":"tool_use","id":"toolu_1","name":"find_book","input":{"query":"Dune"}}],
         "stop_reason":"tool_use","stop_sequence":null,"usage":{"input_tokens":20,"output_tokens":10}}
        """;

    private const string FinalTurn = """
        {"id":"msg_2","type":"message","role":"assistant","model":"claude-opus-5-5",
         "content":[{"type":"text","text":"Dune is book 4."}],
         "stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":30,"output_tokens":5}}
        """;

    [Fact]
    public async Task RunAsync_ToolThenAnswer_RunsToolAndEchoesThinking()
    {
        var handler = new ScriptedHandler(ToolUseTurn, FinalTurn);
        var service = CreateService(handler, new FakeBooks());

        var result = await service.RunAsync("Find Dune");

        Assert.Equal("Dune is book 4.", result.Reply);
        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("find_book", call.Name);
        Assert.False(call.IsError);
        Assert.Contains("Dune", call.Result);
        Assert.Equal(2, handler.Bodies.Count);
        Assert.Contains("\"signature\":\"sig-abc\"", handler.Bodies[1]);
        Assert.Contains("\"tool_use_id\":\"toolu_1\"", handler.Bodies[1]);
        Assert.Equal(50, result.InputTokens);
        Assert.Equal(15, result.OutputTokens);
        Assert.Contains("<user_input>", handler.Bodies[0]);
    }

    [Fact]
    public async Task RunAsync_ToolThrows_SendsErrorResultAndRecordsIt()
    {
        var handler = new ScriptedHandler(ToolUseTurn, FinalTurn);
        var service = CreateService(handler, new FakeBooks { Throw = true });

        var result = await service.RunAsync("Find Dune");

        var call = Assert.Single(result.ToolCalls);
        Assert.True(call.IsError);
        Assert.Contains("\"is_error\":true", handler.Bodies[1]);
    }

    private static ToolRunnerAgentService CreateService(ScriptedHandler handler, IBookService books)
    {
        var client = new AnthropicClient { ApiKey = "test-key", HttpClient = new HttpClient(handler), MaxRetries = 0 };
        var options = Options.Create(new AnthropicOptions
        {
            Model = "claude-opus-5-5", MaxTokens = 1024, SystemPrompt = "Test prompt",
        });
        return new ToolRunnerAgentService(client, options, new BookTrackerTools(books, new NoProgress()));
    }

    private sealed class ScriptedHandler(params string[] replies) : HttpMessageHandler
    {
        private int _next;

        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
            var reply = replies[Math.Min(_next++, replies.Length - 1)];
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(reply, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class FakeBooks : IBookService
    {
        public bool Throw { get; init; }

        public Task<IReadOnlyList<BookDto>> SearchAsync(string term, CancellationToken ct = default)
            => Throw
                ? throw new InvalidOperationException("search is down")
                : Task.FromResult<IReadOnlyList<BookDto>>(
                    [new BookDto(4, "Dune", new AuthorDto(4, "Frank Herbert"), null, 412, "Science Fiction", DateTimeOffset.UnixEpoch)]);

        public Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<BookMutationResult> CreateAsync(CreateBookRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<BookMutationResult> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class NoProgress : IReadingProgressService
    {
        public Task<ReadingProgressDto?> GetForBookAsync(int bookId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<ReadingProgressDto> UpdateAsync(int bookId, UpdateReadingProgressRequest request, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
