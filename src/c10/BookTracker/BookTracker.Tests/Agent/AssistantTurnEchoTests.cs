using Anthropic.Models.Messages;
using BookTracker.Api.Services;

namespace BookTracker.Tests.Agent;

/// <summary>
/// The assistant turn sent back with tool results has to carry the model's thinking blocks unchanged.
/// These tests sit at the SDK seam, below the scripted IAgentLlm the other agent tests use.
/// </summary>
public class AssistantTurnEchoTests
{
    [Fact]
    public void ToAssistantContent_KeepsThinkingBlocksWithSignatureAndOrder()
    {
        IReadOnlyList<ContentBlock> response =
        [
            new ThinkingBlock { Thinking = "", Signature = "sig-1" },
            new TextBlock { Text = "Let me look that up.", Citations = null },
            new RedactedThinkingBlock("opaque-data"),
        ];

        var echoed = AnthropicAgentLlm.ToAssistantContent(response);

        Assert.Equal(3, echoed.Count);

        Assert.True(echoed[0].TryPickThinking(out var thinking));
        Assert.Equal("sig-1", thinking!.Signature);
        Assert.Equal("", thinking.Thinking);

        Assert.True(echoed[1].TryPickText(out var text));
        Assert.Equal("Let me look that up.", text!.Text);

        Assert.True(echoed[2].TryPickRedactedThinking(out var redacted));
        Assert.Equal("opaque-data", redacted!.Data);
    }

    [Fact]
    public void ToAssistantContent_TextOnlyResponse_IsUnchanged()
    {
        IReadOnlyList<ContentBlock> response = [new TextBlock { Text = "Done.", Citations = null }];

        var echoed = AnthropicAgentLlm.ToAssistantContent(response);

        var only = Assert.Single(echoed);
        Assert.True(only.TryPickText(out var text));
        Assert.Equal("Done.", text!.Text);
    }
}
