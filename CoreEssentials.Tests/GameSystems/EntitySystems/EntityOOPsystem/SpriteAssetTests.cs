using System;
using System.IO;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Tests for the declarative <c>SpriteAsset</c> properties on the built-in
    /// <see cref="SpriteComponent"/> and <see cref="AnimationComponent"/>, which let data-driven
    /// (XML) entities declare their visual with a plain string — resolved through the
    /// <see cref="AssetManager"/> on attach — instead of needing per-game loader components.
    /// Loading is proven headlessly with a mock content manager.
    /// </summary>
    public class SpriteAssetTests
    {
        private class TestEntity : Entity
        {
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
        }

        /// <summary>
        /// Stages real content files so the full headless load chain resolves:
        /// each sprite's XML metadata (read by XMLAsset) plus its texture2d source
        /// (loaded by Texture2DAsset, whose mock content returns a null-safe Texture2D).
        /// </summary>
        private static void StageSprites(params string[] assetNames)
        {
            foreach (var name in assetNames)
                WriteContent(name,
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n"
                    + "<SpriteData xmlns=\"http://schemas.coreessentials.monogame/2025/sprite\">\n"
                    + "  <SourceType>texture2d</SourceType>\n"
                    + "  <Source>Sprites/spriteasset_tex.xml</Source>\n"
                    + "  <Frame>0</Frame>\n"
                    + "</SpriteData>");

            var content = new MockContentManager();
            AssetManager.Init(content);
        }

        private static void WriteContent(string fileName, string xml)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Content", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, xml);
        }

        // ===== SpriteComponent.SpriteAsset =====

        [Fact]
        public void SpriteComponent_OnAttach_WithSpriteAsset_LoadsAndAssigns()
        {
            StageSprites("Sprites/hero.xml", "Sprites/spriteasset_tex.xml");
            var entity = new TestEntity();

            var comp = entity.AddComponent(new SpriteComponent { SpriteAsset = "Sprites/hero.xml" });

            Assert.NotNull(comp.Sprite);
            Assert.Equal("Sprites/hero.xml", comp.Sprite.Name);
        }

        [Fact]
        public void SpriteComponent_OnAttach_WithoutSpriteAsset_LeavesSpriteNull()
        {
            var entity = new TestEntity();
            var comp = entity.AddComponent(new SpriteComponent());

            Assert.Null(comp.Sprite);
        }

        [Fact]
        public void SpriteComponent_OnAttach_ExplicitSpriteWinsOverAsset()
        {
            StageSprites("Sprites/hero.xml", "Sprites/spriteasset_tex.xml");
            var explicitSprite = new Sprite("code_assigned.xml");
            var entity = new TestEntity();

            // A sprite assigned in code must not be replaced by the declarative asset.
            var comp = entity.AddComponent(new SpriteComponent { Sprite = explicitSprite, SpriteAsset = "Sprites/hero.xml" });

            Assert.Same(explicitSprite, comp.Sprite);
        }

        [Fact]
        public void SpriteComponent_OnAttach_MissingAsset_LogsAndLeavesNull()
        {
            // Nothing registered — LoadAsset throws; OnAttach must swallow it.
            AssetManager.Init(new MockContentManager());
            var entity = new TestEntity();

            var comp = entity.AddComponent(new SpriteComponent { SpriteAsset = "Sprites/does_not_exist.xml" });

            Assert.Null(comp.Sprite);
        }

        // ===== AnimationComponent.SpriteAsset =====

        [Fact]
        public void AnimationComponent_OnAttach_WithSpriteAsset_RegistersAndPlays()
        {
            StageSprites("Sprites/walk.xml", "Sprites/spriteasset_tex.xml");
            var entity = new TestEntity();

            var comp = entity.AddComponent(new AnimationComponent { SpriteAsset = "Sprites/walk.xml", AnimationName = "walk" });

            Assert.Contains("walk", comp.Animations);
            Assert.Equal("walk", comp.CurrentAnimation);
        }

        [Fact]
        public void AnimationComponent_OnAttach_WithoutSpriteAsset_HasNoAnimations()
        {
            var entity = new TestEntity();
            var comp = entity.AddComponent(new AnimationComponent());

            Assert.Empty(comp.Animations);
            Assert.Null(comp.CurrentAnimation);
        }

        [Fact]
        public void AnimationComponent_OnAttach_CodeRegisteredAnimationWinsOverAsset()
        {
            // In the prefab flow, an entity's own OnStart may register animations before XML
            // properties are applied and OnAttach fires — code-registered ones must win.
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", new Sprite("code_registered.xml"));
            comp.SpriteAsset = "Sprites/walk.xml";

            Assert.Null(Record.Exception(() => comp.OnAttach()));

            Assert.Single(comp.Animations);
            // The asset was never loaded nor played over the code-registered animation.
            Assert.Null(comp.CurrentAnimation);
        }

        [Fact]
        public void AnimationComponent_OnAttach_MissingAsset_LogsAndLeavesEmpty()
        {
            // Nothing registered — LoadAsset throws; OnAttach must swallow it.
            AssetManager.Init(new MockContentManager());
            var entity = new TestEntity();

            var comp = entity.AddComponent(new AnimationComponent { SpriteAsset = "Sprites/does_not_exist.xml" });

            Assert.Empty(comp.Animations);
            Assert.Null(comp.CurrentAnimation);
        }
    }
}
