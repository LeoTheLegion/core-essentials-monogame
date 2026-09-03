# Sprint 1 — Scene Smoke-Run Harness 🎬

**Points:** 4 | **Status:** ✅ Done | **Goal:** Let the playground launch any scene from a command-line arg and auto-exit after a configurable number of seconds, so every data-driven scene can be smoke-run (booted, let run, then closed) without manual interaction.

> **Branches off `feature/scene-as-data`.** This harness depends on the data-driven boot path (`Program.cs` launching from XML) that lands in that branch, so it stacks on top of it rather than on `development`.

## Why This Sprint

Every scene now runs purely from an XML file, but there is no fast way to *verify* a given scene actually boots and survives a few seconds of the game loop without opening the window, waiting, and clicking close — for each of the eight scenes. A small command-line harness turns "does this scene crash on load?" into a repeatable, scriptable check that can run locally or in CI:

```
dotnet run --project CoreEssentials.Playground -- --scene CharacterScene.xml --run-for 5
```

## Tasks

- [x] T1 ⭐ Add an `AutoExitTimer` helper (CoreEssentials): given a duration, report whether elapsed game time has exceeded it; used to `game.Exit()` after N seconds so the process terminates on its own.
- [x] T2 ⭐ Wire a low-intrusion auto-exit hook into `MainGame.Update` that only activates when an exit deadline is set (no behavior change when unset).
- [x] T3 ⭐ Parse command-line args in `Program.cs`: `--scene <file>` (defaults to `HomeScene.xml`) and `--run-for <seconds>` (optional; default = run indefinitely as today). Unknown args are ignored with a console note.
- [x] T4 🔒 Unit tests: arg parser (default scene, explicit scene, explicit run-for, missing value, unknown flag) and `AutoExitTimer` (not-expired / expired / boundary). No test opens a window or blocks on the game loop.
- [x] T5 🔁 Add `scripts/run-all-scenes.ps1`: iterates every `<Scene>` XML in `CoreEssentials.Playground/Content`, runs each with a fixed `--run-for`, and reports pass/fail per scene (non-zero exit if any scene fails to launch).
- [x] T6 📚 Docs: add the CLI args + auto-exit behavior to `docs/GettingStarted.md` (or a short new note) and mention the runner script in `scripts/`.
- [x] T7 🔁 Build + full suite green; manually smoke-run at least two scenes (one simple, one with input/physics) to confirm clean launch and auto-close.
- [x] T8 🐛 **Bug found by the harness:** fix the data-driven boot path — `SceneManager.LoadScene(string)` / `SetLoadingScene(string)` eagerly parsed the scene XML via `AssetManager` at `Program.cs` time, before `MainGame.LoadContent()` had initialized `AssetManager`, so no data-driven scene could actually launch. Deferred the parse to load time.

## Acceptance Criteria

- `dotnet run --project CoreEssentials.Playground` still boots `HomeScene.xml` and runs indefinitely (backwards compatible).
- `--scene <file>` launches the named scene; `--run-for <seconds>` closes the game after that many seconds of runtime.
- The runner script exercises every scene XML and reports a per-scene pass/fail summary.
- New behavior is covered by unit tests; build clean, full suite green.

## Notes & Risks

- **No issue/PR numbers in code or docs** — repo convention.
- **Auto-exit must be opt-in.** When no `--run-for` is given the game behaves exactly as before (runs until closed). The `MainGame` hook must be a no-op when no deadline is set, so the library's default behavior is untouched.
- **Keep the harness thin.** Arg parsing + timer live in small, testable units; the game loop only checks "should I exit now?".
- **Scene list is derived, not hardcoded** — the runner globs `Content/*.xml` and filters to files whose root element is `<Scene>`, so new scenes are picked up automatically.

## ✅ Completion Notes

The harness landed as designed — and immediately did its job by catching a real, pre-existing crash in the data-driven boot path (T8).

**Harness:**
- **T1** — New `CoreEssentials/src/Timing/AutoExitTimer.cs`: value-based timer (`Tick(deltaMs)`, `IsExpired`, `DurationMs`, `ElapsedMs`); rejects non-positive durations and negative deltas. No window/loop/I-O, so it is trivially testable.
- **T2** — `MainGame` gained a private `AutoExitTimer? _autoExitTimer` (null by default) and a public `EnableAutoExit(double seconds)`. At the end of `Update`, when a timer is set it ticks with the frame's elapsed time and calls `Exit()` once expired. When unset the block is a no-op — default game behavior is untouched.
- **T3** — New `CoreEssentials.Playground/SceneLaunchOptionsParser.cs` (pure, side-effect-free): parses `--scene <file>` (default `HomeScene.xml`) and `--run-for <seconds>` (optional); unknown args are ignored with a console note; a recognized option missing its value, or a non-positive `--run-for`, throws `ArgumentException`. `Program.cs` uses it to set the launch scene and opt into auto-exit.
- **T4** — 21 new tests: `AutoExitTimerTests` (9) and `SceneLaunchOptionsParserTests` (12 cases incl. theory). Plus 2 deferred-parse tests (see T8). No test opens a window or blocks on the game loop.
- **T5** — New `scripts/run-all-scenes.ps1`: builds the playground, globs `Content/*.xml` for `<Scene>` roots (or takes `-Scenes`), runs each with `-Seconds` (default 5) via `--scene`/`--run-for`, and prints a per-scene PASS/FAIL table. Exits non-zero if any scene fails.
- **T6** — `docs/GettingStarted.md`: new "Smoke-Running a Scene from the Command Line" section with both args, an argument table, and the runner-script usage.

**Bug fix (T8) — the reason this sprint matters:**
- **Root cause:** `Program.cs` calls `SceneManager.LoadScene(string)` / `SetLoadingScene(string)` right after game construction, *before* `game.Run()`. Those overloads eagerly called `SceneParser.LoadFromAsset(...)` → `AssetManager.LoadAsset(...)`, but `AssetManager` is only initialized in `MainGame.LoadContent()` (which runs *inside* `Run()`). Result: `InvalidOperationException: AssetManager has not been initialized with a ContentManager` on every data-driven boot. The existing tests never caught it because they call `AssetManager.Init(...)` manually first.
- **Fix:** added a deferred constructor `DataDrivenScene(string sceneAssetName)` that stores the asset name and parses lazily via `EnsureDefinition()` during the scene's load phase (once assets are available). `Definition` now resolves on first access; both `LoadGameSystems()` and `OnStartCoroutine()` route through it. The two `SceneManager` string overloads now construct `new DataDrivenScene(sceneAssetName)` instead of parsing up front.
- **Tests:** `DataDrivenScene_FromAssetName_DoesNotParseUntilLoad` (constructs with the asset file *not yet written* — proof that construction does not parse eagerly — then loads and asserts the entity instantiated) and `DataDrivenScene_FromAssetName_NullOrEmpty_Throws`.

**Verification:** build clean; full suite **1121 passed / 0 failed / 3 skipped (Total 1124)** (up from 1098 by the 21 harness tests + 2 deferred-parse tests). Manual smoke-runs: `HomeScene.xml` and `PhysicsEntityScene.xml` both launched, transitioned through the data-driven loading screen, and auto-exited with code 0. Full runner: **all 8 scenes PASS** (CameraScene, CharacterScene, GuiAnchorDemo, HomeScene, LabelAlignmentDemoScene, loading, PhysicsEntityScene, SendMessageDemoScene).
