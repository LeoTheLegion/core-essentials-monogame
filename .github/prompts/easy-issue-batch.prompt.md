---
description: "Launch Repo Issue Fixer with an easy-issues-first pass and a default batch size of 3."
name: "Easy Issue Batch"
argument-hint: "Optional scope, such as repo-wide, CoreEssentials, Playground, or a file area"
agent: "Repo Issue Fixer"
model: "GPT-5 (copilot)"
---
Run an issue-fixing pass for this repository with these defaults:

- Prioritize easy issues first.
- Start with XML documentation warnings, comment formatting issues, and other low-risk local diagnostics before harder changes.
- Work in batches of exactly 3 issues unless the available easy issues are fewer.
- Build first with direct `dotnet` commands so the Problems surface is current.
- Prefer `dotnet build` and `dotnet test` commands over VS Code tasks.
- Use the todo tool to track the current batch.
- Delegate each concrete issue slice to the limited-scope `Issue Slice Fixer` subagent.
- Rebuild or rerun the narrowest relevant validation after each fix.
- Stop after the batch and summarize what was fixed, what remains, and what the next easy slice should be.

If the user provides a scope, apply the same workflow within that scope. If no scope is provided, start with the broadest build target that gives a useful easy-first batch.