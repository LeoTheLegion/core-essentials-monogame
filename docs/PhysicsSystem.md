# Physics System ⭐

The Physics System in CoreEssentials-MonoGame provides a clean, abstraction-layer API over [Aether.Physics2D](https://github.com/nkastnen/AetherPhysics2D). It handles collision detection, rigid body dynamics, and physics-based movement while keeping your game code decoupled from the underlying physics engine.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│              Your Game Code                     │
│  (Entity, Scene, etc.)                          │
└──────────────────┬──────────────────────────────┘
                   │ uses interfaces
                   ▼
┌─────────────────────────────────────────────────┐
│         Public API Layer (Types)                │
│  IPhysicsBody | ICollider | IShape              │
│  IConstraint | IRevoluteJoint | IWeldJoint      │
│  IDistanceJoint | IPhysicsWorld                 │
│  IPhysicsFactory | ISpatialShapeFactory         │
└──────────────────┬──────────────────────────────┘
                   │ implemented by
                   ▼
┌─────────────────────────────────────────────────┐
│      Aether Engine Implementation               │
│  PhysicsEngine (GameSystem)                     │
│  PhysicsBody / Collider                         │
│  CircleShape / PolygonShape / RectangleShape    │
│  RevoluteJoint / WeldJoint / DistanceJoint      │
└──────────────────┬──────────────────────────────┘
                   │ wraps
                   ▼
┌─────────────────────────────────────────────────┐
│         Aether.Physics2D (Aether)               │
│  World | Body | Fixture | Joint                 │
└─────────────────────────────────────────────────┘
```

### Key Design Decisions

- **⭐ User-facing API**: `IPhysicsBody` is the only interface users should interact with directly.
- **🔒 Internal interfaces**: `ICollider`, `IShape`, `IConstraint`, joint types, and factory interfaces are internal use — exposed for advanced scenarios but not recommended for direct dependency.
- **Object pooling**: Bodies are recycled via an internal pool on destroy, reducing GC pressure during frequent create/destroy cycles.

---

## Getting Started

### 1. Register the PhysicsEngine in Your Scene

```csharp
protected override GameSystem[] LoadGameSystems()
{
    var physicsEngine = new PhysicsEngine(); // Default gravity: (0, -9.81)
    
    // Or with custom gravity:
    // var physicsEngine = new PhysicsEngine(Vector2.Zero);
    
    var debugRenderer = new PhysicsDebugRenderer(physicsEngine);

    return new GameSystem[]
    {
        physicsEngine,
        debugRenderer,
        // ... other systems
    };
}
```

### 2. Create Bodies

```csharp
PhysicsEngine physics = GetGameSystem<PhysicsEngine>();

// Dynamic body — affected by forces and collisions
IPhysicsBody dynamicBody = physics.CreateDynamic(position);

// Static body — immovable, infinite mass (e.g., terrain)
IPhysicsBody staticBody = physics.CreateStatic(position);

// Kinematic body — user-controlled, pushes other bodies
IPhysicsBody kinematicBody = physics.CreateKinematic(position);
```

### 3. Add Colliders to Bodies

```csharp
// Circle collider (radius in world units)
ICollider circle = dynamicBody.CreateCircleCollider(radius: 16f, offset: null);

// Rectangle collider (width × height)
ICollider rect = dynamicBody.CreateRectangleCollider(
    size: new Vector2(32f, 64f), 
    offset: null
);

// Polygon collider (vertices must be in local space, counter-clockwise order)
ICollider polygon = dynamicBody.CreatePolygonCollider(
    new Vector2(-10, -10),
    new Vector2(10, -10),
    new Vector2(10, 10),
    new Vector2(-10, 10)
);

// Convex hull from arbitrary points
ICollider hull = dynamicBody.CreateConvexHullCollider(
    new Vector2(0, -20),
    new Vector2(15, 10),
    new Vector2(-15, 10)
);
```

---

## Body Properties

### Position & Rotation
```csharp
Vector2 worldPos = body.WorldPosition;
float rotation = body.Rotation;
body.Rotation = MathF.PI / 4; // Set rotation in radians
```

### Type Information
```csharp
bool isStatic   = body.IsStatic;
bool isDynamic  = body.IsDynamic;
bool isKinematic = body.IsKinematic;
string? type    = body.Type;        // Custom identifier for filtering
body.Type = "player";               // Set for collision categorization
```

### Material Properties
```csharp
body.Friction       = 0.5f;   // 0 = slippery, 1 = sticky
body.Restitution    = 0.7f;   // 0 = no bounce, 1 = full bounce
body.FixedRotation  = true;   // Prevent rotation
body.Mass           = 10f;    // In kilograms (0 for static)
```

### Velocity Control
```csharp
Vector2 linearVel = body.LinearVelocity;
body.SetLinearVelocity(new Vector2(5f, 0f));

float angularVel = body.AngularVelocity;
body.AngularVelocity = 2f; // Radians per second
```

### Body State
```csharp
bool isAwake   = body.IsAwake;
bool isActive  = body.IsActive;
body.IsActive  = false;     // Deactivate (paused)
body.StopAll();             // Reset all velocity
```

---

## Forces, Torque & Impulses

```csharp
// Apply force at center of mass
body.ApplyForce(new Vector2(10f, 0f));

// Apply torque (rotational force)
body.ApplyTorque(5f);

// Apply impulse (instant velocity change)
body.ApplyImpulse(new Vector2(5f, 0f));
```

> **Note:** Point-of-application for forces is not currently exposed through the abstraction layer.

---

## Fixture Management

```csharp
// Get all colliders on a body
IReadOnlyList<ICollider> colliders = body.Colliders;

// Add/remove individual colliders
body.AddCollider(collider);
body.RemoveCollider(collider);

// Collider properties
float friction    = collider.Friction;
collider.Friction = 0.8f;

collider.Activate();   // Enable collision detection
collider.Deactivate(); // Disable collision detection
```

---

## World Management

```csharp
PhysicsEngine physics = GetGameSystem<PhysicsEngine>();

// Gravity
physics.Gravity = Vector2.Zero;         // No gravity
physics.Gravity = new Vector2(0, -9.81f); // Default

// Solver settings (tune for accuracy vs performance)
physics.VelocityIterations  = 8;
physics.PositionIterations  = 3;

// Query
IReadOnlyList<IPhysicsBody> bodies = physics.GetBodies();
ICollider? hitCollider = physics.TestPoint(worldSpacePoint);

// Clear all bodies (recycles into pool)
physics.ClearAllBodies();

// Destroy a single body (recycles into pool)
physics.Destroy(body);
```

---

## Joints / Constraints 🔒

> ⚠️ Joint interfaces are internal use — exposed for advanced scenarios.

### Revolute Joint (Hinge)
```csharp
IRevoluteJoint revolute = physics.CreateRevoluteJoint(
    bodyA: dynamicBody,
    bodyB: otherBody,   // null for single-body joints
    localAnchorA: Vector2.Zero,
    localAnchorB: new Vector2(5f, 0f)
);

revolute.MinAngle     = -MathF.PI;   // Angle limits
revolute.MaxAngle     = MathF.PI;
revolute.MotorEnabled = true;
revolute.MotorSpeed   = 1.0f;
revolute.MaxMotorTorque = 10f;
```

### Weld Joint (Rigid Connection)
```csharp
IWeldJoint weld = physics.CreateWeldJoint(
    bodyA: dynamicBody,
    bodyB: otherBody,
    localAnchorA: Vector2.Zero,
    localAnchorB: new Vector2(5f, 0f)
);

weld.Stiffness    = 1.0f;   // 0 = soft, 1 = rigid
weld.Damping      = 0.0f;
weld.CollideConnected = false;
```

### Distance Joint (Spring)
```csharp
IDistanceJoint distance = physics.CreateDistanceJoint(
    bodyA: dynamicBody,
    bodyB: otherBody,
    localAnchorA: Vector2.Zero,
    localAnchorB: new Vector2(10f, 0f)
);

distance.Length         = 10f;
distance.MaxForce       = 100f;
distance.FrequencyHz    = 5f;   // >0 for springy behavior
distance.DampingRatio   = 0.7f;
distance.CollideConnected = false;
```

---

## Debug Rendering

The `PhysicsDebugRenderer` visualizes all physics bodies and their colliders:

```csharp
// Toggle in your Update method (e.g., press F1)
if (Keyboard.GetState().IsKeyDown(Keys.F1))
{
    debugRenderer.IsEnabled = !debugRenderer.IsEnabled;
}

// Draw during rendering
debugRenderer.Draw(spriteBatch);
```

---

## Complete Example: Ball Entity

```csharp
public class Ball : Entity
{
    private IPhysicsBody _body;   // Use interface, not Aether Body directly

    public override void Initialize()
    {
        base.Initialize();
        
        PhysicsEngine physics = Scene.GetGameSystem<PhysicsEngine>();
        
        // Create dynamic body with circle collider
        _body = physics.CreateDynamic(Position);
        _body.CreateCircleCollider(radius: 16f, offset: null);
        
        // Set material properties
        _body.Restitution = 0.8f;   // Bouncy
        _body.Friction    = 0.2f;   // Slightly slippery
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Sync entity position with physics body
        Position = _body.WorldPosition;
        Rotation = _body.Rotation;
    }
}
```

---

## Collision Events 🔔

Per-body collision and separation events let you react when bodies make or break contact:

```csharp
IPhysicsBody player = physics.CreateDynamic(new Vector2(100, 50));
player.CreateCircleCollider(radius: 16f);

// OnCollision fires once per new contact; return true to allow, false to reject.
player.OnCollision += args =>
{
    // args.BodyA and args.BodyB are the two colliding bodies.
    IPhysicsBody other = args.BodyB == player ? args.BodyA : args.BodyB;

    if (other.Type == "enemy")
        DebugLog("Hit an enemy!");

    return true; // Allow collision (return false to reject)
};

// OnSeparation fires once when the last contact between two bodies ends.
player.OnSeparation += args =>
{
    IPhysicsBody other = args.BodyB == player ? args.BodyA : args.BodyB;
    DebugLog($"Lost contact with {other.Type}");
};
```

**Key behaviors:**
- Events fire on the game thread (via `FixedUpdate`), no threading concerns.
- Returning `false` from an `OnCollision` handler disables the contact, rejecting the collision.
- If a body has multiple colliders touching another body, `OnSeparation` fires only after **all** contacts break.
- Events are safe to subscribe/unsubscribe at any time; disposing a body while contacts are active will not throw.

---

## Best Practices

1. **Use interfaces** — Depend on `IPhysicsBody`, not concrete implementations.
2. **Pick the right body type** — Static for terrain, Kinematic for moving platforms, Dynamic for everything else.
3. **Pool awareness** — Bodies are recycled; don't hold references to destroyed bodies across frames.
4. **Tune solver iterations** — Higher values (10–15) improve accuracy but cost CPU; lower values (3–6) improve performance.
5. **Disable unused colliders** — Use `collider.Deactivate()` instead of removing fixtures when toggling collision on/off.
6. **Debug render during development** — Enable `PhysicsDebugRenderer` to visualize shapes and detect issues early.
7. **Synchronize carefully** — Only update entity position from physics in `Update()`, never modify physics body position directly for dynamic bodies.

---

## Migration Guide

Migrating from the legacy Aether API? See [Migration_Guide_Physics.md](./Migration_Guide_Physics.md) for side-by-side examples covering:
- Body creation
- Shape/collider addition
- Joint constraints
- Collision handling
- World management