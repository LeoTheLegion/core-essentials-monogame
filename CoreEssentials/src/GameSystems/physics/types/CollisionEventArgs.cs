namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// An unordered pair of physics bodies that are currently in active contact.
/// Returned by <c>PhysicsEngine.GetActiveContacts()</c>.
/// </summary>
/// <param name="BodyA">The first body in the pair.</param>
/// <param name="BodyB">The second body in the pair.</param>
public record BodyContactPair(IPhysicsBody BodyA, IPhysicsBody BodyB);

/// <summary>
/// Arguments for body-level collision events.
/// </summary>
public record BodyCollisionEventArgs(IPhysicsBody BodyA, IPhysicsBody BodyB);

/// <summary>
/// Arguments for body-level separation events.
/// Fires once when the last contact pair between two bodies has separated.
/// </summary>
public record BodySeparationEventArgs(IPhysicsBody BodyA, IPhysicsBody BodyB);

/// <summary>
/// Arguments for collider-level collision events.
/// </summary>
public record ColliderCollisionEventArgs(ICollider ColliderA, ICollider ColliderB);

/// <summary>
/// Arguments for collider-level separation events.
/// Fires once per separated collider pair (independent of body-level OnSeparation).
/// </summary>
public record ColliderSeparationEventArgs(ICollider ColliderA, ICollider ColliderB);
