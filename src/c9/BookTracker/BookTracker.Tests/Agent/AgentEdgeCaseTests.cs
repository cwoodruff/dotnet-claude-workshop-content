using BookTracker.Api.Tools;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using NSubstitute;
using static BookTracker.Tests.Agent.AgentTestHarness;

namespace BookTracker.Tests.Agent;

/// <summary>
/// The adversarial pass — the loop's guards and edge behaviors: the iteration cap, an unknown tool
/// name, and a tool that returns an error string (not an exception).
/// </summary>
public class AgentEdgeCaseTests
{
    private static BookTrackerTools Tools(IBookService books, IReadingProgressService progress)
        => new(books, progress);

    [Fact]
    public async Task RunAsync_ModelNeverStops_HitsIterationCapAndReturnsGuardMessage()
    {
        var books = Substitute.For<IBookService>();
        books.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<BookDto>());
        var progress = Substitute.For<IReadingProgressService>();

        // The model asks for a tool on every turn and never produces a final answer.
        var llm = new AlwaysCallsToolLlm(ToolRequest("find_book", new { query = "x" }));

        var result = await Service(llm, Tools(books, progress)).RunAsync("loop forever");

        Assert.Contains("maximum number of tool iterations", result.Reply);
        Assert.Equal(5, result.ToolCalls.Count); // MaxIterations
    }

    [Fact]
    public async Task RunAsync_UnknownTool_ReturnsUnknownMessageWithoutError()
    {
        var books = Substitute.For<IBookService>();
        var progress = Substitute.For<IReadingProgressService>();

        var llm = new ScriptedLlm(
            ToolTurn(ToolRequest("does_not_exist", new { foo = "bar" })),
            Final("ok"));

        var result = await Service(llm, Tools(books, progress)).RunAsync("call a bad tool");

        var call = Assert.Single(result.ToolCalls);
        Assert.False(call.IsError); // dispatch returns a message, not an exception
        Assert.Contains("Unknown tool", call.Result);
    }

    [Fact]
    public async Task RunAsync_ToolReturnsNoData_StillCompletes()
    {
        var books = Substitute.For<IBookService>();
        var progress = Substitute.For<IReadingProgressService>();
        progress.GetForBookAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((ReadingProgressDto?)null);

        var llm = new ScriptedLlm(
            ToolTurn(ToolRequest("get_reading_progress", new { bookId = 999 })),
            Final("No progress yet."));

        var result = await Service(llm, Tools(books, progress)).RunAsync("progress for unknown book");

        Assert.Equal("No progress yet.", result.Reply);
        Assert.False(result.ToolCalls[0].IsError);
    }
}
