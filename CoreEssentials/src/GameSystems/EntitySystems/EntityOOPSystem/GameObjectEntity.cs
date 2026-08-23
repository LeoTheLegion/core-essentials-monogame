namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

/// <summary>
/// A basic, behavior-free entity — the equivalent of Unity's plain GameObject.
/// </summary>
/// <remarks>
/// <see cref="Entity"/> is abstract so that gameplay entities are forced to declare their
/// update/render responsibilities. When you need a pure container for components (or a node
/// in an XML-defined scene hierarchy) with no behavior of its own, use this class instead.
/// It inherits all default entity behavior: component updates and rendering are driven by the
/// attached components themselves, so a <see cref="GameObjectEntity"/> can be composed entirely
/// from built-in components (e.g. <c>CanvasComponent</c> + <c>LabelComponent</c>) with zero code.
/// </remarks>
public class GameObjectEntity : Entity
{
}
