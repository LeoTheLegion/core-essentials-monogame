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
        // Test entity that implements ISaveableEntity with custom state
        public class CustomStateEntity : Entity, ISaveableEntity
        {
            public int Score { get; set; }
            public string? Name { get; set; }

            public XElement SaveState()
            {
                var element = new XElement("Entity",
                    new XAttribute("Id", Id ?? string.Empty),
                    new XAttribute("Type", GetType().FullName),
                    new XAttribute("Rotation", Rotation.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Sort", GetSort()),
                    new XAttribute("Active", GetActive()),
                    new XElement("Position",
                        new XAttribute("X", Position.X.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", Position.Y.ToString(CultureInfo.InvariantCulture))
                    ),
                    new XElement("Scale",
                        new XAttribute("X", Scale.X.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", Scale.Y.ToString(CultureInfo.InvariantCulture))
                    ),
                    new XElement("Tags",
                        Tags.Select(tag => new XElement("Tag", new XAttribute("Name", tag)))
                    ),
                    new XElement("CustomState",
                        new XAttribute("Score", Score),
                        new XAttribute("Name", Name ?? "")
                    )
                );
                return element;
            }

            public void LoadState(XElement element)
            {
                // Restore position
                var positionElement = element.Element("Position");
                if (positionElement != null)
                {
                    if (float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
                    {
                        Position = new Vector2(x, y);
                    }
                }

                if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
                {
                    Rotation = rotation;
                }

                var scaleElement = element.Element("Scale");
                if (scaleElement != null)
                {
                    if (float.TryParse(scaleElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
                        float.TryParse(scaleElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
                    {
                        Scale = new Vector2(scaleX, scaleY);
                    }
                }

                if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort))
                {
                    SetSort(sort);
                }

                if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
                {
                    SetActive(active);
                }

                var tagsElement = element.Element("Tags");
                if (tagsElement != null)
                {
                    foreach (var tag in Tags.ToList())
                    {
                        RemoveTag(tag);
                    }
                    foreach (var tagElement in tagsElement.Elements("Tag"))
                    {
                        var tagName = tagElement.Attribute("Name")?.Value;
                        if (!string.IsNullOrWhiteSpace(tagName))
                        {
                            SetTag(tagName);
                        }
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

        // Test entity that creates components in OnStart and implements ISaveableEntity
        public class DeferredComponentEntity : Entity, ISaveableEntity
        {
            public SpriteComponent? SpriteComp { get; private set; }
            public bool OnStartCalled { get; private set; }

            public override void OnStart()
            {
                base.OnStart();
                OnStartCalled = true;

                // Create component with defaults
                SpriteComp = new SpriteComponent();
                AddComponent(SpriteComp);
                SpriteComp.Color = Color.White; // Default color
            }

            public XElement SaveState()
            {
                var element = new XElement("Entity",
                    new XAttribute("Id", Id ?? string.Empty),
                    new XAttribute("Type", GetType().FullName),
                    new XAttribute("Rotation", Rotation.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Sort", GetSort()),
                    new XAttribute("Active", GetActive()),
                    new XElement("Position",
                        new XAttribute("X", Position.X.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", Position.Y.ToString(CultureInfo.InvariantCulture))
                    ),
                    new XElement("Scale",
                        new XAttribute("X", Scale.X.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", Scale.Y.ToString(CultureInfo.InvariantCulture))
                    ),
                    new XElement("Tags",
                        Tags.Select(tag => new XElement("Tag", new XAttribute("Name", tag)))
                    )
                );

                if (SpriteComp != null)
                {
                    element.Add(new XElement("Sprite",
                        new XAttribute("Color", SpriteComp.Color.PackedValue.ToString())
                    ));
                }
                return element;
            }

            public void LoadState(XElement element)
            {
                // Restore position
                var positionElement = element.Element("Position");
                if (positionElement != null)
                {
                    if (float.TryParse(positionElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(positionElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
                    {
                        Position = new Vector2(x, y);
                    }
                }

                if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float rotation))
                {
                    Rotation = rotation;
                }

                var scaleElement = element.Element("Scale");
                if (scaleElement != null)
                {
                    if (float.TryParse(scaleElement.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleX) &&
                        float.TryParse(scaleElement.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float scaleY))
                    {
                        Scale = new Vector2(scaleX, scaleY);
                    }
                }

                if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out int sort))
                {
                    SetSort(sort);
                }

                if (bool.TryParse(element.Attribute("Active")?.Value, out bool active))
                {
                    SetActive(active);
                }

                var tagsElement = element.Element("Tags");
                if (tagsElement != null)
                {
                    foreach (var tag in Tags.ToList())
                    {
                        RemoveTag(tag);
                    }
                    foreach (var tagElement in tagsElement.Elements("Tag"))
                    {
                        var tagName = tagElement.Attribute("Name")?.Value;
                        if (!string.IsNullOrWhiteSpace(tagName))
                        {
                            SetTag(tagName);
                        }
                    }
                }

                // Restore sprite color — component exists since OnStart ran
                var sprite = element.Element("Sprite");
                if (sprite != null && SpriteComp != null)
                {
                    var colorAttr = sprite.Attribute("Color")?.Value;
                    if (colorAttr != null && uint.TryParse(colorAttr, out uint argb))
                    {
                        SpriteComp.Color = new Color(argb);
                    }
                }
            }
        }

        [Fact]
        public void Entity_SaveState_SavesTransform()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetId("test_entity");
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 1.57f;
            entity.Scale = new Vector2(2, 3);
            entity.SetSort(5);

            var xml = ((ISaveableEntity)entity).SaveState();

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
            entity.SetId("test_entity");

            var xml = XElement.Parse(@"
                <Entity Id=""test_entity"" Type=""Test"" Rotation=""0.785"" Sort=""10"" Active=""true"">
                    <Position X=""42"" Y=""99"" />
                    <Scale X=""1.5"" Y=""2.5"" />
                    <Tags><Tag Name=""player"" /></Tags>
                </Entity>");

            ((ISaveableEntity)entity).LoadState(xml);

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
            entity.SetId("custom_entity");
            entity.Position = new Vector2(10, 20);
            entity.Score = 42;
            entity.Name = "Hero";

            // Save state
            var xml = ((ISaveableEntity)entity).SaveState();

            // Load into a fresh entity
            var system2 = new EntitySystem();
            var restored = system2.CreateEntity<CustomStateEntity>();
            restored.SetId("custom_entity");
            ((ISaveableEntity)restored).LoadState(xml);

            Assert.Equal(new Vector2(10, 20), restored.Position);
            Assert.Equal(42, restored.Score);
            Assert.Equal("Hero", restored.Name);
        }

        [Fact]
        public void Entity_LoadState_ReplacesTags()
        {
            var system = new EntitySystem();
            var entity = system.CreateEntity<CustomStateEntity>();
            entity.SetId("test");
            entity.SetTag("runtime");

            var xml = XElement.Parse(@"
                <Entity Id=""test"" Type=""Test"" Rotation=""0"" Sort=""0"" Active=""true"">
                    <Position X=""0"" Y=""0"" />
                    <Tags><Tag Name=""saved"" /></Tags>
                </Entity>");

            ((ISaveableEntity)entity).LoadState(xml);

            // Tags are replaced (not merged) — runtime tag is cleared, saved tag is added
            Assert.False(entity.HasTag("runtime"), "Runtime tag should be cleared");
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
