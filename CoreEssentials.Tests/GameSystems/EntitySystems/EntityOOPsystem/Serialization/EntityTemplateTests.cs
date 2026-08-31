using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class EntityTemplateTests
    {
        [Fact]
        public void LoadFromXml_ParsesTemplateCorrectly()
        {
            // Arrange
            string xml = @"
                <EntityTemplate Type=""TestEntity"" Rotation=""45"" Sort=""5"" Active=""true"">
                    <Tags>
                        <Tag Name=""enemy"" />
                        <Tag Name=""flying"" />
                    </Tags>
                    <Components>
                        <Component Type=""SpriteComponent"">
                            <Properties>
                                <Property Name=""Color"" Value=""Red"" />
                                <Property Name=""Scale"" Value=""2.0"" />
                            </Properties>
                        </Component>
                    </Components>
                </EntityTemplate>";

            // Act
            var template = EntityTemplateLoader.LoadFromXml(xml);

            // Assert
            Assert.Equal("TestEntity", template.Type);
            Assert.Equal(45f, template.Rotation);
            Assert.Equal(5, template.Sort);
            Assert.True(template.Active);
            Assert.Contains("enemy", template.Tags);
            Assert.Contains("flying", template.Tags);
            Assert.Single(template.Components);
            Assert.Equal("SpriteComponent", template.Components[0].Type);
            Assert.Equal("Red", template.Components[0].Properties["Color"]);
            Assert.Equal("2.0", template.Components[0].Properties["Scale"]);
        }

        [Fact]
        public void LoadFromXml_ThrowsOnMissingType()
        {
            // Arrange
            string xml = @"<EntityTemplate Rotation=""45"" />";

            // Act & Assert
            Assert.Throws<FormatException>(() => EntityTemplateLoader.LoadFromXml(xml));
        }

        [Fact]
        public void LoadFromXml_ThrowsOnInvalidRoot()
        {
            // Arrange
            string xml = @"<WrongRoot Type=""TestEntity"" />";

            // Act & Assert
            Assert.Throws<FormatException>(() => EntityTemplateLoader.LoadFromXml(xml));
        }

        [Fact]
        public void LoadFromFile_LoadsTemplateFromFile()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            string xml = @"<EntityTemplate Type=""TestEntity"" />";
            File.WriteAllText(tempFile, xml);

            try
            {
                // Act
                var template = EntityTemplateLoader.LoadFromFile(tempFile);

                // Assert
                Assert.Equal("TestEntity", template.Type);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void LoadFromFile_ThrowsOnMissingFile()
        {
            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => 
                EntityTemplateLoader.LoadFromFile("nonexistent.xml"));
        }

        [Fact]
        public void RegisterPrefab_RegistersPrefabSuccessfully()
        {
            // Arrange
            var entitySystem = new EntitySystem();
            
            // Mock AssetManager - we'll test with direct loader instead
            var prefab = EntityTemplateLoader.LoadFromXml(@"<EntityTemplate Type=""TestEntity"" />");
            
            // Act
            entitySystem.RegisterPrefab("TestPrefab", prefab);

            // Assert
            Assert.True(entitySystem.HasPrefab("TestPrefab"));
        }

        [Fact]
        public void RegisterPrefab_IsIdempotentAndReplaces()
        {
            // Arrange
            var entitySystem = new EntitySystem();
            var first = new Prefab { Type = "TemplateTestEntity", Sort = 1 };
            var second = new Prefab { Type = "TemplateTestEntity", Sort = 2 };

            // Act
            entitySystem.RegisterPrefab("dup", first);
            entitySystem.RegisterPrefab("dup", second);

            // Assert — re-registration replaces rather than throwing
            Assert.True(entitySystem.HasPrefab("dup"));
            var entity = entitySystem.Instantiate("dup", Vector2.Zero);
            Assert.Equal(2, entity.GetSort());
        }

        [Fact]
        public void HasPrefab_ReturnsFalseForUnknownOrEmptyName()
        {
            // Arrange
            var entitySystem = new EntitySystem();

            // Act & Assert
            Assert.False(entitySystem.HasPrefab("unknown"));
            Assert.False(entitySystem.HasPrefab(""));
            Assert.False(entitySystem.HasPrefab(null!));
        }

        [Fact]
        public void Instantiate_CreatesEntityFromTemplate()
        {
            // Arrange
            var entitySystem = new EntitySystem();
            var prefab = new Prefab
            {
                Type = "TemplateTestEntity",
                Rotation = 90f,
                Sort = 10,
                Active = true,
                Tags = { "test", "template" }
            };

            entitySystem.RegisterPrefab("TestTemplate", prefab);

            var position = new Vector2(100, 200);

            // Act
            var entity = entitySystem.Instantiate("TestTemplate", position);

            // Assert
            Assert.NotNull(entity);
            Assert.IsType<TemplateTestEntity>(entity);
            Assert.Equal(position, entity.Position);
            Assert.Equal(90f, entity.Rotation);
            Assert.Equal(10, entity.GetSort());
            Assert.True(entity.GetActive());
            Assert.True(entity.HasTag("test"));
            Assert.True(entity.HasTag("template"));
        }

        [Fact]
        public void Instantiate_ThrowsOnUnregisteredTemplate()
        {
            // Arrange
            var entitySystem = new EntitySystem();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => 
                entitySystem.Instantiate("NonExistent", Vector2.Zero));
        }

        [Fact]
        public void Instantiate_CreatesIndependentInstances()
        {
            // Arrange
            var entitySystem = new EntitySystem();
            var prefab = new Prefab
            {
                Type = "TemplateTestEntity",
                Rotation = 0f
            };

            entitySystem.RegisterPrefab("TestTemplate", prefab);

            // Act
            var entity1 = entitySystem.Instantiate("TestTemplate", new Vector2(10, 20));
            var entity2 = entitySystem.Instantiate("TestTemplate", new Vector2(30, 40));

            // Assert
            Assert.NotSame(entity1, entity2);
            Assert.Equal(new Vector2(10, 20), entity1.Position);
            Assert.Equal(new Vector2(30, 40), entity2.Position);
            
            // Modify one entity
            entity1.Position = new Vector2(999, 999);
            Assert.NotEqual(entity1.Position, entity2.Position);
        }

        [Fact]
        public void Instantiate_SupportsPositionOverride()
        {
            // Arrange
            var entitySystem = new EntitySystem();
            var prefab = new Prefab
            {
                Type = "TemplateTestEntity"
            };

            entitySystem.RegisterPrefab("TestTemplate", prefab);

            // Act
            var entity = entitySystem.Instantiate("TestTemplate", new Vector2(500, 600));

            // Assert
            Assert.Equal(new Vector2(500, 600), entity.Position);
        }

        [Fact]
        public void LoadFromXml_ParsesChildrenTemplates()
        {
            // Arrange
            string xml = @"
                <EntityTemplate Type=""ParentEntity"">
                    <Children>
                        <EntityTemplate Type=""ChildEntity"" Rotation=""30"" />
                    </Children>
                </EntityTemplate>";

            // Act
            var template = EntityTemplateLoader.LoadFromXml(xml);

            // Assert
            Assert.Equal("ParentEntity", template.Type);
            Assert.Single(template.Children);
            Assert.Equal("ChildEntity", template.Children[0].Type);
            Assert.Equal(30f, template.Children[0].Rotation);
        }

        [Fact]
        public void LoadFromXml_HandlesMissingOptionalElements()
        {
            // Arrange
            string xml = @"<EntityTemplate Type=""TestEntity"" />";

            // Act
            var template = EntityTemplateLoader.LoadFromXml(xml);

            // Assert
            Assert.Equal("TestEntity", template.Type);
            Assert.Equal(0f, template.Rotation);
            Assert.Equal(0, template.Sort);
            Assert.True(template.Active);
            Assert.Empty(template.Tags);
            Assert.Empty(template.Components);
            Assert.Empty(template.Children);
        }

        private class TemplateTestEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }
    }
}
