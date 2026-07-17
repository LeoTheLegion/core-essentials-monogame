using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// 🔒 Internal use only by PhysicsBody. Not exposed to end users.
/// </summary>
public interface IFixture : IDisposable
{
    /// <summary>
    /// Gets the shape associated with this fixture.
    /// </summary>
    IShape? Shape { get; }

    /// <summary>
    /// Gets whether this fixture is currently active in the simulation.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets the body that owns this fixture.
    /// </summary>
    IPhysicsBody OwnerBody { get; }

    /// <summary>
    /// Gets or sets the friction coefficient (0 = slippery, 1 = sticky).
    /// </summary>
    float Friction { get; set; }

    /// <summary>
    /// Gets or sets the restitution/bounciness (0 = no bounce, 1 = full bounce).
    /// </summary>
    float Restitution { get; set; }

    /// <summary>
    /// Activates this fixture so it participates in collision detection.
    /// </summary>
    void Activate();

    /// <summary>
    /// Deactivates this fixture so it no longer participates in collision detection.
    /// </summary>
    void Deactivate();
}
