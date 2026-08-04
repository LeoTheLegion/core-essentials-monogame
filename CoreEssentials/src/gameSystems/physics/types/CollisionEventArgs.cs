namespace CoreEssentials.GameSystems.Physics.Types;

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
