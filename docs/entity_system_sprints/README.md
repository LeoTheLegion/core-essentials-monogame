# Entity System Enhancements — Scrum Sprints 🚀

This folder contains sprint plans for the Entity System enhancement project using an agile/Scrum approach. Each file represents one sprint with tasks estimated in story points (1, 2, or 5 points).

## Why This Project? ⚠️

Currently, the Entity System is a **basic OOP architecture** with minimal features:

```csharp
// CURRENT — Basic entity creation, no tags, queries, or hierarchy ❌
var ball = entitySystem.CreateEntity<Ball>(position);
// No way to group, query, or efficiently find entities
foreach (var e in allEntities) {
    if (e is Ball b && Vector2.Distance(b.Position, player.Position) < 500) {
        // manual distance check 😫
    }
}
```

**The enhancements** introduce tagging, querying, pooling, eventing, components, XML definitions, tweening, and more — making the entity system a complete, data-driven game architecture.

---

## Project Structure — Before & After

### Current Structure ❌
```
CoreEssentials/src/GameSystems/EntitySystems/EntityOOPSystem/
├── Entity.cs              ← Basic properties (Position, Rotation, Sort)
├── EntitySystem.cs        ← List<Entity> management, no queries
└── (nothing else)
```

### Target Structure ✅
```
CoreEssentials/src/GameSystems/EntitySystems/EntityOOPSystem/
├── Entity.cs              ← Enhanced with tags, IDs, components, lifecycle hooks
├── EntitySystem.cs        ← Query API, pooling, layers, groups
├── Tagging/
│   ├── EntityTags.cs      ← String-based tagging system
│   └── EntityQuery.cs     ← Query API (FindByType, FindNearby, FindByTag)
├── Pooling/
│   ├── EntityPool.cs      ← Object pooling for high-spawn-rate entities
│   └── IPooledEntity.cs   ← Interface for poolable entities
├── Events/
│   ├── EntityEventSystem.cs ← Publish/subscribe event system
│   └── EntityEventArgs.cs  ← Event data container
├── Hierarchy/
│   ├── EntityParent.cs    ← Parent-child transform inheritance
│   └── EntityChild.cs     ← Child entity with local offset
├── Components/
│   ├── EntityComponent.cs ← Base component class
│   ├── ComponentSystem.cs ← Component management on entities
│   └── BuiltIn/           ← Built-in components (Health, Velocity, etc.)
├── Spatial/
│   ├── SpatialGrid.cs     ← Grid-based spatial partitioning
│   └── SpatialQuery.cs    ← Fast spatial queries (FindInBounds, FindClosest)
├── Layers/
│   ├── EntityLayer.cs     ← Layer definition
│   └── LayerManager.cs    ← Layer management & update control
├── Serialization/
│   ├── EntitySerializer.cs ← XML serialization (save/load)
│   ├── EntityTemplate.cs  ← Template/prefab definitions
│   └── EntityReference.cs ← Entity ID & reference resolution
├── Tweening/
│   ├── EntityTween.cs     ← Entity tweening wrapper (uses MonoGame.Extended)
│   └── TweenBuilder.cs    ← Fluent tween API
├── Debug/
│   ├── EntityDebugDraw.cs ← Debug visualization (bounds, IDs, tags, hierarchy)
│   └── DebugConfig.cs     ← Configurable debug overlays
└── Collision/
    ├── CollisionGroup.cs  ← Collision group definitions
    └── CollisionMatrix.cs ← Collision filtering matrix
```

---

## Sprint Structure

Each sprint is designed to be approximately **5 total points** worth of work, following standard Scrum principles:
- **1 point** = Small task (30 min - 2 hours)
- **2 points** = Medium task (2-4 hours)
- **5 points** = Large task (1 full day or more)

---

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 🏷️ [0](Sprint_0_EntityTags.md) | Entity Tags | 3 | Not Started | String-based tagging for easy grouping and lookup |
| 🔍 [1](Sprint_1_EntityQueryAPI.md) | Entity Query API | 4 | Not Started | FindByType, FindNearby, FindByTag methods |
| ♻️ [2](Sprint_2_EntityPooling.md) | Entity Pooling | 5 | Not Started | Object pooling for high-spawn-rate entities |
| 📡 [3](Sprint_3_EventSystem.md) | Event System | 5 | Not Started | Decoupled publish/subscribe for entities |
| 🌳 [4](Sprint_4_ParentChildHierarchy.md) | Parent-Child Hierarchy | 5 | Not Started | Transform inheritance for child entities |
| 🎨 [5](Sprint_5_RenderBatching.md) | Render Batching | 4 | Not Started | Sort entities by texture for better GPU utilization |
| 🧩 [6](Sprint_6_LightweightComponents.md) | Lightweight Components | 5 | Not Started | Mixin-style component system |
| 🗺️ [7](Sprint_7_SpatialPartitioning.md) | Spatial Partitioning | 7 | Not Started | Grid/quadtree for fast spatial queries |
| 📚 [8](Sprint_8_EntityGroupsLayers.md) | Entity Groups/Layers | 4 | Not Started | Logical grouping for independent update/render |
| ⏱️ [9](Sprint_9_DelayedLifecycle.md) | Delayed Lifecycle | 3 | Not Started | Built-in spawn/destroy/respawn scheduling |
| 📄 [10](Sprint_10_XMLEntityDefinitions.md) | XML Entity Definitions | 7 | Not Started | Declarative entity definitions using XML |
| 📦 [11](Sprint_11_EntityTemplates.md) | Entity Templates/Prefabs | 5 | Not Started | Reusable entity blueprints |
| � [11.5](Sprint_11_5_UserDocumentation.md) | User Documentation | 8 | Not Started | User guides for completed sprints |
| �🔖 [12](Sprint_12_EntityIDs.md) | Entity IDs & References | 4 | Not Started | Unique identifiers and cross-entity linking |
| 💾 [13](Sprint_13_GameStateSerialization.md) | Game State Serialization | 6 | Not Started | Save/load full entity state |
| 🎬 [14](Sprint_14_EntityTweening.md) | Entity Tweening | 4 | Not Started | Built-in animation with MonoGame.Extended |
| 🔍 [15](Sprint_15_DebugVisualization.md) | Debug Visualization | 3 | In Progress | Draw entity metadata in debug mode |
| 🎞️ [15.5](Sprint_15_5_SpriteConsolidation.md) | Sprite Consolidation & Animation Component | 5.5 | ✅ Completed | Unify `Sprite`/`AnimatedSprite`, add multi-animation `AnimationComponent` |
| 🔄 [16](Sprint_16_LifecycleHooks.md) | Entity Lifecycle Hooks | 3 | Not Started | OnEnable, OnDisable, OnPause, OnResume, OnAwake |
| 🔗 [17](Sprint_17_EntityRelationships.md) | Entity Relationships | 4 | Not Started | Weak-reference relationships between entities |
| 📜 [18](Sprint_18_ScriptableBehaviors.md) | Scriptable Behaviors | 6 | Not Started | Attach coroutines/scripts declaratively |
| 💥 [19](Sprint_19_CollisionGroups.md) | Collision Groups | 5 | Not Started | Filtered collision interaction groups |
| 🎚️ [20](Sprint_20_ZOrderRenderLayers.md) | Z-Order & Render Layers | 4.5 | ✅ Completed | Combine texture batching with z-order layers |

---

## Sprint Point Summary

- **Total Points:** 110 points across 23 sprints
- **Average Per Sprint:** ~4.8 points
- **Timeline Estimate:** 23 weeks (one sprint per week) or compressed to 12-14 weeks with parallel work on independent sprints

---

## Key Workflow Phases

**Foundation (Sprint 0-3):** Tags, Query API, Pooling, Events — core utilities that improve day-to-day development.

**Architecture (Sprint 4-9):** Hierarchy, Render Batching, Components, Spatial Partitioning, Layers, Delayed Lifecycle — structural improvements.

**Data-Driven (Sprint 10-13):** XML Definitions, Templates, IDs, Serialization — designer-friendly workflow.

**Polish (Sprint 14-20):** Tweening, Debug Viz, Sprite Consolidation, Lifecycle Hooks, Relationships, Scripts, Collision Groups, Z-Order Layers — quality of life and advanced features.

---

## Dependencies Between Sprints

Some sprints have dependencies on others:

| Sprint | Depends On | Reason |
|--------|------------|--------|
| Sprint 1 (Query API) | Sprint 0 (Tags) | FindByTag needs tagging system |
| Sprint 10 (XML) | Sprint 0, 6, 12 | XML needs tags, components, and IDs |
| Sprint 11 (Templates) | Sprint 10 (XML) | Templates build on XML definitions |
| Sprint 13 (Serialization) | Sprint 10, 12 | Save/load needs XML and IDs |
| Sprint 14 (Tweening) | None | Independent, wraps MonoGame.Extended |
| Sprint 15 (Debug) | Sprint 0, 16 | Debug needs tags and lifecycle hooks |
| Sprint 15.5 (Sprite Consolidation) | Sprint 6, 10 | AnimationComponent needs components and XML definitions |
| Sprint 18 (Scripts) | Sprint 3, 6 | Scripts need events and components |
