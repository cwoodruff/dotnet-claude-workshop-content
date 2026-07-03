using BookTracker.Api.Tools;
using BookTracker.Core.Dtos;
using BookTracker.Core.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static BookTracker.Tests.Agent.AgentTestHarness;

namespace BookTracker.Tests.Agent;

/// <summary>
/// Unit tests for the AgentService loop. The LLM is the scripted seam (IAgentLlm); the Core services
/// the tools call are NSubstitute mocks — no real DB, no real API.
/// </summary>
public class AgentServiceTests
{
    private static BookTrackerTools Tools(IBookService books, IReadingProgressService progress)
        => new(books, progress);

    [Fact]
    public async Task RunAsync_HappyPath_RunsOneToolThenReturnsFinalText()
    {
        var books = Substitute.For<IBookService>();
        books.SearchAsync("Dune", Arg.Any<CancellationToken>())
            .Returns(new List<BookDto> { new(4, "Dune", new AuthorDto(4, "Frank Herbert"), null, 412, "Science Fiction", default) });
        var progress = Substitute.For<IReadingProgressService>();

        var llm = new ScriptedLlm(
            ToolTurn(ToolRequest("find_book", new { query = "Dune" })),
            Final("Found Dune."));

        var result = await Service(llm, Tools(books, progress)).RunAsync("find Dune");

        Assert.Equal("Found Dune.", result.Reply);
        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("find_book", call.Name);
        Assert.False(call.IsError);
        await books.Received(1).SearchAsync("Dune", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_MultiTool_RunsTwoToolsInOrderBeforeFinal()
    {
        var books = Substitute.For<IBookService>();
        books.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<BookDto>());
        var progress = Substitute.For<IReadingProgressService>();
        progress.GetForBookAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((ReadingProgressDto?)null);

        var llm = new ScriptedLlm(
            ToolTurn(ToolRequest("find_book", new { query = "Dune" })),
            ToolTurn(ToolRequest("get_reading_progress", new { bookId = 4 })),
            Final("Done."));

        var result = await Service(llm, Tools(books, progress)).RunAsync("find Dune then progress");

        Assert.Equal("Done.", result.Reply);
        Assert.Equal(2, result.ToolCalls.Count);
        Assert.Equal("find_book", result.ToolCalls[0].Name);
        Assert.Equal("get_reading_progress", result.ToolCalls[1].Name);
    }

    [Fact]
    public async Task RunAsync_ToolThrows_RecordsErrorOutcomeAndDoesNotCrash()
    {
        var books = Substitute.For<IBookService>();
        var progress = Substitute.For<IReadingProgressService>();
        progress.UpdateAsync(Arg.Any<int>(), Arg.Any<UpdateReadingProgressRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new System.ComponentModel.DataAnnotations.ValidationException("Completed is terminal."));

        var llm = new ScriptedLlm(
            ToolTurn(ToolRequest("update_reading_progress", new { bookId = 1, status = "Reading" })),
            Final("That isn't allowed."));

        var result = await Service(llm, Tools(books, progress)).RunAsync("mark it reading again");

        var call = Assert.Single(result.ToolCalls);
        Assert.True(call.IsError);
        Assert.Contains("Completed is terminal", call.Result);
        // The error outcome was fed back to the model.
        var outcome = Assert.Single(llm.Conversation.SubmittedOutcomes[0]);
        Assert.True(outcome.IsError);
    }

    [Fact]
    public async Task RunAsync_CancelledToken_PropagatesCancellation()
    {
        var books = Substitute.For<IBookService>();
        var progress = Substitute.For<IReadingProgressService>();
        var llm = new ScriptedLlm(Final("never reached"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Service(llm, Tools(books, progress)).RunAsync("anything", cts.Token));
    }
}
