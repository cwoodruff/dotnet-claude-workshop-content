# .NET Claude Code Kit

The kit now ships as a Claude Code plugin from its own repo, which is the only place it's edited:
https://github.com/cwoodruff/dotnet-claude-code-kit

Install it with one command:

```bash
claude plugin install dotnet-kit --marketplace cwoodruff/dotnet-claude-code-kit
```

Then, in your solution root, run `/dotnet-kit:setup-project` once to add the CLAUDE.md template,
the six `.claude/rules/` files, and the shared permission rules. A plugin can't carry those three,
so the setup skill copies them in for you to review and commit.

What you get: 9 skills (including `/dotnet-kit:dotnet-review`, the six .NET failure-mode checks),
8 commands, 6 subagents, and 3 C# hooks (build after every edit, protect generated files, format
check at the end of a turn). The kit repo's README has the full list.
