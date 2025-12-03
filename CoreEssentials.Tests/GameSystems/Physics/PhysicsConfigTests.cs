using Xunit;
using CoreEssentials.GameSystems.Physics;

namespace CoreEssentials.Tests.GameSystems.Physics
{
    public class PhysicsConfigTests
    {
        [Fact]
        public void Constructor_DefaultValues_MatchAetherDefaults()
        {
            // Arrange & Act
            var config = new PhysicsConfig();
            
            // Assert - defaults should match Aether's defaults for stability
            Assert.Equal(0, config.Scale);
            Assert.Equal(8, config.VelocityIterations);
            Assert.Equal(3, config.PositionIterations);
            Assert.True(config.ContinuousPhysics);
        }

        [Fact]
        public void Scale_CanBeModified()
        {
            // Arrange
            var config = new PhysicsConfig();
            
            // Act
            config.Scale = 100;
            
            // Assert
            Assert.Equal(100, config.Scale);
        }

        [Fact]
        public void VelocityIterations_CanBeModified()
        {
            // Arrange
            var config = new PhysicsConfig();
            
            // Act
            config.VelocityIterations = 4;
            
            // Assert
            Assert.Equal(4, config.VelocityIterations);
        }

        [Fact]
        public void PositionIterations_CanBeModified()
        {
            // Arrange
            var config = new PhysicsConfig();
            
            // Act
            config.PositionIterations = 2;
            
            // Assert
            Assert.Equal(2, config.PositionIterations);
        }

        [Fact]
        public void ContinuousPhysics_CanBeModified()
        {
            // Arrange
            var config = new PhysicsConfig();
            
            // Act
            config.ContinuousPhysics = false;
            
            // Assert
            Assert.False(config.ContinuousPhysics);
        }

        [Fact]
        public void ParticleSystemRecommendedSettings_CanBeApplied()
        {
            // Arrange
            var config = new PhysicsConfig
            {
                // Recommended settings for 1000+ particle systems
                VelocityIterations = 4,
                PositionIterations = 2,
                ContinuousPhysics = false
            };
            
            // Assert
            Assert.Equal(4, config.VelocityIterations);
            Assert.Equal(2, config.PositionIterations);
            Assert.False(config.ContinuousPhysics);
        }

        [Fact]
        public void HighPrecisionSettings_CanBeApplied()
        {
            // Arrange
            var config = new PhysicsConfig
            {
                // Recommended settings for precision stacking/complex constraints
                VelocityIterations = 10,
                PositionIterations = 4,
                ContinuousPhysics = true
            };
            
            // Assert
            Assert.Equal(10, config.VelocityIterations);
            Assert.Equal(4, config.PositionIterations);
            Assert.True(config.ContinuousPhysics);
        }

        [Fact]
        public void AllProperties_CanBeSetIndependently()
        {
            // Arrange
            var config = new PhysicsConfig();
            
            // Act
            config.Scale = 50;
            config.VelocityIterations = 6;
            config.PositionIterations = 5;
            config.ContinuousPhysics = false;
            
            // Assert - verify all properties are independent
            Assert.Equal(50, config.Scale);
            Assert.Equal(6, config.VelocityIterations);
            Assert.Equal(5, config.PositionIterations);
            Assert.False(config.ContinuousPhysics);
        }
    }
}
