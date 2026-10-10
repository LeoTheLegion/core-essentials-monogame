using System;
using CoreEssentials.Assets;
using Xunit;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Tests.Asset;

/// <summary>
/// Drives <see cref="AnimationState"/> frame-timing logic (pure math) by configuring a real
/// <see cref="Sprite"/> through its internal test seams, so no graphics device is required.
/// Covers frame advancement, looping vs one-shot completion (and the completed event), the
/// speed clamp, and the progress properties.
/// </summary>
public class AnimationStateLogicTests
{
    private static Sprite MakeSprite(int frameCount, float frameRate = 0.1f)
    {
        var sprite = new Sprite("anim");
        // Device-free: configure the frame sequence and rate via internal seams (InternalsVisibleTo).
        var frames = new int[frameCount];
        for (var i = 0; i < frameCount; i++) frames[i] = i;
        sprite.TestFrames = frames;
        sprite.TestFrameRate = frameRate;
        return sprite;
    }

    private static GameTime Dt(float seconds) => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));

    [Fact]
    public void EffectiveFrameTime_DividesFrameRateBySpeed()
    {
        var state = new AnimationState(MakeSprite(4, frameRate: 0.2f)) { Speed = 2f };
        Assert.Equal(0.1f, state.EffectiveFrameTime, precision: 5);
    }

    [Fact]
    public void Speed_IsClampedToMinimum()
    {
        var state = new AnimationState(MakeSprite(4));
        state.Speed = 0f;
        Assert.Equal(0.01f, state.Speed, precision: 5);
        state.Speed = -5f;
        Assert.Equal(0.01f, state.Speed, precision: 5);
    }

    [Fact]
    public void Update_AdvancesFrameWhenTimerCrossesEffectiveFrameTime()
    {
        // frameRate 0.1s, speed 1 => one step per 0.1s.
        var state = new AnimationState(MakeSprite(8, frameRate: 0.1f));
        state.Update(Dt(0.1f));
        Assert.Equal(1, state.CurrentFrame);
        state.Update(Dt(0.1f));
        Assert.Equal(2, state.CurrentFrame);
    }

    [Fact]
    public void Update_SubStepAccumulatesAcrossTicks()
    {
        // Two 0.05s ticks should advance a 0.1s frame by exactly one step.
        var state = new AnimationState(MakeSprite(8, frameRate: 0.1f));
        state.Update(Dt(0.05f));
        Assert.Equal(0, state.CurrentFrame);
        state.Update(Dt(0.05f));
        Assert.Equal(1, state.CurrentFrame);
    }

    [Fact]
    public void Update_LoopingAnimation_WrapsToFirstFrame()
    {
        var state = new AnimationState(MakeSprite(3, frameRate: 0.1f)) { IsLooping = true };
        for (var i = 0; i < 3; i++) state.Update(Dt(0.1f)); // reach the last frame then wrap
        Assert.Equal(0, state.CurrentFrame);
    }

    [Fact]
    public void Update_NonLoopingAnimation_StopsAtLastFrameAndRaisesCompleted()
    {
        var state = new AnimationState(MakeSprite(3, frameRate: 0.1f)) { IsLooping = false };
        bool raised = false;
        state.AnimationCompleted += (_, _) => raised = true;

        // Advance past the end (3 steps on a 3-frame clip).
        for (var i = 0; i < 3; i++) state.Update(Dt(0.1f));

        Assert.Equal(2, state.CurrentFrame);   // clamped to the last frame
        Assert.False(state.IsPlaying);          // playback stops
        Assert.True(raised);                    // completed event fired exactly on end
    }

    [Fact]
    public void Update_PausedAnimation_DoesNotAdvance()
    {
        var state = new AnimationState(MakeSprite(8, frameRate: 0.1f));
        state.Pause();
        for (var i = 0; i < 5; i++) state.Update(Dt(0.1f));
        Assert.Equal(0, state.CurrentFrame);
    }

    [Fact]
    public void Update_SingleFrameSprite_DoesNotAdvance()
    {
        var state = new AnimationState(MakeSprite(1, frameRate: 0.1f));
        for (var i = 0; i < 5; i++) state.Update(Dt(0.1f));
        Assert.Equal(0, state.CurrentFrame);
    }

    [Fact]
    public void Reset_RestartsFromFirstFrameAndPlays()
    {
        var state = new AnimationState(MakeSprite(8, frameRate: 0.1f)) { IsLooping = false };
        for (var i = 0; i < 8; i++) state.Update(Dt(0.1f)); // run to completion => paused on last frame
        Assert.False(state.IsPlaying);

        state.Reset();
        Assert.Equal(0, state.CurrentFrame);
        Assert.True(state.IsPlaying);
    }

    [Fact]
    public void AnimationProgress_TracksFractionAcrossSequence()
    {
        // 4 frames; at the very start progress is ~0. After one full frame step it should be > 0 and < 1.
        var state = new AnimationState(MakeSprite(4, frameRate: 0.25f));
        Assert.Equal(0f, state.AnimationProgress, precision: 5);
        state.Update(Dt(0.1f)); // mid-second-frame => partial progress
        var mid = state.AnimationProgress;
        Assert.InRange(mid, 0f, 1f);
    }

    [Fact]
    public void AnimationProgress_SingleFrame_IsZero()
    {
        var state = new AnimationState(MakeSprite(1));
        Assert.Equal(0f, state.AnimationProgress, precision: 5);
    }

    [Fact]
    public void SetFrame_Valid_SetsFrameAndResetsTimer()
    {
        var state = new AnimationState(MakeSprite(8, frameRate: 0.1f));
        state.SetFrame(5);
        Assert.Equal(5, state.CurrentFrame);
    }

    [Fact]
    public void SetFrame_OutOfRange_Throws()
    {
        var state = new AnimationState(MakeSprite(4, frameRate: 0.1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.SetFrame(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.SetFrame(4));
    }

    [Fact]
    public void Stop_ResetsToFirstFrameAndPauses()
    {
        var state = new AnimationState(MakeSprite(8, frameRate: 0.1f));
        for (var i = 0; i < 3; i++) state.Update(Dt(0.1f));
        Assert.Equal(3, state.CurrentFrame);

        state.Stop();
        Assert.Equal(0, state.CurrentFrame);
        Assert.False(state.IsPlaying);
    }

    [Fact]
    public void Play_ResumesAfterPause()
    {
        var state = new AnimationState(MakeSprite(8, frameRate: 0.1f));
        state.Pause();
        Assert.False(state.IsPlaying);

        state.Play();
        Assert.True(state.IsPlaying);
    }
}
