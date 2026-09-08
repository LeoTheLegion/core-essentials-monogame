using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Tweening;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Makes the owning entity bob up and down with a looping, eased Y-offset — the behavior that used
/// to live inline in the deleted <c>CharacterEntity</c>. The baseline Y is captured on the first
/// <see cref="Update"/> (the XML position is applied *after* <c>OnStart</c>, so it cannot be read at
/// construction) and every frame the entity's Y is set to <c>baseline + tweenedOffset</c> while X is
/// left untouched, so it composes with horizontal movement. The tween math (eased ping-pong) is
/// driven directly by a private <see cref="TweenFloat"/> so no sibling component is required.
/// </summary>
public class BounceTweenComponent : EntityComponent
{
    /// <summary>How far (in pixels, upward) the entity bobs. The offset eases from 0 to -Amplitude.</summary>
    public float Amplitude { get; set; } = 50f;

    /// <summary>Duration of one 0→-Amplitude leg in seconds.</summary>
    public float Duration { get; set; } = 1.5f;

    /// <summary>Whether the bounce repeats forever (default true).</summary>
    public bool Loop { get; set; } = true;

    /// <summary>When <see cref="Loop"/> is on, ping-pong (reverse each leg) instead of snapping back.</summary>
    public bool Reverse { get; set; } = true;

    /// <summary>Easing applied to each leg. Defaults to InOutSine (matches the original character).</summary>
    public Func<float, float> Easing { get; set; } = EasingFunctions.InOutSine;

    private TweenFloat? _tween;
    private float _originalY;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (Owner == null) return;

        // The tween is created on the first frame — the XML position is applied after OnStart, so the
        // baseline Y cannot be read earlier.
        var tween = GetOrCreateTween();
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        tween.Advance(dt);
        if (tween.IsComplete)
        {
            if (!Loop) tween.Reset();
            else if (Reverse) tween.ToggleDirection();
            else tween.Reset();
        }

        Owner.Position = new Vector2(Owner.Position.X, _originalY + tween.GetValue());
    }

    /// <summary>Returns the active tween, creating it (and capturing the baseline Y) on first use.</summary>
    private TweenFloat GetOrCreateTween()
    {
        if (_tween != null)
            return _tween;

        var tween = new TweenFloat(0f, -Amplitude, Duration, Easing);
        tween.Loop = Loop;
        tween.Reverse = Reverse;
        _originalY = Owner.Position.Y;
        _tween = tween;
        return tween;
    }
}
