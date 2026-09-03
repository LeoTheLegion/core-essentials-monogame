using System;
using Xunit;
using CoreEssentials.Timing;

namespace CoreEssentials.Tests.Timing
{
    public class AutoExitTimerTests
    {
        [Fact]
        public void Constructor_WithPositiveDuration_SetsDurationMs()
        {
            var timer = new AutoExitTimer(5.0);

            Assert.Equal(5000.0, timer.DurationMs);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void Constructor_WithNonPositiveDuration_Throws(double seconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutoExitTimer(seconds));
        }

        [Fact]
        public void New_Timer_IsNotExpired()
        {
            var timer = new AutoExitTimer(1.0);

            Assert.False(timer.IsExpired);
            Assert.Equal(0.0, timer.ElapsedMs);
        }

        [Fact]
        public void Tick_BelowDuration_IsNotExpired()
        {
            var timer = new AutoExitTimer(1.0); // 1000ms

            timer.Tick(999.0);

            Assert.False(timer.IsExpired);
            Assert.Equal(999.0, timer.ElapsedMs);
        }

        [Fact]
        public void Tick_ExactlyAtDuration_IsExpired()
        {
            var timer = new AutoExitTimer(1.0); // 1000ms

            timer.Tick(500.0);
            timer.Tick(500.0);

            Assert.True(timer.IsExpired);
            Assert.Equal(1000.0, timer.ElapsedMs);
        }

        [Fact]
        public void Tick_BeyondDuration_IsExpired()
        {
            var timer = new AutoExitTimer(1.0); // 1000ms

            timer.Tick(1500.0);

            Assert.True(timer.IsExpired);
        }

        [Fact]
        public void Tick_AccumulatesAcrossFrames()
        {
            var timer = new AutoExitTimer(2.0); // 2000ms

            timer.Tick(800.0);
            timer.Tick(700.0);
            Assert.False(timer.IsExpired);

            timer.Tick(600.0);
            Assert.True(timer.IsExpired);
            Assert.Equal(2100.0, timer.ElapsedMs);
        }

        [Fact]
        public void Tick_WithNegativeDelta_Throws()
        {
            var timer = new AutoExitTimer(1.0);

            Assert.Throws<ArgumentOutOfRangeException>(() => timer.Tick(-1.0));
        }
    }
}
