using System;
using System.Collections.Generic;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Tests for the <see cref="AnimationComponent"/> (Sprint 15.5).
    /// Covers multi-animation add/play/stop, frame advance, GetSize × scale, the base
    /// Entity.GetSize() fallback chain, and serialization round-trip.
    /// </summary>
    public class AnimationComponentTests
    {
        private static Sprite CreateWalkSprite()
        {
            var sprite = new Sprite("walk.xml");
            sprite.TestMetaData = new Sprite.SpriteMeta { SourceType = "spritesheet" };
            sprite.TestSpriteSheet = new FakeSpriteSheet();
            sprite.TestFrames = new[] { 0, 1, 2, 3 };
            sprite.TestFrameRate = 1f / 10f; // 10 fps
            return sprite;
        }

        // ===== Add / Play / Stop =====

        [Fact]
        public void AddAnimation_RegistersName()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", CreateWalkSprite());

            Assert.Contains("walk", comp.Animations);
        }

        [Fact]
        public void AddAnimation_DuplicateName_Throws()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", CreateWalkSprite());

            Assert.Throws<InvalidOperationException>(() => comp.AddAnimation("walk", CreateWalkSprite()));
        }

        [Fact]
        public void AddAnimation_NullSprite_Throws()
        {
            var comp = new AnimationComponent();

            Assert.Throws<ArgumentNullException>(() => comp.AddAnimation("walk", null!));
        }

        [Fact]
        public void Play_SetsCurrentAnimation()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("idle", CreateWalkSprite());
            comp.AddAnimation("walk", CreateWalkSprite());

            comp.Play("walk");

            Assert.Equal("walk", comp.CurrentAnimation);
        }

        [Fact]
        public void Play_MissingName_Throws()
        {
            var comp = new AnimationComponent();

            Assert.Throws<KeyNotFoundException>(() => comp.Play("nope"));
        }

        [Fact]
        public void Stop_AllStopsEveryAnimation()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", CreateWalkSprite());
            comp.Play("walk");

            comp.Stop();

            Assert.False(comp.GetAnimation("walk")!.IsPlaying);
        }

        [Fact]
        public void SetSpeed_UpdatesAnimationState()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", CreateWalkSprite());

            comp.SetSpeed("walk", 2f);

            Assert.Equal(2f, comp.GetAnimation("walk")!.Speed);
        }

        // ===== Frame advance =====

        [Fact]
        public void Update_AdvancesPlayingFrame()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", CreateWalkSprite());
            comp.Play("walk");

            // 0.2s at 10fps advances one frame. (GameTime arg order: total, elapsed.)
            comp.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(200)));

            Assert.Equal(1, comp.GetAnimation("walk")!.CurrentFrame);
        }

        [Fact]
        public void Update_PushesFrameIntoSpriteComponent()
        {
            var entity = new TestEntity();
            entity.AddComponent(new SpriteComponent(CreateWalkSprite()));
            var comp = new AnimationComponent();
            entity.AddComponent(comp);
            comp.AddAnimation("walk", CreateWalkSprite());
            comp.Play("walk");

            entity.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(200)));

            Assert.Equal(1, entity.GetComponent<SpriteComponent>()!.AnimationFrame);
        }

        // ===== GetSize × scale =====

        [Fact]
        public void GetSize_MultipliesByOwnerScale()
        {
            var entity = new TestEntity();
            entity.Scale = new Vector2(2, 2);
            var comp = new AnimationComponent();
            entity.AddComponent(comp);
            comp.AddAnimation("walk", CreateWalkSprite()); // 32x32 frame
            comp.Play("walk");

            Assert.Equal(new Vector2(64, 64), comp.GetSize());
        }

        [Fact]
        public void GetSize_NoCurrentAnimation_ReturnsZero()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", CreateWalkSprite());

            Assert.Equal(Vector2.Zero, comp.GetSize());
        }

        // ===== Base Entity.GetSize() fallback chain =====

        [Fact]
        public void EntityGetSize_FallsBackToAnimationComponent()
        {
            var entity = new TestEntity();
            entity.Scale = new Vector2(2, 2);
            var comp = new AnimationComponent();
            entity.AddComponent(comp);
            comp.AddAnimation("walk", CreateWalkSprite()); // 32x32
            comp.Play("walk");

            Assert.Equal(new Vector2(64, 64), entity.GetSize());
        }

        [Fact]
        public void EntityGetSize_SpriteComponentTakesPrecedence()
        {
            var entity = new TestEntity();
            entity.Scale = Vector2.One;

            // SpriteComponent sprite: 64x64 (metadata size).
            var staticSprite = new Sprite("ball.xml");
            staticSprite.TestMetaData = new Sprite.SpriteMeta
            {
                SourceType = "texture2d",
                Size = new Sprite.Size { Width = 64, Height = 64 }
            };
            entity.AddComponent(new SpriteComponent(staticSprite));

            // AnimationComponent sprite: 32x32.
            var comp = new AnimationComponent();
            entity.AddComponent(comp);
            comp.AddAnimation("walk", CreateWalkSprite());
            comp.Play("walk");

            Assert.Equal(new Vector2(64, 64), entity.GetSize());
        }

        // ===== Serialization round-trip =====

        [Fact]
        public void Serialize_Deserialize_RoundTrips()
        {
            var comp = new AnimationComponent();
            comp.AddAnimation("walk", CreateWalkSprite());
            comp.Play("walk");
            comp.SetSpeed("walk", 2f);

            var xml = comp.SerializeToXml();
            var restored = new AnimationComponent();
            restored.DeserializeFromXml(xml);

            Assert.Equal("walk", restored.CurrentAnimation);
            Assert.Contains("walk", restored.Animations);
            Assert.Equal(2f, restored.GetAnimation("walk")!.Speed);
        }

        // ===== Fakes =====

        private class TestEntity : Entity
        {
        }

        private class FakeSpriteSheet : SpriteSheet
        {
            public FakeSpriteSheet() : base("fake_sheet.xml") { }

            public override Vector2 GetFrameSize() => new Vector2(32, 32);

            public override Rectangle GetFrame(int index) => new Rectangle(index * 32, 0, 32, 32);

            public override Vector2 FrameOrigin => new Vector2(16, 16);
        }
    }
}
