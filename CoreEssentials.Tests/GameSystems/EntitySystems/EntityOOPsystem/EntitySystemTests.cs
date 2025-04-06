using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    public class EntitySystemTests
    {
        [Fact]
        public void EntitySystem_CallsLoadAssets()
        {
            // Just confirms LoadAssets doesn't throw or fail
            var entitySystem = new EntitySystem();
            entitySystem.LoadAssets();
        }
    }
}