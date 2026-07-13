using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Enum representing the type of physics body.
/// Mirrors Aether Physics2D BodyType for compatibility.
/// </summary>
public enum BodyTypeEnum
{
    /// <summary>A static body that never moves and has infinite mass.</summary>
    Static,

    /// <summary>A dynamic body that responds to forces and collisions.</summary>
    Dynamic,

    /// <summary>A kinematic body that moves without being affected by physics.</summary>
    Kinematic
}

/// <summary>
/// Interface representing a physics body in the simulation.
/// This abstracts away the underlying Aether Physics2D Body implementation,
/// allowing for future engine swapping without breaking user code.
/// </summary>
public interface IPhysicsBodyAdapter : IDisposable
{
    /// <summary>
    /// Gets or sets the position of the body in world space.
    /// </summary>
    Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets the rotation angle of the body in radians.
    /// </summary>
    float Rotation { get; set; }

    /// <summary>
    /// Gets the type of physics body (static, dynamic, or kinematic).
    /// </summary>
    BodyTypeEnum BodyType { get; }

    /// <summary>
    /// Gets or sets the mass of the body.
    /// For static bodies, this value is ignored.
    /// </summary>
    float Mass { get; set; }

    /// <summary>
    /// Gets a collection of all fixtures attached to this body.
    /// </summary>
    IEnumerable<IFixtureAdapter> Fixtures { get; }

    /// <summary>
    /// Creates a circular fixture on this body.
    /// </summary>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="density">Mass per unit area (0 = infinite mass).</param>
    /// <returns>The created fixture adapter.</returns>
    IFixtureAdapter CreateCircle(float radius, float density);

    /// <summary>
    /// Creates a rectangular fixture on this body.
    /// </summary>
    /// <param name="width">Width of the rectangle along local X axis.</param>
    /// <param name="height">Height of the rectangle along local Y axis.</param>
    /// <param name="density">Mass per unit area (0 = infinite mass).</param>
    /// <param name="localCenter">Local offset of the center from body origin.</param>
    /// <returns>The created fixture adapter.</returns>
    IFixtureAdapter CreateRectangle(float width, float height, float density, Vector2 localCenter);

    /// <summary>
    /// Enables or disables this body in the physics simulation.
    /// Disabled bodies do not participate in collision detection.
    /// </summary>
    void Enable();

    /// <summary>
    /// Disables and removes this body from the physics simulation.
    /// The body will be returned to the pool for reuse.
    /// </summary>
    void Disable();

    /// <summary>
    /// Checks if the body is currently enabled in the simulation.
    /// </summary>
    bool IsEnabled { get; }
}
