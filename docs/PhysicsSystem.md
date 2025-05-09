# Physics System

The Physics System in CoreEssentials-MonoGame provides 2D physics simulation capabilities using Aether.Physics2D integration. This system handles collision detection, rigid body dynamics, and physics-based movement.

## Key Components

### PhysicsEngine

The `PhysicsEngine` class is a `GameSystem` that manages the physics world and all physics bodies.

```csharp
// Get the PhysicsEngine from a scene
PhysicsEngine physicsEngine = GetGameSystem<PhysicsEngine>();

// Create a circle physics body
Body circleBody = physicsEngine.CreateCircle(position, radius, density);

// Create a rectangle physics body
Body rectangleBody = physicsEngine.CreateRectangle(position, width, height, density);

// Create a polygon physics body
Body polygonBody = physicsEngine.CreatePolygon(position, vertices, density);
```

### Body Types

Aether.Physics2D provides three body types:

- **Static**: Bodies that don't move (e.g., terrain, walls)
- **Kinematic**: Bodies that move but aren't affected by forces
- **Dynamic**: Bodies that are fully physics-driven

```csharp
// Set the body type
body.BodyType = BodyType.Dynamic;  // Physics-driven
body.BodyType = BodyType.Static;   // Immovable
body.BodyType = BodyType.Kinematic; // Manually controlled
```

### PhysicsDebugRenderer

The `PhysicsDebugRenderer` allows visualization of physics bodies for debugging:

```csharp
// Create a physics debug renderer (in your scene's LoadGameSystems method)
PhysicsEngine physicsEngine = new PhysicsEngine();
PhysicsDebugRenderer debugRenderer = new PhysicsDebugRenderer(physicsEngine);

return new GameSystem[]
{
    physicsEngine,
    debugRenderer
};
```

## Physics Properties

Control various physics properties of bodies:

```csharp
// Set friction (0 to 1)
body.Friction = 0.5f;

// Set restitution/bounciness (0 to 1)
body.Restitution = 0.7f;

// Set linear damping
body.LinearDamping = 0.2f;

// Set angular damping
body.AngularDamping = 0.1f;

// Prevent rotation
body.FixedRotation = true;

// Set mass
body.Mass = 10f;
```

## Forces and Movement

Apply forces and impulses to dynamic bodies:

```csharp
// Apply force at the center of the body
body.ApplyForce(new Vector2(10f, 0f));

// Apply force at a specific point
body.ApplyForce(new Vector2(10f, 0f), body.Position + new Vector2(0f, 1f));

// Apply immediate impulse
body.ApplyLinearImpulse(new Vector2(5f, 0f));

// Set linear velocity directly
body.LinearVelocity = new Vector2(5f, 0f);

// Set angular velocity (in radians per second)
body.AngularVelocity = 2f;
```

## Collision Detection

Set up collision categories and detection:

```csharp
// Set collision categories
body.CollisionCategories = Category.Cat1;
body.CollidesWith = Category.Cat2 | Category.Cat3;

// Add collision event handlers
body.OnCollision += (bodyA, bodyB, contact) =>
{
    // Called at the start of a collision
    Console.WriteLine("Collision started!");
    return true; // Return true to allow the collision
};

body.OnSeparation += (bodyA, bodyB) =>
{
    // Called when bodies separate
    Console.WriteLine("Collision ended!");
};
```

## Example from Playground

The `PhysicsEntityScene` demonstrates physics integration:

```csharp
// Ball.cs - Example entity with physics
public class Ball : Entity
{
    public Body Body { get; private set; }
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Get the physics engine
        PhysicsEngine physics = Scene.GetGameSystem<PhysicsEngine>();
        
        // Create a circle physics body
        Body = physics.CreateCircle(Position, 16f, 1f);
        Body.BodyType = BodyType.Dynamic;
        Body.Restitution = 0.8f;
        Body.Friction = 0.2f;
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update entity position based on physics body
        Position = Body.Position;
        Rotation = Body.Rotation;
    }
}

// WorldBorder.cs - Example static physics boundaries
public class WorldBorder : Entity
{
    private Body[] _bodies;
    
    public WorldBorder(Vector2 topLeft, Vector2 bottomRight)
    {
        // Create world boundaries
        PhysicsEngine physics = Scene.GetGameSystem<PhysicsEngine>();
        
        _bodies = new Body[4];
        
        // Top wall
        _bodies[0] = physics.CreateRectangle(
            new Vector2((bottomRight.X - topLeft.X) / 2, topLeft.Y - 10), 
            bottomRight.X - topLeft.X, 20, 0);
            
        // ...similar code for other walls...
        
        // Make all bodies static
        foreach (var body in _bodies)
        {
            body.BodyType = BodyType.Static;
            body.Restitution = 0.8f;
        }
    }
}
```

## Best Practices

- Use the correct body type for each entity's purpose
- Clean up physics bodies when destroying entities
- Use physics debug rendering during development
- Tune friction and restitution values for realistic behavior
- Consider using Categories for collision filtering
- Synchronize entity positions with their physics bodies
- Apply forces at appropriate points on bodies for realistic behavior