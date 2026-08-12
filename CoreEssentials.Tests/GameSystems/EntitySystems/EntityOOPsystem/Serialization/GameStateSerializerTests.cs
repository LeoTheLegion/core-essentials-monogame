using System;
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
                GameStateSerializer.LoadState(system, tempFile, mergeExisting: false);

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
        public void LoadState_MergeModePreservesExistingEntities()
        {
            // Arrange
            var system = CreateTestSystem();
            var existingEntity = system.CreateEntity<TestEntity>();
            existingEntity.SetId("existing_entity");
            existingEntity.Position = new Vector2(50, 50);
            existingEntity.SetTag("runtime");

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
                GameStateSerializer.LoadState(system, tempFile, mergeExisting: true);

                // Assert
                var entities = system.GetEntities();
                Assert.Equal(2, entities.Count);
                
                var updatedEntity = entities.FirstOrDefault(e => e.Id == "existing_entity");
                Assert.NotNull(updatedEntity);
                Assert.Equal(new Vector2(100, 200), updatedEntity.Position);
                // Runtime tag should be preserved in merge mode
                Assert.True(updatedEntity.HasTag("runtime"));
                
                var newEntity = entities.FirstOrDefault(e => e.Id == "new_entity");
                Assert.NotNull(newEntity);
                Assert.Equal(new Vector2(300, 400), newEntity.Position);
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
                GameStateSerializer.LoadState(newSystem, tempFile, mergeExisting: false);

                // Assert
                var entities = newSystem.GetEntities();
                Assert.Single(entities); // Only parent at root level
                
                var loadedParent = entities[0];
                Assert.Equal("parent", loadedParent.Id);
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

        private class TestEntity : Entity
        {
            public override void OnStart()
            {
                base.OnStart();
            }
        }
    }
}
