using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace CoreEssentials.Physics.Adapters.Interfaces;

/// <summary>
/// Interface representing a physics fixture attached to a body.
/// A fixture defines the collision shape and sensor properties of an object.
/// This abstracts away the underlying Aether Physics2D Fixture implementation.
/// </summary>
public interface IFixtureAdapter
{
    /// <summary>
    /// Gets or sets the restitution (bounciness) value, from 0 to 1.
    /// Higher values indicate more bouncy collisions.
    /// </summary>
    float Restitution { get; set; }

    /// <summary>
    /// Gets or sets whether this fixture is a sensor (triggers collision events without physical response).
    /// Sensors do not participate in collision resolution.
    /// </summary>
    bool IsSensor { get; set; }

    /// <summary>
    /// Gets the underlying spatial shape of this fixture.
    /// </summary>
    ISpatialShapeAdapter Shape { get; }

    /// <summary>
    /// Gets or sets the friction coefficient, from 0 to 1.
    /// Higher values indicate more friction between colliding bodies.
    /// </summary>
    float Friction { get; set; }

    /// <summary>
    /// Attaches this fixture to a physics body.
    /// </summary>
    /// <param name="body">The body to attach this fixture to.</param>
    void Attach(IPhysicsBodyAdapter body);

    /// <summary>
    /// Removes this fixture from its attached body.
    /// </summary>
    void Detach();
}
