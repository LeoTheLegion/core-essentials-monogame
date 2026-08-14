using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Tweening;

/// <summary>
/// Entity component that manages tweens for animating entity properties.
/// Attach this to an entity and use <see cref="TweenToVector2"/> or <see cref="TweenToFloat"/> to create tweens.
/// The component automatically advances all active tweens each frame.
/// </summary>
public class TweenComponent : EntityComponent
{
    private readonly List<TweenVector2> _vectorTweens = new();
    private readonly List<TweenFloat> _floatTweens = new();

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = _vectorTweens.Count - 1; i >= 0; i--)
        {
            _vectorTweens[i].Advance(dt);
            if (_vectorTweens[i].IsComplete)
            {
                if (_vectorTweens[i].Loop)
                {
                    if (_vectorTweens[i].Reverse)
                        _vectorTweens[i].ToggleDirection();
                    else
                        _vectorTweens[i].Reset();
                }
                else
                {
                    _vectorTweens.RemoveAt(i);
                }
            }
        }

        for (int i = _floatTweens.Count - 1; i >= 0; i--)
        {
            _floatTweens[i].Advance(dt);
            if (_floatTweens[i].IsComplete)
            {
                if (_floatTweens[i].Loop)
                {
                    if (_floatTweens[i].Reverse)
                        _floatTweens[i].ToggleDirection();
                    else
                        _floatTweens[i].Reset();
                }
                else
                {
                    _floatTweens.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// Cancels all active tweens.
    /// </summary>
    public void CancelAll()
    {
        _vectorTweens.Clear();
        _floatTweens.Clear();
    }

    /// <summary>
    /// Creates a tween that interpolates a Vector2 value from <paramref name="startValue"/> to <paramref name="endValue"/> over <paramref name="duration"/> seconds.
    /// The returned tween can be used to read the current eased value via <see cref="TweenVector2.GetValue"/>.
    /// </summary>
    /// <param name="startValue">The starting value.</param>
    /// <param name="endValue">The end value.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="easingFunction">The easing function to apply. Defaults to linear.</param>
    public TweenVector2 TweenToVector2(Vector2 startValue, Vector2 endValue, float duration, Func<float, float>? easingFunction = null)
    {
        var tween = new TweenVector2(startValue, endValue, duration, easingFunction ?? (t => t));
        _vectorTweens.Add(tween);
        return tween;
    }

    /// <summary>
    /// Creates a tween that interpolates a float value from <paramref name="startValue"/> to <paramref name="endValue"/> over <paramref name="duration"/> seconds.
    /// The returned tween can be used to read the current eased value via <see cref="TweenFloat.GetValue"/>.
    /// </summary>
    /// <param name="startValue">The starting value.</param>
    /// <param name="endValue">The end value.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="easingFunction">The easing function to apply. Defaults to linear.</param>
    public TweenFloat TweenToFloat(float startValue, float endValue, float duration, Func<float, float>? easingFunction = null)
    {
        var tween = new TweenFloat(startValue, endValue, duration, easingFunction ?? (t => t));
        _floatTweens.Add(tween);
        return tween;
    }
}
