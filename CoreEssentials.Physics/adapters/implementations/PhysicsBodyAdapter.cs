using System;
using System.Collections.Generic;
using CoreEssentials.Physics.Adapters.Interfaces;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Implementations;

/// <summary>
/// Adapter that wraps Aether's Body, implementing IPhysicsBodyAdapter.
/// This is the ONLY user-facing adapter - all other adapters are internal.
/// </summary>
public class PhysicsBodyAdapter : IPhysicsBodyAdapter
{
    private readonly nkast.Aether.Physics2D.Dynamics.Body _body;

    public PhysicsBodyAdapter(nkast.Aether.Physics2D.Dynamics.Body body)
    {
        _body = body ?? throw new ArgumentNullException(nameof(body));
    }

    /// <inheritdoc />
    public Vector2 Position
    {
        get => _body.Position;
        set => _body.Position = value;
    }

    /// <inheritdoc />
    public float Rotation
    {
        get => _body.Rotation;
        set => _body.Rotation = value;
    }

    /// <inheritdoc />
    public BodyTypeEnum BodyType
    {
        get => _body.BodyType switch
        {
            nkast.Aether.Physics2D.Dynamics.BodyType.Static => BodyTypeEnum.Static,
            nkast.Aether.Physics2D.Dynamics.BodyType.Dynamic => BodyTypeEnum.Dynamic,
            nkast.Aether.Physics2D.Dynamics.BodyType.Kinematic => BodyTypeEnum.Kinematic,
            _ => throw new ArgumentException($"Unknown body type: {_body.BodyType}", nameof(_body))
        };
    }

    /// <inheritdoc />
    public float Mass
    {
        get => _body.Mass;
        set => _body.Mass = value;
    }

    /// <inheritdoc />
    public IEnumerable<IFixtureAdapter> Fixtures
    {
        get
        {
            foreach (var fixture in _body.FixtureList)
            {
                yield return new FixtureAdapter(fixture);
            }
        }
    }

    /// <inheritdoc />
    public bool IsEnabled => _body.Enabled;

    /// <inheritdoc />
    public void Enable()
    {
        _body.Enabled = true;
    }

    /// <inheritdoc />
    public void Disable()
    {
        _body.Enabled = false;
    }

    /// <summary>
    /// Gets the underlying Aether Body instance. Used internally by other adapters.
    /// </summary>
    internal nkast.Aether.Physics2D.Dynamics.Body Body => _body;

    public void Dispose() { }

    /// <inheritdoc />
    public IFixtureAdapter CreateCircle(float radius, float density)
    {
        if (radius <= 0f)
            throw new ArgumentException($"Radius must be greater than 0. Got: {radius}", nameof(radius));

        var shape = _body.CreateCircle(radius, density);
        return new FixtureAdapter(shape);
    }

    /// <inheritdoc />
    public IFixtureAdapter CreateRectangle(float width, float height, float density)
    {
        return CreateRectangle(width, height, density, Vector2.Zero);
    }

    public IFixtureAdapter CreateRectangle(float width, float height, float density, Vector2 localCenter)
    {
        if (width <= 0f || height <= 0f)
            throw new ArgumentException($"Width and height must be greater than 0. Got: width={width}, height={height}", nameof(width));

        var vertices = new nkast.Aether.Physics2D.Common.Vertices(4)
        {
            new Vector2(-width / 2f, -height / 2f) + localCenter,
            new Vector2(width / 2f, -height / 2f) + localCenter,
            new Vector2(width / 2f, height / 2f) + localCenter,
            new Vector2(-width / 2f, height / 2f) + localCenter
        };

        var shape = _body.CreatePolygon(vertices, density);
        return new FixtureAdapter(shape);
    }

}
