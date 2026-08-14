using System;
using CoreEssentials.Tweening;
using Xunit;

namespace CoreEssentials.Tests.Tweening;

public class EasingFunctionsTests
{
    [Fact]
    public void Linear_ReturnsIdentity()
    {
        Assert.Equal(0f, EasingFunctions.Linear(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.Linear(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.Linear(1f), 4);
    }

    [Fact]
    public void InQuad_AcceleratesFromRest()
    {
        Assert.Equal(0f, EasingFunctions.InQuad(0f), 4);
        Assert.Equal(0.25f, EasingFunctions.InQuad(0.5f), 4); // (0.5)² = 0.25
        Assert.Equal(1f, EasingFunctions.InQuad(1f), 4);
    }

    [Fact]
    public void OutQuad_DeceleratesToRest()
    {
        Assert.Equal(0f, EasingFunctions.OutQuad(0f), 4);
        Assert.Equal(0.75f, EasingFunctions.OutQuad(0.5f), 4); // 0.5 * (2 - 0.5) = 0.75
        Assert.Equal(1f, EasingFunctions.OutQuad(1f), 4);
    }

    [Fact]
    public void InOutQuad_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutQuad(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutQuad(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutQuad(1f), 4);
    }

    [Fact]
    public void InCubic_AcceleratesFromRest()
    {
        Assert.Equal(0f, EasingFunctions.InCubic(0f), 4);
        Assert.Equal(0.125f, EasingFunctions.InCubic(0.5f), 4); // (0.5)³ = 0.125
        Assert.Equal(1f, EasingFunctions.InCubic(1f), 4);
    }

    [Fact]
    public void OutCubic_DeceleratesToRest()
    {
        Assert.Equal(0f, EasingFunctions.OutCubic(0f), 4);
        Assert.Equal(0.875f, EasingFunctions.OutCubic(0.5f), 4); // (−0.5)³ + 1 = 0.875
        Assert.Equal(1f, EasingFunctions.OutCubic(1f), 4);
    }

    [Fact]
    public void InSine_AcceleratesUsingSine()
    {
        Assert.Equal(0f, EasingFunctions.InSine(0f), 4);
        Assert.Equal(0.2929f, EasingFunctions.InSine(0.5f), 3); // 1 - cos(π/4) ≈ 0.2929
        Assert.Equal(1f, EasingFunctions.InSine(1f), 4);
    }

    [Fact]
    public void OutSine_DeceleratesUsingSine()
    {
        Assert.Equal(0f, EasingFunctions.OutSine(0f), 4);
        Assert.Equal(0.7071f, EasingFunctions.OutSine(0.5f), 3); // sin(π/4) ≈ 0.7071
        Assert.Equal(1f, EasingFunctions.OutSine(1f), 4);
    }

    [Fact]
    public void InOutSine_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutSine(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutSine(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutSine(1f), 4);
    }

    [Fact]
    public void OutElastic_DeceleratesWithOvershoot()
    {
        Assert.Equal(0f, EasingFunctions.OutElastic(0f), 4);
        Assert.Equal(1f, EasingFunctions.OutElastic(1f), 4);
        // At halfway should be somewhere between 0 and 1
        var mid = EasingFunctions.OutElastic(0.5f);
        Assert.True(mid > 0f && mid < 1.5f);
    }

    [Fact]
    public void OutBounce_DeceleratesWithBounce()
    {
        Assert.Equal(0f, EasingFunctions.OutBounce(0f), 4);
        Assert.Equal(1f, EasingFunctions.OutBounce(1f), 4);
        // At halfway should be > 0.5 (bounces up fast)
        var mid = EasingFunctions.OutBounce(0.5f);
        Assert.True(mid > 0.5f && mid < 1f);
    }

    [Fact]
    public void InBack_AcceleratesWithOvershoot()
    {
        Assert.Equal(0f, EasingFunctions.InBack(0f), 4);
        Assert.Equal(1f, EasingFunctions.InBack(1f), 4);
        // At halfway should overshoot negative then come back
        var mid = EasingFunctions.InBack(0.5f);
        Assert.True(mid > -0.2f && mid < 0.5f);
    }

    [Fact]
    public void OutBack_DeceleratesWithOvershoot()
    {
        Assert.Equal(0f, EasingFunctions.OutBack(0f), 4);
        Assert.Equal(1f, EasingFunctions.OutBack(1f), 4);
        // At halfway should overshoot past 1 then settle
        var mid = EasingFunctions.OutBack(0.5f);
        Assert.True(mid > 0.5f && mid < 1.3f);
    }

    [Fact]
    public void AllEasingFunctions_StartAtZero()
    {
        // All easing functions should return 0 at t=0
        Assert.Equal(0f, EasingFunctions.Linear(0f), 4);
        Assert.Equal(0f, EasingFunctions.InQuad(0f), 4);
        Assert.Equal(0f, EasingFunctions.InCubic(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutQuad(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutCubic(0f), 4);
        Assert.Equal(0f, EasingFunctions.InOutQuad(0f), 4);
        Assert.Equal(0f, EasingFunctions.InSine(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutSine(0f), 4);
        Assert.Equal(0f, EasingFunctions.InOutSine(0f), 4);
        Assert.Equal(0f, EasingFunctions.InExpo(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutExpo(0f), 4);
        Assert.Equal(0f, EasingFunctions.InCirc(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutCirc(0f), 4);
        Assert.Equal(0f, EasingFunctions.InElastic(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutElastic(0f), 4);
        Assert.Equal(0f, EasingFunctions.InBack(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutBack(0f), 4);
        Assert.Equal(0f, EasingFunctions.InBounce(0f), 4);
        Assert.Equal(0f, EasingFunctions.OutBounce(0f), 4);
    }

    [Fact]
    public void AllEasingFunctions_EndAtOne()
    {
        // All easing functions should return 1 at t=1
        Assert.Equal(1f, EasingFunctions.Linear(1f), 4);
        Assert.Equal(1f, EasingFunctions.InQuad(1f), 4);
        Assert.Equal(1f, EasingFunctions.InCubic(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutQuad(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutCubic(1f), 4);
        Assert.Equal(1f, EasingFunctions.InOutQuad(1f), 4);
        Assert.Equal(1f, EasingFunctions.InSine(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutSine(1f), 4);
        Assert.Equal(1f, EasingFunctions.InOutSine(1f), 4);
        Assert.Equal(1f, EasingFunctions.InExpo(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutExpo(1f), 4);
        Assert.Equal(1f, EasingFunctions.InCirc(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutCirc(1f), 4);
        Assert.Equal(1f, EasingFunctions.InElastic(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutElastic(1f), 4);
        Assert.Equal(1f, EasingFunctions.InBack(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutBack(1f), 4);
        Assert.Equal(1f, EasingFunctions.InBounce(1f), 4);
        Assert.Equal(1f, EasingFunctions.OutBounce(1f), 4);
    }

    [Fact]
    public void EasingFunction_WithTweenVector2_AppliesCorrectly()
    {
        var tween = new TweenVector2(
            Microsoft.Xna.Framework.Vector2.Zero,
            new Microsoft.Xna.Framework.Vector2(100f, 100f),
            1f,
            EasingFunctions.InQuad);

        tween.Advance(0.5f); // Halfway through time

        var value = tween.GetValue();
        Assert.Equal(25f, value.X, 2); // InQuad(0.5) = 0.25 → Lerp(0, 100, 0.25) = 25
        Assert.Equal(25f, value.Y, 2);
    }

    [Fact]
    public void EasingFunction_WithTweenFloat_AppliesCorrectly()
    {
        var tween = new TweenFloat(0f, 100f, 1f, EasingFunctions.OutCubic);

        tween.Advance(0.5f); // Halfway through time

        var value = tween.GetValue();
        Assert.Equal(87.5f, value, 2); // OutCubic(0.5) = 0.875 → Lerp(0, 100, 0.875) = 87.5
    }
}
