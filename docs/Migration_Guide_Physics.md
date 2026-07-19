# Physics System Migration Guide

> **Important:** This guide helps you transition from the legacy Aether.Physics2D API to CoreEssentials' unified physics abstraction layer.

---

## Why Migrate?

The new physics system provides:

- **Unified interface**: `IPhysicsBody`, `ICollider`, etc. — no direct Aether dependency in your game code
- **Object pooling**: Bodies are recycled on destroy, reducing GC pressure
- **Type safety**: Strongly typed shapes and constraints via the factory pattern
- **Better architecture**: Separation of concerns between engine internals and user-facing API

---

## Quick Overview: Key Changes

| Old (Legacy) | New (Current) |
| --- | --- |
| `Aether.Physics2D.Dynamics.Body` | `CoreEssentials.GameSystems.Physics.Types.IPhysicsBody` |
| `Aether.World` | `CoreEssentials.GameSystems.Physics.Engines.Aether.PhysicsEngine` |
| Direct body creation via World | Via `PhysicsEngine.CreateDynamic()` / `CreateStatic()` / `CreateKinematic()` |
| No shape abstraction | Shape factory: `IPhysicsBody.CreateCircleCollider()`, etc. |

---

## 1. Body Creation

### Before (Legacy)

```csharp
// Direct Aether World access
var world = new World(Vector2.Zero);
var body = world.CreateBody(position, rotation: 0f, BodyType.Dynamic);
```

### After (Current)

```csharp
// Via PhysicsEngine GameSystem
PhysicsEngine physics = GetGameSystem<PhysicsEngine>();
IPhysicsBody body = physics.CreateDynamic(position); // or CreateStatic / CreateKinematic
```

---

## 2. Body Types

### Before (Legacy)

```csharp
body.BodyType = BodyType.Dynamic;
body.BodyType = BodyType.Static;
body.BodyType = BodyType.Kinematic;
```

### After (Current)

```csharp
// Body type is determined at creation time and cannot be changed.
IPhysicsBody dynamicBody   = physics.CreateDynamic(position);
IPhysicsBody staticBody    = physics.CreateStatic(position);
IPhysicsBody kinematicBody = physics.CreateKinematic(position);

// Check type via properties:
if (body.IsDynamic) { /* ... */ }
if (body.IsStatic)  { /* ... */ }
```

---

## 3. Adding Shapes / Colliders

### Before (Legacy)

```csharp
var circleShape = new CircleShape(world, radius);
var fixture = body.CreateFixture(circleShape);

var rectShape = PolygonShape.CreateRectangle(world, width, height);
fixture = body.CreateFixture(rectShape);
```

### After (Current)

```csharp
// Circle collider
ICollider circle = body.CreateCircleCollider(radius: 16f, offset: null);

// Rectangle collider
ICollider rect = body.CreateRectangleCollider(
    size: new Vector2(32f, 64f), offset: null
);

// Polygon collider (vertices in local space)
ICollider polygon = body.CreatePolygonCollider(
    new Vector2(-10, -10),
    new Vector2( 10, -10),
    new Vector2( 10,  10),
    new Vector2(-10,  10)
);

// Convex hull from arbitrary points
ICollider hull = body.CreateConvexHullCollider(
    new Vector2(  0, -20),
    new Vector2( 15,  10),
    new Vector2(-15,  10)
);
```

---

## 4. Physics Properties

### Before (Legacy)

```csharp
body.Friction = 0.5f;
body.Restitution = 0.7f;
body.FixedRotation = true;
body.Mass = 10f;
body.LinearDamping = 0.2f;
```

### After (Current)

```csharp
// Same properties, via IPhysicsBody interface
body.Friction      = 0.5f;   // 0 = slippery, 1 = sticky
body.Restitution   = 0.7f;   // 0 = no bounce, 1 = full bounce
body.FixedRotation = true;
body.Mass          = 10f;

// Note: LinearDamping / AngularDamping are NOT exposed via IPhysicsBody.
// Use SetLinearVelocity() and AngularVelocity property instead for velocity control.
```

---

## 5. Forces, Torque & Impulses

### Before (Legacy)

```csharp
body.ApplyForce(new Vector2(10f, 0f));
body.ApplyForce(new Vector2(10f, 0f), body.Position + new Vector2(0f, 1f));
body.ApplyLinearImpulse(new Vector2(5f, 0f));
```

### After (Current)

```csharp
// Force applied at center of mass (point-of-application not currently exposed)
body.ApplyForce(new Vector2(10f, 0f));

// Torque
body.ApplyTorque(5f);

// Impulse at center of mass
body.ApplyImpulse(new Vector2(5f, 0f));
```

---

## 6. Velocity Control

### Before (Legacy)

```csharp
body.LinearVelocity = new Vector2(5f, 0f);
body.AngularVelocity = 2f;
```

### After (Current)

```csharp
// Get current velocity
Vector2 linearVel   = body.LinearVelocity;
float angularVel    = body.AngularVelocity;

// Set velocity
body.SetLinearVelocity(new Vector2(5f, 0f));
body.AngularVelocity = 2f; // Angular velocity is still a property
```

---

## 7. Collision Detection & Events

### Before (Legacy)

```csharp
// Set collision categories
body.CollisionCategories = Category.Cat1;
body.CollidesWith        = Category.Cat2 | Category.Cat3;

// Add event handlers
body.OnCollision += (bodyA, bodyB, contact) => {
    Console.WriteLine("Hit!");
    return true;
};
body.OnSeparation += (bodyA, bodyB) => {
    Console.WriteLine("Separated");
};
```

### After (Current)

```csharp
// Per-body collision events via IPhysicsBody:
body.OnCollision += args =>
{
    IPhysicsBody other = args.BodyB == body ? args.BodyA : args.BodyB;
    Console.WriteLine($"Collided with {other.Type}");
    return true; // Return false to reject the collision
};

body.OnSeparation += args =>
{
    IPhysicsBody other = args.BodyB == body ? args.BodyA : args.BodyB;
    Console.WriteLine($"Separated from {other.Type}");
};

// Collision categories can be filtered via the IPhysicsBody.Type property.
```

---

## 8. Joints / Constraints

### Before (Legacy)

```csharp
var jointDef = new RevoluteJointDefinition();
jointDef.Initialize(bodyA.Body, bodyB.Body, anchor);
jointDef.FrictionMotor = 0.5f;
world.CreateJoint(jointDef);
```

### After (Current)

```csharp
// Joints are created via the PhysicsEngine (factory pattern).
// Internally managed — access through IPhysicsWorld / IPhysicsFactory.

// Example: Revolute joint between two bodies
IRevoluteJoint revolute = physics.CreateRevoluteJoint(
    bodyA,
    bodyB, // may be null for single-body joints
    localAnchorA: new Vector2(0f, 0f),
    localAnchorB: new Vector2(5f, 0f)
);

revolute.MinAngle      = -MathF.PI;   // Angle limits
revolute.MaxAngle      = MathF.PI;
revolute.MotorEnabled  = true;
revolute.MotorSpeed    = 1.0f;
revolute.MaxMotorTorque = 10f;
```

> **Note:** The joint creation API may vary depending on your current sprint implementation. Refer to `IPhysicsFactory` for the exact method signatures.

---

## 9. Debug Rendering

### Before (Legacy)

```csharp
var debugRenderer = new DebugRenderer(graphicsDevice);
debugRenderer.Draw(world);
```

### After (Current)

```csharp
// Create via scene's LoadGameSystems()
PhysicsEngine physicsEngine   = new PhysicsEngine();
PhysicsDebugRenderer debugRenderer = new PhysicsDebugRenderer(physicsEngine);

return new GameSystem[] {
    physicsEngine,
    debugRenderer
};

// In your Update method:
debugRenderer.IsEnabled = true;  // Toggle with a key, e.g., F1
debugRenderer.Draw(spriteBatch);
```

---

## 10. World Management

### Before (Legacy)

```csharp
var world = new World(gravity);
world.Step(deltaTime);
world.Clear(); // Remove all bodies and joints
```

### After (Current)

```csharp
// Gravity is set on PhysicsEngine directly
PhysicsEngine physics = GetGameSystem<PhysicsEngine>();
physics.Gravity = Vector2.Zero;  // or any custom vector

// Stepping is automatic — PhysicsEngine implements IFixedUpdateGameSystem
// No manual Step() call needed!

// Clear all bodies (recycles them into pool)
physics.ClearAllBodies();

// Get all current bodies
IReadOnlyList<IPhysicsBody> bodies = physics.GetBodies();
```

---

## Complete Example: Ball Entity

### Before (Legacy)

```csharp
public class Ball : Entity {
    public Body Body { get; private set; }
    
    public override void Initialize() {
        var physics = Scene.GetGameSystem<PhysicsEngine>();
        Body = physics.CreateCircle(Position, 16f, 1f);
        Body.BodyType = BodyType.Dynamic;
        Body.Restitution = 0.8f;
        Body.Friction = 0.2f;
    }
    
    public override void Update(GameTime gameTime) {
        Position = Body.Position;
        Rotation = Body.Rotation;
    }
}
```

### After (Current)

```csharp
public class Ball : Entity {
    private IPhysicsBody _body; // Use interface, not Aether Body
    
    public override void Initialize() {
        var physics = Scene.GetGameSystem<PhysicsEngine>();
        
        // Create body via engine factory
        _body = physics.CreateDynamic(Position);
        
        // Add a circle collider
        _body.CreateCircleCollider(radius: 16f, offset: null);
        
        // Set material properties
        _body.Restitution = 0.8f;
        _body.Friction    = 0.2f;
    }
    
    public override void Update(GameTime gameTime) {
        Position = _body.WorldPosition;  // Use WorldPosition, not Position
        Rotation = _body.Rotation;
    }
}
```

---

## Common Pitfalls

| Issue | Cause | Fix |
| --- | --- | --- |
| `ObjectDisposedException` when creating body | Calling after world is disposed | Check `_disposed` or ensure engine is alive |
| Cannot create body during step | World is locked while stepping | Create bodies outside of FixedUpdate() |
| Body not responding to forces | Body created as Static type | Use `CreateDynamic()` for physics-driven bodies |
| Collider returns null from TestPoint | Point query failed / no fixtures at that location | Check point coordinates are in world space |

---

*Generated: 2026-07-18 | Part of Physics System Refactoring Project*
