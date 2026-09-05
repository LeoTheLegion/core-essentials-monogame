using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class PrefabOverrideTests : IDisposable
    {
        private readonly EntitySystem _system = new();

        [Fact]
        public void Override_CreatedComponent_SeesFinalValueInOnAttach()
        {
            // Arrange — prefab creates ProbeComponent with Base="default"
            var prefab = new Prefab
            {
                Type = nameof(ProbeEntity),
                Components =
                {
                    new Prefab.ComponentDefinition
                    {
                        Type = nameof(ProbeComponent),
                        Properties = { ["Base"] = "default" }
                    }
                }
            };
            _system.RegisterPrefab("probe", prefab);

            // Act
            var entity = (ProbeEntity)_system.Instantiate("probe", Vector2.Zero,
                new Dictionary<string, Dictionary<string, string>>
                {
                    [nameof(ProbeComponent)] = new() { ["Base"] = "overridden" }
                });

            // Assert — the override was visible when OnAttach ran, not just afterwards
            var probe = entity.Components.OfType<ProbeComponent>().Single();
            Assert.Equal("overridden", probe.Base);
            Assert.Equal("overridden", probe.SeenBaseAtOnAttach);
        }

        [Fact]
        public void Override_ExistingComponent_SeesFinalValueInOnAttach()
        {
            // Arrange — entity adds its own ProbeComponent in OnStart (like TextEntity + LabelComponent)
            var prefab = new Prefab
            {
                Type = nameof(SelfHostingEntity),
                Components =
                {
                    new Prefab.ComponentDefinition
                    {
                        Type = nameof(ProbeComponent),
                        Properties = { ["Base"] = "overridden" }
                    }
                }
            };
            _system.RegisterPrefab("selfhost", prefab);

            // Act
            var entity = (SelfHostingEntity)_system.Instantiate("selfhost", Vector2.Zero, null);

            // Assert — the component that OnStart added saw the override in OnAttach
            var probe = entity.Components.OfType<ProbeComponent>().Single();
            Assert.Equal("overridden", probe.Base);
            Assert.Equal("overridden", probe.SeenBaseAtOnAttach);
        }

        [Fact]
        public void Override_MergesIntoExistingProperties_WithoutMutatingPrefab()
        {
            // Arrange
            var prefab = new Prefab
            {
                Type = nameof(ProbeEntity),
                Components =
                {
                    new Prefab.ComponentDefinition
                    {
                        Type = nameof(ProbeComponent),
                        Properties = { ["Base"] = "default", ["Count"] = "1" }
                    }
                }
            };
            _system.RegisterPrefab("probe", prefab);

            // Act — override only Base; Count must survive the merge
            var entity = (ProbeEntity)_system.Instantiate("probe", Vector2.Zero,
                new Dictionary<string, Dictionary<string, string>>
                {
                    [nameof(ProbeComponent)] = new() { ["Base"] = "overridden" }
                });

            var probe = entity.Components.OfType<ProbeComponent>().Single();
            Assert.Equal("overridden", probe.Base);
            Assert.Equal(1, probe.Count);

            // Assert — the registered prefab is untouched
            Assert.Equal("default", prefab.Components[0].Properties["Base"]);
            Assert.Equal(2, prefab.Components[0].Properties.Count);
        }

        [Fact]
        public void Override_UnresolvableComponentType_Throws()
        {
            // Arrange
            var prefab = new Prefab { Type = nameof(ProbeEntity) };
            _system.RegisterPrefab("probe", prefab);

            // Act & Assert
            Assert.Throws<FormatException>(() => _system.Instantiate("probe", Vector2.Zero,
                new Dictionary<string, Dictionary<string, string>>
                {
                    ["NoSuchComponent12345"] = new() { ["Base"] = "x" }
                }));
        }

        [Fact]
        public void Override_MatchesComponentByShortName()
        {
            // Arrange — the prefab stores a fully-qualified component type name
            var prefab = new Prefab
            {
                Type = nameof(ProbeEntity),
                Components =
                {
                    new Prefab.ComponentDefinition
                    {
                        Type = typeof(ProbeComponent).FullName,
                        Properties = { ["Base"] = "default" }
                    }
                }
            };
            _system.RegisterPrefab("probe", prefab);

            // Act — override key uses the short name
            var entity = (ProbeEntity)_system.Instantiate("probe", Vector2.Zero,
                new Dictionary<string, Dictionary<string, string>>
                {
                    [nameof(ProbeComponent)] = new() { ["Base"] = "short" }
                });

            // Assert — merged into the existing component, not added as a duplicate
            var probe = entity.Components.OfType<ProbeComponent>().Single();
            Assert.Equal("short", probe.Base);
        }

        [Fact]
        public void Instantiate_WithoutOverrides_PrefabUnchanged()
        {
            // Arrange
            var prefab = new Prefab
            {
                Type = nameof(ProbeEntity),
                Components =
                {
                    new Prefab.ComponentDefinition
                    {
                        Type = nameof(ProbeComponent),
                        Properties = { ["Base"] = "default" }
                    }
                }
            };
            _system.RegisterPrefab("probe", prefab);

            // Act — plain instantiation, twice
            var first = (ProbeEntity)_system.Instantiate("probe", Vector2.Zero);
            var second = (ProbeEntity)_system.Instantiate("probe", Vector2.Zero);

            // Assert
            Assert.Equal("default", first.Components.OfType<ProbeComponent>().Single().Base);
            Assert.Equal("default", second.Components.OfType<ProbeComponent>().Single().Base);
            Assert.Equal("default", prefab.Components[0].Properties["Base"]);
        }

        public void Dispose() => _system.Dispose();

        // ──────────────────────────── Test fixtures ────────────────────────────

        public class ProbeEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        /// <summary>Entity that adds its own component in OnStart — models entities like TextEntity.</summary>
        public class SelfHostingEntity : ProbeEntity
        {
            public override void OnStart()
            {
                base.OnStart();
                AddComponent(new ProbeComponent { Base = "start" });
            }
        }

        /// <summary>Component that records what it saw in OnAttach.</summary>
        public class ProbeComponent : EntityComponent
        {
            private string _base = "unset";
            private int _count;

            public string Base
            {
                get => _base;
                set => _base = value;
            }

            public int Count
            {
                get => _count;
                set => _count = value; // string→int conversion happens in SerializationUtils.ParseValue before this setter runs
            }

            public string? SeenBaseAtOnAttach { get; private set; }

            public override void OnAttach()
            {
                SeenBaseAtOnAttach = Base;
            }
        }
    }
}
