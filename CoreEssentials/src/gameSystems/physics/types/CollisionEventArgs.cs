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
