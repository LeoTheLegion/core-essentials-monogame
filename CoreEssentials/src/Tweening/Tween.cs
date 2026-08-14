using System;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Tweening;

/// <summary>
/// A single-value tween that interpolates a Vector2 from a start value to an end value over time using an easing function.
/// The owning <see cref="TweenComponent"/> updates elapsed time each frame; call <see cref="GetValue"/> to get the current eased value.
/// </summary>
public class TweenVector2
{
    private readonly Vector2 _startValue;
    private readonly Vector2 _endValue;
    private readonly Func<float, float> _easingFunction;
    private bool _reversed;

    /// <summary>
    /// Creates a Vector2 tween with the specified easing function.
    /// </summary>
    /// <param name="startValue">The starting vector value.</param>
    /// <param name="endValue">The target vector value.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="easingFunction">The easing function to apply.</param>
    public TweenVector2(Vector2 startValue, Vector2 endValue, float duration, Func<float, float> easingFunction)
    {
        _startValue = startValue;
        _endValue = endValue;
        Duration = duration;
        _easingFunction = easingFunction ?? (t => t);
    }

    /// <summary>Creates a tween with linear easing (no easing function).</summary>
    public TweenVector2(Vector2 startValue, Vector2 endValue, float duration)
        : this(startValue, endValue, duration, t => t) { }

    /// <summary>The starting value of the tween.</summary>
    public Vector2 StartValue => _startValue;

    /// <summary>The end value of the tween.</summary>
    public Vector2 EndValue => _endValue;

    /// <summary>Total duration of the tween in seconds.</summary>
    public float Duration { get; }

    /// <summary>Time elapsed since the tween started, in seconds. Updated by the owning TweenComponent each frame.</summary>
    internal float Elapsed { get; private set; }

    /// <summary>The easing function applied to the interpolation.</summary>
    public Func<float, float> EasingFunction => _easingFunction;

    /// <summary>Gets whether the tween has completed (elapsed time >= duration).</summary>
    public bool IsComplete => Elapsed >= Duration;

    /// <summary>Whether the tween should loop back to the start when it completes.</summary>
    public bool Loop { get; set; }

    /// <summary>When true and <see cref="Loop"/> is enabled, the tween smoothly reverses direction instead of snapping back (ping-pong).</summary>
    public bool Reverse { get; set; }

    /// <summary>Resets the tween elapsed time to 0 and un-reverses direction (full restart).</summary>
    internal void Reset()
    {
        Elapsed = 0f;
        _reversed = false;
    }

    /// <summary>Toggles direction for reverse looping — flips start/end and resets elapsed.</summary>
    internal void ToggleDirection()
    {
        _reversed = !_reversed;
        Elapsed = 0f;
    }

    /// <summary>Advances the tween by the specified delta time. Called automatically by TweenComponent.Update().</summary>
    internal void Advance(float deltaTime)
    {
        Elapsed = Math.Min(Elapsed + deltaTime, Duration);
    }

    /// <summary>Gets the current eased interpolated value.</summary>
    public Vector2 GetValue()
    {
        float progress = Math.Min(Elapsed / Duration, 1f);
        float t = _easingFunction(progress);
        return _reversed
            ? Vector2.Lerp(_endValue, _startValue, t)
            : Vector2.Lerp(_startValue, _endValue, t);
    }

    /// <summary>Returns a string representation of the Vector2 tween.</summary>
    /// <returns>A string describing the tween's start, end, duration, and elapsed time.</returns>
    public override string ToString() => $"TweenVector2({_startValue} -> {_endValue}, {Duration}s, {Elapsed}s elapsed)";
}

/// <summary>
/// A single-value tween that interpolates a float from a start value to an end value over time using an easing function.
/// The owning <see cref="TweenComponent"/> updates elapsed time each frame; call <see cref="GetValue"/> to get the current eased value.
/// </summary>
public class TweenFloat
{
    private readonly float _startValue;
    private readonly float _endValue;
    private readonly Func<float, float> _easingFunction;
    private bool _reversed;

    /// <summary>
    /// Creates a float tween with the specified easing function.
    /// </summary>
    /// <param name="startValue">The starting float value.</param>
    /// <param name="endValue">The target float value.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="easingFunction">The easing function to apply.</param>
    public TweenFloat(float startValue, float endValue, float duration, Func<float, float> easingFunction)
    {
        _startValue = startValue;
        _endValue = endValue;
        Duration = duration;
        _easingFunction = easingFunction ?? (t => t);
    }

    /// <summary>Creates a tween with linear easing (no easing function).</summary>
    public TweenFloat(float startValue, float endValue, float duration)
        : this(startValue, endValue, duration, t => t) { }

    /// <summary>The starting value of the tween.</summary>
    public float StartValue => _startValue;

    /// <summary>The end value of the tween.</summary>
    public float EndValue => _endValue;

    /// <summary>Total duration of the tween in seconds.</summary>
    public float Duration { get; }

    /// <summary>Time elapsed since the tween started, in seconds. Updated by the owning TweenComponent each frame.</summary>
    internal float Elapsed { get; private set; }

    /// <summary>The easing function applied to the interpolation.</summary>
    public Func<float, float> EasingFunction => _easingFunction;

    /// <summary>Gets whether the tween has completed (elapsed time >= duration).</summary>
    public bool IsComplete => Elapsed >= Duration;

    /// <summary>Whether the tween should loop back to the start when it completes.</summary>
    public bool Loop { get; set; }

    /// <summary>When true and <see cref="Loop"/> is enabled, the tween smoothly reverses direction instead of snapping back (ping-pong).</summary>
    public bool Reverse { get; set; }

    /// <summary>Resets the tween elapsed time to 0 and un-reverses direction (full restart).</summary>
    internal void Reset()
    {
        Elapsed = 0f;
        _reversed = false;
    }

    /// <summary>Toggles direction for reverse looping — flips start/end and resets elapsed.</summary>
    internal void ToggleDirection()
    {
        _reversed = !_reversed;
        Elapsed = 0f;
    }

    /// <summary>Advances the tween by the specified delta time. Called automatically by TweenComponent.Update().</summary>
    internal void Advance(float deltaTime)
    {
        Elapsed = Math.Min(Elapsed + deltaTime, Duration);
    }

    /// <summary>Gets the current eased interpolated value.</summary>
    public float GetValue()
    {
        float progress = Math.Min(Elapsed / Duration, 1f);
        float t = _easingFunction(progress);
        return _reversed
            ? MathHelper.Lerp(_endValue, _startValue, t)
            : MathHelper.Lerp(_startValue, _endValue, t);
    }

    /// <summary>Returns a string representation of the float tween.</summary>
    /// <returns>A string describing the tween's start, end, duration, and elapsed time.</returns>
    public override string ToString() => $"TweenFloat({_startValue} -> {_endValue}, {Duration}s, {Elapsed}s elapsed)";
}
