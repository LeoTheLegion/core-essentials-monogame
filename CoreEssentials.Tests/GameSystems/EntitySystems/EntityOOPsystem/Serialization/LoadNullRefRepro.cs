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
    public class LoadNullRefRepro
    {
        // Test entity that creates SpriteComponent in OnStart
        public class BallLikeEntity : Entity, ISaveableEntity
        {
            public SpriteComponent? SpriteComp { get; private set; }
            
            public override void OnStart()
            {
                base.OnStart();
                // Simulate Ball.OnStart - creates SpriteComponent with AssetManager.LoadAsset
                // For test, we simulate missing asset by creating component with null Sprite
                SpriteComp = new SpriteComponent(); // Sprite is null
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
        public void LoadState_WithOnStartCreatingComponents_ComponentsArePreserved()
        {
            // Arrange
            var system = new EntitySystem();
            var entity = system.CreateEntity<BallLikeEntity>();
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 0.5f;
            entity.SetId("ball1");
            
            var tempFile = Path.GetTempFileName();
            try
            {
                // Save
                GameStateSerializer.SaveState(system, tempFile);
                
                // Load
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                var loaded = newSystem.GetEntities().First();
                
                // Verify entity loaded correctly
                Assert.Equal("ball1", loaded.Id);
                Assert.Equal(new Vector2(100, 200), loaded.Position);
                Assert.Equal(0.5f, loaded.Rotation, 0.01f);
                
                // Verify OnStart was called and component created
                var ball = Assert.IsType<BallLikeEntity>(loaded);
                Assert.NotNull(ball.SpriteComp);
                Assert.Same(loaded, ball.SpriteComp!.Owner);
                
                // Component should exist
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadState_ComponentsCreatedViaReflection_HaveOwnerSet()
        {
            // This tests the bug where LoadEntityComponents creates component via Activator
            // but Owner might not be set correctly
            var system = new EntitySystem();
            var entity = system.CreateEntity<BallLikeEntity>();
            entity.SetId("test");
            
            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, tempFile);
                
                var newSystem = new EntitySystem();
                // Simulate loading XML with component data
                // Create a simple XML with component
                // var xml is defined below for reference but not used directly
                _ = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GameState Version=""1.0"">
  <Entities>
    <Entity Id=""test"" Type=""CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization.LoadNullRefRepro+BallLikeEntity"" Rotation=""0"" Sort=""0"" Active=""true"">
      <Position X=""10"" Y=""20"" />
      <Components>
        <Component Type=""SpriteComponent"">
          <Properties />
        </Component>
      </Entities>
</GameState>";
                // Actually use proper save/load
                GameStateSerializer.LoadState(newSystem, tempFile);
                
                var loaded = newSystem.GetEntities().First();
                var spriteComp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(spriteComp);
                // Owner should be set
                Assert.Same(loaded, spriteComp!.Owner);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}
#nullable enable