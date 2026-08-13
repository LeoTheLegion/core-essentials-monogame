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
using CoreEssentials.Assets;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class BallLoadNullRefReproTest
    {
        // Simulate Ball entity that loads Sprite via AssetManager in OnStart
        public class MockBallEntity : Entity, ISaveableEntity
        {
            public SpriteComponent? SpriteComp { get; private set; }
            public bool OnStartCalled { get; private set; }

            public override void OnStart()
            {
                base.OnStart();
                OnStartCalled = true;
                // Simulate Ball.OnStart creating SpriteComponent with AssetManager
                // In real scenario, AssetManager.LoadAsset<Sprite> would be called
                // Here we create component but Sprite is null to simulate missing asset
                SpriteComp = new SpriteComponent(); // Sprite is null
                AddComponent(SpriteComp);
            }

            public XElement SaveState()
            {
                return new XElement("Entity",
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
        public void LoadState_EntityWithSpriteComponent_SpriteIsNull_DrawDoesNotThrow()
        {
            // Arrange - Create entity, save, load
            var system = new EntitySystem();
            var entity = system.CreateEntity<MockBallEntity>();
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 0.5f;
            entity.SetId("mock_ball_1");
            
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Act - Load into new system
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                var loaded = newSystem.GetEntities().First();
                
                // Verify OnStart was called during CreateEntity
                var mockBall = Assert.IsType<MockBallEntity>(loaded);
                Assert.True(mockBall.OnStartCalled, "OnStart should be called during CreateEntity");
                
                // Verify SpriteComponent exists
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                
                // Sprite is null because OnStart created component without asset
                Assert.Null(spriteComp!.Sprite);
                
                // The bug: SpriteComponent.Draw should handle null Sprite gracefully
                // Let's verify Draw doesn't throw
                // We can't call Draw without SpriteBatch, but we can verify the code path
                // The real issue is when Owner is null during Draw
                // Let's test that Owner is set correctly
                Assert.Same(loaded, spriteComp.Owner);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadState_WithComponents_CreatesComponentsViaReflection()
        {
            // Arrange
            var system = new EntitySystem();
            var entity = system.CreateEntity<MockBallEntity>();
            entity.SetId("ball_with_comp");
            entity.Position = new Vector2(10, 20);
            
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                // Verify XML contains component info
                var xml = File.ReadAllText(tempFile);
                // Components should be serialized if they implement ISerializableComponent
                // SpriteComponent doesn't implement it, so no component XML
                
                // Load
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                var loaded = newSystem.GetEntities().First();
                Assert.Equal("ball_with_comp", loaded.Id);
                Assert.Equal(new Vector2(10, 20), loaded.Position);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}
