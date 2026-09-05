# Sprint 2 — Content Organization 🎨

**Points:** 5 | **Status:** ✅ Done | **Goal:** Move the playground's ~30 flat content files into type-based folders, update `Content.mgcb` and every asset-name reference (manifest, XML cross-references, C# defaults), and relocate stray artifacts. Zero behavior change.

> **Branch:** `feature/playground-organization`. Lands after Sprint 1 so the codebase is stable while content paths churn.

## Why This Sprint

`Content/` mixes scenes, prefab templates, sprite descriptors + textures, audio, fonts, and config in one flat folder — the same "where does this live?" problem as the code, but with a harder failure mode: a wrong path only surfaces at runtime (missing asset), not compile time. Grouping by asset type makes the content pipeline auditable and matches how scenes/templates/audio are already conceptually separated.

## Target Folders

| Folder | Contents |
|--------|----------|
| `Content/` (root) | `scenes.xml` — **stays put** (registered by asset name + read by the harness); `DiagnosticsFont.spritefont` — **stays put** (Aether hardcodes its content key, see Notes) |
| `Content/Scenes/` | The 8 `<Scene>`-rooted files: HomeScene, CharacterScene, CameraScene, GuiAnchorDemo, LabelAlignmentDemoScene, PhysicsEntityScene, SendMessageDemoScene, loading.xml |
| `Content/Templates/` | BallTemplate, CharacterTemplate, PingPrefabTemplate, SoundButtonTemplate, TextTemplate, VolumeButtonTemplate (.xml) |
| `Content/Sprites/` | ball_sprite.xml, Ball.png, character_anim_walk.xml, character_malePerson_sheetHD.* (png/xml), character_sheet.xml, character_sprite.xml |
| `Content/Audio/` | footstep00-02.ogg, Goblins_Den_(Regular).wav, footstep*_sound.xml, song1_sound.xml |
| `Content/Fonts/` | base.spritefont, ComicMono.ttf (orphan raw font — no spritefont references it; preserved as source data) |
| `Content/Config/` | PhysicsConfig.xml |

Stray artifacts: `PhysicsScene_Save.xml` (a runtime save file that leaked into the source tree — move to a gitignored location or delete; confirm nothing references it as an input) and any duplicate at the repo root.

## Tasks

- [x] T1 🔁 Move scene XMLs (incl. `loading.xml`) into `Content/Scenes/`; update their `/copy:` entries in `Content.mgcb`. _(Batch A, commit ae24a48)_
- [x] T2 🔁 Move prefab templates into `Content/Templates/`; update mgcb + every XML that references them by name (`PhysicsEntityScene.xml` uses `Asset="BallTemplate.xml"`, etc.). _(Batch B, commit 5384644)_
- [x] T3 🔁 Move sprite descriptors/textures into `Content/Sprites/` and audio sources/descriptors into `Content/Audio/`; update mgcb `/importer`/`/processor`/`/build` paths and any XML/C# asset-name references (e.g. `MusicComponent`'s default `song1_sound.xml`, `SoundKeyComponent`'s footstep names). _(Batch C, commit 1785e4c)_
- [x] T4 🔁 Move fonts into `Content/Fonts/` and `PhysicsConfig.xml` into `Config/`; update mgcb + the config asset name used by `<System Type="PhysicsEngine" Config="PhysicsConfig.xml" />`. _(Batch D, commit 20ecb12 — `DiagnosticsFont.spritefont` deliberately left at root)_
- [x] T5 🔁 Update test content lookups that walk to `CoreEssentials.Playground/Content/{name}` (`BootFromFilesTests.ReadSourceContentFile`, Sprint5b/c/d fixtures) for the new subfolder paths; confirm `scenes.xml` references still resolve (scene names are relative to Content root — verify how asset names map after the move and update `scenes.xml` entries if the runtime asset key changes). _(Done per batch; `WriteContentAsset` made subfolder-aware in Batch A)_
- [x] T6 🔁 Relocate/delete stray save artifacts (`PhysicsScene_Save.xml`) and add a gitignore entry for runtime saves. _(No action needed: neither copy is git-tracked, and `.gitignore` already covers them via `**/*_Save.xml`, `**/PhysicsScene_Save.xml`, and an explicit path)_
- [x] T7 🔒 Build clean + full suite green; smoke-run all 7 scenes via the harness — same PASS list as before. _(Verified after every batch: build clean, 1174 passed / 0 failed / 3 skipped, all 7 scenes PASS)_

## Completion Notes

**Two distinct load mechanisms drive everything in this sprint:**
- `XMLAsset.Load` reads via **raw file path** (`AppContext.BaseDirectory/Content/{name}`), so a moved XML changes its required asset name (e.g. `HomeScene.xml` → `Scenes/HomeScene.xml`). Covers scenes, templates, sprite/sound descriptors, config, manifest.
- `Texture2DAsset`/`SoundEffectAsset`/`FontAsset` load via **content-pipeline key** (`contentManager.Load<T>(name)`), so a moved media file changes its key (e.g. `ball` → `Sprites/ball`, `base` → `Fonts/base`).

**`DiagnosticsFont.spritefont` stays at the Content root.** Aether's `DebugView.LoadContent(GraphicsDevice, ContentManager, IPrimitiveBatch = null)` hardcodes `_font = content.Load<SpriteFont>("DiagnosticsFont")` — there is no font-name parameter or property to reconfigure. Moving it would change its key to `Fonts/DiagnosticsFont` and silently break the physics debug overlay at runtime (uncatchable by the smoke harness, which only renders when toggled). Verified against the Aether.Physics2D source before deciding.

**Orphan raw assets were preserved, not deleted:** `Sprites/character_malePerson_sheetHD.xml` (artist atlas source) and `Fonts/ComicMono.ttf` (no spritefont references it; `base.spritefont` uses Arial). Both are outside the content pipeline and referenced by nothing — kept as source data.

**Test coupling handled per batch:** `WriteContentAsset` now creates subfolders on write; Sprint5b/c/d fixtures stage/assert the new subfolder-relative names (`Scenes/...`, `Templates/...`, `Sprites/...`, `Audio/...`, `Fonts/base`, `Config/PhysicsConfig.xml`). Self-contained test data (e.g. `ChildPosTestPhysicsConfig.xml`, `RecordingDebugStart`'s arbitrary font string, `"Base"` property-*key* matches in override tests) was intentionally left untouched.

**Final Content root:** `scenes.xml`, `Content.mgcb`, `DiagnosticsFont.spritefont`, plus the `Scenes/`, `Templates/`, `Sprites/`, `Audio/`, `Fonts/`, `Config/` subfolders. (`layout/main.xml` remains an orphan in its own folder, referenced only by mgcb.)

## Acceptance Criteria

- `Content/` root holds only `scenes.xml`, `Content.mgcb`, and the new subfolders; no loose scene/template/sprite/audio/font/config files remain at the root.
- Every asset still loads by name at runtime (harness PASS for all 7 scenes proves the full chain: manifest → scene XML → templates → sprites → audio).
- Full suite green, including the tests that read real playground content.

## Notes & Risks

- **Asset key vs. file path** — MonoGame's content pipeline keys assets by their *root name relative to the content root*, not the full path. Moving a file into a subfolder can change its runtime asset key (e.g. `Scenes/HomeScene.xml` may no longer be loadable as `"HomeScene.xml"`). Verify per file type (`/copy:` XML assets vs. imported textures/fonts) and update references — `scenes.xml`, XML cross-refs, and C# defaults all use these keys. This is the central risk of the sprint; do T1–T4 in small commits and smoke-run after each.
- **`/copy:` XML assets** keep their file content but may change key with subfolders — test early with one scene before moving all eight.
- **Tests copy real content into their own Content dir** (`WriteContentAsset(name, ReadSourceContentFile(name))`) — both the read path and the written name (the asset key) matter; a mismatch is a runtime failure in an otherwise-green build.
- Keep `scenes.xml` at the Content root throughout; if verification shows its entries must become subfolder-relative keys, that update belongs to T5, not a move of the file itself.
