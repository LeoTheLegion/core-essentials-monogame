using System;
using Xunit;
using CoreEssentials.Timing;

namespace CoreEssentials.Tests.Timing
{
    public class TimeTests
    {
        [Fact]
        public void SetDeltaTime_WithPositiveValue_SetsDeltaTimeCorrectly()
        {
            // Arrange
            double positiveDeltaTime = 16.0; // e.g., 16ms for 60 FPS

            // Act
            Time.SetDeltaTime(positiveDeltaTime);

            // Assert
            Assert.Equal(positiveDeltaTime, Time.DeltaTime);
        }

        [Fact]
        public void SetDeltaTime_WithZeroValue_SetsDeltaTimeCorrectly()
        {
            // Arrange
            double zeroDeltaTime = 0.0;

            // Act
            Time.SetDeltaTime(zeroDeltaTime);

            // Assert
            Assert.Equal(zeroDeltaTime, Time.DeltaTime);
        }

        [Fact]
        public void SetDeltaTime_WithNegativeValue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double negativeDeltaTime = -1.0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Time.SetDeltaTime(negativeDeltaTime));
            Assert.Equal("deltaTime", exception.ParamName); // Verify the parameter name in the exception
        }
    }
}
