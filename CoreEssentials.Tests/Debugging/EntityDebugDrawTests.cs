using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Tests.Debugging;

public class EntityDebugDrawTests
{
    private readonly EntitySystem _system;
    private readonly DebugConfig _config;

    public EntityDebugDrawTests()
    {
        _system = new EntitySystem();
        _config = _system.DebugConfig;
    }

    // T4.1 - Test debug mode enable/disable
    [Fact]
    public void DebugMode_DefaultIsFalse()
    {
        Assert.False(_system.DebugMode);
    }

    [Fact]
    public void DebugMode_CanBeEnabled()
    {
        _system.DebugMode = true;
        Assert.True(_system.DebugMode);
    }

    [Fact]
    public void DebugMode_CanBeDisabled()
    {
        _system.DebugMode = true;
        _system.DebugMode = false;
        Assert.False(_system.DebugMode);
    }

    // T4.2 - Test individual debug overlays
    [Fact]
    public void DebugConfig_BoundsDefaultIsFalse()
    {
        Assert.False(_config.ShowEntityBounds);
    }

    [Fact]
    public void DebugConfig_BoundsCanBeEnabled()
    {
        _config.ShowEntityBounds = true;
        Assert.True(_config.ShowEntityBounds);
    }

    [Fact]
    public void DebugConfig_IdsDefaultIsFalse()
    {
        Assert.False(_config.ShowEntityIds);
    }

    [Fact]
    public void DebugConfig_IdsCanBeEnabled()
    {
        _config.ShowEntityIds = true;
        Assert.True(_config.ShowEntityIds);
    }

    [Fact]
    public void DebugConfig_TagsDefaultIsFalse()
    {
        Assert.False(_config.ShowEntityTags);
    }

    [Fact]
    public void DebugConfig_TagsCanBeEnabled()
    {
        _config.ShowEntityTags = true;
        Assert.True(_config.ShowEntityTags);
    }

    // T4.3 - Test Entity.GetSize() default behavior and overrides
    [Fact]
    public void GetSize_NoSpriteComponent_ReturnsZero()
    {
        var entity = new PlainEntity();
        Assert.Equal(Vector2.Zero, entity.GetSize());
    }

    [Fact]
    public void GetSize_SpriteComponentWithoutSprite_ReturnsZero()
    {
        var entity = new PlainEntity();
        entity.AddComponent(new SpriteComponent());
        Assert.Equal(Vector2.Zero, entity.GetSize());
    }

    [Fact]
    public void GetSize_Override_ReturnsCustomSize()
    {
        var entity = new FixedSizeEntity();
        Assert.Equal(new Vector2(100f, 50f), entity.GetSize());
    }

    private class PlainEntity : Entity
    {
    }

    private class FixedSizeEntity : Entity
    {
        public override Vector2 GetSize() => new Vector2(100f, 50f);
    }

    [Fact]
    public void DebugConfig_HierarchyDefaultIsFalse()
    {
        Assert.False(_config.ShowEntityHierarchy);
    }

    [Fact]
    public void DebugConfig_HierarchyCanBeEnabled()
    {
        _config.ShowEntityHierarchy = true;
        Assert.True(_config.ShowEntityHierarchy);
    }

    [Fact]
    public void DebugConfig_PositionDefaultIsFalse()
    {
        Assert.False(_config.ShowEntityPosition);
    }

    [Fact]
    public void DebugConfig_PositionCanBeEnabled()
    {
        _config.ShowEntityPosition = true;
        Assert.True(_config.ShowEntityPosition);
    }

    // T4.3 - Test configurable colors
    [Fact]
    public void DebugConfig_BoundsColor_DefaultIsLime()
    {
        Assert.Equal(Microsoft.Xna.Framework.Color.Lime, _config.BoundsColor);
    }

    [Fact]
    public void DebugConfig_BoundsColor_CanBeChanged()
    {
        _config.BoundsColor = Microsoft.Xna.Framework.Color.Red;
        Assert.Equal(Microsoft.Xna.Framework.Color.Red, _config.BoundsColor);
    }

    [Fact]
    public void DebugConfig_IdColor_DefaultIsYellow()
    {
        Assert.Equal(Microsoft.Xna.Framework.Color.Yellow, _config.IdColor);
    }

    [Fact]
    public void DebugConfig_TagColor_DefaultIsCyan()
    {
        Assert.Equal(Microsoft.Xna.Framework.Color.Cyan, _config.TagColor);
    }

    [Fact]
    public void DebugConfig_HierarchyColor_DefaultIsMagenta()
    {
        Assert.Equal(Microsoft.Xna.Framework.Color.Magenta, _config.HierarchyColor);
    }

    [Fact]
    public void DebugConfig_PositionColor_DefaultIsRed()
    {
        Assert.Equal(Microsoft.Xna.Framework.Color.Red, _config.PositionColor);
    }

    // T4.4 - Test line thickness
    [Fact]
    public void DebugConfig_LineThickness_DefaultIs1()
    {
        Assert.Equal(1f, _config.LineThickness);
    }

    [Fact]
    public void DebugConfig_LineThickness_CanBeChanged()
    {
        _config.LineThickness = 2.5f;
        Assert.Equal(2.5f, _config.LineThickness);
    }

    // T4.5 - Test debug draw initialization
    [Fact]
    public void EntityDebugDraw_InitializesWithConfig()
    {
        var debugDraw = new EntityDebugDraw(_config);
        Assert.NotNull(debugDraw);
    }

    private class TestEntity : Entity
    {
        public override void OnStart()
        {
            // No-op for testing
        }
    }
}
