using System;
using System.IO;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

public class EntitySerializerTests
{
    private class TestEntity : Entity
    {
        public override void Render(SpriteBatch spriteBatch) { }
    }

    private EntitySystem CreateEntitySystem()
    {
        return new EntitySystem();
    }

    #region LoadEntity Tests

    [Fact]
    public void LoadEntity_WithValidXml_ShouldCreateEntity()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.NotNull(entity);
        Assert.IsType<TestEntity>(entity);
    }

    [Fact]
    public void LoadEntity_WithPosition_ShouldSetCorrectPosition()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""100"" Y=""200"" />
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.Equal(100f, entity.Position.X);
        Assert.Equal(200f, entity.Position.Y);
    }

    [Fact]
    public void LoadEntity_WithRotation_ShouldSetCorrectRotation()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity Rotation=""1.57"">
                <Position X=""0"" Y=""0"" />
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.Equal(1.57f, entity.Rotation);
    }

    [Fact]
    public void LoadEntity_WithSort_ShouldSetCorrectSort()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity Sort=""42"">
                <Position X=""0"" Y=""0"" />
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.Equal(42, entity.GetSort());
    }

    [Fact]
    public void LoadEntity_WithTags_ShouldSetCorrectTags()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
                <Tags>
                    <Tag Name=""Enemy"" />
                    <Tag Name=""Hostile"" />
                </Tags>
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.True(entity.HasTag("Enemy"));
        Assert.True(entity.HasTag("Hostile"));
        Assert.Equal(2, entity.Tags.Count);
    }

    [Fact]
    public void LoadEntity_WithActive_ShouldSetActiveState()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity Active=""false"">
                <Position X=""0"" Y=""0"" />
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.False(entity.GetActive());
    }

    [Fact]
    public void LoadEntity_WithAllProperties_ShouldConfigureAll()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity Rotation=""3.14"" Sort=""10"" Active=""true"">
                <Position X=""50"" Y=""75"" />
                <Tags>
                    <Tag Name=""Player"" />
                </Tags>
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.Equal(new Vector2(50f, 75f), entity.Position);
        Assert.Equal(3.14f, entity.Rotation);
        Assert.Equal(10, entity.GetSort());
        Assert.True(entity.GetActive());
        Assert.True(entity.HasTag("Player"));
    }

    [Fact]
    public void LoadEntity_WithoutOptionalProperties_ShouldUseDefaults()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
            </Entity>";

        // Act
        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);

        // Assert
        Assert.Equal(0f, entity.Rotation);
        Assert.Equal(-1, entity.GetSort());
        Assert.True(entity.GetActive());
        Assert.Empty(entity.Tags);
    }

    [Fact]
    public void LoadEntity_WithInvalidXml_ShouldThrowFormatException()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = "<invalid>not an entity</invalid>";

        // Act & Assert
        Assert.Throws<FormatException>(() => EntitySerializer.LoadEntity<TestEntity>(xml, system));
    }

    [Fact]
    public void LoadEntity_WithMalformedXml_ShouldThrowFormatException()
    {
        // Arrange
        var system = CreateEntitySystem();
        var xml = "<Entity><Position>";

        // Act & Assert
        Assert.Throws<FormatException>(() => EntitySerializer.LoadEntity<TestEntity>(xml, system));
    }

    [Fact]
    public void LoadEntityFromFile_WithFilePath_FileNotFound_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var system = CreateEntitySystem();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            EntitySerializer.LoadEntityFromFile<TestEntity>("/nonexistent/path/entity.xml", system));
    }

    [Fact]
    public void LoadEntityFromFile_WithFilePath_ValidFile_ShouldLoadEntity()
    {
        // Arrange
        var system = CreateEntitySystem();
        var tempFile = Path.GetTempFileName();
        var xml = @"
            <Entity>
                <Position X=""123"" Y=""456"" />
            </Entity>";
        File.WriteAllText(tempFile, xml);

        // Act
        var entity = EntitySerializer.LoadEntityFromFile<TestEntity>(tempFile, system);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(123f, entity.Position.X);
        Assert.Equal(456f, entity.Position.Y);

        // Cleanup
        File.Delete(tempFile);
    }

    #endregion

    #region SaveEntity Tests

    [Fact]
    public void SaveEntityToString_ShouldReturnValidXml()
    {
        // Arrange
        var system = CreateEntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(100f, 200f);
        entity.Rotation = 1.5f;
        entity.SetSort(5);
        entity.SetTag("Player");

        // Act
        var xml = EntitySerializer.SaveEntityToString(entity);

        // Assert
        Assert.Contains("<Entity", xml);
        Assert.Contains("Position", xml);
        Assert.Contains("Player", xml);
    }

    [Fact]
    public void SaveEntity_ToFile_ShouldCreateFile()
    {
        // Arrange
        var system = CreateEntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(50f, 50f);
        var tempFile = Path.GetTempFileName();

        // Act
        EntitySerializer.SaveEntity(entity, tempFile);

        // Assert
        Assert.True(File.Exists(tempFile));
        var content = File.ReadAllText(tempFile);
        Assert.Contains("<Entity", content);

        // Cleanup
        File.Delete(tempFile);
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void SaveAndLoadEntity_ShouldPreserveProperties()
    {
        // Arrange
        var system1 = CreateEntitySystem();
        var original = system1.CreateEntity<TestEntity>();
        original.Position = new Vector2(100f, 200f);
        original.Rotation = 2.5f;
        original.SetSort(7);
        original.SetTag("Enemy");
        original.SetActive(true);

        // Act
        var xml = EntitySerializer.SaveEntityToString(original);
        var system2 = CreateEntitySystem();
        var loaded = EntitySerializer.LoadEntity<TestEntity>(xml, system2);

        // Assert
        Assert.Equal(original.Position, loaded.Position);
        Assert.Equal(original.Rotation, loaded.Rotation);
        Assert.Equal(original.GetSort(), loaded.GetSort());
        Assert.True(loaded.HasTag("Enemy"));
    }

    [Fact]
    public void SaveAndLoadEntityFromFile_ShouldPreserveProperties()
    {
        // Arrange
        var system1 = CreateEntitySystem();
        var original = system1.CreateEntity<TestEntity>();
        original.Position = new Vector2(300f, 400f);
        original.SetSort(15);
        original.SetTag("Boss");
        var tempFile = Path.GetTempFileName();

        // Act
        EntitySerializer.SaveEntity(original, tempFile);
        var system2 = CreateEntitySystem();
        var loaded = EntitySerializer.LoadEntityFromFile<TestEntity>(tempFile, system2);

        // Assert
        Assert.Equal(original.Position, loaded.Position);
        Assert.Equal(original.GetSort(), loaded.GetSort());
        Assert.True(loaded.HasTag("Boss"));

        // Cleanup
        File.Delete(tempFile);
    }

    #endregion

    #region Component Loading Tests (T2)

    [Fact]
    public void LoadEntity_WithSpriteComponent_ShouldAttachComponent()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
                <Components>
                    <Component Type=""SpriteComponent"">
                        <Properties>
                            <Property Name=""Scale"" Value=""2,2"" />
                        </Properties>
                    </Component>
                </Components>
            </Entity>";

        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);
        var sprite = entity.GetComponent<SpriteComponent>();
        Assert.NotNull(sprite);
    }

    [Fact]
    public void LoadEntity_WithMultipleComponents_ShouldAttachAll()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
                <Components>
                    <Component Type=""SpriteComponent"" />
                    <Component Type=""RigidbodyComponent"" />
                </Components>
            </Entity>";

        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);
        Assert.NotNull(entity.GetComponent<SpriteComponent>());
        Assert.NotNull(entity.GetComponent<RigidbodyComponent>());
    }

    [Fact]
    public void LoadEntity_WithComponentPropertyVector2_ShouldSetCorrectValue()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
                <Components>
                    <Component Type=""SpriteComponent"">
                        <Properties>
                            <Property Name=""Scale"" Value=""3,4"" />
                        </Properties>
                    </Component>
                </Components>
            </Entity>";

        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);
        var sprite = entity.GetComponent<SpriteComponent>();

        Assert.NotNull(sprite);
        Assert.Equal(new Vector2(3f, 4f), sprite.Scale);
    }

    [Fact]
    public void LoadEntity_WithComponentPropertyColor_ShouldSetCorrectValue()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
                <Components>
                    <Component Type=""SpriteComponent"">
                        <Properties>
                            <Property Name=""Color"" Value=""Red"" />
                        </Properties>
                    </Component>
                </Components>
            </Entity>";

        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);
        var sprite = entity.GetComponent<SpriteComponent>();

        Assert.NotNull(sprite);
        Assert.Equal(Color.Red, sprite.Color);
    }

    [Fact]
    public void LoadEntity_WithMissingComponentType_ShouldSkipAndSucceed()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
                <Components>
                    <Component Type=""NonExistentComponent"" />
                </Components>
            </Entity>";

        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system);
        Assert.NotNull(entity);
    }

    [Fact]
    public void LoadEntity_WithCustomComponentFactory_ShouldUseFactory()
    {
        var system = CreateEntitySystem();
        var factory = new DefaultComponentFactory();
        factory.RegisterBuiltIns();
        var xml = @"
            <Entity>
                <Position X=""0"" Y=""0"" />
                <Components>
                    <Component Type=""SpriteComponent"" />
                </Components>
            </Entity>";

        var entity = EntitySerializer.LoadEntity<TestEntity>(xml, system, factory);
        Assert.NotNull(entity.GetComponent<SpriteComponent>());
    }

    #endregion

    #region Scene Loading Tests (T3)

    [Fact]
    public void LoadSceneFromXml_WithValidScene_ShouldReturnEntities()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Scene>
                <EntityDefinition Type=""TestEntity"" Id=""player"">
                    <Position X=""100"" Y=""200"" />
                </EntityDefinition>
                <EntityDefinition Type=""TestEntity"" Id=""enemy"">
                    <Position X=""300"" Y=""400"" />
                </EntityDefinition>
            </Scene>";

        var entities = EntitySerializer.LoadSceneFromXml(xml, system);
        Assert.Equal(2, entities.Count);
    }

    [Fact]
    public void LoadSceneFromXml_WithChildren_ShouldCreateHierarchy()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Scene>
                <EntityDefinition Type=""TestEntity"" Id=""parent"">
                    <Position X=""0"" Y=""0"" />
                    <Children>
                        <EntityDefinition Type=""TestEntity"" Id=""child1"">
                            <Position X=""10"" Y=""20"" />
                        </EntityDefinition>
                        <EntityDefinition Type=""TestEntity"" Id=""child2"">
                            <Position X=""30"" Y=""40"" />
                        </EntityDefinition>
                    </Children>
                </EntityDefinition>
            </Scene>";

        var entities = EntitySerializer.LoadSceneFromXml(xml, system);

        Assert.Single(entities);
        Assert.Equal(2, entities[0].Children.Count);
    }

    [Fact]
    public void LoadSceneFromXml_WithTagsAndComponents_ShouldConfigureEntities()
    {
        var system = CreateEntitySystem();
        var xml = @"
            <Scene>
                <EntityDefinition Type=""TestEntity"" Id=""player"">
                    <Position X=""50"" Y=""50"" />
                    <Tags>
                        <Tag Name=""Player"" />
                        <Tag Name=""Controllable"" />
                    </Tags>
                    <Components>
                        <Component Type=""SpriteComponent"" />
                    </Components>
                </EntityDefinition>
            </Scene>";

        var entities = EntitySerializer.LoadSceneFromXml(xml, system);

        Assert.Single(entities);
        Assert.True(entities[0].HasTag("Player"));
        Assert.True(entities[0].HasTag("Controllable"));
        Assert.NotNull(entities[0].GetComponent<SpriteComponent>());
    }

    [Fact]
    public void LoadSceneFromXml_EmptyScene_ShouldReturnEmptyList()
    {
        var system = CreateEntitySystem();
        var xml = "<Scene></Scene>";

        var entities = EntitySerializer.LoadSceneFromXml(xml, system);
        Assert.Empty(entities);
    }

    [Fact]
    public void LoadSceneFromFile_WithValidFile_ShouldLoadScene()
    {
        var system = CreateEntitySystem();
        var tempFile = Path.GetTempFileName();
        var xml = @"
            <Scene>
                <EntityDefinition Type=""TestEntity"" Id=""entity1"">
                    <Position X=""100"" Y=""200"" />
                </EntityDefinition>
            </Scene>";
        File.WriteAllText(tempFile, xml);

        var entities = EntitySerializer.LoadSceneFromFile(tempFile, system);

        Assert.Single(entities);
        Assert.Equal(100f, entities[0].Position.X);

        File.Delete(tempFile);
    }

    [Fact]
    public void LoadSceneFromFile_MissingFile_ShouldThrowFileNotFoundException()
    {
        var system = CreateEntitySystem();

        Assert.Throws<FileNotFoundException>(() =>
            EntitySerializer.LoadSceneFromFile("/nonexistent/scene.xml", system));
    }

    [Fact]
    public void LoadSceneFromXml_InvalidRoot_ShouldThrowFormatException()
    {
        var system = CreateEntitySystem();
        var xml = @"<Entity>
            <Position X=""0"" Y=""0"" />
        </Entity>";

        Assert.Throws<FormatException>(() =>
            EntitySerializer.LoadSceneFromXml(xml, system));
    }

    #endregion
}
