# Implementation Plan for Physics Adapters (Sprint 1+)

## Overview

This document outlines the implementation strategy for wrapping Aether Physics2D types with clean adapter interfaces.

## Adapter Mapping Strategy

### Core Type Mappings

| User-Facing Interface | Internal Aether Type | Namespace | Implementation File |
|----------------------|---------------------|-----------|-------------------|
| `IPhysicsBodyAdapter` | `nkast.Aether.Physics2D.Dynamics.Body` | Physics2D.Dynamics | `adapters/implementations/PhysicsBodyAdapter.cs` |
| `IFixtureAdapter` | `nkast.Aether.Physics2D.Dynamics.Fixture` | Physics2D.Dynamics | `adapters/implementations/FixtureAdapter.cs` |
| `ISpatialShapeAdapter` | Various Shape classes | Physics2D.Dynamics/Collision/Shapes | ShapeAdapters/ |
| `IPhysicsWorldAdapter` | `nkast.Aether.Physics2D.Dynamics.World` | Physics2D.Dynamics | `adapters/implementations/PhysicsWorldAdapter.cs` |
| `IConstraintAdapter` | Various Constraint classes | Physics2D.Constraints | Constraints/ |
| `IPhysicsFactory` | Factory pattern wrapper | Factory | `factory/PhysicsFactory.cs` |

## Implementation Order (Sprints 1-4)

### Sprint 1: Core Interface Implementations
- [ ] PhysicsBodyAdapter.cs - Wrap Body class
- [ ] FixtureAdapter.cs - Wrap Fixture class  
- [ ] SimpleShapeAdapters.cs - Basic Circle, Rectangle shapes
- [ ] PhysicsWorldAdapter.cs - Wrap World class (excluding complex solver settings)

### Sprint 2: Advanced Shape Adapters
- [ ] PolygonShapeAdapter.cs - Convex polygon support
- [ ] ChainShapeAdapter.cs - Chain of vertices
- [ ] EdgeShapeAdapter.cs - Line segments for raycasting

### Sprint 3: Constraint/Joint Adapters
- [ ] RevoluteJointAdapter.cs - Hinge constraints
- [ ] DistanceConstraintAdapter.cs - Spring constraints
- [ ] WeldConstraintAdapter.cs - Rigid body connections

### Sprint 4: Factory & Pooling Integration
- [ ] PhysicsFactoryAdapter.cs - Implement factory pattern
- [ ] BodyPoolAdapter.cs - Integrate with existing WorldPool architecture
- [ ] FixturePoolAdapter.cs - Reuse fixtures when possible

## Key Design Decisions

### 1. No Aether Types in Public APIs
All public interfaces (`*.cs` files starting from `adapters/interfaces/`) must NOT reference any types from the `nkast.Aether.*` namespace. This ensures:
- Clean abstraction boundary
- Future engine swapping capability  
- Testability without requiring Aether assemblies

### 2. Preserve Pooling Architecture
The existing WorldPool pattern should be preserved in the adapter layer, wrapping Aether's body management while maintaining the pooling behavior.

### 3. Minimal Configuration Exposure
Only essential configuration options are exposed (gravity, basic solver iterations). Advanced settings like multithreading thresholds remain internal optimizations.

### 4. Shape Type Enum for Flexibility
The `ShapeType` enum provides a type-safe way to identify shapes without exposing Aether's internal shape hierarchy.

## Testing Strategy

Each adapter implementation will be tested in:
1. Unit tests (CoreEssentials.Tests) - Interface compliance
2. Integration tests (CoreEssentials.Playground) - Real physics scenarios  
3. Performance benchmarks - Compare with direct Aether usage

## Migration Path for Existing Code

Current code using `Body`, `Fixture`, `World` directly will need refactoring to:
```csharp
// Before
var body = _world.CreateBody(position, rotation, nkast.Aether.Physics2D.Dynamics.BodyType.Dynamic);
body.CreateCircle(radius, density);

// After  
var factory = _physicsFactory;
var body = factory.CreateDynamicBody(position, rotation);
var fixture = body.CreateCircle(radius, 1f);
```

## Known Limitations (Future Work)

- Continuous Collision Detection (CCD) settings are applied globally via `nkast.Aether.Physics2D.Settings` - may need future abstraction
- Complex joint types (Prismatic, Slider, etc.) not yet implemented
- Sensor fixtures work but lack dedicated API methods for common sensor patterns
