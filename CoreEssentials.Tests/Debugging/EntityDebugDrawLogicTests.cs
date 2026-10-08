using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.Tests.Debugging;

/// <summary>
/// Drives <see cref="EntityDebugDraw"/> overlay geometry through the internal
/// <see cref="IEntityDebugTarget"/> seam with a recording fake, so the real logic
/// (bounds math, crosshair endpoints, hierarchy grouping, active filter and per-flag
/// gating) is exercised without a graphics device.
/// </summary>
public class EntityDebugDrawLogicTests
{
    private readonly DebugConfig _config;

    public EntityDebugDrawLogicTests()
    {
        _config = new EntitySystem().DebugConfig;
    }

    private sealed class RecordingTarget : IEntityDebugTarget
    {
        public List<(Vector2 Start, Vector2 End, Color Color, float Thickness)> Lines { get; } = new();
        public List<(Rectangle Bounds, Color Color, float Thickness)> Rectangles { get; } = new();
        public List<(SpriteFont? Font, string Text, Vector2 Position, Color Color)> Texts { get; } = new();

        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
            => Lines.Add((start, end, color, thickness));

        public void DrawRectangle(Rectangle bounds, Color color, float thickness)
            => Rectangles.Add((bounds, color, thickness));

        public void DrawText(SpriteFont font, string text, Vector2 position, Color color)
            => Texts.Add((font, text, position, color));
    }

    private Entity MakeEntity(Vector2 position, Vector2 size, Vector2 origin = default, bool active = true)
    {
        var e = new FixedSizeOriginEntity(size, origin);
        e.Position = position;
        if (!active) e.SetActive(false);
        return e;
    }

    private sealed class FixedSizeOriginEntity : Entity
    {
        private readonly Vector2 _size;
        private readonly Vector2 _origin;

        public FixedSizeOriginEntity(Vector2 size, Vector2 origin)
        {
            _size = size;
            _origin = origin;
        }

        public override Vector2 GetSize() => _size;
        public override Vector2 GetOrigin() => _origin;
    }

    [Fact]
    public void Bounds_UsesPositionMinusOriginForTopLeftAndSizeForDimensions()
    {
        _config.ShowEntityBounds = true;
        var target = new RecordingTarget();
        // Origin at center of a 40x20 sprite, placed at (100,50): top-left should be (80,40).
        var e = MakeEntity(new Vector2(100f, 50f), new Vector2(40f, 20f), origin: new Vector2(20f, 10f));

        new EntityDebugDraw(_config).DrawOverlays(new[] { e }, target);

        var rect = Assert.Single(target.Rectangles);
        Assert.Equal(new Rectangle(80, 40, 40, 20), rect.Bounds);
        Assert.Equal(_config.BoundsColor, rect.Color);
    }

    [Fact]
    public void Bounds_ZeroSizeEntity_IsSkipped()
    {
        _config.ShowEntityBounds = true;
        var target = new RecordingTarget();
        var e = MakeEntity(Vector2.Zero, Vector2.Zero);

        new EntityDebugDraw(_config).DrawOverlays(new[] { e }, target);

        Assert.Empty(target.Rectangles);
    }

    [Fact]
    public void PositionMarker_DrawsHorizontalAndVerticalCrosshairAroundPosition()
    {
        _config.ShowEntityPosition = true;
        var target = new RecordingTarget();
        var e = MakeEntity(new Vector2(10f, 20f), Vector2.Zero);

        new EntityDebugDraw(_config).DrawOverlays(new[] { e }, target);

        Assert.Collection(target.Lines,
            first =>
            {
                Assert.Equal(new Vector2(6f, 20f), first.Start);   // pos.X - 4
                Assert.Equal(new Vector2(14f, 20f), first.End);    // pos.X + 4
            },
            second =>
            {
                Assert.Equal(new Vector2(10f, 16f), second.Start); // pos.Y - 4
                Assert.Equal(new Vector2(10f, 24f), second.End);   // pos.Y + 4
            });
    }

    [Fact]
    public void Hierarchy_DrawsLineFromParentPositionToChildPosition()
    {
        _config.ShowEntityHierarchy = true;
        var target = new RecordingTarget();
        var parent = MakeEntity(new Vector2(0f, 0f), Vector2.Zero);
        var child = MakeEntity(Vector2.Zero, Vector2.Zero);
        parent.AddChild(child);
        // Position the child relative to its parent so world position is (30,40).
        child.LocalPosition = new Vector2(30f, 40f);

        new EntityDebugDraw(_config).DrawOverlays(new[] { parent, child }, target);

        var line = Assert.Single(target.Lines);
        Assert.Equal(new Vector2(0f, 0f), line.Start);
        Assert.Equal(new Vector2(30f, 40f), line.End);
    }

    [Fact]
    public void InactiveEntities_AreSkipped()
    {
        _config.ShowEntityPosition = true;
        var target = new RecordingTarget();
        var inactive = MakeEntity(Vector2.One, Vector2.Zero, active: false);

        new EntityDebugDraw(_config).DrawOverlays(new[] { inactive }, target);

        Assert.Empty(target.Lines);
    }

    [Fact]
    public void AllFlagsOff_DrawsNothing()
    {
        _config.ShowEntityBounds = false;
        _config.ShowEntityPosition = false;
        _config.ShowEntityHierarchy = false;
        var target = new RecordingTarget();
        var e = MakeEntity(new Vector2(5f, 5f), new Vector2(10f, 10f));

        new EntityDebugDraw(_config).DrawOverlays(new[] { e }, target);

        Assert.Empty(target.Lines);
        Assert.Empty(target.Rectangles);
    }

    [Fact]
    public void NullFont_SkipsTextOverlays()
    {
        _config.ShowEntityIds = true;
        _config.ShowEntityTags = true;
        var target = new RecordingTarget();
        var e = MakeEntity(Vector2.Zero, Vector2.Zero);
        e.SetId("e1");
        e.SetTag("hero");

        // fontAsset null => no text, even though id/tag flags are on.
        new EntityDebugDraw(_config).DrawOverlays(new[] { e }, target, fontAsset: null);

        Assert.Empty(target.Texts);
    }
}
