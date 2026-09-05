using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Playground.Components;

namespace CoreEssentials.Tests.Playground;

/// <summary>
/// Unit tests for the character behavior components created in Sprint 2, which replaced the deleted
/// <c>CharacterEntity</c>, <c>AnimatedCharacterEntity</c>, and <c>PlayerEntity</c> classes:
///   • <see cref="BounceTweenComponent"/> — looping eased Y-bounce (baseline captured on first update).
///   • <see cref="MoveByKeysComponent"/> — arrow-key movement.
///   • <see cref="PauseScaleComponent"/> — scale-while-paused.
///   • <see cref="CharacterSpriteLoader"/> / <see cref="CharacterWalkAnimation"/> — sprite mounting.
/// Movement and bounce math are exercised via the components' virtual seams so no live keyboard or
/// frame clock is required; sprite loading is proven headlessly with a mock content manager.
/// </summary>
public class CharacterComponentTests
{
    private class TestEntity : Entity
    {
        // Forward to base so attached components receive their per-frame Update.
        public override void Update(GameTime gameTime) => base.Update(gameTime);
        public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
    }

    /// <summary>Pins the frame delta and simulates held keys so movement is deterministic.</summary>
    private class MoveProbe : MoveByKeysComponent
    {
        public float PinnedDelta = 16f;
        public Keys HeldKey;
        public bool KeyHeld;

        protected override float GetDeltaTime() => PinnedDelta;
        protected override bool IsKeyHeld(Keys key) => KeyHeld && key == HeldKey;
    }

    /// <summary>Registers a mock sprite asset so OnAttach's LoadAsset works headlessly.</summary>
    private static void InitSprite(string assetName)
    {
        var content = new MockContentManager();
        content.AddAsset<Sprite>(assetName, new Sprite(assetName));
        AssetManager.Init(content);
    }

    // ─────────────── BounceTweenComponent ───────────────

    [Fact]
    public void Bounce_Defaults_MatchOriginalCharacter()
    {
        var comp = new BounceTweenComponent();
        Assert.Equal(50f, comp.Amplitude);
        Assert.Equal(1.5f, comp.Duration);
        Assert.True(comp.Loop);
        Assert.True(comp.Reverse);
    }

    [Fact]
    public void Bounce_FirstUpdate_CapturesBaselineAndLeavesYUnchanged()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(320f, 360f);
        entity.AddComponent(new BounceTweenComponent());

        // At t=0 the eased offset is 0, so Y stays at the captured baseline.
        entity.Update(new GameTime());

        Assert.Equal(new Vector2(320f, 360f), entity.Position);
    }

    [Fact]
    public void Bounce_OffsetIsAppliedRelativeToBaseline_XUntouched()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(320f, 360f);
        var comp = new BounceTweenComponent { Amplitude = 50f, Duration = 1.0f };
        entity.AddComponent(comp);

        // Advance one full leg: the offset should be at its -Amplitude extreme (eased to ~-50).
        entity.Update(new GameTime());                        // capture baseline
        for (int i = 0; i < 71; i++)
            entity.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16)));

        // X is never touched by the bounce.
        Assert.Equal(320f, entity.Position.X);
        // Y has moved away from the baseline (the bob took effect).
        Assert.NotEqual(360f, entity.Position.Y);
    }

    // ─────────────── MoveByKeysComponent ───────────────

    [Fact]
    public void Move_EachKeyMovesExpectedAxis()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = Vector2.Zero;
        var probe = (MoveProbe)entity.AddComponent(new MoveProbe());

        var dt = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16));

        probe.KeyHeld = true;
        probe.HeldKey = Keys.Left;
        entity.Update(dt);
        Assert.Equal(new Vector2(-1f * 16f, 0f), entity.Position);

        entity.Position = Vector2.Zero;
        probe.HeldKey = Keys.Right;
        entity.Update(dt);
        Assert.Equal(new Vector2(1f * 16f, 0f), entity.Position);

        entity.Position = Vector2.Zero;
        probe.HeldKey = Keys.Up;
        entity.Update(dt);
        Assert.Equal(new Vector2(0f, -1f * 16f), entity.Position);

        entity.Position = Vector2.Zero;
        probe.HeldKey = Keys.Down;
        entity.Update(dt);
        Assert.Equal(new Vector2(0f, 1f * 16f), entity.Position);
    }

    [Fact]
    public void Move_NoKeysHeld_DoesNotMove()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(10f, 20f);
        var probe = (MoveProbe)entity.AddComponent(new MoveProbe());
        probe.KeyHeld = false;

        entity.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16)));

        Assert.Equal(new Vector2(10f, 20f), entity.Position);
    }

    // ─────────────── PauseScaleComponent ───────────────

    [Fact]
    public void PauseScale_TogglesOnPauseState()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Scale = Vector2.One;
        var comp = (PauseScaleComponent)entity.AddComponent(new PauseScaleComponent());

        comp.OnApplicationPause(true);
        Assert.Equal(new Vector2(1.5f, 1.5f), entity.Scale);

        comp.OnApplicationPause(false);
        Assert.Equal(Vector2.One, entity.Scale);
    }

    [Fact]
    public void PauseScale_UsesConfiguredFactor()
    {
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = (PauseScaleComponent)entity.AddComponent(new PauseScaleComponent { PausedScale = 2f });

        comp.OnApplicationPause(true);
        Assert.Equal(new Vector2(2f, 2f), entity.Scale);
    }

    // ─────────────── CharacterSpriteLoader ───────────────

    [Fact]
    public void SpriteLoader_OnAttach_LoadsAsset_WithoutThrowing()
    {
        InitSprite("Sprites/character_sprite.xml");
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        Assert.Null(Record.Exception(() =>
            entity.AddComponent(new CharacterSpriteLoader { SpriteAsset = "Sprites/character_sprite.xml" })));
    }

    [Fact]
    public void SpriteLoader_FirstUpdate_AssignsSpriteToPreDeclaredComponent()
    {
        InitSprite("Sprites/character_sprite.xml");
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();

        // The SpriteComponent is declared in XML alongside the loader; the loader only configures it.
        var spriteComp = (SpriteComponent)entity.AddComponent(new SpriteComponent());
        Assert.Null(spriteComp.Sprite);
        entity.AddComponent(new CharacterSpriteLoader { SpriteAsset = "Sprites/character_sprite.xml" });

        // The assignment is deferred to the first update (a component must not create a sibling).
        entity.Update(new GameTime());

        Assert.NotNull(spriteComp.Sprite);
    }

    [Fact]
    public void SpriteLoader_NoSpriteComponent_IsNoOp()
    {
        InitSprite("Sprites/character_sprite.xml");
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.AddComponent(new CharacterSpriteLoader { SpriteAsset = "Sprites/character_sprite.xml" });

        // With no pre-declared SpriteComponent the loader does nothing (and never creates one).
        Assert.Null(Record.Exception(() => entity.Update(new GameTime())));
        Assert.Null(entity.GetComponent<SpriteComponent>());
    }

    // ─────────────── CharacterWalkAnimation ───────────────

    [Fact]
    public void WalkAnimation_FirstUpdate_WiresPreDeclaredSiblingsAndPlaysAnimation()
    {
        InitSprite("Sprites/character_anim_walk.xml");
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();

        // Both siblings are declared in XML; the component only configures them.
        var spriteComp = (SpriteComponent)entity.AddComponent(new SpriteComponent());
        var anim = (AnimationComponent)entity.AddComponent(new AnimationComponent());
        entity.AddComponent(new CharacterWalkAnimation { SpriteAsset = "Sprites/character_anim_walk.xml", AnimationName = "walk" });

        entity.Update(new GameTime());

        Assert.NotNull(spriteComp.Sprite);
        Assert.Contains("walk", anim.Animations);
        Assert.Equal("walk", anim.CurrentAnimation);
    }
}
