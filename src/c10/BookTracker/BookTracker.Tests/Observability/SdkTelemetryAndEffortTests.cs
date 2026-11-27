using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using Anthropic;
using BookTracker.Api.Options;
using BookTracker.Api.Services;
using Microsoft.Extensions.Options;

namespace BookTracker.Tests.Observability;

/// <summary>
/// Runs ClaudeService against a fake HTTP handler (no API key, no network) to prove two C10 claims:
/// the Effort setting reaches the request, and the SDK emits a span per call on its ActivitySource.
/// </summary>
public class SdkTelemetryAndEffortTests
{
    private const string CannedReply = """
        {"id":"msg_test","type":"message","role":"assistant","model":"claude-opus-5-5",
         "content":[{"type":"text","text":"Hello"}],"stop_reason":"end_turn","stop_sequence":null,
         "usage":{"input_tokens":12,"output_tokens":3}}
        """;

    [Fact]
    public async Task ChatAsync_WithEffort_SendsOutputConfigEffort()
    {
        var (service, handler) = Create(effort: "low");

        await service.ChatAsync("Hi", []);

        Assert.Contains("\"output_config\":{\"effort\":\"low\"}", handler.LastBody);
    }

    [Fact]
    public async Task ChatAsync_WithoutEffort_SendsNoOutputConfig()
    {
        var (service, handler) = Create(effort: null);

        await service.ChatAsync("Hi", []);

        Assert.DoesNotContain("output_config", handler.LastBody);
    }

    [Fact]
    public async Task ChatAsync_WithListener_EmitsSpanWithTokenUsage()
    {
        // The listener sees every SDK call in the process, and other test classes run in parallel.
        // Wrap this call in a root activity and keep only spans from its trace.
        using var testSource = new ActivitySource("BookTracker.Tests.Telemetry");
        var spans = new ConcurrentQueue<Activity>();
        ActivityTraceId? traceId = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name is "com.anthropic.sdk.csharp" or "BookTracker.Tests.Telemetry",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity =>
            {
                if (activity.TraceId == traceId && activity.Source.Name == "com.anthropic.sdk.csharp")
                {
                    spans.Enqueue(activity);
                }
            },
        };
        ActivitySource.AddActivityListener(listener);
        var (service, _) = Create(effort: null);

        using (var root = testSource.StartActivity("test-root"))
        {
            Assert.NotNull(root);
            traceId = root.TraceId;
            await service.ChatAsync("Hi", []);
        }

        var span = Assert.Single(spans);
        Assert.Equal("claude-opus-5-5", span.GetTagItem("gen_ai.request.model"));
        Assert.Equal(12L, Convert.ToInt64(span.GetTagItem("gen_ai.usage.input_tokens")));
        Assert.Equal(3L, Convert.ToInt64(span.GetTagItem("gen_ai.usage.output_tokens")));
    }

    private static (ClaudeService Service, CapturingHandler Handler) Create(string? effort)
    {
        var handler = new CapturingHandler(CannedReply);
        var client = new AnthropicClient { ApiKey = "test-key", HttpClient = new HttpClient(handler), MaxRetries = 0 };
        var options = Options.Create(new AnthropicOptions
        {
            Model = "claude-opus-5-5",
            MaxTokens = 256,
            SystemPrompt = "Test prompt",
            Effort = effort,
        });
        return (new ClaudeService(client, options), handler);
    }

    private sealed class CapturingHandler(string reply) : HttpMessageHandler
    {
        public string LastBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(reply, Encoding.UTF8, "application/json"),
            };
        }
    }
}
