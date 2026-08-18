#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class BallLoadNullRefTest
    {
        public class TestBallEntity : Entity, ISaveableEntity
        {
            public SpriteComponent? SpriteComp { get; private set; }

            public override void OnStart()
            {
                base.OnStart();
                // Simulate Ball.OnStart - create sprite component with asset load
                // In real scenario, AssetManager.LoadAsset would return a Sprite
                // For test, we create a component without Sprite set to simulate load failure
                SpriteComp = new SpriteComponent();
                AddComponent(SpriteComp);
            }

            public XElement SaveState()
            {
                return new XElement("Entity",
                    new XAttribute("Id", Id ?? string.Empty),
                    new XAttribute("Type", GetType().FullName ?? string.Empty),
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
                    ));
            }

            public void LoadState(XElement element)
            {
                var pos = element.Element("Position");
                if (pos != null)
                    Position = new Vector2(float.Parse(pos.Attribute("X")?.Value ?? "0", CultureInfo.InvariantCulture),
                        float.Parse(pos.Attribute("Y")?.Value ?? "0", CultureInfo.InvariantCulture));

                if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rot))
                    Rotation = rot;

                var sc = element.Element("Scale");
                if (sc != null)
                    Scale = new Vector2(float.Parse(sc.Attribute("X")?.Value ?? "1", CultureInfo.InvariantCulture),
                        float.Parse(sc.Attribute("Y")?.Value ?? "1", CultureInfo.InvariantCulture));

                if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sort))
                    SetSort(sort);

                if (bool.TryParse(element.Attribute("Active")?.Value, out var active))
                    SetActive(active);

                foreach (var t in Tags.ToList()) RemoveTag(t);
                var tagsEl = element.Element("Tags");
                if (tagsEl != null)
                    foreach (var tag in tagsEl.Elements("Tag"))
                        SetTag(tag.Attribute("Name")?.Value ?? "default");
            }
        }

        [Fact]
        public void LoadState_BallEntity_WithSpriteComponent_SpriteIsNull_DoesNotThrow()
        {
            // Arrange
            var system = new EntitySystem();
            var entity = system.CreateEntity<TestBallEntity>();
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 0.5f;
            entity.SetId("test_ball_1");
            
            // Save state
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Act - Load into new system
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                // Assert - Should load without throwing, and entity should exist
                var entities = newSystem.GetEntities();
                Assert.Single(entities);
                var loaded = entities.First();
                Assert.Equal("test_ball_1", loaded.Id);
                Assert.Equal(new Vector2(100, 200), loaded.Position);
                Assert.Equal(0.5f, loaded.Rotation, 0.01f);
                
                // Verify SpriteComponent exists but Sprite is null (since OnStart created it without asset)
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                // Sprite should be null because we didn't set it
                Assert.Null(spriteComp!.Sprite);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadState_BallEntity_CreatesEntityViaReflection_CallsOnStart()
        {
            // Arrange
            var system = new EntitySystem();
            var entity = system.CreateEntity<TestBallEntity>();
            entity.Position = new Vector2(50, 60);
            entity.SetId("ball_test");
            
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Act
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                // Assert
                var entities = newSystem.GetEntities();
                Assert.Single(entities);
                var loaded = entities.First();
                
                // OnStart should have been called during CreateEntity
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                
                // Position should be restored
                Assert.Equal(new Vector2(50, 60), loaded.Position);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
#nullable enable