// PreToolUse guardrail for the Bash and PowerShell tools: refuse destructive commands.
// One C# file, same on macOS, Linux and Windows. Registered in settings.json as:
//   dotnet run .claude/hooks/block-destructive.cs
// Claude Code sends the tool call as JSON on stdin. Exit 2 blocks the call and the
// stderr message goes back to Claude as the reason. Exit 0 lets it run.
using System.Text.Json;
using System.Text.RegularExpressions;

(string Pattern, string Reason)[] rules =
[
    (@"\brm\s+-[a-z]*(r[a-z]*f|f[a-z]*r)", "recursive force delete ('rm -rf')"),
    (@"\bRemove-Item\b(?=.*\s-Recurse\b)(?=.*\s-Force\b)", "recursive force delete ('Remove-Item -Recurse -Force')"),
    (@"\b(del|erase|rd|rmdir)\b.*\s/s\b", "recursive delete ('del /s', 'rd /s')"),
    (@"\bgit\s+reset\s+--hard\b", "discarding commits and changes ('git reset --hard')"),
    (@"\bgit\s+push\b.*(--force|\s-f\b)", "force push ('git push --force')"),
    (@"\bgit\s+clean\s+-[a-z]*f", "deleting untracked files ('git clean -f')"),
    (@"\bdrop\s+(table|database)\b", "dropping a table or database"),
];

var command = "";
try
{
    using var doc = JsonDocument.Parse(Console.In.ReadToEnd());
    if (doc.RootElement.TryGetProperty("tool_input", out var toolInput) &&
        toolInput.TryGetProperty("command", out var value) &&
        value.ValueKind == JsonValueKind.String)
    {
        command = value.GetString() ?? "";
    }
}
catch (JsonException)
{
    return 0; // Not a tool call we understand: let Claude Code decide.
}

foreach (var (pattern, reason) in rules)
{
    if (Regex.IsMatch(command, pattern, RegexOptions.IgnoreCase))
    {
        Console.Error.WriteLine($"Blocked by guardrail: {reason} is not allowed. " +
            "Use a safer alternative (for example dotnet clean, or delete specific files).");
        return 2;
    }
}

return 0;
