using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Rendering;

/// <summary>
/// The ordered render pipeline for full-screen post passes. Post passes are full-screen quads drawn
/// through a MonoGame <see cref="Effect"/> **after** the scene and GUI have rendered, so they can
/// apply screen-space shaders (vignette, kill flash, color grade) without subclassing
/// <see cref="MainGame"/>.
/// <para>
/// Registration order is render order: the first pass added is drawn first. A pass's per-pass
/// parameters are set directly on its <see cref="Effect"/> by game code (e.g.
/// <c>effect.Parameters["Intensity"].SetValue(0.5f)</c>), since the pipeline holds the effect by
/// reference and re-draws it every frame.
/// </para>
/// <para>
/// The default state is empty: when no passes are registered, <see cref="DrawPostPasses"/> returns
/// immediately without touching the SpriteBatch, so games that don't opt in keep the exact current
/// render loop (no perf or rendering regression).
/// </para>
/// </summary>
public static class RenderPipeline
{
    /// <summary>
    /// The ordered list of registered post-pass effects. Game-thread only.
    /// </summary>
    private static readonly List<Effect> _postPasses = new();

    /// <summary>
    /// Lazily created 1x1 white texture used as the source for full-screen quads. Created on first
    /// use against the active <see cref="GraphicsDevice"/> (a device is only available during draw).
    /// </summary>
    private static Texture2D? _quadTexture;

    /// <summary>
    /// Gets the number of registered post passes.
    /// </summary>
    public static int PostPassCount => _postPasses.Count;

    /// <summary>
    /// Gets an ordered snapshot of the registered post-pass effects (registration order = render order).
    /// </summary>
    public static IReadOnlyList<Effect> PostPasses => new List<Effect>(_postPasses);

    /// <summary>
    /// Registers a full-screen post pass. The pass draws a full-screen quad through the given effect
    /// after the scene and GUI, in registration order. Duplicate effects are allowed (drawn twice).
    /// </summary>
    /// <param name="effect">The effect to apply as a full-screen pass.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="effect"/> is null.</exception>
    public static void AddPostPass(Effect effect)
    {
        if (effect == null)
            throw new ArgumentNullException(nameof(effect), "Post-pass effect cannot be null.");

        _postPasses.Add(effect);
    }

    /// <summary>
    /// Removes the first registered occurrence of a post pass.
    /// </summary>
    /// <param name="effect">The post-pass effect to remove.</param>
    /// <returns>True if the pass was found and removed; false otherwise.</returns>
    public static bool RemovePostPass(Effect effect)
    {
        return _postPasses.Remove(effect);
    }

    /// <summary>
    /// Removes all registered post passes, restoring the default (no-op) pipeline state.
    /// </summary>
    public static void ClearPostPasses()
    {
        _postPasses.Clear();
    }

    /// <summary>
    /// Draws all registered post passes: one full-screen quad through each effect, in registration
    /// order, with screen-space (identity) transforms. This is a no-op when no passes are registered —
    /// the SpriteBatch is not touched at all, so the default game loop is untouched.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="spriteBatch">The SpriteBatch used to draw the passes.</param>
    public static void DrawPostPasses(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (_postPasses.Count == 0)
            return;

        var device = spriteBatch.GraphicsDevice;
        EnsureQuadTexture(device);

        foreach (var effect in _postPasses)
        {
            // Each pass gets its own Begin/End with its effect — MonoGame applies the effect at
            // Begin, not per draw. Identity transform: post passes are screen-space.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                effect, null);

            var viewport = device.Viewport;
            _quadTexture!.DrawFullQuad(spriteBatch, new Rectangle(0, 0, viewport.Width, viewport.Height));

            spriteBatch.End();
        }
    }

    /// <summary>
    /// Creates the shared 1x1 white quad texture if it doesn't exist yet.
    /// </summary>
    private static void EnsureQuadTexture(GraphicsDevice device)
    {
        if (_quadTexture != null)
            return;

        _quadTexture = new Texture2D(device, 1, 1);
        _quadTexture.SetData(new[] { Color.White });
    }
}

/// <summary>
/// Draws a 1x1 texture stretched over the given screen rectangle — the canonical full-screen quad.
/// </summary>
internal static class QuadTextureExtensions
{
    /// <summary>
    /// Draws this (1x1) texture stretched across <paramref name="screenRect"/> as a single quad.
    /// </summary>
    public static void DrawFullQuad(this Texture2D texture, SpriteBatch spriteBatch, Rectangle screenRect)
    {
        spriteBatch.Draw(texture, screenRect, Color.White);
    }
}
