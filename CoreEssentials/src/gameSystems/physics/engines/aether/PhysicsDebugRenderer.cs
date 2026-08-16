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
    private readonly PhysicsEngine _engine;
    private DebugView? _debugView;
    private bool _contentLoaded;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the PhysicsDebugRenderer class.
    /// </summary>
    /// <param name="engine">The Aether-backed physics engine whose world will be visualized.</param>
    public PhysicsDebugRenderer(PhysicsEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _debugView = new DebugView(_engine.AetherWorld);
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
    public DebugView DebugView => _debugView!;

    /// <summary>
    /// Gets or sets which categories of debug data to render (shapes, joints, AABBs,
    /// contact points, performance graph, etc.).
    /// </summary>
    public DebugViewFlags Flags
    {
        get => _debugView!.Flags;
        set => _debugView!.Flags = value;
    }

    /// <summary>
    /// Loads the renderer's content (Aether's <c>DiagnosticsFont</c>) from the game's
    /// <see cref="Game.Content"/>. Safe to call multiple times; it only loads once.
    /// Must be called after the graphics device exists (e.g. during scene start).
    /// </summary>
    public void LoadContent()
    {
        if (_contentLoaded) return;

        var game = Game;
        if (game == null)
            throw new InvalidOperationException("Cannot load debug renderer content before the scene is attached.");

        _debugView!.LoadContent(game.Graphics.GraphicsDevice, game.Content);
        _contentLoaded = true;
    }

    /// <summary>
    /// Draws debug visualizations for all physics bodies using Aether's DebugView.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing (unused by the Aether primitive batch, kept for interface compatibility).</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsEnabled || _debugView == null)
            return;

        if (!_contentLoaded)
            LoadContent();

        // The physics world uses screen-pixel coordinates, so map world (x, y)
        // straight onto the viewport with an identity view/world matrix.
        var viewport = Game!.Graphics.GraphicsDevice.Viewport;
        var projection = Matrix.CreateOrthographicOffCenter(
            0f, viewport.Width, viewport.Height, 0f, 0f, 1f);

        _debugView.RenderDebugData(projection, Matrix.Identity, Matrix.Identity);
    }
}
