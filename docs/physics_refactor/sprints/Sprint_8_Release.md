# Sprint 8 — Code Review & Release Prep 🚀

**Points:** 3  
**Status:** Not Started (depends on Sprint 7)  
**Sprint Goal:** Final quality pass, performance profiling, NuGet package configuration, and prepare for release.

---

## Tasks

- [ ] **T1: Full code review (1 pt)**
  - Review all CoreEssentials.Physics source files for:
    - Consistent naming conventions
    - Proper null handling / argument validation
    - Resource disposal patterns (all `IDisposable` types correctly clean up)
    - No Aether types leaking through public API surface
  - Address any remaining code smells or technical debt

- [ ] **T2: Performance profiling (1 pt)**
  - Profile typical workloads in Playground:
    - Body creation/destruction throughput (with and without pooling)
    - Frame time budget: ensure `PhysicsEngine.Update()` stays well within fixed timestep budget
    - Memory allocation: verify pooling is reducing GC pressure vs. naive approach
  - Document findings; optimize hot paths if needed

- [ ] **T3: NuGet package configuration (1 pt)**
  - Configure `.csproj` for proper NuGet packaging:
    - `<PackageId>CoreEssentials.Physics</PackageId>`
    - Version, description, authors, tags
    - `<RepositoryUrl>` pointing to GitHub repo
    - `<PackageReadmeFile>` if applicable
  - Verify `dotnet pack` produces a valid `.nupkg`
  - Test restore from local package in a clean test project

---

## Acceptance Criteria

- [ ] All code review findings addressed
- [ ] Performance profiled: body creation, simulation step time, memory allocations measured and documented
- [ ] NuGet package builds successfully (`dotnet pack`)
- [ ] Package can be restored and used in a separate test project
- [ ] Full test suite passes on clean build

---

## Deliverables

| Artifact | Purpose |
|----------|---------|
| `CoreEssentials.Physics/CoreEssentials.Physics.csproj` | Updated with NuGet metadata (PackageId, Version, Description, etc.) |
| Performance notes in `.github/memory.md` or docs/ | Documented benchmark results |
| Valid `.nupkg` file | Ready for publishing to NuGet.org |

---

## Notes & Risks

- **NuGet publish:** The `./scripts/publish.sh` script handles publishing — verify it's configured correctly for the new project.
- Consider whether CoreEssentials.Physics should be a standalone package or bundled into `CoreEssentials-MonoGame` (see SUMMARY.md Option A).
- If bundling into single package: configure CoreEssentials.csproj to reference Physics, and pack CoreEssentials as the transitive-inclusive package.

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
