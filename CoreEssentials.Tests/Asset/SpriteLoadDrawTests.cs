using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.Asset
{
    /// <summary>
    /// Covers the device-free guard and delegation paths of <see cref="Sprite"/> that other tests
    /// skip: every Draw/DrawFrame overload short-circuits on unloaded metadata before it ever
    /// touches the (null) SpriteBatch, and Load's entry guards reject null manager / non-XML names.
    /// Unload's state-clearing is driven through the internal test seams.
    /// </summary>
    public class SpriteLoadDrawTests
    {
        [Fact]
        public void Draw_Overloads_WithoutMetadata_ThrowBeforeUsingSpriteBatch()
        {
            var sprite = new Sprite("sprite.xml"); // metadata never loaded

            // All three entry overloads delegate toward the single metadata guard; pass a null
            // batch to prove none of them reach a draw call.
            Assert.Throws<InvalidOperationException>(
                () => sprite.Draw(null!, Vector2.Zero, Color.White, 0f, SpriteEffects.None, 0f));
            Assert.Throws<InvalidOperationException>(
                () => sprite.Draw(null!, Vector2.Zero, Color.White, 0f, Vector2.One, SpriteEffects.None, 0f));
            Assert.Throws<InvalidOperationException>(
                () => sprite.Draw(null!, Vector2.Zero, Color.White, 0f, 2f, SpriteEffects.None, 0f));
        }

        [Fact]
        public void DrawFrame_Overloads_WithoutMetadata_ThrowBeforeUsingSpriteBatch()
        {
            var sprite = new Sprite("sprite.xml"); // metadata never loaded

            Assert.Throws<InvalidOperationException>(
                () => sprite.DrawFrame(null!, Vector2.Zero, 0, Color.White));
            Assert.Throws<InvalidOperationException>(
                () => sprite.DrawFrame(null!, Vector2.Zero, 0, Color.White, 0f, Vector2.One, SpriteEffects.None, 0f));
        }

        [Fact]
        public void Load_NullContentManager_Throws()
        {
            var sprite = new Sprite("sprite.xml");

            Assert.Throws<ArgumentNullException>(() => sprite.Load(null!));
        }

        [Fact]
        public void Load_NonXmlExtension_ThrowsUnsupportedFormat()
        {
            var sprite = new Sprite("sprite.png");
            var manager = new NoopContentManager();

            Assert.Throws<InvalidOperationException>(() => sprite.Load(manager));
        }

        [Fact]
        public void Unload_ClearsInjectedState()
        {
            var texture = new StubTexture2DAsset("t");
            var sheet = new SpriteSheet("s.xml");
            var sprite = new Sprite("sprite.xml")
            {
                TestTexture = texture,
                TestSpriteSheet = sheet,
                TestMetaData = new Sprite.SpriteMeta { SourceType = "texture2d" }
            };

            sprite.Unload(new NoopContentManager());

            Assert.Null(sprite.Texture);
            Assert.Null(sprite.SpriteSheet);
            Assert.Null(sprite.TestMetaData);
        }

        // Content manager whose Load always yields null: fine here because the tested paths throw
        // before any asset is actually requested.
        private sealed class NoopContentManager : IContentManager
        {
            public T Load<T>(string assetName) => default!;
            public void Unload(string assetName) { }
        }

        // Minimal Texture2DAsset stand-in so Unload can release a non-null texture without a device.
        private sealed class StubTexture2DAsset : Texture2DAsset
        {
            public StubTexture2DAsset(string name) : base(name) { }
            public override void Load(IContentManager contentManager) { }
            public override void Unload(IContentManager contentManager) { }
        }
    }
}
