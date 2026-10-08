using CoreEssentials.GameSystems.Physics.Types;
using Xunit;

namespace CoreEssentials.Tests
{
    /// <summary>
    /// SolverConfig is a plain configuration POCO. These tests lock in the documented
    /// defaults and confirm each property is settable (round-trips through the setter).
    /// </summary>
    public class SolverConfigTests
    {
        [Fact]
        public void Defaults_MatchDocumentedValues()
        {
            var config = new SolverConfig();

            Assert.Equal(8, config.VelocityIterations);
            Assert.Equal(3, config.PositionIterations);
            Assert.False(config.ContinuousCollisionDetection);
        }

        [Fact]
        public void VelocityIterations_CanBeSet()
        {
            var config = new SolverConfig();

            config.VelocityIterations = 16;

            Assert.Equal(16, config.VelocityIterations);
        }

        [Fact]
        public void PositionIterations_CanBeSet()
        {
            var config = new SolverConfig();

            config.PositionIterations = 5;

            Assert.Equal(5, config.PositionIterations);
        }

        [Fact]
        public void ContinuousCollisionDetection_CanBeEnabled()
        {
            var config = new SolverConfig();

            config.ContinuousCollisionDetection = true;

            Assert.True(config.ContinuousCollisionDetection);
        }
    }
}
