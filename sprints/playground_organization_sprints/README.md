# Playground Organization — Scrum Sprints 🗃️

The playground is the reference game and the smoke-run target, but its source and content have grown into a flat pile: 27 C# files in one folder and ~30 assets (scenes, templates, sprites, audio, fonts) in `Content/`. This feature organizes both — code by responsibility with matching namespaces, content by asset type — without changing any runtime behavior.

> **Branch:** work landed directly on `feature/scene-as-data`. Pure reorganization: build clean, full suite green, and every scene still smoke-runs PASS before and after each task.

## Current State

**Code** — 27 files flat in `CoreEssentials.Playground/`, all in the single namespace `CoreEssentials.Playground` (except top-level `Program.cs`):

- 9 entities: `Ball`, `PlayerEntity`, `CharacterEntity`, `AnimatedCharacterEntity`, `CameraEntity`, `TextEntity`, `SoundButtonEntity`, `VolumeButtonEntity`, `WorldBorder`
- 17 components: camera (`CameraInputComponent`, `CameraFollowToggleComponent`), physics (`PhysicsSpawnComponent`, `PhysicsDebugOverlayComponent`), GUI/HUD (`HudLabelRefreshComponent`, `SaveLoadButtonsComponent`, `OrbitPanelComponent`, `LabelAlignmentDebugOverlayComponent`), audio (`MusicComponent`, `SoundKeyComponent`, `VolumeKeyComponent`), debug/flow (`DebugToggleComponent`, `NavigateOnKeyComponent`, `PingControlComponent`, `PingReceiverComponent`, `ScoreKeeperComponent`)
- 1 scene class: `XmlLayoutScene` (referenced by no XML — removed in Sprint 1)
- Entry point: `Program.cs` + `SceneLaunchOptionsParser.cs`

**Content** — ~30 files flat in `CoreEssentials.Playground/Content/`: 8 `<Scene>`-rooted scene files, `loading.xml`, `scenes.xml` (the manifest), 7 prefab templates, character/ball sprite XMLs + PNGs, footstep/music audio + sound descriptor XMLs, 2 spritefonts, 1 raw TTF, plus a stray `PhysicsScene_Save.xml` save artifact.

## Target Layout

```
CoreEssentials.Playground/
├── Program.cs                      # top-level entry point (stays at root)
├── SceneLaunchOptionsParser.cs     # command-line harness options
├── Entities/                       # ns CoreEssentials.Playground.Entities
│   ├── Ball.cs  PlayerEntity.cs  CharacterEntity.cs  AnimatedCharacterEntity.cs
│   ├── CameraEntity.cs  TextEntity.cs  SoundButtonEntity.cs  VolumeButtonEntity.cs  WorldBorder.cs
├── Components/                     # ns CoreEssentials.Playground.Components
│   ├── CameraInputComponent.cs  CameraFollowToggleComponent.cs
│   ├── PhysicsSpawnComponent.cs  PhysicsDebugOverlayComponent.cs
│   ├── HudLabelRefreshComponent.cs  SaveLoadButtonsComponent.cs  OrbitPanelComponent.cs
│   ├── LabelAlignmentDebugOverlayComponent.cs
│   ├── MusicComponent.cs  SoundKeyComponent.cs  VolumeKeyComponent.cs
│   ├── DebugToggleComponent.cs  NavigateOnKeyComponent.cs  PingControlComponent.cs
│   ├── PingReceiverComponent.cs  ScoreKeeperComponent.cs
├── Scenes/
│   └── XmlLayoutScene.cs           # ns CoreEssentials.Playground.Scenes (or removed if dead)
└── Content/
    ├── scenes.xml                  # manifest — stays at Content root (registered by asset name "scenes.xml")
    ├── DiagnosticsFont.spritefont  # stays at root — Aether's DebugView hardcodes content.Load<SpriteFont>("DiagnosticsFont")
    ├── Scenes/                     # <Scene>-rooted scene files + the loading screen
    │   ├── HomeScene.xml  CharacterScene.xml  CameraScene.xml  GuiAnchorDemo.xml
    │   ├── LabelAlignmentDemoScene.xml  PhysicsEntityScene.xml  SendMessageDemoScene.xml  loading.xml
    ├── Templates/                  # prefab templates
    │   ├── BallTemplate.xml  CharacterTemplate.xml  PingPrefabTemplate.xml
    │   ├── SoundButtonTemplate.xml  TextTemplate.xml  VolumeButtonTemplate.xml
    ├── Sprites/                    # sprite descriptor XMLs + textures
    │   ├── ball_sprite.xml  Ball.png  character_anim_walk.xml  character_malePerson_sheetHD.*
    │   └── character_sheet.xml  character_sprite.xml
    ├── Audio/                      # sound effect descriptors + OGG/WAV sources
    │   ├── footstep*_sound.xml  song1_sound.xml  footstep00-02.ogg  Goblins_Den_(Regular).wav
    ├── Fonts/                      # spritefonts + raw font (DiagnosticsFont stays at root)
    │   ├── base.spritefont  ComicMono.ttf
    └── Config/
        └── PhysicsConfig.xml
```

## Conventions & Invariants

- **Namespaces mirror folders** (`CoreEssentials.Playground.Entities` etc.) — matches the core's convention that folder and namespace stay in sync. This means XML `Type=` references, which use **fully-qualified type names**, must be updated when a type moves (see risk notes).
- **`Program.cs` stays top-level** at the project root; it has no namespace today and keeps working.
- **Behavior is unchanged.** No logic edits beyond moving code, updating namespaces/usings, XML `Type=` strings, and content paths. The smoke-run harness must pass all 7 scenes before and after each task.
- **`scenes.xml` stays at the Content root** — it is registered by asset name (`SetManifestAsset("scenes.xml")`) and read by the harness; moving it would ripple into both.
- **No issue/PR numbers in code or docs** — repo convention.

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 1 | [Code Organization](Sprint_1_Code_Organization.md) | 5 | ✅ Done | Move C# files into Entities/Components, update namespaces + usings, update all XML `Type=` references (scenes, templates, tests), removed dead `XmlLayoutScene` |
| 2 | [Content Organization](Sprint_2_Content_Organization.md) | 5 | ✅ Done | Move content into Scenes/Templates/Sprites/Audio/Fonts/Config, update `Content.mgcb` paths + every asset-name reference (manifest, XML cross-refs, C# defaults). `DiagnosticsFont.spritefont` left at root (Aether hardcodes its key); save artifacts already gitignored |

## Risk Notes

- **XML `Type=` references are fully-qualified** — renaming namespaces touches scene XMLs, prefab templates, and test assertions that compare type strings (e.g. `Sprint5dDataSceneTests`). Mitigation: do it in one commit per sprint so the build+suite catches stragglers immediately.
- **MonoGame content pipeline paths** — `.mgcb` entries are relative to the mgcb file; moving assets into subfolders changes both the `/copy:`/`/build:` source paths and, for some importers, the asset root used at runtime. `scenes.xml` must not move (see invariants).
- **Tests read real playground content** — `BootFromFilesTests` / `Sprint5b/c/d` walk up to `CoreEssentials.Playground/Content/{name}`; content moves require updating those lookup paths too.
