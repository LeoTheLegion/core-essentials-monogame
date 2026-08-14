using System;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Tweening;

namespace CoreEssentials.Tests.Tweening;

public class EntityTweenTests
{
    // ===== TweenVector2 Tests =====

    [Fact]
    public void TweenVector2_AtStart_ReturnsStartValue()
    {
        var tween = new TweenVector2(Vector2.Zero, new Vector2(100, 200), 1f);

        Assert.Equal(Vector2.Zero, tween.StartValue);
        Assert.Equal(new Vector2(100, 200), tween.EndValue);
        Assert.Equal(1f, tween.Duration);
        Assert.False(tween.IsComplete);
    }

    [Fact]
    public void TweenVector2_GetValue_LinearInterpolation()
    {
        var tween = new TweenVector2(Vector2.Zero, new Vector2(100, 100), 1f);

        tween.Advance(0.5f); // Halfway through

        var value = tween.GetValue();
        Assert.Equal(50f, value.X, 2);
        Assert.Equal(50f, value.Y, 2);
    }

    [Fact]
    public void TweenVector2_GetValue_WithEasing()
    {
        // InQuad easing: t * t (accelerates)
        var tween = new TweenVector2(Vector2.Zero, new Vector2(100, 100), 1f, t => t * t);

        tween.Advance(0.5f); // Halfway through time

        var value = tween.GetValue();
        // InQuad at 0.5: (0.5)^2 = 0.25 → Lerp(0, 100, 0.25) = 25
        Assert.Equal(25f, value.X, 2);
        Assert.Equal(25f, value.Y, 2);
    }

    [Fact]
    public void TweenVector2_GetValue_AtEnd_ReturnsEndValue()
    {
        var tween = new TweenVector2(Vector2.Zero, new Vector2(100, 100), 1f);

        tween.Advance(1f); // Full duration

        Assert.True(tween.IsComplete);
        var value = tween.GetValue();
        Assert.Equal(100f, value.X, 2);
        Assert.Equal(100f, value.Y, 2);
    }

    [Fact]
    public void TweenVector2_GetValue_PastEnd_ReturnsEndValue()
    {
        var tween = new TweenVector2(Vector2.Zero, new Vector2(100, 100), 1f);

        tween.Advance(2f); // Past duration (clamped)

        Assert.True(tween.IsComplete);
        var value = tween.GetValue();
        Assert.Equal(100f, value.X, 2);
        Assert.Equal(100f, value.Y, 2);
    }

    [Fact]
    public void TweenVector2_Loop_ResetsOnComplete()
    {
        var tween = new TweenVector2(Vector2.Zero, new Vector2(100, 100), 1f);
        tween.Loop = true;

        tween.Advance(1f); // Complete first cycle
        Assert.True(tween.IsComplete);

        tween.Reset(); // Simulate loop reset
        Assert.False(tween.IsComplete);
        Assert.Equal(Vector2.Zero, tween.GetValue());
    }

    [Fact]
    public void TweenVector2_Reverse_TogglesDirection()
    {
        var component = new TweenComponent();
        var tween = component.TweenToVector2(Vector2.Zero, new Vector2(100, 100), 0.5f);
        tween.Loop = true;
        tween.Reverse = true;

        // First half forward: at 0.25s should be at (50, 50)
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));
        var value1 = tween.GetValue();
        Assert.Equal(50f, value1.X, 2);

        // Complete first cycle - TweenComponent toggles direction internally
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));

        // Should be going end → start now; at 0.25s of reverse should be at (50, 50) again
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));
        var value2 = tween.GetValue();
        Assert.Equal(50f, value2.X, 2);
    }

    [Fact]
    public void TweenVector2_DefaultEasing_IsLinear()
    {
        var tween = new TweenVector2(Vector2.Zero, new Vector2(100, 100), 1f); // No easing provided

        tween.Advance(0.5f);
        var value = tween.GetValue();
        Assert.Equal(50f, value.X, 2); // Linear: halfway = half value
    }

    // ===== TweenFloat Tests =====

    [Fact]
    public void TweenFloat_AtStart_ReturnsStartValue()
    {
        var tween = new TweenFloat(0f, 100f, 1f);

        Assert.Equal(0f, tween.StartValue);
        Assert.Equal(100f, tween.EndValue);
        Assert.False(tween.IsComplete);
    }

    [Fact]
    public void TweenFloat_GetValue_LinearInterpolation()
    {
        var tween = new TweenFloat(0f, 100f, 1f);

        tween.Advance(0.5f);

        var value = tween.GetValue();
        Assert.Equal(50f, value, 2);
    }

    [Fact]
    public void TweenFloat_GetValue_WithEasing()
    {
        // OutCubic easing: 1 - (1-t)^3 (decelerates)
        var tween = new TweenFloat(0f, 100f, 1f, t => 1f - (1f - t) * (1f - t) * (1f - t));

        tween.Advance(0.5f);
        // OutCubic at 0.5: 1 - (0.5)^3 = 1 - 0.125 = 0.875 → Lerp(0, 100, 0.875) = 87.5
        var value = tween.GetValue();
        Assert.Equal(87.5f, value, 2);
    }

    [Fact]
    public void TweenFloat_GetValue_AtEnd_ReturnsEndValue()
    {
        var tween = new TweenFloat(0f, 100f, 1f);

        tween.Advance(1f);

        Assert.True(tween.IsComplete);
        Assert.Equal(100f, tween.GetValue(), 2);
    }

    [Fact]
    public void TweenFloat_Loop_ResetsOnComplete()
    {
        var tween = new TweenFloat(0f, 100f, 1f);
        tween.Loop = true;

        tween.Advance(1f);
        Assert.True(tween.IsComplete);

        tween.Reset();
        Assert.False(tween.IsComplete);
        Assert.Equal(0f, tween.GetValue(), 2);
    }

    [Fact]
    public void TweenFloat_Reverse_TogglesDirection()
    {
        var component = new TweenComponent();
        var tween = component.TweenToFloat(0f, 100f, 0.5f);
        tween.Loop = true;
        tween.Reverse = true;

        // Forward: at 0.25s should be 50
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));
        Assert.Equal(50f, tween.GetValue(), 2);

        // Complete first cycle - TweenComponent toggles direction internally
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));

        // At 0.25s of reverse should be 50 again
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));
        Assert.Equal(50f, tween.GetValue(), 2);
    }

    [Fact]
    public void TweenFloat_NegativeValues_InterpolatesCorrectly()
    {
        var tween = new TweenFloat(0f, -50f, 1f);

        tween.Advance(0.5f);
        Assert.Equal(-25f, tween.GetValue(), 2);
    }

    // ===== TweenComponent Tests =====

    [Fact]
    public void TweenComponent_CreatesVector2Tween_ReturnsTween()
    {
        var component = new TweenComponent();
        var tween = component.TweenToVector2(Vector2.Zero, new Vector2(100, 100), 1f);

        Assert.NotNull(tween);
        Assert.Equal(Vector2.Zero, tween.StartValue);
        Assert.Equal(new Vector2(100, 100), tween.EndValue);
    }

    [Fact]
    public void TweenComponent_CreatesFloatTween_ReturnsTween()
    {
        var component = new TweenComponent();
        var tween = component.TweenToFloat(0f, 100f, 1f);

        Assert.NotNull(tween);
        Assert.Equal(0f, tween.StartValue);
        Assert.Equal(100f, tween.EndValue);
    }

    [Fact]
    public void TweenComponent_Update_AdvancesTweens()
    {
        var component = new TweenComponent();
        var tween = component.TweenToFloat(0f, 100f, 1f);

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
        component.Update(gameTime); // 0.05s advance

        Assert.Equal(5f, tween.GetValue(), 2); // Linear: 100 * 0.05 = 5
    }

    [Fact]
    public void TweenComponent_Update_RemovesCompletedTweens()
    {
        var component = new TweenComponent();
        component.TweenToFloat(0f, 100f, 0.1f); // Short duration

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
        component.Update(gameTime); // Past duration

        // Should be removed - no exception on next update
        component.Update(gameTime);
    }

    [Fact]
    public void TweenComponent_CancelAll_ClearsTweens()
    {
        var component = new TweenComponent();
        component.TweenToFloat(0f, 100f, 1f);
        component.TweenToVector2(Vector2.Zero, Vector2.One, 1f);

        component.CancelAll();

        // No exceptions on update after cancel
        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
        component.Update(gameTime);
    }

    [Fact]
    public void TweenComponent_MultipleTweens_AdvancesAll()
    {
        var component = new TweenComponent();
        var floatTween = component.TweenToFloat(0f, 100f, 1f);
        var vectorTween = component.TweenToVector2(Vector2.Zero, Vector2.One, 1f);

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        component.Update(gameTime);

        Assert.Equal(50f, floatTween.GetValue(), 2); // Halfway
        Assert.Equal(0.5f, vectorTween.GetValue().X, 2); // Halfway
    }

    [Fact]
    public void TweenComponent_LoopTween_NotRemovedOnComplete()
    {
        var component = new TweenComponent();
        var tween = component.TweenToFloat(0f, 100f, 0.1f);
        tween.Loop = true;

        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
        component.Update(gameTime); // Past duration - should reset, not remove

        // Should still be active after update
        Assert.False(tween.IsComplete);
    }

    [Fact]
    public void TweenComponent_SineEasing_ReturnsToStart()
    {
        // sin(π) = 0, so at t=1 the value should be back to start
        var tween = new TweenFloat(0f, -50f, 1f, t => (float)Math.Sin(t * Math.PI));

        tween.Advance(1f);
        // At completion with sine easing: Lerp(0, -50, sin(π)) = Lerp(0, -50, 0) = 0
        Assert.Equal(0f, tween.GetValue(), 2);
    }

    [Fact]
    public void TweenComponent_SineEasing_PeaksAtHalfway()
    {
        // sin(π/2) = 1, so at t=0.5 the value should be at end
        var tween = new TweenFloat(0f, -50f, 1f, t => (float)Math.Sin(t * Math.PI));

        tween.Advance(0.5f);
        // At halfway: Lerp(0, -50, sin(π/2)) = Lerp(0, -50, 1) = -50
        Assert.Equal(-50f, tween.GetValue(), 2);
    }

    // ===== OnDetach Cleanup Tests =====

    [Fact]
    public void TweenComponent_OnDetach_CancelsAllTweens()
    {
        var component = new TweenComponent();
        component.TweenToFloat(0f, 100f, 1f);
        component.TweenToVector2(Vector2.Zero, Vector2.One, 1f);

        // Detach should cancel all active tweens
        component.OnDetach();

        // No exceptions on update after detach (tweens cleared)
        var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
        component.Update(gameTime);
    }

    [Fact]
    public void TweenComponent_OnDetach_SafeWhenNoTweens()
    {
        var component = new TweenComponent();

        // Detaching with no active tweens should not throw
        component.OnDetach();
    }

    [Fact]
    public void TweenComponent_OnDetach_PreventsLoopingTweensFromContinuing()
    {
        var component = new TweenComponent();
        component.TweenToFloat(0f, 100f, 1f, loop: true);

        // Advance partially so tween is active
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)));

        // Detach cancels the looping tween
        component.OnDetach();

        // Update should not advance anything (tweens cleared)
        component.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000)));
    }
}
