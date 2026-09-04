# Sprint 1 — Scene Manifest Parsing 🗂️

**Points:** 3 | **Status:** ✅ Done (2026-09-04) | **Goal:** Add a pure, testable `SceneManifest` to the core that parses and validates the two-list scene manifest format. No behavior change yet — nothing consumes it in this sprint.

> **Single shared branch: `feature/scene-manifest` (off `feature/scene-as-data`).** Sprints 1–3 all land on this branch; Sprint 2 builds on this one.

## Why This Sprint

The manifest is the foundation for both core enforcement (Sprint 2) and ordered navigation. Keeping parsing as a pure unit first means: the format can be validated exhaustively with temp-XML tests before any `SceneManager` wiring, and the enforcement sprint stays small and focused.

## The Format

```xml
<Scenes>
    <GameScenes>
        <Scene Name="HomeScene.xml" />
        <Scene Name="CharacterScene.xml" LoadingScreen="loading_main.xml" />
    </GameScenes>

    <LoadingScenes>
        <LoadingScene Name="loading_main.xml" Default="true" />
        <LoadingScene Name="loading_physics.xml" />
    </LoadingScenes>
</Scenes>
```

**Rules:**
- **Startup scene** = first `<Scene>` in `<GameScenes>`. Empty/missing list → parse error.
- **`<LoadingScenes>`** is optional (a game may have no loading screens); at most one `Default="true"`.
- A `<Scene>`'s optional `LoadingScreen` attribute must reference a name declared in `<LoadingScenes>`.

## Tasks

- [x] T1 ⭐ New `CoreEssentials/src/Scene/SceneManifest.cs`: parse from an XML string. Expose: `GameScenes` (ordered), `LoadingScenes`, `StartupScene`, `IndexOf(name)`, `NextOf(index)`, `PreviousOf(index)` (±1, clamped at ends), `LoadingScreenFor(sceneName)` (attribute → default → null).
- [x] T2 ⭐ Validation: throw descriptive errors for — missing/empty `<GameScenes>`, duplicate names within either list, a `LoadingScreen` attribute referencing an undeclared loading screen, more than one `Default="true"`, malformed XML.
- [x] T3 🔒 Unit tests (temp-XML strings, no windows): happy path, startup = first entry, Next/Previous clamping, per-scene + default + absent loading-screen resolution, every validation error case, and the "no manifest content at all" error.
- [x] T4 📚 Docs: new `docs/SceneManifest.md` — format reference, rules table, and a complete example.
- [x] T5 🔁 Build clean, full suite green.

## Acceptance Criteria

- `SceneManifest` parses the two-list format and exposes ordered navigation helpers with clamp-at-ends semantics.
- Every malformed input produces a descriptive exception naming the offending scene/entry.
- No existing behavior changes; build clean, full suite green.

## Notes & Risks

- **Pure data only.** No `SceneManager`, no asset loading in this sprint — that keeps Sprint 2's blast radius small.
- **Deferred asset resolution is Sprint 2's job** — the core boot path will resolve the manifest after `AssetManager` init; see the smoke-run harness bug where eager parsing at `Program.cs` time ran before assets existed.
- **No issue/PR numbers in code or docs** — repo convention.

## ✅ Completion Notes

Landed as designed, tests-first (28 failing cases written before the implementation).

- **T1/T2** — New `CoreEssentials/src/Scene/SceneManifest.cs` in namespace `CoreEssentials.Scenes` (matching the existing scene types — an initial `CoreEssentials.Scene` namespace collided with the `Scene` type name and was renamed). Records: `SceneEntry(Name, LoadingScreen?)`, `LoadingSceneEntry(Name, IsDefault)`. Parser follows the `SceneParser` conventions: strict `XDocument` parsing, unknown elements/attributes are `FormatException`s naming the offender, plus the cross-list check that a `<Scene LoadingScreen="...">` references a declared loading screen.
- **T3** — 28 tests in `CoreEssentials.Tests/SceneManagement/SceneManifestTests.cs`: happy path (ordered lists, per-scene attribute, default marker, no-`<LoadingScenes>` case), startup = first entry, `IndexOf` known/unknown, Next/Previous clamp at both ends, loading-screen resolution (explicit → default → null, unknown scene → null), and every validation error case.
- **T4** — New `docs/SceneManifest.md`: format reference, elements/attributes table, rules, validation-error table, and a minimal-game example.

**Verification:** build clean; full suite **1156 passed / 0 failed / 3 skipped (Total 1159)** (up from 1128 by the 28 manifest tests). No behavior change — nothing consumes the manifest yet.
