using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class GameStateSerializerTests
    {
        private EntitySystem CreateTestSystem()
        {
            return new EntitySystem();
        }

        [Fact]
        public void SaveState_CreatesValidXmlFile()
        {
            // Arrange
            var system = CreateTestSystem();
            var entity = system.CreateEntity<TestEntity>();
            entity.SetId("test_entity_1");
            entity.Position = new Vector2(100, 200);
            entity.Rotation = 1.57f;
            entity.SetSort(5);
            entity.SetTag("player");

            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                GameStateSerializer.SaveState(system, tempFile);

                // Assert
                Assert.True(File.Exists(tempFile));
                var xml = XDocument.Load(tempFile);
                Assert.Equal("GameState", xml.Root?.Name.LocalName);
                
                var entitiesElement = xml.Root?.Element("Entities");
                Assert.NotNull(entitiesElement);
                
                var entityElement = entitiesElement?.Elements("Entity").FirstOrDefault();
                Assert.NotNull(entityElement);
                Assert.Equal("test_entity_1", entityElement?.Attribute("Id")?.Value);
                Assert.Equal("100", entityElement?.Element("Position")?.Attribute("X")?.Value);
                Assert.Equal("200", entityElement?.Element("Position")?.Attribute("Y")?.Value);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadState_RestoresEntityState()
        {
            // Arrange
            var system = CreateTestSystem();
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GameState Version=""1.0"" Timestamp=""2026-01-01T00:00:00Z"">
  <Entities>
    <Entity Id=""test_entity_1"" Type=""CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization.GameStateSerializerTests+TestEntity"" Rotation=""1.57"" Sort=""5"" Active=""true"">
      <Position X=""100"" Y=""200"" />
      <Tags>
        <Tag Name=""player"" />
      </Tags>
    </Entity>
  </Entities>
</GameState>";

            var tempFile = Path.GetTempFileName();

            try
            {
                File.WriteAllText(tempFile, xml);

                // Act
                GameStateSerializer.LoadState(system, tempFile);

                // Assert
                var entities = system.GetEntities();
                Assert.Single(entities);
                var entity = entities[0];
                Assert.Equal("test_entity_1", entity.Id);
                Assert.Equal(new Vector2(100, 200), entity.Position);
                Assert.Equal(1.57f, entity.Rotation, 0.01f);
                Assert.Equal(5, entity.GetSort());
                Assert.True(entity.HasTag("player"));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadState_UpdatesExistingEntitiesById()
        {
            // Arrange: create an entity that will also be in the save file
            var system = CreateTestSystem();
            var existingEntity = system.CreateEntity<TestEntity>();
            existingEntity.SetId("existing_entity");
            existingEntity.Position = new Vector2(50, 50);
            existingEntity.SetTag("runtime");

            // Also create an entity NOT in the save file (should be removed)
            var extraEntity = system.CreateEntity<TestEntity>();
            extraEntity.SetId("extra_entity");
            extraEntity.Position = new Vector2(999, 999);
            extraEntity.SetTag("extra");

            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GameState Version=""1.0"" Timestamp=""2026-01-01T00:00:00Z"">
  <Entities>
    <Entity Id=""existing_entity"" Type=""CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization.GameStateSerializerTests+TestEntity"" Rotation=""0.5"" Sort=""3"" Active=""true"">
      <Position X=""100"" Y=""200"" />
      <Tags>
        <Tag Name=""saved"" />
      </Tags>
    </Entity>
    <Entity Id=""new_entity"" Type=""CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization.GameStateSerializerTests+TestEntity"" Rotation=""0"" Sort=""0"" Active=""true"">
      <Position X=""300"" Y=""400"" />
      <Tags>
        <Tag Name=""new"" />
      </Tags>
    </Entity>
  </Entities>
</GameState>";

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, xml);

            try
            {
                // Act
                GameStateSerializer.LoadState(system, tempFile);

                // Assert: existing entity updated from save file, new entity created, extra entity removed
                var entities = system.GetEntities();
                Assert.Equal(2, entities.Count);

                var updatedEntity = entities.FirstOrDefault(e => e.Id == "existing_entity");
                Assert.NotNull(updatedEntity);
                Assert.Equal(new Vector2(100, 200), updatedEntity.Position);
                // Tags are replaced (not merged) — runtime tag is cleared, saved tag is added
                Assert.False(updatedEntity.HasTag("runtime"), "Runtime tag should be replaced");
                Assert.True(updatedEntity.HasTag("saved"));

                var newEntity = entities.FirstOrDefault(e => e.Id == "new_entity");
                Assert.NotNull(newEntity);
                Assert.Equal(new Vector2(300, 400), newEntity.Position);

                // Extra entity should have been removed (not in save file)
                var extraRemaining = entities.FirstOrDefault(e => e.Id == "extra_entity");
                Assert.True(extraRemaining == null, "Extra entity not in save file should be removed");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void SaveLoadRoundTrip_PreservesEntityHierarchy()
        {
            // Arrange
            var system = CreateTestSystem();
            var parent = system.CreateEntity<TestEntity>();
            parent.SetId("parent");
            parent.Position = new Vector2(100, 100);
            
            var child = system.CreateEntity<TestEntity>();
            child.SetId("child");
            child.Position = new Vector2(10, 10);
            parent.AddChild(child);

            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                GameStateSerializer.SaveState(system, tempFile);
                
                var newSystem = CreateTestSystem();
                GameStateSerializer.LoadState(newSystem, tempFile);

                // Assert - both parent and child are at root level since all entities with IDs are serialized
                var entities = newSystem.GetEntities();
                Assert.Equal(2, entities.Count);
                
                var loadedParent = entities.FirstOrDefault(e => e.Id == "parent");
                Assert.NotNull(loadedParent);
                Assert.Single(loadedParent.Children);
                Assert.Equal("child", loadedParent.Children[0].Id);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void SaveState_ThrowsOnNullSystem()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                GameStateSerializer.SaveState(null!, "test.xml"));
        }

        [Fact]
        public void LoadState_ThrowsOnMissingFile()
        {
            // Arrange
            var system = CreateTestSystem();

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => 
                GameStateSerializer.LoadState(system, "nonexistent.xml"));
        }

        [Fact]
        public void LoadState_ThrowsOnInvalidXml()
        {
            // Arrange
            var system = CreateTestSystem();
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "invalid xml content");

            try
            {
                // Act & Assert
                Assert.ThrowsAny<Exception>(() => 
                    GameStateSerializer.LoadState(system, tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        private class TestEntity : Entity, ISaveableEntity
        {
            public override void OnStart()
            {
                base.OnStart();
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
    }
}
