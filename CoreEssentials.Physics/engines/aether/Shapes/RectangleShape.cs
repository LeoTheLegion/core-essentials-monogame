using CoreEssentials.Physics.Types;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Physics.Engines.Aether.Shapes;

/// <summary>
/// 🔒 Implements IShape, wraps Aether RectangleShape.
/// </summary>
public class RectangleShape : IShape
{
    // TODO: Implement in Sprint 3 - wrapper around Aether RectangleShape

    public void Dispose() { }

    public Vector2 Center => throw new NotImplementedException();
    public float Radius => throw new NotImplementedException();
    public IReadOnlyList<Vector2> Vertices => throw new NotImplementedException();

    public void Translate(Vector2 offset) => throw new NotImplementedException();
    public void Rotate(float angleRadians) => throw new NotImplementedException();
    public bool PointContains(Vector2 point) => throw new NotImplementedException();
    public ShapeType GetShapeType() => throw new NotImplementedException();
}
