# Scene Smoke-Run — Scrum Sprints 🎬

Let every data-driven scene be smoke-run unattended: launch it from the command line, let it run for a set number of seconds, then auto-close. Catches "does this scene crash on load?" as a repeatable, scriptable check (local or CI).

> **Stacks on `feature/scene-as-data`** — the harness depends on the data-driven boot path (`Program.cs` launching from XML) that lands in that branch.

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 1 | [Scene Smoke-Run Harness](Sprint_1_Scene_Smoke_Run_Harness.md) | 4 | ✅ Done (2026-09-03) | `--scene`/`--run-for` CLI args, opt-in auto-exit, `run-all-scenes.ps1`, and the boot-path bug fix it exposed |
| 2 | [No Focus Pause](Sprint_2_No_Focus_Pause.md) | 1 | ✅ Done (2026-09-03) | Opt-in `--no-focus-pause` flag so unattended runs keep background audio playing when the window loses focus |

## Why This?

Every scene runs purely from an XML file, but there was no fast way to *verify* a given scene actually boots and survives a few seconds of the game loop without opening the window, waiting, and clicking close — for each scene. A thin command-line harness turns that into:

```
dotnet run --project CoreEssentials.Playground -- --scene CharacterScene.xml --run-for 5
```

## Point Summary

- Sizing: 1 = small, 2 = medium, 5 = large

## Workflow Phases

Harness (Sprint 1) → future: wire the runner into CI on scene-affecting changes.
