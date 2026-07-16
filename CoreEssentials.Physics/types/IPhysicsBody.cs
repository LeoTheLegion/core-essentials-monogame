using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Types;

/// <summary>
/// ⭐ User-facing interface for physics objects in the world.
/// This is the ONLY interface exposed to users — everything else should be marked internal or Obsolete.
/// </summary>
public interface IPhysicsBody : IDisposable
{
    // ─── Position & Rotation ─────────────────────────────────────────────

    /// <summary>
    /// Gets the current world position of this body.
    /// </summary>
    Vector2 WorldPosition { get; }

    /// <summary>
    /// Gets or sets the rotation of this body in radians.
    /// </summary>
    float Rotation { get; set; }

    // ─── Type & Category ────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the type identifier for this body (used for collision filtering).
    /// </summary>
    string? Type { get; set; }

    /// <summary>
    /// Gets whether this body is static (immovable, infinite mass).
    /// </summary>
    bool IsStatic { get; }

    /// <summary>
    /// Gets whether this body is dynamic (affected by forces and collisions).
    /// </summary>
    bool IsDynamic { get; }

    /// <summary>
    /// Gets whether this body is kinematic (user-controlled, infinite mass for collisions).
    /// </summary>
    bool IsKinematic { get; }

    // ─── Shape Creation ─────────────────────────────────────────────────

    /// <summary>
    /// Creates and adds a circle shape fixture to this body.
    /// </summary>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="offset">Local offset from the body's center.</param>
    /// <returns>The created fixture (internal use only).</returns>
    IFixture CreateCircle(float radius, Vector2? offset = null);

    /// <summary>
    /// Creates and adds a rectangle shape fixture to this body.
    /// </summary>
    /// <param name="size">Width and height of the rectangle.</param>
    /// <param name="offset">Local offset from the body's center.</param>
    /// <returns>The created fixture (internal use only).</returns>
    IFixture CreateRectangle(Vector2 size, Vector2? offset = null);

    /// <summary>
    /// Creates and adds a polygon shape fixture to this body.
    /// </summary>
    /// <param name="vertices">Array of vertices in local space (counter-clockwise order).</param>
    /// <returns>The created fixture (internal use only).</returns>
    IFixture CreatePolygon(params Vector2[] vertices);

    /// <summary>
    /// Creates and adds a convex hull shape from the given points.
    /// </summary>
    /// <param name="points">Points to compute the convex hull from.</param>
    /// <returns>The created fixture (internal use only).</returns>
    IFixture CreateConvexHull(params Vector2[] points);

    // ─── Fixture Management ─────────────────────────────────────────────

    /// <summary>
    /// Adds a fixture to this body.
    /// </summary>
    void AddFixture(IFixture fixture);

    /// <summary>
    /// Removes a fixture from this body.
    /// </summary>
    void RemoveFixture(IFixture fixture);

    // ─── Material Properties ────────────────────────────────────────────

    /// <summary>
    /// Gets the mass of this body in kilograms (0 for static bodies).
    /// </summary>
    float Mass { get; }

    /// <summary>
    /// Gets the moment of inertia of this body (0 for static bodies).
    /// </summary>
    float Inertia { get; }

    /// <summary>
    /// Gets or sets the friction coefficient (0 = slippery, 1 = sticky).
    /// </summary>
    float Friction { get; set; }

    /// <summary>
    /// Gets or sets the restitution/bounciness (0 = no bounce, 1 = full bounce).
    /// </summary>
    float Restitution { get; set; }

    /// <summary>
    /// Gets or sets whether rotation is locked (true = fixed rotation).
    /// </summary>
    bool FixedRotation { get; set; }

    // ─── Forces, Torque & Impulses ──────────────────────────────────────

    /// <summary>
    /// Applies a force at the body's center of mass.
    /// </summary>
    /// <param name="force">Force vector in world space.</param>
    void ApplyForce(Vector2 force);

    /// <summary>
    /// Applies a torque (rotational force) to the body.
    /// </summary>
    /// <param name="torque">Torque value in Newton-meters.</param>
    void ApplyTorque(float torque);

    /// <summary>
    /// Applies an impulse at the body's center of mass.
    /// </summary>
    /// <param name="impulse">Impulse vector in world space.</param>
    void ApplyImpulse(Vector2 impulse);

    // ─── Velocity Control ───────────────────────────────────────────────

    /// <summary>
    /// Gets the current linear velocity.
    /// </summary>
    Vector2 LinearVelocity { get; }

    /// <summary>
    /// Sets the linear velocity directly (bypasses forces).
    /// </summary>
    void SetLinearVelocity(Vector2 linearVelocity);

    /// <summary>
    /// Gets or sets the current angular velocity in radians per second.
    /// </summary>
    float AngularVelocity { get; set; }

    // ─── Body State ─────────────────────────────────────────────────────

    /// <summary>
    /// Stops all motion (linear and angular velocity).
    /// </summary>
    void StopAll();

    /// <summary>
    /// Gets whether this body is currently awake (simulated).
    /// </summary>
    bool IsAwake { get; }

    /// <summary>
    /// Gets or sets whether this body is active in the simulation.
    /// Inactive bodies do not participate in physics.
    /// </summary>
    bool IsActive { get; set; }
}
