using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Rendering;

/// <summary>
/// A single full-screen post pass: a quad drawn through a MonoGame <see cref="Effect"/> after the
/// scene and GUI have rendered.
/// <para>
/// Two input modes exist:
/// <list type="bullet">
/// <item><b>Additive</b> (<c>SamplesSceneTarget == false</c>) — drawn over the current frame; the
/// shader samples the backbuffer automatically (via its <c>Texture2D</c>). No render target needed.
/// Use for vignette, kill flash, color grade.</item>
/// <item><b>Sampling</b> (<c>SamplesSceneTarget == true</c>) — the pipeline feeds the clean,
/// pre-composited scene frame (the render target from the process pass) into the shader's input texture
/// parameter. Requires <see cref="RenderPipeline.EnableRenderToTarget"/> to be enabled. Use for bloom,
/// blur, chromatic aberration, dissolve-the-whole-frame.</item>
/// </list>
/// </para>
/// Per-pass parameters are set directly on the <see cref="Effect"/> by game code (e.g.
/// <c>effect.Parameters["Intensity"].SetValue(0.5f)</c>), since the pipeline holds the effect by
/// reference and re-draws it every frame.
/// </summary>
public sealed class PostPass
{
    /// <summary>The default shader input-texture parameter name for sampling passes (MonoGame convention).</summary>
    public const string DefaultInputParameterName = "Texture2D";

    /// <summary>Gets the effect this pass applies.</summary>
    public Effect Effect { get; }

    /// <summary>
    /// Gets whether this pass samples the pre-bound scene render target (true) or draws additively
    /// over the frame (false). Sampling passes require render-to-target to be enabled.
    /// </summary>
    public bool SamplesSceneTarget { get; }

    /// <summary>
    /// Gets the name of the shader parameter that receives the scene render target for sampling passes.
    /// Defaults to <see cref="DefaultInputParameterName"/>. Ignored for additive passes.
    /// </summary>
    public string InputParameterName { get; }

    /// <summary>
    /// Initializes a new post pass.
    /// </summary>
    /// <param name="effect">The effect to apply as a full-screen pass.</param>
    /// <param name="samplesSceneTarget">True to sample the scene render target; false for an additive overlay.</param>
    /// <param name="inputParameterName">For sampling passes, the shader parameter that receives the scene target.</param>
    public PostPass(Effect effect, bool samplesSceneTarget = false, string? inputParameterName = null)
    {
        Effect = effect ?? throw new ArgumentNullException(nameof(effect), "Post-pass effect cannot be null.");
        SamplesSceneTarget = samplesSceneTarget;
        InputParameterName = string.IsNullOrWhiteSpace(inputParameterName) ? DefaultInputParameterName : inputParameterName!;
    }
}

/// <summary>
/// The ordered render pipeline: pre passes (setup before the scene), the process pass (scene + GUI,
/// optionally rendered into a <see cref="RenderTarget2D"/>), and post passes (full-screen shader quads
/// after the scene). Everything is opt-in — the default state reproduces the original straight-to-
/// backbuffer render loop exactly (no perf or rendering regression for games that don't opt in).
/// </summary>
public static class RenderPipeline
{
    /// <summary>The ordered list of registered post passes. Game-thread only.</summary>
    private static readonly List<PostPass> _postPasses = new();

    /// <summary>The ordered list of registered pre-pass actions. Game-thread only.</summary>
    private static readonly List<Action<GameTime>> _prePasses = new();

    /// <summary>Lazily created 1x1 white texture used as the source for full-screen quads.</summary>
    private static Texture2D? _quadTexture;

    // Render-to-target state. Only touched on the game thread during draw.
    private static bool _renderToTargetEnabled;
    private static RenderTarget2D? _sceneTarget;
    private static GraphicsDevice? _device;

    /// <summary>Gets the number of registered post passes.</summary>
    public static int PostPassCount => _postPasses.Count;

    /// <summary>Gets an ordered snapshot of the registered post passes (registration order = render order).</summary>
    public static IReadOnlyList<PostPass> PostPasses => new List<PostPass>(_postPasses);

    /// <summary>Gets the number of registered pre passes.</summary>
    public static int PrePassCount => _prePasses.Count;

    /// <summary>
    /// Gets whether the process pass renders into a <see cref="RenderTarget2D"/> instead of straight to
    /// the backbuffer. Default is false (straight-to-backbuffer, the original behavior).
    /// </summary>
    public static bool RenderToTargetEnabled => _renderToTargetEnabled;

    /// <summary>
    /// Gets the scene render target that the process pass rendered into this frame, or null when
    /// render-to-target is disabled or the process pass has not yet run. Exposed so sampling post-pass
    /// shaders (and tests) can inspect the clean pre-composited frame.
    /// </summary>
    public static RenderTarget2D? SceneTarget => _sceneTarget;

    // ===== Post passes =====

    /// <summary>
    /// Registers a full-screen post pass. The pass draws a full-screen quad through its effect after
    /// the scene and GUI, in registration order. By default it is an additive overlay (no render
    /// target required). Duplicate effects are allowed (drawn twice).
    /// </summary>
    /// <param name="effect">The effect to apply as a full-screen pass.</param>
    /// <param name="samplesSceneTarget">True to sample the scene render target; false for an additive overlay.</param>
    /// <param name="inputParameterName">For sampling passes, the shader parameter that receives the scene target.</param>
    public static void AddPostPass(Effect effect, bool samplesSceneTarget = false, string? inputParameterName = null)
        => _postPasses.Add(new PostPass(effect, samplesSceneTarget, inputParameterName));

    /// <summary>
    /// Removes the first registered post pass whose effect is <paramref name="effect"/>.
    /// </summary>
    /// <param name="effect">The post-pass effect to remove.</param>
    /// <returns>True if a matching pass was found and removed; false otherwise.</returns>
    public static bool RemovePostPass(Effect effect)
    {
        for (int i = 0; i < _postPasses.Count; i++)
        {
            if (ReferenceEquals(_postPasses[i].Effect, effect))
            {
                _postPasses.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>Removes all registered post passes, restoring the default (no-op) state.</summary>
    public static void ClearPostPasses() => _postPasses.Clear();

    // ===== Pre passes =====

    /// <summary>
    /// Registers an ordered setup action that runs after <c>GraphicsDevice.Clear</c> and before the
    /// scene is drawn (e.g. camera setup). No-op when no pre passes are registered.
    /// </summary>
    /// <param name="action">The setup action to run each frame before the process pass.</param>
    public static void AddPrePass(Action<GameTime> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action), "Pre-pass action cannot be null.");

        _prePasses.Add(action);
    }

    /// <summary>Removes the first registered pre pass matching <paramref name="action"/>.</summary>
    /// <param name="action">The pre-pass action to remove.</param>
    /// <returns>True if a matching pre pass was found and removed; false otherwise.</returns>
    public static bool RemovePrePass(Action<GameTime> action) => _prePasses.Remove(action);

    /// <summary>Removes all registered pre passes, restoring the default (no-op) state.</summary>
    public static void ClearPrePasses() => _prePasses.Clear();

    // ===== Render-to-target opt-in =====

    /// <summary>
    /// Enables or disables rendering the process pass (scene + GUI) into a full-screen
    /// <see cref="RenderTarget2D"/> instead of straight to the backbuffer. Required for sampling post
    /// passes; not needed for additive overlays. When disabled (default), the original behavior is
    /// preserved exactly. The render target is created lazily on first use and resized automatically
    /// when the backbuffer size changes.
    /// </summary>
    /// <param name="enabled">True to render the scene into a render target; false for straight-to-backbuffer.</param>
    public static void EnableRenderToTarget(bool enabled) => _renderToTargetEnabled = enabled;

    // ===== Draw orchestration (called by MainGame.Draw) =====

    /// <summary>
    /// Runs all registered pre passes in registration order. No-op when none are registered.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public static void DrawPrePasses(GameTime gameTime)
    {
        for (int i = 0; i < _prePasses.Count; i++)
            _prePasses[i](gameTime);
    }

    /// <summary>
    /// Begins the process pass under render-to-target: binds (creating/resizing as needed) a full-screen
    /// render target and clears it. Call before drawing the scene + GUI when
    /// <see cref="RenderToTargetEnabled"/> is true; pair with <see cref="EndProcessPass"/>.
    /// </summary>
    /// <param name="device">The graphics device to bind the render target on.</param>
    internal static void BeginProcessPass(GraphicsDevice device)
    {
        _device = device;
        EnsureSceneTarget(device);
        device.SetRenderTarget(_sceneTarget!);
        device.Clear(Color.Black);
    }

    /// <summary>
    /// Ends the process pass under render-to-target: unbinds the render target (back to the backbuffer),
    /// clears it, and blits the scene frame onto the backbuffer so additive post passes composite over it
    /// and the final image is visible. Pair with <see cref="BeginProcessPass"/>.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used to blit the scene frame to the backbuffer.</param>
    internal static void EndProcessPass(SpriteBatch spriteBatch)
    {
        var device = _device;
        if (device == null || _sceneTarget == null)
            return;

        device.SetRenderTarget(null); // back to the backbuffer
        device.Clear(Color.Black);

        var viewport = device.Viewport;
        spriteBatch.Begin();
        spriteBatch.Draw(_sceneTarget, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);
        spriteBatch.End();
    }

    /// <summary>
    /// Draws all registered post passes: one full-screen quad through each effect, in registration
    /// order, with screen-space (identity) transforms. Sampling passes are fed the scene render target;
    /// additive passes draw over the current frame. This is a no-op when no passes are registered — the
    /// SpriteBatch is not touched at all, so the default game loop is untouched.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="spriteBatch">The SpriteBatch used to draw the passes.</param>
    public static void DrawPostPasses(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (_postPasses.Count == 0)
            return;

        var device = spriteBatch.GraphicsDevice;
        EnsureQuadTexture(device);

        foreach (var pass in _postPasses)
        {
            if (pass.SamplesSceneTarget)
            {
                if (!_renderToTargetEnabled || _sceneTarget == null)
                    throw new InvalidOperationException(
                        $"Sampling post pass '{pass.Effect}' requires RenderPipeline.EnableRenderToTarget(true).");

                // Feed the clean, pre-composited scene frame into the shader's input texture parameter.
                pass.Effect.Parameters[pass.InputParameterName].SetValue(_sceneTarget);
            }

            // Each pass gets its own Begin/End with its effect — MonoGame applies the effect at Begin,
            // not per draw. Identity transform: post passes are screen-space.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                pass.Effect, null);

            var viewport = device.Viewport;
            _quadTexture!.DrawFullQuad(spriteBatch, new Rectangle(0, 0, viewport.Width, viewport.Height));

            spriteBatch.End();
        }
    }

    // ===== Internals =====

    /// <summary>Creates the shared 1x1 white quad texture if it doesn't exist yet.</summary>
    private static void EnsureQuadTexture(GraphicsDevice device)
    {
        if (_quadTexture != null)
            return;

        _quadTexture = new Texture2D(device, 1, 1);
        _quadTexture.SetData(new[] { Color.White });
    }

    /// <summary>Creates or resizes the scene render target to match the current backbuffer size.</summary>
    private static void EnsureSceneTarget(GraphicsDevice device)
    {
        var viewport = device.Viewport;
        if (_sceneTarget == null || _sceneTarget.Width != viewport.Width || _sceneTarget.Height != viewport.Height)
        {
            _sceneTarget?.Dispose();
            _sceneTarget = new RenderTarget2D(device, viewport.Width, viewport.Height);
        }
    }

    /// <summary>
    /// Resets all pipeline state (pre passes, post passes, render-to-target flag, and disposes any
    /// allocated scene target). Intended for tests and application shutdown.
    /// </summary>
    internal static void ResetForTesting()
    {
        _postPasses.Clear();
        _prePasses.Clear();
        _renderToTargetEnabled = false;
        _sceneTarget?.Dispose();
        _sceneTarget = null;
        _device = null;
    }
}

/// <summary>Draws a 1x1 texture stretched over the given screen rectangle — the canonical full-screen quad.</summary>
internal static class QuadTextureExtensions
{
    /// <summary>Draws this (1x1) texture stretched across <paramref name="screenRect"/> as a single quad.</summary>
    public static void DrawFullQuad(this Texture2D texture, SpriteBatch spriteBatch, Rectangle screenRect)
        => spriteBatch.Draw(texture, screenRect, Color.White);
}
