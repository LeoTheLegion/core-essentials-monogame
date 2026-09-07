using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Playground.Components;

namespace CoreEssentials.Tests.Playground;

/// <summary>
/// Unit tests for <see cref="TextComponent"/> — the world-space, canvas-free text renderer that
/// replaced the deleted <c>TextEntity</c>. The alignment math (ported from the old
/// <c>TextEntity.Render</c>) is exercised directly via the public <c>ComputeDrawPosition</c> seam so
/// no live SpriteBatch is required; font loading on attach is proven headlessly with a mock font.
/// </summary>
public class TextComponentTests
{
    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch _spriteBatch) { }
    }

    /// <summary>Records that OnAttach ran (and therefore the font load path executed without throwing).</summary>
    private class TextProbe : TextComponent
    {
        public bool Attached;
        public override void OnAttach()
        {
            base.OnAttach();
            Attached = true;
        }
    }

    /// <summary>Registers a mock "Fonts/base" font so OnAttach's LoadAsset works headlessly.</summary>
    private static void InitFont()
    {
        var content = new MockContentManager();
        content.AddAsset<SpriteFont>("Fonts/base", CoreEssentials.Tests.MockSpriteFont.Instance);
        AssetManager.Init(content);
    }

    [Fact]
    public void OnAttach_LoadsFont_WithoutThrowing()
    {
        InitFont();
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        var comp = entity.AddComponent(new TextProbe());
        try
        {
            Assert.True(comp.Attached);
        }
        finally { entity.RemoveComponent<TextProbe>(); }
    }

    [Fact]
    public void Defaults_AreEmptyWhiteLeftZeroOffset()
    {
        var comp = new TextComponent();
        Assert.Equal("", comp.Text);
        Assert.Equal(Color.White, comp.Color);
        Assert.Equal(TextComponent.TextAlignment.Left, comp.Alignment);
        Assert.Equal(Vector2.Zero, comp.Offset);
    }

    [Fact]
    public void ComputeDrawPosition_Left_IsPositionPlusOffset()
    {
        InitFont(); // AddComponent attaches → OnAttach loads the font.
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(100f, 40f);
        var comp = entity.AddComponent(new TextProbe());
        try
        {
            comp.Alignment = TextComponent.TextAlignment.Left;
            comp.Offset = Vector2.Zero;

            // Left: no horizontal shift regardless of measured size.
            Assert.Equal(new Vector2(100f, 40f), comp.ComputeDrawPosition(new Vector2(80f, 20f)));

            // Offset is added to the position.
            comp.Offset = new Vector2(5f, -10f);
            Assert.Equal(new Vector2(105f, 30f), comp.ComputeDrawPosition(new Vector2(80f, 20f)));
        }
        finally { entity.RemoveComponent<TextProbe>(); }
    }

    [Fact]
    public void ComputeDrawPosition_Center_ShiftsLeftByHalfWidth()
    {
        InitFont(); // AddComponent attaches → OnAttach loads the font.
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(100f, 40f);
        var comp = entity.AddComponent(new TextProbe());
        try
        {
            comp.Alignment = TextComponent.TextAlignment.Center;

            // Center: shift left by half the measured width.
            Assert.Equal(new Vector2(60f, 40f), comp.ComputeDrawPosition(new Vector2(80f, 20f)));
        }
        finally { entity.RemoveComponent<TextProbe>(); }
    }

    [Fact]
    public void ComputeDrawPosition_Right_ShiftsLeftByFullWidth()
    {
        InitFont(); // AddComponent attaches → OnAttach loads the font.
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(100f, 40f);
        var comp = entity.AddComponent(new TextProbe());
        try
        {
            comp.Alignment = TextComponent.TextAlignment.Right;

            // Right: shift left by the full measured width.
            Assert.Equal(new Vector2(20f, 40f), comp.ComputeDrawPosition(new Vector2(80f, 20f)));
        }
        finally { entity.RemoveComponent<TextProbe>(); }
    }

    [Fact]
    public void Draw_WithNoOwnerOrFont_DoesNotThrow()
    {
        // A detached component (no owner) must not throw when drawn.
        var comp = new TextComponent();
        Assert.Null(Record.Exception(() => comp.Draw(null)));
    }
}
