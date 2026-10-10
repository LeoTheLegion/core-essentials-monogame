#nullable enable
using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Covers the <see cref="Entity.GetSize"/>/<see cref="Entity.GetOrigin"/> resolution logic (which
    /// reads the entity's <see cref="SpriteComponent"/>, applies <see cref="Entity.Scale"/>, and falls
    /// back to zero when the sprite is absent or not yet loaded) — pure logic, no graphics device. Also
    /// exercises a couple of small guard branches that were otherwise uncovered.
    /// </summary>
    public class EntitySizeOriginTests
    {
        private sealed class TestEntity : Entity
        {
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
        }

        // ──────────────── GetSize ────────────────

        [Fact]
        public void GetSize_NoSpriteComponent_ReturnsZero()
        {
            var entity = new TestEntity();

            Assert.Equal(Vector2.Zero, entity.GetSize());
        }

        [Fact]
        public void GetSize_SpriteNotLoaded_FallsBackToZero()
        {
            // A Sprite with no metadata throws on GetSize; Entity must swallow it and return zero.
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent(new Sprite("missing.xml")));

            Assert.Equal(Vector2.Zero, entity.GetSize());
        }

        [Fact]
        public void GetSize_WithLoadedSprite_ReturnsSpriteSizeTimesScale()
        {
            var sprite = new Sprite("tex.xml")
            {
                TestMetaData = new Sprite.SpriteMeta
                {
                    SourceType = "texture2d",
                    Size = new Sprite.Size { Width = 10f, Height = 20f }
                }
            };
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent(sprite));
            entity.Scale = new Vector2(2f, 3f);

            Assert.Equal(new Vector2(20f, 60f), entity.GetSize());
        }

        [Fact]
        public void GetSize_UnitScale_ReturnsRawSpriteSize()
        {
            var sprite = new Sprite("tex.xml")
            {
                TestMetaData = new Sprite.SpriteMeta
                {
                    SourceType = "texture2d",
                    Size = new Sprite.Size { Width = 16f, Height = 8f }
                }
            };
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent(sprite));

            Assert.Equal(new Vector2(16f, 8f), entity.GetSize());
        }

        // ──────────────── GetOrigin ────────────────

        [Fact]
        public void GetOrigin_NoSpriteComponent_ReturnsZero()
        {
            var entity = new TestEntity();

            Assert.Equal(Vector2.Zero, entity.GetOrigin());
        }

        [Fact]
        public void GetOrigin_SpriteNotLoaded_FallsBackToZero()
        {
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent(new Sprite("missing.xml")));

            Assert.Equal(Vector2.Zero, entity.GetOrigin());
        }

        [Fact]
        public void GetOrigin_WithLoadedSprite_ReturnsSpriteOriginTimesScale()
        {
            var sprite = new Sprite("tex.xml")
            {
                TestMetaData = new Sprite.SpriteMeta
                {
                    SourceType = "texture2d",
                    Origin = new Sprite.Origin { X = 3f, Y = 4f }
                }
            };
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent(sprite));
            entity.Scale = new Vector2(2f, 2f);

            Assert.Equal(new Vector2(6f, 8f), entity.GetOrigin());
        }

        [Fact]
        public void GetOrigin_NullOrigin_ReturnsZero()
        {
            var sprite = new Sprite("tex.xml")
            {
                TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d", Origin = null }
            };
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent(sprite));

            Assert.Equal(Vector2.Zero, entity.GetOrigin());
        }

        // ──────────────── Small guard branches ────────────────

        [Fact]
        public void RegisterForInstancedRendering_Sprite_UsesSpriteTexture()
        {
            var entity = new TestEntity();
            // No texture behind the sprite → BatchTexture stays null, dirty flag raised.
            entity.RegisterForInstancedRendering(new Sprite("missing.xml"));

            Assert.Null(entity.BatchTexture);
            Assert.True(entity.BatchTextureDirty);
        }

        [Fact]
        public void RemoveChild_RemovesAndClearsParent()
        {
            var parent = new TestEntity();
            var child = new TestEntity();
            parent.AddChild(child);
            Assert.Same(parent, child.Parent);

            bool removed = parent.RemoveChild(child);

            Assert.True(removed);
            Assert.Null(child.Parent);
        }

        [Fact]
        public void RemoveChild_NotAChild_ReturnsFalse()
        {
            var parent = new TestEntity();
            var other = new TestEntity();

            Assert.False(parent.RemoveChild(other));
        }

        [Fact]
        public void AddComponent_DuplicateType_Throws()
        {
            var entity = new TestEntity();
            entity.AddComponent(new PlainComponent());

            Assert.Throws<InvalidOperationException>(() => entity.AddComponent(new PlainComponent()));
        }

        [Fact]
        public void AddComponent_Null_Throws()
        {
            var entity = new TestEntity();

            Assert.ThrowsAny<ArgumentNullException>(() => entity.AddComponent((EntityComponent)null!));
        }

        private sealed class PlainComponent : EntityComponent { }
    }
}
