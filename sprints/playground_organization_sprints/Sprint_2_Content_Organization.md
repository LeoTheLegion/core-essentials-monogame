# Sprint 2 — Content Organization 🎨

**Points:** 5 | **Status:** ⬜ Not started | **Goal:** Move the playground's ~30 flat content files into type-based folders, update `Content.mgcb` and every asset-name reference (manifest, XML cross-references, C# defaults), and relocate stray artifacts. Zero behavior change.

> **Branch:** `feature/playground-organization`. Lands after Sprint 1 so the codebase is stable while content paths churn.

## Why This Sprint

`Content/` mixes scenes, prefab templates, sprite descriptors + textures, audio, fonts, and config in one flat folder — the same "where does this live?" problem as the code, but with a harder failure mode: a wrong path only surfaces at runtime (missing asset), not compile time. Grouping by asset type makes the content pipeline auditable and matches how scenes/templates/audio are already conceptually separated.

## Target Folders

| Folder | Contents |
|--------|----------|
| `Content/` (root) | `scenes.xml` — **stays put** (registered by asset name + read by the harness) |
| `Content/Scenes/` | The 8 `<Scene>`-rooted files: HomeScene, CharacterScene, CameraScene, GuiAnchorDemo, LabelAlignmentDemoScene, PhysicsEntityScene, SendMessageDemoScene, loading.xml |
| `Content/Templates/` | BallTemplate, CharacterTemplate, PingPrefabTemplate, SoundButtonTemplate, TextTemplate, VolumeButtonTemplate (.xml) |
| `Content/Sprites/` | ball_sprite.xml, Ball.png, character_anim_walk.xml, character_malePerson_sheetHD.* (png/xml), character_sheet.xml, character_sprite.xml |
| `Content/Audio/` | footstep00-02.ogg, Goblins_Den_(Regular).wav, footstep*_sound.xml, song1_sound.xml |
| `Content/Fonts/` | base.spritefont, DiagnosticsFont.spritefont, ComicMono.ttf |
| `Content/Config/` | PhysicsConfig.xml |

Stray artifacts: `PhysicsScene_Save.xml` (a runtime save file that leaked into the source tree — move to a gitignored location or delete; confirm nothing references it as an input) and any duplicate at the repo root.

## Tasks

- [ ] T1 🔁 Move scene XMLs (incl. `loading.xml`) into `Content/Scenes/`; update their `/copy:` entries in `Content.mgcb`.
- [ ] T2 🔁 Move prefab templates into `Content/Templates/`; update mgcb + every XML that references them by name (`PhysicsEntityScene.xml` uses `Asset="BallTemplate.xml"`, etc.).
- [ ] T3 🔁 Move sprite descriptors/textures into `Content/Sprites/` and audio sources/descriptors into `Content/Audio/`; update mgcb `/importer`/`/processor`/`/build` paths and any XML/C# asset-name references (e.g. `MusicComponent`'s default `song1_sound.xml`, `SoundKeyComponent`'s footstep names).
- [ ] T4 🔁 Move fonts into `Content/Fonts/` and `PhysicsConfig.xml` into `Config/`; update mgcb + the config asset name used by `<System Type="PhysicsEngine" Config="PhysicsConfig.xml" />`.
- [ ] T5 🔁 Update test content lookups that walk to `CoreEssentials.Playground/Content/{name}` (`BootFromFilesTests.ReadSourceContentFile`, Sprint5b/c/d fixtures) for the new subfolder paths; confirm `scenes.xml` references still resolve (scene names are relative to Content root — verify how asset names map after the move and update `scenes.xml` entries if the runtime asset key changes).
- [ ] T6 🔁 Relocate/delete stray save artifacts (`PhysicsScene_Save.xml`) and add a gitignore entry for runtime saves.
- [ ] T7 🔒 Build clean + full suite green; smoke-run all 7 scenes via the harness — same PASS list as before.

## Acceptance Criteria

- `Content/` root holds only `scenes.xml`, `Content.mgcb`, and the new subfolders; no loose scene/template/sprite/audio/font/config files remain at the root.
- Every asset still loads by name at runtime (harness PASS for all 7 scenes proves the full chain: manifest → scene XML → templates → sprites → audio).
- Full suite green, including the tests that read real playground content.

## Notes & Risks

- **Asset key vs. file path** — MonoGame's content pipeline keys assets by their *root name relative to the content root*, not the full path. Moving a file into a subfolder can change its runtime asset key (e.g. `Scenes/HomeScene.xml` may no longer be loadable as `"HomeScene.xml"`). Verify per file type (`/copy:` XML assets vs. imported textures/fonts) and update references — `scenes.xml`, XML cross-refs, and C# defaults all use these keys. This is the central risk of the sprint; do T1–T4 in small commits and smoke-run after each.
- **`/copy:` XML assets** keep their file content but may change key with subfolders — test early with one scene before moving all eight.
- **Tests copy real content into their own Content dir** (`WriteContentAsset(name, ReadSourceContentFile(name))`) — both the read path and the written name (the asset key) matter; a mismatch is a runtime failure in an otherwise-green build.
- Keep `scenes.xml` at the Content root throughout; if verification shows its entries must become subfolder-relative keys, that update belongs to T5, not a move of the file itself.
