using System.Net.Sockets;

namespace BookTracker.Tests.Integration;

/// <summary>
/// A Fact that auto-skips when Docker isn't available (so `dotnet test` stays green without it) and,
/// optionally, unless a named env var is set (used to gate the slow SQL Server image behind opt-in).
/// In CI (ubuntu has Docker) these run automatically.
/// </summary>
public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute(string? requireEnv = null)
    {
        if (!DockerAvailable())
        {
            Skip = "Docker is not available — skipping the TestContainers integration test.";
            return;
        }

        if (requireEnv is not null &&
            Environment.GetEnvironmentVariable(requireEnv) is not ("1" or "true"))
        {
            Skip = $"Set {requireEnv}=1 to run this test (pulls a large container image).";
        }
    }

    private static bool DockerAvailable()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_HOST")))
        {
            return true;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] sockets =
        [
            "/var/run/docker.sock",
            Path.Combine(home, ".docker/run/docker.sock"),
            Path.Combine(home, ".colima/default/docker.sock"),
        ];

        return sockets.Any(CanConnect);
    }

    // A socket file alone isn't enough: Docker Desktop leaves it behind when it's stopped, and the
    // test would then fail instead of skipping. Connecting proves something is listening.
    private static bool CanConnect(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            // Bounded, so a wedged daemon can't hang test discovery.
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.ConnectAsync(new UnixDomainSocketEndPoint(path), timeout.Token).AsTask().GetAwaiter().GetResult();
            return true;
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            return false;
        }
    }
}
