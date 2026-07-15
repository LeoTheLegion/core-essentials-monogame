using System;
using CoreEssentials.Physics.Adapters.Interfaces;
using CoreEssentials.Physics.Adapters.Implementations.ShapeAdapters;

namespace CoreEssentials.Physics.Adapters.Implementations;

/// <summary>
/// Adapter that wraps Aether's Fixture, implementing IFixtureAdapter.
/// INTERNAL USE ONLY - Not exposed to users directly.
/// Fixtures define collision shapes and properties attached to a physics body.
/// </summary>
internal class FixtureAdapter : IFixtureAdapter
{
    private readonly nkast.Aether.Physics2D.Dynamics.Fixture _fixture;

    /// <summary>
    /// Creates a new fixture adapter wrapping the specified Aether Fixture.
    /// </summary>
    public FixtureAdapter(nkast.Aether.Physics2D.Dynamics.Fixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    /// <inheritdoc />
    public float Restitution
    {
        get => _fixture.Restitution;
        set => _fixture.Restitution = value;
    }

    /// <inheritdoc />
    public bool IsSensor
    {
        get => _fixture.IsSensor;
        set => _fixture.IsSensor = value;
    }

    /// <inheritdoc />
    public ISpatialShapeAdapter Shape
    {
        get
        {
            return _fixture.Shape switch
            {
                nkast.Aether.Physics2D.Collision.Shapes.CircleShape circle => 
                    new CircleShapeAdapter(circle),
                nkast.Aether.Physics2D.Collision.Shapes.PolygonShape polygon => 
                    new PolygonShapeAdapter(polygon),
                var shape when shape is nkast.Aether.Physics2D.Collision.Shapes.Shape => 
                    throw new NotSupportedException($"Unsupported shape type: {shape.GetType().Name}"),
                _ => throw new InvalidOperationException("Fixture has no valid shape")
            };
        }
    }

    /// <inheritdoc />
    public float Friction
    {
        get => _fixture.Friction;
        set => _fixture.Friction = value;
    }

    /// <inheritdoc />
    public float Density
    {
        get => _fixture.Shape.Density;
        set => _fixture.Shape.Density = value;
    }

    /// <inheritdoc />
    public void Attach(IPhysicsBodyAdapter body)
    {
        // In Aether, fixtures are created attached to a body via Body.CreateFixture() etc.
        // This method is not typically used since the fixture should already be attached.
        throw new NotImplementedException("Fixtures are attached during creation in Aether Physics2D");
    }

    /// <inheritdoc />
    public void Detach()
    {
        throw new NotImplementedException("Fixtures are detached by removing them from their parent body in Aether Physics2D");
    }

    /// <summary>
    /// Destroys this fixture from its parent body. Used internally by PhysicsBodyAdapter.
    /// </summary>
    internal void Destroy()
    {
        if (_fixture.Body != null)
        {
            _fixture.Body.Remove(_fixture);
        }
    }

    public void Dispose()
    {
        // Fixtures are managed by their parent Body and don't need explicit disposal.
        // If the body is destroyed, all fixtures are cleaned up automatically.
    }
}
