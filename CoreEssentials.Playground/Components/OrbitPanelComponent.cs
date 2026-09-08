using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Moves its owning entity around an ellipse in world space, every frame. This ports the
/// per-frame orbit loop that used to live in a scene subclass (the LabelAlignment demo's floating
/// panel), so the behavior can be declared purely from data:
/// <code>
/// &lt;Component Type="OrbitPanelComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="CenterX" Value="640" /&gt;
///     &lt;Property Name="CenterY" Value="360" /&gt;
///     &lt;Property Name="RadiusX" Value="150" /&gt;
///     &lt;Property Name="RadiusY" Value="90" /&gt;
///     &lt;Property Name="Speed"   Value="0.6" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// The position is <c>(CenterX + cos(t·Speed)·RadiusX, CenterY + sin(t·Speed)·RadiusY)</c>, where
/// <c>t</c> accumulates elapsed seconds. Because the panel lives in world space, panning/zooming the
/// camera carries it along — which is exactly what the demo wants to show.
/// </summary>
public class OrbitPanelComponent : EntityComponent
{
    /// <summary>The ellipse center X (world units).</summary>
    public float CenterX { get; set; }

    /// <summary>The ellipse center Y (world units).</summary>
    public float CenterY { get; set; }

    /// <summary>The ellipse radius along X. Defaults to 150.</summary>
    public float RadiusX { get; set; } = 150f;

    /// <summary>The ellipse radius along Y. Defaults to 90.</summary>
    public float RadiusY { get; set; } = 90f;

    /// <summary>The angular speed (radians per second of accumulated time). Defaults to 0.6.</summary>
    public float Speed { get; set; } = 0.6f;

    private float _time;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (Owner == null) return;

        _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
        Owner.Position = ComputePosition(_time);
    }

    /// <summary>
    /// Computes the orbit position at the given accumulated time. Virtual so unit tests can assert
    /// the trajectory without driving a live update loop.
    /// </summary>
    protected virtual Vector2 ComputePosition(float time) => new(
        CenterX + (float)Math.Cos(time * Speed) * RadiusX,
        CenterY + (float)Math.Sin(time * Speed) * RadiusY);

    /// <summary>The accumulated orbit time (seconds). Exposed for tests and diagnostics.</summary>
    public float ElapsedTime => _time;
}
