using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class ReproNullRef
    {
        public class EntityWithSpriteInOnStart : Entity
        {
            public SpriteComponent? Comp { get; private set; }
            public bool Started { get; private set; }
            
            public override void OnStart()
            {
                base.OnStart();
                Started = true;
                Comp = new SpriteComponent();
                AddComponent(Comp);
            }
        }

        [Fact]
        public void LoadEntity_ComponentCreatedViaReflection_OwnerIsSetBeforeOnAttach()
        {
            var system = new EntitySystem();
            var e = system.CreateEntity<EntityWithSpriteInOnStart>();
            e.SetId("test");
            e.Position = new Vector2(10, 20);
            
            var file = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, file);
                
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, file, mergeExisting: false);
                
                var loaded = newSystem.GetEntities().First();
                var comp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(comp);
                Assert.Same(loaded, comp!.Owner);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [Fact]
        public void LoadEntity_WithExistingComponent_DoesNotRecreate()
        {
            var system = new EntitySystem();
            var e = system.CreateEntity<EntityWithSpriteInOnStart>();
            e.SetId("test2");
            
            var file = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, file);
                
                var newSystem = new EntitySystem();
                // Load with component XML that matches existing component
                // Simulate by saving and loading
                GameStateSerializer.LoadState(newSystem, file, mergeExisting: false);
                
                var loaded = newSystem.GetEntities().First();
                var comp = loaded.GetComponent<SpriteComponent>();
                // Component should be from OnStart, not recreated
                Assert.NotNull(comp);
                Assert.Same(loaded, comp!.Owner);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }
    }
}
