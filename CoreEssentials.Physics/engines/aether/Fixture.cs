using CoreEssentials.Physics.Types;
using Microsoft.Xna.Framework;
namespace CoreEssentials.Physics.Engines.Aether;

/// <summary>
/// 🔒 Internal use only by PhysicsBody. Implements IFixture.
/// </summary>
public class Fixture : IFixture
{
    // TODO: Implement in Sprint 2 - wrapper around Aether.Fixture

    public void Dispose() { }

    public IShape Shape => throw new NotImplementedException();
    public bool IsActive => throw new NotImplementedException();
    public IPhysicsBody OwnerBody => throw new NotImplementedException();

    public void Activate() => throw new NotImplementedException();
    public void Deactivate() => throw new NotImplementedException();
}
