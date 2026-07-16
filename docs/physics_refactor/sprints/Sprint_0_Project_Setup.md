# Sprint 0 — Project Setup & Build Validation 📋

**Points:** 2  
**Status:** ✅ Completed  
**Completed Date:** 2026-07-16  
**Sprint Goal:** Create the `CoreEssentials.Physics` project with correct folder structure, dependencies, and verify it builds cleanly.

---

## Tasks

- [x] **T1: Create project folder structure (1 pt)**
  - Create `CoreEssentials.Physics/` directory with subfolders:
    ```
    CoreEssentials.Physics/
    ├── types/                   ← Pure interface abstractions (NO Aether refs)
    ├── engines/aether/          ← Aether engine implementations
    │   └── Shapes/              ← CircleShape, RectangleShape, PolygonShape
    ├── factory/                 ← Factory classes for creating physics objects
    ├── bin/Debug/
    └── obj/
    ```
  - Create placeholder `.cs` files in each folder so the build can succeed
  - ✅ Done: Created `types/`, `engines/aether/Shapes/`, `factory/` with all placeholder `.cs` files

- [x] **T2: Configure project file (1 pt)**
  - Create `CoreEssentials.Physics.csproj` with correct target framework matching CoreEssentials (`net8.0`)
  - Add package reference to `nkast.Aether.Physics2D.MG`
  - Add project reference to `CoreEssentials/CoreEssentials.csproj`
  - Set `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` for NuGet prep
  - Configure output path: `<OutputPath>bin\$(Configuration)</OutputPath>`
  - ✅ Done: `.csproj` created with `net8.0`, Aether 2.1.1, CoreEssentials reference, `GeneratePackageOnBuild=true`

- [x] **T3: Add to solution and verify build (0.5 pt)**
  - Add `CoreEssentials.Physics.csproj` to `core-essentials-monogame.sln`
  - Run `dotnet build` on the new project — must compile with zero errors
  - Verify Playground project can reference CoreEssentials.Physics

---

## Acceptance Criteria

- [x] `CoreEssentials.Physics/` folder exists with all subdirectories (`types/`, `engines/aether/Shapes/`, `factory/`)
- [x] `.csproj` file references both Aether and CoreEssentials correctly
- [x] Project builds cleanly with zero errors/warnings — confirmed via `dotnet build`
- [x] Solution file includes the new project — verified in `core-essentials-monogame.sln`

---

## Deliverables

| File | Purpose |
|------|---------|
| `CoreEssentials.Physics/CoreEssentials.Physics.csproj` | Project definition with dependencies |
| Placeholder `.cs` files in each folder | Allow clean build for now (replaced in Sprint 1) |
| Updated `core-essentials-monogame.sln` | Includes new project |

---

## Notes & Risks

- **Risk:** Aether package version mismatch with CoreEssentials. Verify both projects use the same `nkast.Aether.Physics2D.MG` version.
- The existing `CoreEssentials.Physics/` folder may already exist from previous work — if so, restructure it to match the new layout above (rename `adapters/interfaces/` → `types/`, etc.)

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
