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
    private bool _initialized;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (Owner == null) return;

        // Capture the baseline on the first frame — the XML position is applied after OnStart.
        if (!_initialized)
        {
            _tween = new TweenFloat(0f, -Amplitude, Duration, Easing);
            _tween.Loop = Loop;
            _tween.Reverse = Reverse;
            _originalY = Owner.Position.Y;
            _initialized = true;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _tween.Advance(dt);
        if (_tween.IsComplete)
        {
            if (!Loop) _tween.Reset();
            else if (Reverse) _tween.ToggleDirection();
            else _tween.Reset();
        }

        Owner.Position = new Vector2(Owner.Position.X, _originalY + _tween.GetValue());
    }
}
