#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    /// <summary>
    /// Tests for entity-driven serialization — entities explicitly declare what to save.
    /// </summary>
    public class EntityDrivenSerializationTests
    {
        // Test entity carrying a save component with custom state
        public class CustomStateEntity : Entity
        {
        }

        /// <summary>Save component for <see cref="CustomStateEntity"/> — persists transform + Score/Name.</summary>
        public class CustomStateSaveComponent : EntityComponent, ISaveableComponent
        {
            public int Score { get; set; }
            public string? Name { get; set; }

            public XElement SaveState()
            {
                var o = Owner;
                return new XElement("Entity",
                    new XAttribute("Id", o?.Id ?? string.Empty),
                    new XAttribute("Type", o?.GetType().FullName ?? string.Empty),
                    new XAttribute("Rotation", (o?.Rotation ?? 0f).ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Sort", o?.GetSort() ?? 0),
                    new XAttribute("Active", o?.GetActive() ?? true),
                    new XElement("Position",
                        new XAttribute("X", (o?.Position.X ?? 0f).ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", (o?.Position.Y ?? 0f).ToString(CultureInfo.InvariantCulture))),
                    new XElement("Scale",
                        new XAttribute("X", (o?.Scale.X ?? 1f).ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", (o?.Scale.Y ?? 1f).ToString(CultureInfo.InvariantCulture))),
                    new XElement("Tags",
                        (o?.Tags ?? Enumerable.Empty<string>()).Select(tag => new XElement("Tag", new XAttribute("Name", tag)))),
                    new XElement("CustomState",
                        new XAttribute("Score", Score),
                        new XAttribute("Name", Name ?? "")));
            }

            public void LoadState(XElement element)
            {
                var o = Owner;
                if (o == null) return;

                var positionElement = element.Element("Position");
                if (positionElement != null &&
                    float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
                {
                    o.Position = new Vector2(x, y);
                }

                if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
                    o.Rotation = rotation;

                var scaleElement = element.Element("Scale");
                if (scaleElement != null &&
                    float.TryParse(scaleElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
                    float.TryParse(scaleElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
                {
                    o.Scale = new Vector2(scaleX, scaleY);
                }

                if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort))
                    o.SetSort(sort);

                if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
                    o.SetActive(active);

                var tagsElement = element.Element("Tags");
                if (tagsElement != null)
                {
                    foreach (var tag in o.Tags.ToList())
                        o.RemoveTag(tag);
                    foreach (var tagElement in tagsElement.Elements("Tag"))
                    {
                        var tagName = tagElement.Attribute("Name")?.Value;
                        if (!string.IsNullOrWhiteSpace(tagName))
                            o.SetTag(tagName);
                    }
                }

                // Custom state
                var custom = element.Element("CustomState");
                if (custom != null)
                {
                    if (int.TryParse(custom.Attribute("Score")?.Value, out int score))
                        Score = score;
                    Name = custom.Attribute("Name")?.Value;
                }
            }
        }

        // Test entity that creates a component in OnStart; its save component persists the sprite color
        public class DeferredComponentEntity : Entity
        {
            public SpriteComponent? SpriteComp { get; private set; }

            public override void OnStart()
            {
                base.OnStart();
                // Create component with defaults
                SpriteComp = new SpriteComponent();
                AddComponent(SpriteComp);
                SpriteComp.Color = Color.White; // Default color
            }
        }

        /// <summary>Save component for <see cref="DeferredComponentEntity"/> — persists transform + sprite color.</summary>
        public class DeferredSpriteSaveComponent : EntityComponent, ISaveableComponent
        {
            public XElement SaveState()
            {
                var o = Owner as DeferredComponentEntity;
                return new XElement("Entity",
                    new XAttribute("Id", o?.Id ?? string.Empty),
                    new XAttribute("Type", o?.GetType().FullName ?? string.Empty),
                    new XAttribute("Rotation", (o?.Rotation ?? 0f).ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Sort", o?.GetSort() ?? 0),
                    new XAttribute("Active", o?.GetActive() ?? true),
                    new XElement("Position",
                        new XAttribute("X", (o?.Position.X ?? 0f).ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", (o?.Position.Y ?? 0f).ToString(CultureInfo.InvariantCulture))),
                    new XElement("Scale",
                        new XAttribute("X", (o?.Scale.X ?? 1f).ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", (o?.Scale.Y ?? 1f).ToString(CultureInfo.InvariantCulture))),
                    new XElement("Tags",
                        (o?.Tags ?? Enumerable.Empty<string>()).Select(tag => new XElement("Tag", new XAttribute("Name", tag)))),
                    new XElement("Sprite",
                        new XAttribute("Color", o?.SpriteComp?.Color.PackedValue.ToString() ?? "0")));
            }

            public void LoadState(XElement element)
            {
                var o = Owner as DeferredComponentEntity;
                if (o == null) return;

                var positionElement = element.Element("Position");
                if (positionElement != null &&
                    float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
                {
                    o.Position = new Vector2(x, y);
                }

                if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
                    o.Rotation = rotation;

                var scaleElement = element.Element("Scale");
                if (scaleElement != null &&
                    float.TryParse(scaleElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
                    float.TryParse(scaleElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
                {
                    o.Scale = new Vector2(scaleX, scaleY);
                }

                if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort))
                    o.SetSort(sort);

                if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
                    o.SetActive(active);

                var tagsElement = element.Element("Tags");
                if (tagsElement != null)
                {
                    foreach (var tag in o.Tags.ToList())
                        o.RemoveTag(tag);
                    foreach (var tagElement in tagsElement.Elements("Tag"))
                    {
                        var tagName = tagElement.Attribute("Name")?.Value;
                        if (!string.IsNullOrWhiteSpace(tagName))
                            o.SetTag(tagName);
                    }
                }

                // Restore sprite color — the component exists since OnStart ran during instantiation.
                var sprite = element.Element("Sprite");
                if (sprite != null && o.SpriteComp != null)
                {
                    var colorAttr = sprite.Attribute("Color")?.Value;
                    if (colorAttr != null && uint.TryParse(colorAttr, out uint argb))
                        o.SpriteComp.Color = new Color(argb);
                }
            }
        }

        [Fact]
        public void Entity_SaveState_SavesTransform()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            var saveComp = (CustomStateSaveComponent)entity.AddComponent(new CustomStateSaveComponent());
            entity.SetId("test_entity");
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 1.57f;
            entity.Scale = new Vector2(2, 3);
            entity.SetSort(5);

            var xml = saveComp.SaveState();

            Assert.Equal("test_entity", xml.Attribute("Id")?.Value);
            Assert.Equal("100", xml.Element("Position")?.Attribute("X")?.Value);
            Assert.Equal("200", xml.Element("Position")?.Attribute("Y")?.Value);
            Assert.Equal(1.57f, float.Parse(xml.Attribute("Rotation")?.Value ?? "0", CultureInfo.InvariantCulture), 0.01f);
            Assert.Equal(2f, float.Parse(xml.Element("Scale")?.Attribute("X")?.Value ?? "0", CultureInfo.InvariantCulture));
            Assert.Equal("3", xml.Element("Scale")?.Attribute("Y")?.Value);
        }

        [Fact]
        public void Entity_LoadState_RestoresTransform()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            var saveComp = (CustomStateSaveComponent)entity.AddComponent(new CustomStateSaveComponent());
            entity.SetId("test_entity");

            var xml = XElement.Parse(@"
                <Entity Id=""test_entity"" Type=""Test"" Rotation=""0.785"" Sort=""10"" Active=""true"">
                    <Position X=""42"" Y=""99"" />
                    <Scale X=""1.5"" Y=""2.5"" />
                    <Tags><Tag Name=""player"" /></Tags>
                </Entity>");

            saveComp.LoadState(xml);

            Assert.Equal(new Vector2(42, 99), entity.Position);
            Assert.Equal(0.785f, entity.Rotation, 0.01f);
            Assert.Equal(new Vector2(1.5f, 2.5f), entity.Scale);
            Assert.True(entity.HasTag("player"));
        }

        [Fact]
        public void Entity_SaveLoadRoundTrip_PreservesCustomState()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            var saveComp = (CustomStateSaveComponent)entity.AddComponent(new CustomStateSaveComponent());
            entity.SetId("custom_entity");
            entity.Position = new Vector2(10, 20);
            saveComp.Score = 42;
            saveComp.Name = "Hero";

            // Save state
            var xml = saveComp.SaveState();

            // Load into a fresh entity
            var system2 = new EntitySystem();
            var restored = system2.CreateEntity<CustomStateEntity>();
            var restoredSave = (CustomStateSaveComponent)restored.AddComponent(new CustomStateSaveComponent());
            restored.SetId("custom_entity");
            restoredSave.LoadState(xml);

            Assert.Equal(new Vector2(10, 20), restored.Position);
            Assert.Equal(42, restoredSave.Score);
            Assert.Equal("Hero", restoredSave.Name);
        }

        [Fact]
        public void Entity_LoadState_ReplacesTags()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            var saveComp = (CustomStateSaveComponent)entity.AddComponent(new CustomStateSaveComponent());
            entity.SetId("test");
            entity.SetTag("runtime");

            var xml = XElement.Parse(@"
                <Entity Id=""test"" Type=""Test"" Rotation=""0"" Sort=""0"" Active=""true"">
                    <Position X=""0"" Y=""0"" />
                    <Tags><Tag Name=""saved"" /></Tags>
                </Entity>");

            saveComp.LoadState(xml);

            // Tags are replaced (not merged) — runtime tag is cleared, saved tag is added
            Assert.False(entity.HasTag("runtime"), "Runtime tag should be cleared");
            Assert.True(entity.HasTag("saved"));
        }

        [Fact]
        public void DeferredComponentEntity_ColorRoundTrip_PreservesColor()
        {
            var system = new EntitySystem();
            system.RegisterPrefab("deferred", EntityPrefabLoader.LoadFromXml(
                "<Prefab Type=\"DeferredComponentEntity\"><Components>" +
                "<Component Type=\"DeferredSpriteSaveComponent\" /></Components></Prefab>"));

            // Instantiate from the prefab so OnStart runs and PrefabName is stamped (required to save).
            var entity = (DeferredComponentEntity)system.Instantiate("deferred", Vector2.Zero);
            entity.SetId("colored_entity");

            // Set a non-white color after OnStart
            if (entity.SpriteComp != null)
                entity.SpriteComp.Color = Color.Blue;

            // Save state
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);

                // Load into a system that has the prefab registered so it can be recreated.
                var newSystem = new EntitySystem();
                newSystem.RegisterPrefab("deferred", EntityPrefabLoader.LoadFromXml(
                    "<Prefab Type=\"DeferredComponentEntity\"><Components>" +
                    "<Component Type=\"DeferredSpriteSaveComponent\" /></Components></Prefab>"));

                GameStateSerializer.LoadState(newSystem, tempFile);

                // Verify the loaded entity has the correct color
                var loaded = newSystem.FindById("colored_entity") as DeferredComponentEntity;
                Assert.NotNull(loaded);
                if (loaded?.SpriteComp != null)
                {
                    Assert.Equal(Color.Blue, loaded.SpriteComp.Color);
                }
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
#nullable enable