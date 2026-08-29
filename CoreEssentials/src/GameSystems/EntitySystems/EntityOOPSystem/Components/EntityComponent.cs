using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

/// <summary>
/// Base class for all entity components in the mixin-style component system.
/// Components provide composable behavior that can be attached to any Entity.
/// </summary>
public abstract class EntityComponent
{
    /// <summary>
    /// The Entity that owns this component.
    /// Set automatically when the component is attached to an entity.
    /// </summary>
    public Entity Owner { get; internal set; } = null!;

    /// <summary>
    /// Gets the EntitySystem managing the owning entity, or null if the owner has not been
    /// added to a system yet. Gives component code public access to spawn/destroy/query and
    /// SendMessage: <c>EntitySystem?.SendMessage("PlayerDied")</c>.
    /// </summary>
    public EntitySystem? EntitySystem => Owner?.GetEntitySystem();

    /// <summary>
    /// Gets the MainGame this component's entity belongs to (via system → scene → game), or null.
    /// Enables scene-level work from components, e.g. <c>Game?.SceneManager.LoadScene(...)</c>.
    /// </summary>
    public MainGame? Game => EntitySystem?.Game;

    /// <summary>
    /// Sends a scene-wide message on behalf of the owning entity — Unity SendMessage style.
    /// Convenience so component code can broadcast without reaching for the system:
    /// <c>SendMessage("PlayerDied", damage)</c>.
    /// </summary>
    /// <param name="message">The name of the handler methods to invoke.</param>
    /// <param name="payload">Optional payload delivered to single-parameter handlers.</param>
    /// <returns>The number of handlers invoked, or -1 if the owner is not in a system.</returns>
    public int SendMessage(string message, object? payload = null)
        => EntitySystem?.SendMessage(message, payload) ?? -1;

    /// <summary>
    /// Creates a new entity of the specified type in the owning entity's system — Unity-style
    /// one-liner from component code: <c>CreateGameObject&lt;Ball&gt;()</c>. Pairs with
    /// <see cref="DestroyOwner"/>.
    /// </summary>
    /// <typeparam name="T">The concrete Entity type to create.</typeparam>
    /// <param name="args">Constructor arguments for the entity.</param>
    /// <returns>The newly created entity, or null if the owner is not in a system.</returns>
    public T? CreateGameObject<T>(params object[] args) where T : Entity
        => Owner?.CreateGameObject<T>(args);

    /// <summary>
    /// Instantiates a registered template (prefab) at the given position in the owning entity's
    /// system — Unity-style prefab spawn from component code:
    /// <c>InstantiateTemplate("popup", position)</c>.
    /// </summary>
    /// <param name="templateName">The name of the registered template to instantiate.</param>
    /// <param name="position">The world position to place the instantiated entity.</param>
    /// <returns>The newly created entity, or null if the owner is not in a system.</returns>
    public Entity? InstantiateTemplate(string templateName, Vector2 position)
        => Owner?.InstantiateTemplate(templateName, position);

    /// <summary>
    /// Destroys the owning entity (and its children) — Unity-style one-liner from component code.
    /// </summary>
    public void DestroyOwner() => Owner?.Destroy();

    /// <summary>
    /// Called when the component is attached to an entity.
    /// Override to perform initialization that requires access to the owning entity.
    /// </summary>
    public virtual void OnAttach()
    {
    }

    /// <summary>
    /// Called when the component is detached from an entity.
    /// Override to perform cleanup or resource disposal.
    /// </summary>
    public virtual void OnDetach()
    {
    }

    /// <summary>
    /// Called every frame while the component is attached to an active entity.
    /// Override to implement per-frame update logic.
    /// </summary>
    /// <param name="gameTime">Provides timing information.</param>
    public virtual void Update(GameTime gameTime)
    {
    }

    /// <summary>
    /// Called every frame after all regular updates, while the component is attached to an
    /// active entity. Override for logic that must see the final state of the frame (e.g.
    /// camera sync).
    /// </summary>
    /// <param name="gameTime">Provides timing information.</param>
    public virtual void LateUpdate(GameTime gameTime)
    {
    }
}
