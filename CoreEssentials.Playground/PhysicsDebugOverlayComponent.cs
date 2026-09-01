using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using CoreEssentials.Inputs;

namespace CoreEssentials.Playground;

/// <summary>
/// Declaratively reproduces the physics debug overlay that used to live in a scene subclass, so
/// the scene can be pure data. A configurable key (default F1) toggles the scene's
/// <see cref="PhysicsDebugRenderer"/> on and off. Because it implements
/// <see cref="IDrawableComponent"/>, the owning entity's render pass draws the overlay each frame
/// whenever it is enabled — no scene <c>Draw</c> override is required.
/// <code>
/// &lt;Component Type="PhysicsDebugOverlayComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="ToggleKey" Value="F1" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// Reaching the renderer and drawing are small <c>protected virtual</c> seams so unit tests can
/// observe the toggle/draw without a live physics engine.
/// </summary>
public class PhysicsDebugOverlayComponent : EntityComponent, IDrawableComponent
{
    /// <summary>The key that toggles the physics debug overlay. Defaults to F1.</summary>
    public Keys ToggleKey { get; set; } = Keys.F1;

    private EventHandler<KeyboardEventArgs>? _onKeyReleased;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _onKeyReleased = (_, args) => HandleKey(args.Key);
        Input.Keyboard.KeyReleased += _onKeyReleased;
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_onKeyReleased != null)
            Input.Keyboard.KeyReleased -= _onKeyReleased;
        _onKeyReleased = null;
    }

    /// <summary>
    /// Handles a key release, toggling the overlay when it matches <see cref="ToggleKey"/>. Exposed
    /// publicly so it can be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key == ToggleKey)
            Toggle();
    }

    // ── Testability seams ────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the scene's physics debug renderer. Returns null when no such system is registered
    /// (e.g. in unit tests). Virtual so tests can stub it.
    /// </summary>
    protected virtual IPhysicsDebugRenderer? GetDebugRenderer()
    {
        var system = EntitySystem;
        if (system == null) return null;

        try
        {
            // Scene.GetGameSystem is an exact-type lookup, so request the concrete renderer.
            return system.GetGameSystem<PhysicsDebugRenderer>();
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Whether the physics debug overlay is currently enabled.</summary>
    public bool IsEnabled => GetDebugRenderer()?.IsEnabled ?? false;

    /// <summary>Toggles the physics debug renderer. Virtual so unit tests can observe it.</summary>
    protected virtual void Toggle()
    {
        var renderer = GetDebugRenderer();
        if (renderer != null)
            renderer.IsEnabled = !renderer.IsEnabled;
    }

    // ── IDrawableComponent ───────────────────────────────────────────────────────

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch)
    {
        var renderer = GetDebugRenderer();
        if (renderer != null && renderer.IsEnabled)
            renderer.Draw(spriteBatch);
    }
}
