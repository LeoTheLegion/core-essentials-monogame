# Physics System Refactoring: Adapter Pattern Implementation

## Overview

This document outlines the refactoring plan to convert the current physics engine from a direct wrapper around `nkast.Aether.Physics2D` into a proper **adapter library** that provides a complete, ready-to-use 2D physics system. This will allow users to interact with the physics system through clean, intuitive interfaces without needing to understand or depend on Aether Physics2D directly.

The new [`CoreEssentials.Physics`](c:\repo\core-essentials-monogame\CoreEssentials.Physics\CoreEssentials.Physics.csproj) library will provide:
- **Clean, high-level API** - No need to read Aether documentation
- **Complete physics system** - Ready-to-use engine with factory patterns  
- **Adapter pattern internally** - Hides all external physics engine complexity

---

## Current Architecture Problems ✅

### 1. Direct External Library Dependency

The current architecture exposes `nkast.Aether.Physics2D` classes throughout the codebase:

- **PhysicsEngine.cs** - Exposes `_world` of type `Aether.World` and returns `Body` objects directly
- **WorldPool.cs** - Works with Aether's internal structures (`FixtureList`, etc.)
- **PhysicsDebugRenderer.cs** - Directly accesses `fixture.Shape` which returns Aether types

### 2. External Types Passed Through to Users

Users must currently work with these external library types:

```csharp
// Current API - exposes Aether directly
Body body = physicsEngine.CreateCircle(position, radius);
body.FixtureList.Add(fixture); // Direct Aether API
var shape = fixture.Shape;     // Returns Aether.CircleShape or similar
```

**Required User Knowledge:**
- Understanding of `nkast.Aether.Physics2D.Dynamics.Body`
- Manual fixture management via `AddFixture()`, `RemoveFixture()`
- Shape type handling (PolygonShape, CircleShape, etc.)
- Constraint/joint APIs from Aether
- Solver configuration options

---

## Classes That Need Abstraction 🔧

### Priority 1: Core Body/Fixture/Shape Adapters ⭐

#### **1. PhysicsBody Adapter** - High Priority

**Current State:** `Aether.Physics2D.Dynamics.Body` is exposed directly to users.

**Solution:** Create an abstraction layer with methods like `CreateCircle()`, `AddFixture()`:

```csharp
// Proposed interface
public interface IPhysicsBody : IDisposable
{
    Vector2 Position { get; }
    float Rotation { get; set; }
    BodyType Type { get; set; }
    
    // Shape creation methods - users never see Aether types
    void CreateCircle(float radius);
    void CreateRectangle(Vector2 size);
    void CreatePolygon(Vector2[] vertices);
    
    // Fixture management
    IFixture AddFixture(IShape shape, float density = 1f, Vector2 friction = default, 
                        Vector2 restitution = default);
    void RemoveFixture(IFixture fixture);
    
    // Properties and methods
    float Mass { get; set; }
    float Friction { get; set; }
    float Restitution { get; set; }
    bool FixedRotation { get; set; }
    
    // Movement controls
    void ApplyForce(Vector2 force, Vector2? relativePoint = null);
    void ApplyImpulse(Vector2 impulse);
    void SetLinearVelocity(Vector2 velocity);
    void SetAngularVelocity(float angularVelocity);
}

// Implementation would wrap Aether.Body internally
public class PhysicsBody : IPhysicsBody { ... }
```

#### **2. Fixture Adapter** - High Priority

**Current State:** Fixtures are returned directly from Aether and must be managed manually.

**Solution:** Abstract fixture lifecycle management:

```csharp
public interface IFixture : IDisposable
{
    IShape Shape { get; }
    bool IsActive { get; }
    
    // Lifecycle management - abstracted from Aether's Add/Remove patterns
    void Activate();
    void Deactivate();
}

// Factory for creating shapes without exposing types
public interface ISpatialShapeFactory
{
    IShape CreateCircle(float radius);
    IShape CreateRectangle(Vector2 size);
    IShape CreatePolygon(Vector2[] vertices);
    IShape CreateConvexHull(Vector2[] points);
    IShape CreateLineSegment(Vector2 start, Vector2 end);
}

public class CircleShape : ISpatialShape { ... }
public class RectangleShape : ISpatialShape { ... }
```

#### **3. Collision Shapes Adapter** - High Priority

**Current State:** `fixture.Shape` returns Aether types directly (`CircleShape`, `PolygonShape`, etc.)

**Solution:** Spatial shape abstraction:

```csharp
// Shape interface - abstracts away Aether's different shape types
public interface ISpatialShape : IDisposable
{
    Vector2 Center { get; }
    float Radius => GetBoundingRadius(); // Unified API for different shapes
    
    // All shapes have these common properties
    void Translate(Vector2 offset);
    void Rotate(float angle);
}

// Shape variants implement the same interface
public class CircleShape : ISpatialShape { ... }
public class RectangleShape : ISpatialShape { ... }
public class PolygonShape : ISpatialShape { ... }
```

---

### Priority 2: World & Constraint Adapters 🔶

#### **4. World Adapter** - High Priority

**Current State:** `new World()` and all Aether world operations are exposed directly.

**Solution:** Interface-based simulation management:

```csharp
public interface IPhysicsWorld : IDisposable
{
    Vector2 Gravity { get; set; }
    
    // Body management
    void AddBody(IPhysicsBody body);
    void RemoveBody(IBody body);
    
    // Simulation control
    void Step(float deltaTime, SolverConfig solverOptions);
    void ClearBodies();
}

// Factory for world creation with configuration
public interface IPhysicsWorldFactory : IDisposable
{
    IPhysicsWorld CreateDefault();
    IPhysicsWorld CreateWithGravity(Vector2 gravity);
    IPhysicsWorld CreateWithConfig(PhysicsConfig config);
}
```

#### **5. Constraints Adapter** - Medium Priority

**Current State:** Direct exposure of constraint APIs (joints, limits).

**Solution:** Encapsulate constraints:

```csharp
public interface IConstraint : IDisposable
{
    void Apply();
    void Remove();
}

// Joint types with unified API
public interface IPivotJoint : IConstraint { ... }
public interface IFixedJoint : IConstraint { ... }
public interface IDistanceJoint : IConstraint { ... }
```

#### **6. Contact Solver Adapter** - Medium Priority

**Current State:** Configuration options for velocity/position iterations are exposed.

**Solution:** Encapsulate the configuration:

```csharp
public class SolverConfig
{
    public int VelocityIterations { get; set; } = 8;
    public int PositionIterations { get; set; } = 3;
    public bool ContinuousCollisionDetection { get; set; } = false;
}

// Usage:
var world = PhysicsWorldFactory.CreateWithConfig(new SolverConfig 
{ 
    VelocityIterations = 16,
    PositionIterations = 8 
});
```

---

## Proposed Interface Structure 📋

### Adapter Layer Abstraction Files

#### **IPhysicsBodyAdapter.cs** - Core Body Interface
```csharp
namespace CoreEssentials.Physics.Adapters
{
    public interface IPhysicsBody : IDisposable
    {
        // Position and rotation
        Vector2 WorldPosition { get; }
        float Rotation { get; set; }
        
        // Type management
        BodyType Type { get; set; }
        bool IsStatic => Type == BodyType.Static;
        bool IsDynamic => Type == BodyType.Dynamic;
        bool IsKinematic => Type == BodyType.Kinematic;
        
        // Shape creation - users never see Aether types
        void CreateCircle(float radius, Vector2? localCenter = null);
        void CreateRectangle(Vector2 size, Vector2? localCenter = null);
        void CreatePolygon(Vector2[] vertices, Vector2? localCenter = null);
        void CreateConvexHull(Vector2[] points);
        
        // Fixture management - unified API
        IFixture AddFixture(ISpatialShape shape, float density = 1f, 
                           CollisionFilterMask mask = default,
                           BodyType typeOverride = BodyType.Dynamic);
        void RemoveFixture(IFixture fixture);
        
        // Material properties
        float Mass { get; set; }
        float Inertia { get; set; }
        float Friction { get; set; }
        float Restitution { get; set; }
        bool FixedRotation { get; set; }
        
        // Movement and forces
        void ApplyForce(Vector2 force, Vector2? relativePoint = null);
        void ApplyTorque(float torque);
        void ApplyImpulse(Vector2 impulse, Vector2? relativePoint = null);
        
        // Velocity control
        void SetLinearVelocity(Vector2 velocity);
        void SetAngularVelocity(float angularVelocity);
        void StopAll();
        
        // Body state
        bool IsAwake { get; }
        bool IsActive { get; }
    }
}
```

#### **IFixtureAdapter.cs** - Fixture Interface
```csharp
namespace CoreEssentials.Physics.Adapters
{
    public interface IFixture : IDisposable
    {
        // Shape access
        ISpatialShape Shape { get; }
        
        // State
        bool IsActive { get; }
        Body OwnerBody { get; }
        
        // Lifecycle management
        void Activate();
        void Deactivate();
        void DisableSleep();
        void EnableSleep();
    }
}
```

#### **ISpatialShapeAdapter.cs** - Shape Interface
```csharp
namespace CoreEssentials.Physics.Adapters
{
    public interface ISpatialShape : IDisposable
    {
        // Common properties for all shape types
        Vector2 Center { get; }
        float Radius => GetBoundingRadius(); // Unified API
        
        // Transform operations
        void Translate(Vector2 offset);
        void Rotate(float angle);
        
        // Shape-specific query methods (polymorphic)
        bool PointContains(Vector2 point, bool localSpace = true);
        Vector2[] Vertices { get; } // Returns array based on shape type
        
        // Type identification for casting if needed
        ShapeType GetType(); 
    }

    public enum ShapeType
    {
        Circle,
        Rectangle,
        Polygon,
        ConvexHull,
        LineSegment,
        Unknown
    }
}
```

#### **IPhysicsWorldAdapter.cs** - World Interface
```csharp
namespace CoreEssentials.Physics.Adapters
{
    public interface IPhysicsWorld : IDisposable
    {
        // Gravity configuration
        Vector2 Gravity 
        { 
            get; 
            set; 
        }
        
        // Body management (returns adapter, never Aether types)
        int BodyCount => CountBodies();
        void AddBody(IPhysicsBody body);
        void RemoveBody(IBody body);
        void ClearAllBodies();
        
        // Simulation stepping
        void Step(float deltaTime, SolverConfig solverOptions = null);
        
        // Debug rendering integration
        void RenderDebug(IDrawingContext context);
    }

    public class SolverConfig
    {
        public int VelocityIterations { get; set; } = 8;
        public int PositionIterations { get; set; } = 3;
        public bool ContinuousCollisionDetection { get; set; } = false;
    }
}
```

#### **IConstraintAdapter.cs** - Constraint Interface
```csharp
namespace CoreEssentials.Physics.Adapters
{
    public interface IConstraint : IDisposable
    {
        // Joint body references
        Body BodyA { get; }
        Body BodyB { get; }
        
        // State management
        bool IsActive { get; }
        void Apply();
        void Remove();
    }

    public interface IPivotJoint : IConstraint
    {
        Vector2 LocalAnchorA { get; set; }
        Vector2 LocalAnchorB { get; set; }
        float LimitAngle { get; set; } // Radians
    }

    public interface IFixedJoint : IConstraint
    {
        bool CollideConnected { get; set; }
    }

    public interface IDistanceJoint : IConstraint
    {
        float Length { get; set; }
        float MaxForce { get; set; }
    }
}
```

#### **IPhysicsFactory.cs** - Factory Interface
```csharp
namespace CoreEssentials.Physics.Adapters
{
    public interface IPhysicsFactory : IDisposable
    {
        // World creation with various configurations
        IPhysicsWorld CreateDefault();
        IPhysicsWorld CreateWithGravity(Vector2 gravity);
        IPhysicsWorld CreateWithConfig(PhysicsConfig config);
        
        // Body type factory methods (returns adapters)
        IPhysicsBody CreateStatic(Vector2 position, float rotation = 0f);
        IPhysicsBody CreateDynamic(Vector2 position, float rotation = 0f);
        IPhysicsBody CreateKinematic(Vector2 position, float rotation = 0f);
        
        // Shape factory - abstracts all Aether shape types
        ISpatialShapeFactory Shapes { get; }
    }

    public class PhysicsConfig
    {
        public int VelocityIterations { get; set; } = 8;
        public int PositionIterations { get; set; } = 3;
        public bool ContinuousCollisionDetection { get; set; } = false;
        public float SubSteppingFactor { get; set; } = 1f;
    }
}
```

---

## Implementation Strategy 🛠️

### Phase 1: Core Interface Definitions

**Files to create:**
- `CoreEssentials/src/gameSystems/physics/adapters/IPhysicsBodyAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/IFixtureAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/ISpatialShapeAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/IPhysicsWorldAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/IConstraintAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/IPhysicsFactory.cs`

**Goal:** Define all public interfaces that users will interact with. No Aether references here.

### Phase 2: Adapter Implementations

**Files to create:**
- `CoreEssentials/src/gameSystems/physics/adapters/PhysicsBodyAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/FixtureAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/CircleShapeAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/RectangleShapeAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/PolygonShapeAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/PhysicsWorldAdapter.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/ConstraintAdapters/*.cs`

**Goal:** Implement all interfaces, wrapping Aether classes internally. Users never see Aether types.

### Phase 3: Factory & Helper Classes

**Files to create:**
- `CoreEssentials/src/gameSystems/physics/adapters/PhysicsFactory.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/SpatialShapeFactory.cs`
- `CoreEssentials/src/gameSystems/physics/adapters/BodyPoolAdapter.cs` (wraps existing WorldPool)

**Goal:** Provide convenient factory methods and helper utilities.

### Phase 4: Updated Existing Classes

**Files to modify:**
- `CoreEssentials/src/gameSystems/physics/PhysicsEngine.cs` - Update to use adapters internally
- `CoreEssentials/src/gameSystems/physics/WorldPool.cs` - Add adapter support or replace with BodyPoolAdapter
- `CoreEssentials/src/gameSystems/physics/PhysicsDebugRenderer.cs` - Accept shape interfaces instead of Aether types

**Goal:** Migrate existing code to use the new adapter pattern.

### Phase 5: Playground Migration Examples

**Files to create:**
- `CoreEssentials.Playground/examples/AdapterPatternMigration.md`
- Updated playground files showing before/after usage

**Goal:** Document migration path for users of the current API.

---

## Breaking Changes & Migration Path ⚠️

### Current Usage (Before Refactor)

```csharp
// Users directly work with Aether types
PhysicsEngine physicsEngine = GetGameSystem<PhysicsEngine>();

Body body = physicsEngine.CreateCircle(position, radius);
body.FixtureList.Add(fixture); // Direct Aether API
var shape = fixture.Shape;     // Returns CircleShape or PolygonShape from Aether

// Manual constraint creation with Aether APIs
Constraint joint = new RevoluteJoint(bodyA, bodyB, anchorPoint);
```

### Future Usage (After Refactor)

```csharp
// Users work only with adapter interfaces
IPhysicsFactory factory = PhysicsWorldFactory.CreateDefault();
IPhysicsBody body = factory.CreateDynamic(position);

// Shape creation - no Aether types visible
body.CreateCircle(radius); // Internally creates and manages CircleShape
var fixture = body.AddFixture(shape); // Returns IFixture, never Aether.Fixture

// Constraint usage through adapter interfaces
IPivotJoint joint = new PivotJoint(bodyA, bodyB, anchorPoint);
joint.Apply();
```

### Migration Requirements

1. **Update all user code** that directly creates or uses Aether types
2. **Replace direct fixture management** with `AddFixture()` / `RemoveFixture()` methods
3. **Use factory pattern** for creating bodies instead of calling `CreateCircle` on PhysicsEngine directly
4. **Update constraint/joint creation** to use adapter interfaces

---

## Benefits of This Architecture 🎯

### 1. Clean User API
- Users never need to know about Aether.Physics2D
- Intuitive method names (`CreateCircle`, `AddFixture`) instead of library-specific APIs
- Consistent interface across all shape types

### 2. Decoupled Dependencies
- CoreEssentials can switch physics engines in the future without breaking user code
- Users depend only on CoreEssentials interfaces, not Aether directly
- Better testability through mocking adapters

### 3. Enhanced Abstraction
- Fixture lifecycle management is abstracted (no more manual Add/Remove)
- Shape types are unified under ISpatialShape interface
- World configuration is centralized and easier to tune

### 4. Future-Proof Design
- Easy to add new physics engines
- Can introduce advanced features without API changes
- Better separation of concerns

---

## Testing Considerations 🧪

### Adapter Pattern Test Strategy

1. **Unit Tests for Adapters** - Test each adapter implementation in isolation
2. **Integration Tests** - Test the complete workflow through factory interfaces
3. **Behavior Verification** - Ensure adapters correctly translate to/from Aether operations

### Example Test Structure

```csharp
// Test PhysicsBodyAdapter
public class PhysicsBodyAdapterTests
{
    [Fact]
    public void CreateCircle_ShouldReturnActiveFixture()
    {
        var factory = new PhysicsFactory();
        var world = factory.CreateDefault();
        
        IPhysicsBody body = factory.CreateDynamic(Vector2.Zero);
        body.CreateCircle(10f);
        
        Assert.NotNull(body);
        Assert.True(body.IsActive);
    }

    [Fact]
    public void AddFixture_ShouldReturnIFixtureNotAetherType()
    {
        var fixture = body.AddFixture(shape);
        
        // Should be able to cast to IFixture, never Aether.Fixture
        Assert.IsType<IFixture>(fixture);
    }
}
```

---

## Documentation Updates Needed 📚

### Required Documentation Files

1. **Updated PhysicsSystem.md** - Rewrite with new adapter API examples
2. **Migration Guide** - Step-by-step migration from current to new API
3. **API Reference** - Document all new interfaces and implementations
4. **Advanced Usage Examples** - Show complex use cases with adapters

### Code Documentation Requirements

- All public interface methods need XML documentation
- Example usage in comments for each adapter class
- Breaking change warnings in migration notes

---

## Implementation Checklist ✅

### Pre-Implementation

- [ ] Review existing test suite to understand current API usage
- [ ] Create branch `feature/physics-adapter-pattern`
- [ ] Backup critical files (PhysicsEngine.cs, WorldPool.cs)

### Phase 1: Interfaces

- [ ] Create all interface definitions
- [ ] Define ShapeType enum and SolverConfig class
- [ ] Review interfaces for completeness

### Phase 2: Implementations

- [ ] PhysicsBodyAdapter implementation
- [ ] FixtureAdapter implementation  
- [ ] SpatialShape adapters (Circle, Rectangle, Polygon)
- [ ] PhysicsWorldAdapter implementation
- [ ] Constraint adapter implementations

### Phase 3: Factories & Helpers

- [ ] PhysicsFactory implementation
- [ ] SpatialShapeFactory implementation
- [ ] BodyPoolAdapter (wraps WorldPool)
- [ ] Update existing classes to use adapters

### Phase 4: Testing

- [ ] Write unit tests for all new interfaces
- [ ] Run existing test suite
- [ ] Fix any broken tests from migration
- [ ] Document breaking changes

### Phase 5: Documentation & Migration

- [ ] Update PhysicsSystem.md with new API
- [ ] Create migration guide
- [ ] Update playground examples
- [ ] Review all documentation for clarity

---

## Related Files Summary

### Core Implementation Files (Need Conversion)
- `CoreEssentials/src/gameSystems/physics/PhysicsEngine.cs` - Main entry point, uses Aether.World
- `CoreEssentials/src/gameSystems/physics/WorldPool.cs` - Body pooling with internal Aether types
- `CoreEssentials/src/gameSystems/physics/PhysicsDebugRenderer.cs` - Debug rendering using Aether shapes

### Test Files (Will Need Updates)
- `CoreEssentials.Tests/GameSystems/Physics/PhysicsEngineTests.cs`
- `CoreEssentials.Tests/GameSystems/Physics/WorldPoolTests.cs`
- `CoreEssentials.Tests/GameSystems/Physics/PhysicsConfigTests.cs`

### Documentation Files (Need Updates)
- `docs/PhysicsSystem.md` - Current documentation, needs complete rewrite
- `docs/AdvancedTopics.md` - May reference physics APIs that change

---

## Next Steps

1. **Review this document** with the team to confirm approach
2. **Start Phase 1** - Create interface definitions first (least breaking)
3. **Test interfaces thoroughly** before implementing adapters
4. **Iterate on API design** based on feedback during implementation
5. **Document as you go** in `.github/memory.md` for project-specific learnings

---

*Last updated: 2026-07-13*  
*Author: AI Assistant - CoreEssentials Repository Refactoring Project*
