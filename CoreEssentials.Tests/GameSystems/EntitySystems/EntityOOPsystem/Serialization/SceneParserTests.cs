using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class SceneParserTests : IDisposable
    {
        [Fact]
        public void Parse_FullDocument_PopulatesEveryPart()
        {
            // Arrange — a scene exercising every element of the locked schema
            const string prefabAsset = "SceneParserFullDocPrefab.xml";
            WriteContentAsset(prefabAsset, @"<EntityTemplate Type=""ProbeEntity"">
                <Components>
                    <Component Type=""SingleMatchComponent"">
                        <Properties><Property Name=""Base"" Value=""default"" /></Properties>
                    </Component>
                </Components>
            </EntityTemplate>");
            AssetManager.Init(new MockContentManager());

            var xml = $@"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Prefabs>
        <Prefab Name=""probe"" Asset=""{prefabAsset}"" />
      </Prefabs>
      <Entities>
        <EntityDefinition Source=""probe"" Id=""fromPrefab"" Base=""overridden"" Rotation=""10"" Sort=""3"" Active=""false"">
          <Position X=""42"" Y=""7"" />
          <Tags><Tag Name=""UI"" /></Tags>
          <Overrides>
            <Component Type=""SingleMatchComponent""><Property Name=""Count"" Value=""9"" /></Component>
          </Overrides>
          <Children>
            <EntityDefinition Type=""ProbeEntity"" Id=""child"" />
          </Children>
        </EntityDefinition>
        <EntityDefinition Type=""ProbeEntity"" Id=""plain"" Base=""flat"">
          <Components><Component Type=""SingleMatchComponent"" /></Components>
        </EntityDefinition>
      </Entities>
    </System>
    <System Type=""PhysicsEngine"" />
  </GameSystems>
</Scene>";

            // Act
            var scene = SceneParser.Parse(xml);

            // Assert — systems
            Assert.Equal(2, scene.Systems.Count);
            Assert.Equal(typeof(EntitySystem), scene.Systems[0].SystemType);
            Assert.Equal(typeof(CoreEssentials.GameSystems.Physics.Engines.Aether.PhysicsEngine), scene.Systems[1].SystemType);

            // Assert — prefab registration loaded the asset
            var system = scene.Systems[0];
            Assert.Single(system.Prefabs);
            Assert.Equal("probe", system.Prefabs[0].Name);
            Assert.NotNull(system.Prefabs[0].Prefab);
            Assert.Equal("ProbeEntity", system.Prefabs[0].Prefab!.Type);

            // Assert — prefab instance with flat + precise overrides
            var fromPrefab = system.Entities.Single(e => e.Id == "fromPrefab");
            Assert.Equal("probe", fromPrefab.Source);
            Assert.Null(fromPrefab.Type);
            Assert.Equal(new Vector2(42, 7), fromPrefab.Position);
            Assert.Equal(10f, fromPrefab.Rotation);
            Assert.Equal(3, fromPrefab.Sort);
            Assert.False(fromPrefab.Active);
            Assert.Contains("UI", fromPrefab.Tags);
            Assert.Equal("overridden", fromPrefab.FlatOverrides["Base"]);
            // Flat attribute resolves to the component's fully-qualified name; the precise form keeps its written key.
            Assert.Equal("overridden", fromPrefab.ResolvedOverrides[typeof(SingleMatchComponent).FullName!]["Base"]);
            Assert.Equal("9", fromPrefab.ResolvedOverrides["SingleMatchComponent"]["Count"]);

            // Assert — nested children
            var child = fromPrefab.Children.Single();
            Assert.Equal("child", child.Id);
            Assert.Equal("ProbeEntity", child.Type);

            // Assert — plain class definition with a flat attribute resolved against declared components
            var plain = system.Entities.Single(e => e.Id == "plain");
            Assert.Equal("ProbeEntity", plain.Type);
            Assert.Null(plain.Source);
            Assert.Contains(plain.DeclaredComponents, c => c.Type == "SingleMatchComponent");
            Assert.Equal("flat", plain.ResolvedOverrides[typeof(SingleMatchComponent).FullName!]["Base"]);
        }

        [Fact]
        public void Parse_PartialSections_MissingOptionalParts()
        {
            // Arrange — a minimal scene: no prefabs, no entities on the second system
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""PhysicsEngine"" />
  </GameSystems>
</Scene>";

            // Act
            var scene = SceneParser.Parse(xml);

            // Assert
            var system = Assert.Single(scene.Systems);
            Assert.Empty(system.Prefabs);
            Assert.Empty(system.Entities);
        }

        [Fact]
        public void Parse_CustomSystemType_ResolvedByReflectionFallback()
        {
            // Arrange — a GameSystem subclass that is not in the built-in table
            var xml = $@"<Scene>
  <GameSystems>
    <System Type=""{typeof(ProbeGameSystem).FullName}"" />
  </GameSystems>
</Scene>";

            // Act
            var scene = SceneParser.Parse(xml);

            // Assert — resolved via the reflection fallback, and short name works too
            Assert.Equal(typeof(ProbeGameSystem), scene.Systems.Single().SystemType);
            var shortNameXml = @"<Scene><GameSystems><System Type=""ProbeGameSystem"" /></GameSystems></Scene>";
            Assert.Equal(typeof(ProbeGameSystem), SceneParser.Parse(shortNameXml).Systems.Single().SystemType);
        }

        [Fact]
        public void Parse_BindsAndReferences_AreCaptured()
        {
            // Arrange
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""ProbeEntity"" Id=""a"">
          <Components>
            <Component Type=""SingleMatchComponent"" />
            <Bind Event=""Clicked"" Command=""DoThing"" />
          </Components>
          <References><Reference Name=""Other"" TargetId=""b"" /></References>
        </EntityDefinition>
        <EntityDefinition Type=""ProbeEntity"" Id=""b"" />
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            // Act
            var scene = SceneParser.Parse(xml);

            // Assert
            var def = scene.Systems[0].Entities.Single(e => e.Id == "a");
            Assert.Single(def.Binds);
            Assert.Equal("Clicked", def.Binds[0].Attribute("Event")!.Value);
            Assert.Single(def.References);
            Assert.Equal("b", def.References[0].Attribute("TargetId")!.Value);
        }

        [Fact]
        public void Parse_FlatAttribute_AmbiguousAcrossComponents_Throws()
        {
            // Arrange — two components both expose a writable 'Base' property
            const string prefabAsset = "SceneParserAmbiguityPrefab.xml";
            WriteContentAsset(prefabAsset, @"<EntityTemplate Type=""ProbeEntity"">
                <Components>
                    <Component Type=""AmbiguityComponentA"" />
                    <Component Type=""AmbiguityComponentB"" />
                </Components>
            </EntityTemplate>");
            AssetManager.Init(new MockContentManager());

            var xml = $@"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Prefabs><Prefab Name=""amb"" Asset=""{prefabAsset}"" /></Prefabs>
      <Entities>
        <EntityDefinition Source=""amb"" Id=""x"" Base=""value"" />
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            // Act & Assert — ambiguity must name the property and both components
            var ex = Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
            Assert.Contains("Base", ex.Message);
            Assert.Contains("AmbiguityComponentA", ex.Message);
            Assert.Contains("AmbiguityComponentB", ex.Message);
        }

        [Fact]
        public void Parse_FlatAttribute_MatchesNoComponent_Throws()
        {
            // Arrange — the prefab's component has no writable 'Missing' property
            const string prefabAsset = "SceneParserNoMatchPrefab.xml";
            WriteContentAsset(prefabAsset, @"<EntityTemplate Type=""ProbeEntity"">
                <Components><Component Type=""SingleMatchComponent"" /></Components>
            </EntityTemplate>");
            AssetManager.Init(new MockContentManager());

            var xml = $@"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Prefabs><Prefab Name=""p"" Asset=""{prefabAsset}"" /></Prefabs>
      <Entities>
        <EntityDefinition Source=""p"" Id=""x"" Missing=""value"" />
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            // Act & Assert
            var ex = Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
            Assert.Contains("Missing", ex.Message);
        }

        [Fact]
        public void Parse_UnknownElement_AtEachLevel_ThrowsNamingTheElement()
        {
            AssetManager.Init(new MockContentManager());

            // Unknown child of <Scene>
            var badRoot = @"<Scene><Bogus /></Scene>";
            Assert.Contains("GameSystems", Assert.Throws<FormatException>(() => SceneParser.Parse(badRoot)).Message);

            // Unknown child of <GameSystems>
            var badSystem = @"<Scene><GameSystems><Widgets /></GameSystems></Scene>";
            Assert.Contains("Widgets", Assert.Throws<FormatException>(() => SceneParser.Parse(badSystem)).Message);

            // Unknown child of <System>
            var badInsideSystem = @"<Scene><GameSystems><System Type=""EntitySystem""><Mistakes /></System></GameSystems></Scene>";
            Assert.Contains("Mistakes", Assert.Throws<FormatException>(() => SceneParser.Parse(badInsideSystem)).Message);

            // Unknown child of <Entities>
            var badInsideEntities = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities><Widget /></Entities></System></GameSystems></Scene>";
            Assert.Contains("Widget", Assert.Throws<FormatException>(() => SceneParser.Parse(badInsideEntities)).Message);

            // Unknown child of <EntityDefinition>
            var badInsideDef = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities>
                <EntityDefinition Type=""ProbeEntity"" Id=""x""><Gadget /></EntityDefinition>
              </Entities></System></GameSystems></Scene>";
            Assert.Contains("Gadget", Assert.Throws<FormatException>(() => SceneParser.Parse(badInsideDef)).Message);

            // Unknown attribute on a known element
            var badAttribute = @"<Scene><GameSystems><System Type=""EntitySystem"" Oops=""1"" /></GameSystems></Scene>";
            Assert.Contains("Oops", Assert.Throws<FormatException>(() => SceneParser.Parse(badAttribute)).Message);
        }

        [Fact]
        public void Parse_TypeXorSource_Violations_Throw()
        {
            // Both set
            var both = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities>
                <EntityDefinition Type=""ProbeEntity"" Source=""p"" Id=""x"" />
              </Entities></System></GameSystems></Scene>";
            var exBoth = Assert.Throws<FormatException>(() => SceneParser.Parse(both));
            Assert.Contains("both", exBoth.Message);

            // Neither set
            var neither = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities>
                <EntityDefinition Id=""x"" />
              </Entities></System></GameSystems></Scene>";
            var exNeither = Assert.Throws<FormatException>(() => SceneParser.Parse(neither));
            Assert.Contains("either", exNeither.Message);
        }

        [Fact]
        public void Parse_UnresolvableEntityOrSystemType_Throws()
        {
            // Entity type that does not exist anywhere
            var badEntity = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities>
                <EntityDefinition Type=""NoSuchEntityClass"" Id=""x"" />
              </Entities></System></GameSystems></Scene>";
            var exEntity = Assert.Throws<FormatException>(() => SceneParser.Parse(badEntity));
            Assert.Contains("NoSuchEntityClass", exEntity.Message);

            // System type that does not exist anywhere
            var badSystem = @"<Scene><GameSystems><System Type=""NoSuchSystem"" /></GameSystems></Scene>";
            var exSystem = Assert.Throws<FormatException>(() => SceneParser.Parse(badSystem));
            Assert.Contains("NoSuchSystem", exSystem.Message);
        }

        [Fact]
        public void Parse_SourceReferringToUnregisteredPrefab_Throws()
        {
            // Arrange — no <Prefabs> at all, but an entity references one
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities>
                <EntityDefinition Source=""ghost"" Id=""x"" />
              </Entities></System></GameSystems></Scene>";

            // Act & Assert
            var ex = Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
            Assert.Contains("ghost", ex.Message);
        }

        [Fact]
        public void Parse_DuplicateEntityIds_Throw()
        {
            // Arrange — same Id on two root entities, and one duplicated in a nested child
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities>
                <EntityDefinition Type=""ProbeEntity"" Id=""dup"" />
                <EntityDefinition Type=""ProbeEntity"" Id=""other"" />
              </Entities></System></GameSystems></Scene>";

            // Act & Assert — the simple duplicate fails
            var ex = Assert.Throws<FormatException>(() => SceneParser.Parse(xml.Replace("Id=\"other\"", "Id=\"dup\"")));
            Assert.Contains("dup", ex.Message);

            // A nested child may reuse an Id that is not used elsewhere
            var okNested = @"<Scene><GameSystems><System Type=""EntitySystem""><Entities>
                <EntityDefinition Type=""ProbeEntity"" Id=""parent"">
                  <Children><EntityDefinition Type=""ProbeEntity"" Id=""child"" /></Children>
                </EntityDefinition>
              </Entities></System></GameSystems></Scene>";
            Assert.Null(Record.Exception(() => SceneParser.Parse(okNested)));
        }

        [Fact]
        public void Parse_MissingGameSystems_Throws()
        {
            // Arrange — entities floating directly under <Scene> are not expressible
            var xml = @"<Scene><Entities><EntityDefinition Type=""ProbeEntity"" /></Entities></Scene>";

            // Act & Assert
            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        public void Dispose() { }

        private static void WriteContentAsset(string fileName, string xml)
        {
            var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(Path.Combine(contentDir, fileName), xml);
        }

        // ──────────────────────────── Test fixtures ────────────────────────────

        public class ProbeEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        /// <summary>A custom game system — not in the built-in table, resolved by reflection.</summary>
        public class ProbeGameSystem : GameSystem { }

        /// <summary>Component with one writable string property and one writable int property.</summary>
        public class SingleMatchComponent : EntityComponent
        {
            private string _base = "unset";
            private int _count;

            public string Base { get => _base; set => _base = value; }
            public int Count { get => _count; set => _count = value; }
        }

        /// <summary>Two components sharing a property name — used to prove ambiguity is an error.</summary>
        public class AmbiguityComponentA : EntityComponent
        {
            private string _base = "unset";
            public string Base { get => _base; set => _base = value; }
        }

        public class AmbiguityComponentB : EntityComponent
        {
            private string _base = "unset";
            public string Base { get => _base; set => _base = value; }
        }
    }
}
