# Sprint 0 — Project Setup & Build Validation 📋

**Points:** 2  
**Status:** Not Started  
**Sprint Goal:** Create the `CoreEssentials.Physics` project with correct folder structure, dependencies, and verify it builds cleanly.

---

## Tasks

- [ ] **T1: Create project folder structure (1 pt)**
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

- [ ] **T2: Configure project file (1 pt)**
  - Create `CoreEssentials.Physics.csproj` with correct target framework matching CoreEssentials (`net8.0`)
  - Add package reference to `nkast.Aether.Physics2D.MG`
  - Add project reference to `CoreEssentials/CoreEssentials.csproj`
  - Set `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` for NuGet prep
  - Configure output path: `<OutputPath>bin\$(Configuration)</OutputPath>`

- [ ] **T3: Add to solution and verify build (0.5 pt)**
  - Add `CoreEssentials.Physics.csproj` to `core-essentials-monogame.sln`
  - Run `dotnet build` on the new project — must compile with zero errors
  - Verify Playground project can reference CoreEssentials.Physics

---

## Acceptance Criteria

- [ ] `CoreEssentials.Physics/` folder exists with all subdirectories (`types/`, `engines/aether/Shapes/`, `factory/`)
- [ ] `.csproj` file references both Aether and CoreEssentials correctly
- [ ] Project builds cleanly with zero errors/warnings
- [ ] Solution file includes the new project

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
