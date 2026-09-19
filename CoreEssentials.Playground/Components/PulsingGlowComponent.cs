using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Tweening;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// A controller that makes a sprite's glow pulse by driving the <c>GlowStrength</c> shader uniform
/// over time with a ping-pong tween. It does NOT own the uniform — the sibling
/// <see cref="ShaderComponent"/> on the same entity owns and controls it (the render pipeline
/// pushes that component's values onto the effect each frame). This component simply writes a new value
/// into that owning component every update, demonstrating code-driven control of shader vars:
/// <code>
/// &lt;EntityDefinition Type="...GameObjectEntity" Id="glowBall"&gt;
///   &lt;Components&gt;
///     &lt;Component Type="SpriteComponent"&gt; ... &lt;/Component&gt;
///     &lt;Component Type="ShaderComponent"&gt;
///       &lt;Property Name="EffectAsset" Value="Effects/Glow" /&gt;   &lt;!-- the shader --&gt;
///       &lt;EffectParameter Name="GlowStrength" Value="1.0" /&gt;    &lt;!-- data-driven base value --&gt;
///     &lt;/Component&gt;
///     &lt;Component Type="CoreEssentials.Playground.Components.PulsingGlowComponent"&gt;
///       &lt;Properties&gt;
///         &lt;Property Name="MinStrength" Value="0.25" /&gt;
///         &lt;Property Name="MaxStrength" Value="1.6" /&gt;
///         &lt;Property Name="Duration" Value="1.4" /&gt;
///       &lt;/Properties&gt;
///     &lt;/Component&gt;
///   &lt;/Components&gt;
/// &lt;/EntityDefinition&gt;
/// </code>
/// The XML <c>&lt;EffectParameter&gt;</c> seeds the base value (so a scene with no controller still renders a
/// fixed glow); while this component is attached it continuously overrides that value with the tweened one.
/// </summary>
public class PulsingGlowComponent : EntityComponent
{
    /// <summary>Name of the shader uniform to drive (must match a parameter on the sprite's effect).</summary>
    public string ParameterName { get; set; } = "GlowStrength";

    /// <summary>The weakest glow value the tween dips down to.</summary>
    public float MinStrength { get; set; } = 0.25f;

    /// <summary>The strongest glow value the tween peaks at.</summary>
    public float MaxStrength { get; set; } = 1.6f;

    /// <summary>Seconds for one full sweep from min to max (ping-pong reverses it back).</summary>
    public float Duration { get; set; } = 1.4f;

    private TweenFloat? _tween;
    private ShaderComponent? _shader;

    /// <inheritdoc />
    public override void OnAttach()
    {
        // The owning component holds the uniform; this controller just writes into it each frame.
        _shader = Owner.GetComponent<ShaderComponent>();

        if (_shader == null)
        {
            Console.WriteLine("[PulsingGlowComponent] No ShaderComponent on this entity — nothing to drive.");
            return;
        }

        // Smooth ease-in-out so the pulse feels organic rather than linear.
        _tween = new TweenFloat(MinStrength, MaxStrength, Duration, t => 0.5f - (float)Math.Cos(t * Math.PI));
        _tween.Loop = true;
        _tween.Reverse = true; // ping-pong: min → max → min → ...
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (_shader == null || _tween == null) return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _tween.Advance(dt);

        // Ping-pong: when a sweep completes, flip direction and restart from the other end.
        if (_tween.IsComplete)
            _tween.ToggleDirection();

        _shader.SetFloat(ParameterName, _tween.GetValue());
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        _tween = null;
        _shader = null;
    }
}
