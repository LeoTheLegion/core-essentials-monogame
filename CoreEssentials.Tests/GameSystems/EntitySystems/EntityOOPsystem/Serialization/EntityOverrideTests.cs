using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    /// <summary>
    /// Tests for per-instantiation ENTITY-level property overrides (issue #81's "entity-level values"
    /// and the Sprint 5d blocker). Component overrides already existed; these prove that properties
    /// living on the entity itself — with no component to target — can be set from both the C# API
    /// and scene XML (&lt;EntityOverrides&gt;), and are applied BEFORE OnStart/OnAttach.
    /// </summary>
    public class EntityOverrideTests : IDisposable
    {
        private readonly EntitySystem _system = new();

        public void Dispose() => _system.Dispose();

        // ──────────────────────────── C# API: Instantiate with entity overrides ────────────────────────────

        [Fact]
        public void Instantiate_EntityOverride_AppliedBeforeOnStart()
        {
            // Arrange — prefab creates a SelfStateEntity (state lives on the entity, like TextEntity)
            var prefab = new Prefab { Type = nameof(SelfStateEntity) };
            _system.RegisterPrefab("self", prefab);

            // Act
            var entity = (SelfStateEntity)_system.Instantiate("self", Vector2.Zero, null,
                new Dictionary<string, string> { ["Text"] = "hello" });

            // Assert — the override was visible when OnStart ran, not just afterwards
            Assert.Equal("hello", entity.Text);
            Assert.Equal("hello", entity.SeenTextAtOnStart);
        }

        [Fact]
        public void Instantiate_EntityOverride_MultiplePropertiesAndTypes()
        {
            // Arrange — exercises string, float, int, bool, Vector2, Color and enum parsing paths
            var prefab = new Prefab { Type = nameof(SelfStateEntity) };
            _system.RegisterPrefab("self", prefab);

            // Act
            var entity = (SelfStateEntity)_system.Instantiate("self", Vector2.Zero, null,
                new Dictionary<string, string>
                {
                    ["Text"] = "t",
                    ["Speed"] = "3.5",
                    ["Hits"] = "7",
                    ["Enabled"] = "true",
                    ["Offset"] = "1,2",
                    ["Tint"] = "Red",
                    ["Mode"] = "Fast"
                });

            // Assert — every type parsed and applied to the entity itself
            Assert.Equal(3.5f, entity.Speed);
            Assert.Equal(7, entity.Hits);
            Assert.True(entity.Enabled);
            Assert.Equal(new Vector2(1, 2), entity.Offset);
            Assert.Equal(Color.Red, entity.Tint);
            Assert.Equal(SelfStateEntity.ModeEnum.Fast, entity.Mode);
        }

        [Fact]
        public void Instantiate_EntityOverride_DoesNotMutateRegisteredPrefab()
        {
            // Arrange
            var prefab = new Prefab { Type = nameof(SelfStateEntity) };
            _system.RegisterPrefab("self", prefab);

            // Act — two instantiations, one with an override
            var plain = (SelfStateEntity)_system.Instantiate("self", Vector2.Zero);
            var overridden = (SelfStateEntity)_system.Instantiate("self", Vector2.Zero, null,
                new Dictionary<string, string> { ["Text"] = "x" });

            // Assert — the override only affected its own instance; the prefab carries no entity overrides
            Assert.Equal("", plain.Text);
            Assert.Equal("x", overridden.Text);
            Assert.Empty(prefab.EntityOverrides);
        }

        [Fact]
        public void Instantiate_EntityAndComponentOverrides_AppliedTogether()
        {
            // Arrange — a component override AND an entity override in the same call
            var prefab = new Prefab
            {
                Type = nameof(SelfStateEntity),
                Components =
                {
                    new Prefab.ComponentDefinition
                    {
                        Type = nameof(EntityProbeComponent),
                        Properties = { ["Base"] = "default" }
                    }
                }
            };
            _system.RegisterPrefab("self", prefab);

            // Act
            var entity = (SelfStateEntity)_system.Instantiate("self", Vector2.Zero,
                new Dictionary<string, Dictionary<string, string>>
                {
                    [nameof(EntityProbeComponent)] = new() { ["Base"] = "comp" }
                },
                new Dictionary<string, string> { ["Text"] = "ent" });

            // Assert — both landed on their respective targets
            Assert.Equal("ent", entity.Text);
            Assert.Equal("comp", entity.Components.OfType<EntityProbeComponent>().Single().Base);
        }

        [Fact]
        public void PrefabOverrides_EntityOnly_ReturnsCloneWithEntityOverrides()
        {
            // Arrange
            var prefab = new Prefab { Type = nameof(SelfStateEntity) };

            // Act — entity overrides only (no component overrides)
            var result = PrefabOverrides.Apply(prefab, null,
                new Dictionary<string, string> { ["Text"] = "v" });

            // Assert — a clone is returned carrying the override; the input is untouched
            Assert.NotSame(prefab, result);
            Assert.Equal("v", result.EntityOverrides["Text"]);
            Assert.Empty(prefab.EntityOverrides);
        }

        [Fact]
        public void PrefabOverrides_Nothing_ReturnsInputUnchanged()
        {
            // Arrange
            var prefab = new Prefab { Type = nameof(SelfStateEntity) };

            // Act & Assert — null/empty overrides return the same instance (no needless clone)
            Assert.Same(prefab, PrefabOverrides.Apply(prefab, null, null));
            Assert.Same(prefab, PrefabOverrides.Apply(prefab,
                new Dictionary<string, Dictionary<string, string>>(),
                new Dictionary<string, string>()));
        }

        // ──────────────────────────── Parser: <EntityOverrides> element ────────────────────────────

        [Fact]
        public void SceneParser_EntityOverridesElement_PopulatesDefinition()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""SelfStateEntity"" Id=""e1"">
          <EntityOverrides>
            <Property Name=""Text"" Value=""from xml"" />
            <Property Name=""Speed"" Value=""2.5"" />
          </EntityOverrides>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var scene = SceneParser.Parse(xml);
            var def = Assert.Single(scene.Systems[0].Entities);

            Assert.Equal("from xml", def.EntityOverrides["Text"]);
            Assert.Equal("2.5", def.EntityOverrides["Speed"]);
        }

        [Fact]
        public void SceneParser_EntityOverrides_MissingName_Throws()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""SelfStateEntity"" Id=""e1"">
          <EntityOverrides>
            <Property Value=""no name"" />
          </EntityOverrides>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void SceneParser_EntityOverrides_UnknownChild_Throws()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""SelfStateEntity"" Id=""e1"">
          <EntityOverrides>
            <NotAProperty Name=""x"" Value=""y"" />
          </EntityOverrides>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        // ──────────────────────────── End-to-end: full data-driven scene ────────────────────────────

        [Fact]
        public void DataDrivenScene_EntityOverride_FromXml_AppliedToLoadedEntity()
        {
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""SelfStateEntity"" Id=""hero"">
          <Position X=""5"" Y=""6"" />
          <EntityOverrides>
            <Property Name=""Text"" Value=""spawned"" />
            <Property Name=""Hits"" Value=""3"" />
          </EntityOverrides>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));

                // Act — drive the load to completion
                scene.Load();
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                // Assert — the entity loaded from data carries its overridden values
                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();
                var hero = (SelfStateEntity)entitySystem.FindById("hero");
                Assert.NotNull(hero);
                Assert.Equal(new Vector2(5, 6), hero.Position);
                Assert.Equal("spawned", hero.Text);
                Assert.Equal(3, hero.Hits);
                // And the override was visible during initialization, not just afterwards.
                Assert.Equal("spawned", hero.SeenTextAtOnStart);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void DataDrivenScene_NestedChild_EntityOverride_Applied()
        {
            // A nested scene <Children> entity also receives its own <EntityOverrides>, proving the
            // override travels with the combined prefab tree (not just root-level entities).
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""SelfStateEntity"" Id=""root"">
          <Children>
            <EntityDefinition Type=""SelfStateEntity"" Id=""kid"">
              <EntityOverrides>
                <Property Name=""Text"" Value=""nested"" />
              </EntityOverrides>
            </EntityDefinition>
          </Children>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));

                scene.Load();
                for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                    helper.Tick();

                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();
                var kid = (SelfStateEntity)entitySystem.FindById("kid");
                Assert.NotNull(kid);
                Assert.Equal("nested", kid.Text);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        // ──────────────────────────── Fixtures ────────────────────────────

        /// <summary>
        /// Entity that keeps its state on itself (no component) — models TextEntity/CameraEntity.
        /// Records what it saw during OnStart to prove overrides are applied before initialization.
        /// </summary>
        public class SelfStateEntity : Entity
        {
            public enum ModeEnum { Slow, Fast }

            private string _text = "";
            private float _speed;
            private int _hits;
            private bool _enabled = true;
            private Vector2 _offset;
            private Color _tint = Color.White;
            private ModeEnum _mode;

            public string Text { get => _text; set => _text = value; }
            public float Speed { get => _speed; set => _speed = value; }
            public int Hits { get => _hits; set => _hits = value; }
            public bool Enabled { get => _enabled; set => _enabled = value; }
            public Vector2 Offset { get => _offset; set => _offset = value; }
            public Color Tint { get => _tint; set => _tint = value; }
            public ModeEnum Mode { get => _mode; set => _mode = value; }

            /// <summary>Snapshot of Text captured in OnStart — proves the override landed before init.</summary>
            public string? SeenTextAtOnStart { get; private set; }

            public SelfStateEntity() { }

            public override void OnStart()
            {
                base.OnStart();
                SeenTextAtOnStart = Text;
            }

            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        /// <summary>Plain component with one writable string property — used to verify component and entity overrides coexist.</summary>
        private class EntityProbeComponent : EntityComponent
        {
            private string _base = "unset";
            public string Base { get => _base; set => _base = value; }
        }
    }
}
