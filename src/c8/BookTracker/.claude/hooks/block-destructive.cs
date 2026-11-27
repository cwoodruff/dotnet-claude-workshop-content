// PreToolUse guardrail for the Bash and PowerShell tools: refuse destructive commands.
// One C# file, same on macOS, Linux and Windows. Registered in settings.json as:
//   dotnet run .claude/hooks/block-destructive.cs
// Claude Code sends the tool call as JSON on stdin. Exit 2 blocks the call and the
// stderr message goes back to Claude as the reason. Exit 0 lets it run.
using System.Text.Json;
using System.Text.RegularExpressions;

(string Pattern, string Reason)[] rules =
[
    (@"\brm\b(?=.*\s-(-recursive\b|[a-z]*r))(?=.*\s-(-force\b|[a-z]*f))", "recursive force delete ('rm -rf')"),
    (@"\bRemove-Item\b(?=.*\s-Recurse\b)(?=.*\s-Force\b)", "recursive force delete ('Remove-Item -Recurse -Force')"),
    (@"\b(del|erase|rd|rmdir)\b.*\s/s\b", "recursive delete ('del /s', 'rd /s')"),
    (@"\bgit\s+reset\s+--hard\b", "discarding commits and changes ('git reset --hard')"),
    (@"\bgit\s+push\b.*(--force|\s-f\b)", "force push ('git push --force')"),
    (@"\bgit\s+clean\b(?=.*\s-([a-z]*f|-force\b))", "deleting untracked files ('git clean -f')"),
    (@"\bdrop\s+(table|database)\b", "dropping a table or database"),
];

var command = "";
try
{
    using var doc = JsonDocument.Parse(await Console.In.ReadToEndAsync());
    if (doc.RootElement.TryGetProperty("tool_input", out var toolInput) &&
        toolInput.TryGetProperty("command", out var value) &&
        value.ValueKind == JsonValueKind.String)
    {
        command = value.GetString() ?? "";
    }
}
catch (JsonException)
{
    // Fail closed: a guardrail that can't read the command doesn't let it through.
    await Console.Error.WriteLineAsync("Blocked by guardrail: could not read the tool call.");
    return 2;
}

// Join continued lines (PowerShell backtick, bash backslash) so a command split
// across lines is checked as one line.
command = Regex.Replace(command, @"[`\\][ \t]*\r?\n", " ", RegexOptions.None, TimeSpan.FromSeconds(1));

foreach (var (pattern, reason) in rules)
{
    bool matched;
    try
    {
        matched = Regex.IsMatch(command, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
    }
    catch (RegexMatchTimeoutException)
    {
        matched = true; // Fail closed on a pathological command.
    }

    if (matched)
    {
        await Console.Error.WriteLineAsync($"Blocked by guardrail: {reason} is not allowed. " +
            "Use a safer alternative (for example dotnet clean, or delete specific files).");
        return 2;
    }
}

return 0;
