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
        UpdateTweens(_vectorTweens, dt);
        UpdateTweens(_floatTweens, dt);
    }

    /// <summary>Updates all tweens in the list, handling completion and looping.</summary>
    private static void UpdateTweens<T>(IList<T> tweens, float deltaTime) where T : ITween
    {
        for (int i = tweens.Count - 1; i >= 0; i--)
        {
            tweens[i].Advance(deltaTime);
            if (!tweens[i].IsComplete)
                continue;

            HandleTweenCompletion(tweens, i);
        }
    }

    /// <summary>Handles a completed tween by either looping/reversing or removing it.</summary>
    private static void HandleTweenCompletion<T>(IList<T> tweens, int index) where T : ITween
    {
        if (!tweens[index].Loop)
        {
            tweens.RemoveAt(index);
            return;
        }

        if (tweens[index].Reverse)
            tweens[index].ToggleDirection();
        else
            tweens[index].Reset();
    }

    /// <inheritdoc/>
    public override void OnDetach()
    {
        // Cancel all active tweens when the component is detached from an entity
        CancelAll();
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
    /// <param name="loop">Whether the tween repeats on completion.</param>
    /// <param name="reverse">Whether the tween reverses direction each cycle (ping-pong). Requires <paramref name="loop"/> to be true.</param>
    public TweenVector2 TweenToVector2(Vector2 startValue, Vector2 endValue, float duration, Func<float, float>? easingFunction = null, bool loop = false, bool reverse = false)
    {
        var tween = new TweenVector2(startValue, endValue, duration, easingFunction ?? (t => t));
        tween.Loop = loop;
        tween.Reverse = reverse;
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
    /// <param name="loop">Whether the tween repeats on completion.</param>
    /// <param name="reverse">Whether the tween reverses direction each cycle (ping-pong). Requires <paramref name="loop"/> to be true.</param>
    public TweenFloat TweenToFloat(float startValue, float endValue, float duration, Func<float, float>? easingFunction = null, bool loop = false, bool reverse = false)
    {
        var tween = new TweenFloat(startValue, endValue, duration, easingFunction ?? (t => t));
        tween.Loop = loop;
        tween.Reverse = reverse;
        _floatTweens.Add(tween);
        return tween;
    }
}
