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

    [Fact]
    public void InQuart_AcceleratesFromRest()
    {
        Assert.Equal(0f, EasingFunctions.InQuart(0f), 4);
        Assert.Equal(0.0625f, EasingFunctions.InQuart(0.5f), 4); // (0.5)⁴
        Assert.Equal(1f, EasingFunctions.InQuart(1f), 4);
    }

    [Fact]
    public void OutQuart_DeceleratesToRest()
    {
        Assert.Equal(0f, EasingFunctions.OutQuart(0f), 4);
        Assert.Equal(1f, EasingFunctions.OutQuart(1f), 4);
        var mid = EasingFunctions.OutQuart(0.5f);
        Assert.True(mid > 0.9f && mid <= 1f);
    }

    [Fact]
    public void InQuint_AcceleratesFromRest()
    {
        Assert.Equal(0f, EasingFunctions.InQuint(0f), 4);
        Assert.Equal(0.03125f, EasingFunctions.InQuint(0.5f), 4); // (0.5)⁵
        Assert.Equal(1f, EasingFunctions.InQuint(1f), 4);
    }

    [Fact]
    public void OutQuint_DeceleratesToRest()
    {
        Assert.Equal(0f, EasingFunctions.OutQuint(0f), 4);
        Assert.Equal(1f, EasingFunctions.OutQuint(1f), 4);
        var mid = EasingFunctions.OutQuint(0.5f); // 1 + (-0.5)^5 ≈ 0.96875
        Assert.True(mid > 0.96f && mid <= 1f);
    }

    [Fact]
    public void InExpo_AcceleratesUsingExponential()
    {
        Assert.Equal(0f, EasingFunctions.InExpo(0f), 4);
        Assert.Equal(1f, EasingFunctions.InExpo(1f), 4);
        var mid = EasingFunctions.InExpo(0.5f);
        Assert.True(mid > 0f && mid < 0.1f); // 2^(10*(0.5-1)) ≈ 0.0312
    }

    [Fact]
    public void OutExpo_DeceleratesUsingExponential()
    {
        Assert.Equal(0f, EasingFunctions.OutExpo(0f), 4);
        Assert.Equal(1f, EasingFunctions.OutExpo(1f), 4);
        var mid = EasingFunctions.OutExpo(0.5f);
        Assert.True(mid > 0.96f && mid < 1f); // 1 - 2^-5 ≈ 0.96875
    }

    [Fact]
    public void InCirc_AcceleratesUsingCircle()
    {
        Assert.Equal(0f, EasingFunctions.InCirc(0f), 4);
        Assert.Equal(1f, EasingFunctions.InCirc(1f), 4);
        var mid = EasingFunctions.InCirc(0.5f); // 1 - sqrt(1-0.25) ≈ 0.134
        Assert.True(mid > 0.1f && mid < 0.2f);
    }

    [Fact]
    public void OutCirc_DeceleratesUsingCircle()
    {
        Assert.Equal(0f, EasingFunctions.OutCirc(0f), 4);
        Assert.Equal(1f, EasingFunctions.OutCirc(1f), 4);
        var mid = EasingFunctions.OutCirc(0.5f); // sqrt(1 - 0.25) ≈ 0.866
        Assert.True(mid > 0.8f && mid < 0.9f);
    }

    [Fact]
    public void InOutCubic_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutCubic(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutCubic(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutCubic(1f), 4);
    }

    [Fact]
    public void InOutQuart_AcceleratesThenDecelerates()
    {
        // Standard quartic in-out contract: anchored at the ends and midpoint.
        Assert.Equal(0f, EasingFunctions.InOutQuart(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutQuart(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutQuart(1f), 4);

        // No discontinuity at the branch boundary: sampling close to 0.5 from either side stays
        // within a small band around 0.5 (a broken else-branch would jump by >> this margin).
        Assert.InRange(EasingFunctions.InOutQuart(0.499f), 0.49f, 0.51f);
        Assert.InRange(EasingFunctions.InOutQuart(0.501f), 0.49f, 0.51f);
    }

    [Fact]
    public void InOutQuint_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutQuint(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutQuint(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutQuint(1f), 4);
    }

    [Fact]
    public void InOutExpo_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutExpo(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutExpo(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutExpo(1f), 4);
    }

    [Fact]
    public void InOutCirc_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutCirc(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutCirc(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutCirc(1f), 4);
    }

    [Fact]
    public void InOutBack_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutBack(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutBack(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutBack(1f), 4);
    }

    [Fact]
    public void InOutBounce_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutBounce(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutBounce(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutBounce(1f), 4);
    }

    [Fact]
    public void InOutElastic_AcceleratesThenDecelerates()
    {
        Assert.Equal(0f, EasingFunctions.InOutElastic(0f), 4);
        Assert.Equal(0.5f, EasingFunctions.InOutElastic(0.5f), 4);
        Assert.Equal(1f, EasingFunctions.InOutElastic(1f), 4);
    }

    [Fact]
    public void InBounce_UsesOutBounceInverse()
    {
        Assert.Equal(0f, EasingFunctions.InBounce(0f), 4);
        Assert.Equal(1f, EasingFunctions.InBounce(1f), 4);
        // Should stay within a reasonable band.
        var mid = EasingFunctions.InBounce(0.5f);
        Assert.True(mid >= 0f && mid <= 1f);
    }

    [Fact]
    public void AllEasingFunctions_AreMonotonicNonDecreasingOrWellBehaved()
    {
        // For the family of functions that are monotonic non-decreasing, ensure value at 1 is >= value at 0.5.
        Func<float, float>[] monotonic =
        {
            EasingFunctions.Linear,
            EasingFunctions.InQuad,
            EasingFunctions.OutQuad,
            EasingFunctions.InCubic,
            EasingFunctions.InQuart,
            EasingFunctions.InQuint,
            EasingFunctions.InSine,
            EasingFunctions.InExpo,
            EasingFunctions.InCirc,
        };

        foreach (var f in monotonic)
        {
            var atHalf = f(0.5f);
            var atOne = f(1f);
            Assert.True(atOne >= atHalf - 1e-4f);
        }
    }
}
