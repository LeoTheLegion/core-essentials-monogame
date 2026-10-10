using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.Physics.Types;
using nkast.Aether.Physics2D.Diagnostics;

namespace CoreEssentials.GameSystems.Physics.Engines.Aether;

/// <summary>
/// Debug renderer for physics bodies.
/// <para>
/// This is the Aether-specific implementation of <see cref="IPhysicsDebugRenderer"/>.
/// It delegates to Aether's built-in <see cref="DebugView"/>, which can visualize
/// shapes (colored by body type), joints, contact points, broad-phase AABBs,
/// controllers, center-of-mass axes, a live performance graph, and a stats panel.
/// </para>
/// <para>
/// The public API stays engine-agnostic: consumers only see <see cref="IsEnabled"/>
/// and <see cref="Draw"/>. The richer Aether features (per-category flags, colors,
/// panels) are exposed on this concrete type for callers that want them.
/// </para>
/// </summary>
public class PhysicsDebugRenderer : GameSystem, IPhysicsDebugRenderer
{
    private DebugView? _debugView;
    private bool _contentLoaded;
    private bool _disposed;
    private IPhysicsDebugRenderTarget? _target;

    /// <summary>
    /// Initializes a new instance of the PhysicsDebugRenderer class without an explicit engine.
    /// The sibling <see cref="PhysicsEngine"/> is resolved from the scene lazily on first use,
    /// which lets data-driven scenes declare <c>&lt;System Type="PhysicsDebugRenderer"/&gt;</c>
    /// next to a <c>&lt;System Type="PhysicsEngine"/&gt;</c> without constructor wiring.
    /// </summary>
    public PhysicsDebugRenderer()
    {
    }

    /// <summary>
    /// Initializes a new instance of the PhysicsDebugRenderer class.
    /// </summary>
    /// <param name="engine">The Aether-backed physics engine whose world will be visualized.</param>
    public PhysicsDebugRenderer(PhysicsEngine engine)
        : this(engine, null)
    {
    }

    /// <summary>
    /// Test seam: accepts an injected implementation of the device-reach boundary (font load, viewport
    /// read, and render dispatch). Production always passes <see langword="null"/>, which defers to the
    /// real <see cref="AetherPhysicsDebugRenderTarget"/>; tests pass a recording fake so the enable +
    /// content-load dispatch can be asserted without a graphics device (same seam pattern as
    /// <c>IEntityDrawTarget</c> and <c>IPrimitiveDrawer</c>).
    /// </summary>
    internal PhysicsDebugRenderer(PhysicsEngine engine, IPhysicsDebugRenderTarget? target)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));
        _debugView = new DebugView(engine.AetherWorld);
        // Sensible defaults: show shapes, joints, and contact points.
        _debugView.AppendFlags(DebugViewFlags.ContactPoints);
        _target = target;
    }

    /// <summary>
    /// Ensures the underlying Aether <see cref="DebugView"/> exists, resolving the sibling
    /// <see cref="PhysicsEngine"/> from the scene when this instance was created without one.
    /// </summary>
    private void EnsureDebugView()
    {
        if (_debugView != null) return;

        var engine = Scene?.GetGameSystem<PhysicsEngine>()
            ?? throw new InvalidOperationException(
                "PhysicsDebugRenderer has no physics engine — declare a <System Type=\"PhysicsEngine\"/> in the scene.");
        _debugView = new DebugView(engine.AetherWorld);
        // Sensible defaults: show shapes, joints, and contact points.
        _debugView.AppendFlags(DebugViewFlags.ContactPoints);
    }

    #region IDisposable (inherited from IPhysicsDebugRenderer)

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the instance. Called from <see cref="Dispose()"/> or when the finalizer runs.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/> (managed resources can be released); false if called from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _debugView?.Dispose();
            _debugView = null;
        }

        _disposed = true;
    }

    #endregion

    /// <inheritdoc />
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets the underlying Aether <see cref="DebugView"/> for advanced configuration
    /// (colors, panel positions, per-category flags).
    /// </summary>
    public DebugView DebugView
    {
        get { EnsureDebugView(); return _debugView!; }
    }

    /// <summary>
    /// Gets or sets which categories of debug data to render (shapes, joints, AABBs,
    /// contact points, performance graph, etc.).
    /// </summary>
    public DebugViewFlags Flags
    {
        get { EnsureDebugView(); return _debugView!.Flags; }
        set { EnsureDebugView(); _debugView!.Flags = value; }
    }

    /// <summary>
    /// Loads the renderer's content (Aether's <c>DiagnosticsFont</c>) from the game's
    /// <see cref="Game.Content"/>. Safe to call multiple times; it only loads once.
    /// Must be called after the graphics device exists (e.g. during scene start).
    /// </summary>
    public void LoadContent()
    {
        if (_contentLoaded) return;

        EnsureDebugView();

        Target.LoadFont(_debugView!);
        _contentLoaded = true;
    }

    /// <summary>
    /// Draws debug visualizations for all physics bodies using Aether's DebugView.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing (unused by the Aether primitive batch, kept for interface compatibility).</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsEnabled)
            return;

        EnsureDebugView();
        if (!_contentLoaded)
            LoadContent();

        Target.Render(_debugView!);
    }

    /// <summary>
    /// The device-reach boundary for this renderer: loads Aether's diagnostics font, reads the current
    /// viewport, and dispatches the render. Production resolves the real adapter (reading from the
    /// attached game); tests inject a recording fake via the internal constructor. Resolved lazily once.
    /// </summary>
    private IPhysicsDebugRenderTarget Target => _target ??= new AetherPhysicsDebugRenderTarget(this);
}

/// <summary>
/// Device-reach boundary for <see cref="PhysicsDebugRenderer"/>: everything that touches a live graphics
/// device (loading Aether's diagnostics font, reading the current viewport, dispatching the render) is
/// funneled through this seam. Production wraps the real device via <see cref="AetherPhysicsDebugRenderTarget"/>;
/// tests inject a recording fake so the enable + content-load dispatch can be asserted without a graphics
/// device (same seam pattern as <c>IEntityDrawTarget</c> and <c>IPrimitiveDrawer</c>). Keeping this at the
/// edge means <see cref="PhysicsDebugRenderer"/> itself stays free of any device dereference.
/// </summary>
internal interface IPhysicsDebugRenderTarget
{
    /// <summary>Loads Aether's diagnostics font into <paramref name="view"/>, guarded by a "scene not yet attached" throw.</summary>
    void LoadFont(DebugView view);

    /// <summary>Reads the current viewport, builds the screen-pixel orthographic projection, and dispatches the render for <paramref name="view"/>.</summary>
    void Render(DebugView view);
}

/// <summary>
/// Production <see cref="IPhysicsDebugRenderTarget"/>: forwards each call to the attached
/// <see cref="GameSystem.Game"/>'s graphics device and content manager, 1:1 with the inline logic it replaced.
/// </summary>
internal sealed class AetherPhysicsDebugRenderTarget : IPhysicsDebugRenderTarget
{
    private readonly PhysicsDebugRenderer _owner;

    internal AetherPhysicsDebugRenderTarget(PhysicsDebugRenderer owner) => _owner = owner;

    public void LoadFont(DebugView view)
    {
        var game = _owner.Game
            ?? throw new InvalidOperationException("Cannot load debug renderer content before the scene is attached.");

        view.LoadContent(game.Graphics.GraphicsDevice, game.Content);
    }

    public void Render(DebugView view)
    {
        var viewport = _owner.Game!.Graphics.GraphicsDevice.Viewport;

        // The physics world uses screen-pixel coordinates, so map world (x, y)
        // straight onto the viewport with an identity view/world matrix.
        var projection = Matrix.CreateOrthographicOffCenter(
            0f, viewport.Width, viewport.Height, 0f, 0f, 1f);

        view.RenderDebugData(projection, Matrix.Identity, Matrix.Identity);
    }
}
