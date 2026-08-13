using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    /// <summary>
    /// Tests for entity-driven serialization — entities explicitly declare what to save.
    /// </summary>
    public class EntityDrivenSerializationTests
    {
        // Test entity that overrides SerializeToXml/DeserializeFromXml with custom state
        public class CustomStateEntity : Entity
        {
            public int Score { get; set; }
            public string? Name { get; set; }

            public override XElement SerializeToXml()
            {
                var element = base.SerializeToXml();
                element.Add(new XElement("CustomState",
                    new XAttribute("Score", Score),
                    new XAttribute("Name", Name ?? "")
                ));
                return element;
            }

            public override void DeserializeFromXml(XElement element, bool mergeExisting = false)
            {
                base.DeserializeFromXml(element, mergeExisting);

                var custom = element.Element("CustomState");
                if (custom != null)
                {
                    if (int.TryParse(custom.Attribute("Score")?.Value, out int score))
                        Score = score;
                    Name = custom.Attribute("Name")?.Value;
                }
            }
        }

        // Test entity that creates components in OnStart and defers restoration
        public class DeferredComponentEntity : Entity
        {
            public SpriteComponent? SpriteComp { get; private set; }
            public bool OnStartCalled { get; private set; }

            // Deferred state
            private XElement? _deferredSpriteElement;

            public override void OnStart()
            {
                base.OnStart();
                OnStartCalled = true;

                SpriteComp = new SpriteComponent();
                AddComponent(SpriteComp);

                // Restore deferred sprite color now that component exists
                if (_deferredSpriteElement != null && SpriteComp != null)
                {
                    var colorAttr = _deferredSpriteElement.Attribute("Color")?.Value;
                    if (colorAttr != null && uint.TryParse(colorAttr, out uint argb))
                    {
                        SpriteComp.Color = new Color(argb);
                    }
                }
                _deferredSpriteElement = null;
            }

            public override XElement SerializeToXml()
            {
                var element = base.SerializeToXml();
                if (SpriteComp != null)
                {
                    element.Add(new XElement("Sprite",
                        new XAttribute("Color", SpriteComp.Color.PackedValue.ToString())
                    ));
                }
                return element;
            }

            public override void DeserializeFromXml(XElement element, bool mergeExisting = false)
            {
                base.DeserializeFromXml(element, mergeExisting);
                // Defer until OnStart creates component
                _deferredSpriteElement = element.Element("Sprite");
            }
        }

        [Fact]
        public void Entity_SerializeToXml_SavesTransform()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetId("test_entity");
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 1.57f;
            entity.Scale = new Vector2(2, 3);
            entity.SetSort(5);

            var xml = entity.SerializeToXml();

            Assert.Equal("test_entity", xml.Attribute("Id")?.Value);
            Assert.Equal("100", xml.Element("Position")?.Attribute("X")?.Value);
            Assert.Equal("200", xml.Element("Position")?.Attribute("Y")?.Value);
            Assert.Equal(1.57f, float.Parse(xml.Attribute("Rotation")?.Value ?? "0", CultureInfo.InvariantCulture), 0.01f);
            Assert.Equal(2f, float.Parse(xml.Element("Scale")?.Attribute("X")?.Value ?? "0", CultureInfo.InvariantCulture));
            Assert.Equal("3", xml.Element("Scale")?.Attribute("Y")?.Value);
        }

        [Fact]
        public void Entity_DeserializeFromXml_RestoresTransform()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetId("test_entity");

            var xml = XElement.Parse(@"
                <Entity Id=""test_entity"" Type=""Test"" Rotation=""0.785"" Sort=""10"" Active=""true"">
                    <Position X=""42"" Y=""99"" />
                    <Scale X=""1.5"" Y=""2.5"" />
                    <Tags><Tag Name=""player"" /></Tags>
                </Entity>");

            entity.DeserializeFromXml(xml);

            Assert.Equal(new Vector2(42, 99), entity.Position);
            Assert.Equal(0.785f, entity.Rotation, 0.01f);
            Assert.Equal(new Vector2(1.5f, 2.5f), entity.Scale);
            Assert.True(entity.HasTag("player"));
        }

        [Fact]
        public void Entity_SerializeDeserializeRoundTrip_PreservesCustomState()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetId("custom_entity");
            entity.Position = new Vector2(10, 20);
            entity.Score = 42;
            entity.Name = "Hero";

            // Serialize
            var xml = entity.SerializeToXml();

            // Deserialize into a fresh entity
            var system2 = new EntitySystem();
            var restored = system2.CreateEntity<CustomStateEntity>();
            restored.DeserializeFromXml(xml);

            Assert.Equal(new Vector2(10, 20), restored.Position);
            Assert.Equal(42, restored.Score);
            Assert.Equal("Hero", restored.Name);
        }

        [Fact]
        public void Entity_DeserializeFromXml_MergeModePreservesRuntimeTags()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetTag("runtime");

            var xml = XElement.Parse(@"
                <Entity Id=""test"" Type=""Test"" Rotation=""0"" Sort=""0"" Active=""true"">
                    <Position X=""0"" Y=""0"" />
                    <Tags><Tag Name=""saved"" /></Tags>
                </Entity>");

            entity.DeserializeFromXml(xml, mergeExisting: true);

            // Both runtime and saved tags should exist
            Assert.True(entity.HasTag("runtime"), "Runtime tag should be preserved in merge mode");
            Assert.True(entity.HasTag("saved"), "Saved tag should be added");
        }

        [Fact]
        public void Entity_DeserializeFromXml_NonMergeModeReplacesTags()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetTag("runtime");

            var xml = XElement.Parse(@"
                <Entity Id=""test"" Type=""Test"" Rotation=""0"" Sort=""0"" Active=""true"">
                    <Position X=""0"" Y=""0"" />
                    <Tags><Tag Name=""saved"" /></Tags>
                </Entity>");

            entity.DeserializeFromXml(xml, mergeExisting: false);

            Assert.False(entity.HasTag("runtime"), "Runtime tag should be cleared in non-merge mode");
            Assert.True(entity.HasTag("saved"));
        }

        [Fact]
        public void DeferredComponentEntity_ColorRoundTrip_PreservesColor()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<DeferredComponentEntity>();
            entity.SetId("colored_entity");

            // Set a non-white color after OnStart
            if (entity.SpriteComp != null)
                entity.SpriteComp.Color = Color.Blue;

            // Save state
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);

                // Load into new system
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);

                var loaded = newSystem.GetEntities().First() as DeferredComponentEntity;
                Assert.NotNull(loaded);
                Assert.True(loaded.OnStartCalled);
                Assert.NotNull(loaded.SpriteComp);

                // Color should be restored after OnStart
                var loadedColor = loaded.SpriteComp!.Color;
                Assert.Equal((uint)Color.Blue.PackedValue, (uint)loadedColor.PackedValue);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void DeferredComponentEntity_RedColorRoundTrip_PreservesRed()
        {
            // Test a color with packed value > int.MaxValue (like red = 4278190335)
            var system = new EntitySystem();
            var entity = system.CreateEntity<DeferredComponentEntity>();
            entity.SetId("red_entity");

            if (entity.SpriteComp != null)
                entity.SpriteComp.Color = Color.Red;

            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);

                // Verify XML contains correct packed color (as uint string)
                var xmlContent = File.ReadAllText(tempFile);
                Assert.True(xmlContent.Contains("Color=\"4278190335\""), "Red color should be serialized as uint");

                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);

                var loaded = newSystem.GetEntities().First() as DeferredComponentEntity;
                Assert.NotNull(loaded);
                Assert.Equal((uint)Color.Red.PackedValue, (uint)loaded.SpriteComp!.Color.PackedValue);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void DeferredComponentEntity_MultipleColorsRoundTrip_PreservesAll()
        {
            var system = new EntitySystem();

            // Create entities with different colors
            var blue = system.CreateEntity<DeferredComponentEntity>();
            blue.SetId("blue");
            if (blue.SpriteComp != null) blue.SpriteComp.Color = Color.Blue;

            var green = system.CreateEntity<DeferredComponentEntity>();
            green.SetId("green");
            if (green.SpriteComp != null) green.SpriteComp.Color = Color.Green;

            var white = system.CreateEntity<DeferredComponentEntity>();
            white.SetId("white");
            // White is default, no color set explicitly

            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);

                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);

                var loadedBlue = newSystem.GetEntities().First(e => e.Id == "blue") as DeferredComponentEntity;
                var loadedGreen = newSystem.GetEntities().First(e => e.Id == "green") as DeferredComponentEntity;
                var loadedWhite = newSystem.GetEntities().First(e => e.Id == "white") as DeferredComponentEntity;

                Assert.Equal((uint)Color.Blue.PackedValue, (uint)loadedBlue!.SpriteComp!.Color.PackedValue);
                Assert.Equal((uint)Color.Green.PackedValue, (uint)loadedGreen!.SpriteComp!.Color.PackedValue);
                Assert.Equal((uint)Color.White.PackedValue, (uint)loadedWhite!.SpriteComp!.Color.PackedValue);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void Entity_SerializeToXml_IncludesType()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetId("typed_entity");

            var xml = entity.SerializeToXml();

            var typeAttr = xml.Attribute("Type")?.Value;
            Assert.NotNull(typeAttr);
            Assert.Contains("CustomStateEntity", typeAttr);
        }
    }
}
