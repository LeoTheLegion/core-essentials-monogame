using System;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using Microsoft.Xna.Framework;
#nullable enable
using Xunit;

namespace CoreEssentials.Tests.GameSystems.Physics;

/// <summary>
/// End-to-end regression tests for offset colliders (rectangle / polygon). These assert that a
/// collider created with a local <c>offset</c> SIMULATES at that offset — not just queries as if it
/// were. PhysicsEngine.TestPoint reads Aether's real fixture geometry, so these would fail if the
/// offset were only tracked wrapper-side and never baked into the fixture.
/// </summary>
public class OffsetColliderSimulationTests : IDisposable
{
    private readonly PhysicsEngine _engine;
    private bool _disposed;

    public OffsetColliderSimulationTests() => _engine = new PhysicsEngine(Vector2.Zero);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _engine.Dispose();
        }
        _disposed = true;
    }

    [Fact]
    public void OffsetRectangle_SimulatesAtOffset_NotOrigin()
    {
        var body = _engine.CreateDynamic(Vector2.Zero);
        // 4x2 rect (half-extents x:2, y:1) shifted to be centered at local (10, 0).
        var collider = body.CreateRectangleCollider(new Vector2(4f, 2f), offset: new Vector2(10f, 0f));

        // The OFFSET center must be inside the simulated fixture (same underlying Aether fixture as
        // the one we created — TestPoint returns a fresh wrapper, so compare the Aether fixture).
        var hitOffset = _engine.TestPoint(new Vector2(10f, 0f)) as CoreEssentials.GameSystems.Physics.Engines.Aether.Collider;
        Assert.NotNull(hitOffset);
        Assert.Same(((CoreEssentials.GameSystems.Physics.Engines.Aether.Collider)collider)._aetherFixture, hitOffset._aetherFixture);

        // ...and the body ORIGIN must no longer be covered by it (pre-fix: the fixture sat here).
        Assert.Null(_engine.TestPoint(Vector2.Zero));
    }

    [Fact]
    public void OffsetRectangle_SimulationAgreesWithQuery()
    {
        var body = _engine.CreateDynamic(Vector2.Zero);
        var collider = body.CreateRectangleCollider(new Vector2(4f, 2f), offset: new Vector2(10f, 0f));

        // Query side (wrapper IShape) and simulation side (Aether fixture via TestPoint) must agree.
        Assert.NotNull(collider.Shape);
        var shape = collider.Shape;
        Assert.True(shape.PointContains(new Vector2(10f, 0f)));   // local offset center: inside
        Assert.False(shape.PointContains(Vector2.Zero));          // origin: outside

        // A point just past the far edge (x=12+eps) is outside in BOTH senses.
        Assert.Null(_engine.TestPoint(new Vector2(12.5f, 0f)));
    }

    [Fact]
    public void OffsetCircle_SimulatesAtOffset_NotOrigin()
    {
        // Circles already baked their offset before this fix — kept as a guard so both shape
        // families stay consistent with each other.
        var body = _engine.CreateDynamic(Vector2.Zero);
        var collider = body.CreateCircleCollider(radius: 1f, offset: new Vector2(0f, 5f));

        var hitOffset = _engine.TestPoint(new Vector2(0f, 5f)) as CoreEssentials.GameSystems.Physics.Engines.Aether.Collider;
        Assert.NotNull(hitOffset);                                       // offset center covered
        Assert.Same(((CoreEssentials.GameSystems.Physics.Engines.Aether.Collider)collider)._aetherFixture, hitOffset._aetherFixture);
        Assert.Null(_engine.TestPoint(Vector2.Zero));                    // origin no longer covered
    }

    [Fact]
    public void RotatedRectangle_QueriesConsistently()
    {
        var body = _engine.CreateDynamic(Vector2.Zero);
        var collider = body.CreateRectangleCollider(new Vector2(4f, 2f), offset: Vector2.Zero);
        Assert.NotNull(collider.Shape);
        var shape = collider.Shape;

        // Rotating bakes into the geometry, so Center and containment reflect the new orientation.
        shape.Rotate((float)Math.PI / 2f); // swap effective half-extents (x:1, y:2)

        Assert.True(shape.PointContains(new Vector2(0f, 1.5f)));   // within rotated y-half (~2)
        Assert.False(shape.PointContains(new Vector2(1.5f, 0f)));  // beyond rotated x-half (~1)
    }
}
