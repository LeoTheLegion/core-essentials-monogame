# Sprint 2 — `--no-focus-pause` Smoke-Run Flag 🔇

**Points:** 1 | **Status:** ✅ Done (2026-09-03) | **Goal:** Add a `--no-focus-pause` flag to the playground so that unattended smoke-runs keep playing background audio even when the window loses focus, instead of silently pausing on blur.

> **Branches off `feature/scene-as-data`.** Stacks on the smoke-run harness from Sprint 1 (same CLI parser and runner script).

## Why This Sprint

When a scene is smoke-run unattended (`--run-for N`), the window often never holds foreground — the terminal or editor stays focused. MonoGame's default behavior pauses the game loop when the window loses focus, which also routes an application-pause through to audio: any `MusicComponent` in the scene gets paused and **background sound stops playing**. That makes it look like a scene has no audio at all during a smoke-run, even though the audio is correct. A small opt-in flag lets unattended runs suppress the focus-driven pause so audio behaves as it would while the window is focused.

## Tasks

- [x] T1 ⭐ Add an `--no-focus-pause` **flag** (no value) to `SceneLaunchOptionsParser`, exposed as `SceneLaunchOptions.NoFocusPause`. Default is `false` — behavior is unchanged unless the flag is supplied.
- [x] T2 ⭐ Wire a low-intrusion hook into `MainGame`: a public `EnableIgnoreFocusForPause()` sets a private flag; when set, `OnDeactivated`/`OnActivated` skip routing `SceneManager.OnApplicationPause(...)`, so focus changes no longer pause/resume systems. No behavior change when the flag is not set.
- [x] T3 ⭐ `Program.cs`: when `options.NoFocusPause` is true, print a console note and call `game.EnableIgnoreFocusForPause()`.
- [x] T4 🔒 Unit tests: parser (flag sets `NoFocusPause`, combined with other options, repeated flag still true, default false, unknown-flag path stays false). No test opens a window.
- [x] T5 🔁 Extend `scripts/run-all-scenes.ps1` with an opt-in `-NoFocusPause` switch that forwards `--no-focus-pause` to each scene launch.
- [x] T6 📚 Docs: add the new flag to the argument table in `docs/GettingStarted.md` and to the runner-script usage.

## Acceptance Criteria

- `dotnet run --project CoreEssentials.Playground` (no flag) behaves exactly as before — focus changes still pause/resume normally.
- `--no-focus-pause` keeps background audio playing when the window loses focus during an unattended run.
- The runner script's `-NoFocusPause` switch forwards the flag to every scene launch.
- New behavior is covered by unit tests; build clean, full suite green.

## Notes & Risks

- **Opt-in only.** Default (`false`) preserves the existing focus-pause behavior so interactive play and any CI default are untouched.
- **Thin wiring.** The parser stays pure/testable; `MainGame` only gains a guarded early-return in the two focus callbacks. Verified end-to-end via the smoke-run script (same convention as Sprint 1's auto-exit — no test instantiates `MainGame`).
- **No issue/PR numbers in code or docs** — repo convention.

## ✅ Completion Notes

Landed as designed.

- **T1** — `SceneLaunchOptionsParser`: new value-less flag `--no-focus-pause`; `SceneLaunchOptions` gains `bool NoFocusPause`. Flag is recognized anywhere in the arg list; unknown args are still ignored with a console note.
- **T2** — `MainGame`: private `_ignoreFocusForPause` (default `false`) + public `EnableIgnoreFocusForPause()`. Both `OnDeactivated` and `OnActivated` early-return before calling `SceneManager.OnApplicationPause(...)` when the flag is set, so focus changes no longer pause/resume systems or audio.
- **T3** — `Program.cs`: after the auto-exit wiring, if `options.NoFocusPause` prints `[Playground] Focus-pause disabled: ...` and calls `game.EnableIgnoreFocusForPause()`.
- **T4** — 3 new parser tests + `Assert.False(options.NoFocusPause)` added to the two default-path tests. No test opens a window or blocks on the game loop.
- **T5** — `scripts/run-all-scenes.ps1`: new `[switch]$NoFocusPause`; when set, appends `--no-focus-pause` to each scene's launch args (built in a local `$runArgs` array to avoid the reserved `$args` name).
- **T6** — `docs/GettingStarted.md`: third row in the argument table + example command; runner-script usage note.

**Verification:** build clean; full suite **1128 passed / 0 failed / 3 skipped (Total 1131)**. End-to-end: `dotnet run --project CoreEssentials.Playground --no-build -- --scene CharacterScene.xml --run-for 4 --no-focus-pause` printed the focus-pause-disabled note and logged `Playing sound with ID: ...`, confirming background audio starts and is not paused by the unfocused window.
