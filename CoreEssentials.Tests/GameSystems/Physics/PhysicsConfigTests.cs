using System;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.GameSystems.Physics.Types;
using Microsoft.Xna.Framework;
#nullable enable
using Xunit;

namespace CoreEssentials.GameSystems.Physics.Tests;

/// <summary>
/// Tests for <see cref="PhysicsConfig"/> — the declarative physics configuration that
/// maps friendly category names to bits and holds engine settings.
/// </summary>
public class PhysicsConfigTests
{
    private const string ValidXml = @"
<PhysicsConfig>
    <Gravity X=""0"" Y=""1000"" />
    <Solver VelocityIterations=""10"" PositionIterations=""4"" />
    <Categories>
        <Category Name=""Player"" />
        <Category Name=""Vip"" />
        <Category Name=""Wall"" />
    </Categories>
</PhysicsConfig>";

    // ===== Parsing =====

    [Fact]
    public void LoadFromXml_ParsesGravity()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(new Vector2(0, 1000), config.Gravity);
    }

    [Fact]
    public void LoadFromXml_ParsesSolverIterations()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(10, config.VelocityIterations);
        Assert.Equal(4, config.PositionIterations);
    }

    [Fact]
    public void LoadFromXml_MissingSolver_UsesDefaults()
    {
        var xml = "<PhysicsConfig><Gravity X=\"1\" Y=\"2\" /></PhysicsConfig>";
        var config = PhysicsConfig.LoadFromXml(xml);

        Assert.Equal(8, config.VelocityIterations);
        Assert.Equal(3, config.PositionIterations);
    }

    [Fact]
    public void LoadFromXml_MissingGravity_UsesZero()
    {
        var xml = "<PhysicsConfig><Categories><Category Name=\"A\" /></Categories></PhysicsConfig>";
        var config = PhysicsConfig.LoadFromXml(xml);

        Assert.Equal(Vector2.Zero, config.Gravity);
    }

    [Fact]
    public void LoadFromXml_ParsesCategories()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(3, config.Categories.Count);
    }

    [Fact]
    public void LoadFromXml_ThrowsOnWrongRoot()
    {
        Assert.Throws<FormatException>(() => PhysicsConfig.LoadFromXml("<NotPhysicsConfig />"));
    }

    [Fact]
    public void LoadFromXml_ThrowsOnEmptyXml()
    {
        Assert.Throws<FormatException>(() => PhysicsConfig.LoadFromXml(""));
    }

    // ===== Resolve =====

    [Fact]
    public void Resolve_ReturnsCorrectBit()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(CollisionCategory.Cat1, config.Resolve("Player"));
        Assert.Equal(CollisionCategory.Cat2, config.Resolve("Vip"));
        Assert.Equal(CollisionCategory.Cat3, config.Resolve("Wall"));
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(CollisionCategory.Cat1, config.Resolve("player"));
        Assert.Equal(CollisionCategory.Cat1, config.Resolve("PLAYER"));
    }

    [Fact]
    public void Resolve_UnknownName_Throws()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        var ex = Assert.Throws<KeyNotFoundException>(() => config.Resolve("Ghost"));
        Assert.Contains("Ghost", ex.Message);
    }

    [Fact]
    public void Resolve_NullName_Throws()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Throws<ArgumentNullException>(() => config.Resolve(null!));
    }

    [Fact]
    public void TryResolve_KnownName_ReturnsTrue()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.True(config.TryResolve("Vip", out var category));
        Assert.Equal(CollisionCategory.Cat2, category);
    }

    [Fact]
    public void TryResolve_UnknownName_ReturnsFalseAndNone()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.False(config.TryResolve("Ghost", out var category));
        Assert.Equal(CollisionCategory.None, category);
    }

    // ===== ResolveMask =====

    [Fact]
    public void ResolveMask_SingleName()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(CollisionCategory.Cat1, config.ResolveMask("Player"));
    }

    [Fact]
    public void ResolveMask_MultipleNames_CombinesBits()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(CollisionCategory.Cat1 | CollisionCategory.Cat2, config.ResolveMask("Player|Vip"));
    }

    [Fact]
    public void ResolveMask_Empty_ReturnsNone()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(CollisionCategory.None, config.ResolveMask(""));
        Assert.Equal(CollisionCategory.None, config.ResolveMask("   "));
    }

    [Fact]
    public void ResolveMask_TrimWhitespaceAroundNames()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal(CollisionCategory.Cat1 | CollisionCategory.Cat3, config.ResolveMask("Player | Wall"));
    }

    [Fact]
    public void ResolveMask_UnknownName_Throws()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Throws<KeyNotFoundException>(() => config.ResolveMask("Player|Ghost"));
    }

    // ===== GetCategoryName =====

    [Fact]
    public void GetCategoryName_ReturnsDefinedName()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Equal("Player", config.GetCategoryName(CollisionCategory.Cat1));
        Assert.Equal("Vip", config.GetCategoryName(CollisionCategory.Cat2));
    }

    [Fact]
    public void GetCategoryName_UndefinedCategory_ReturnsNull()
    {
        var config = PhysicsConfig.LoadFromXml(ValidXml);

        Assert.Null(config.GetCategoryName(CollisionCategory.Cat5));
    }

    // ===== Validation =====

    [Fact]
    public void LoadFromXml_DuplicateName_Throws()
    {
        var xml = "<PhysicsConfig><Categories>" +
                  "<Category Name=\"A\" /><Category Name=\"A\" />" +
                  "</Categories></PhysicsConfig>";

        var ex = Assert.Throws<FormatException>(() => PhysicsConfig.LoadFromXml(xml));
        Assert.Contains("Duplicate category name", ex.Message);
    }

    [Fact]
    public void LoadFromXml_TooManyCategories_Throws()
    {
        // Order-based bits: the 32nd category would need bit 32, which doesn't exist.
        var categories = string.Concat(
            Enumerable.Range(1, 32).Select(i => $"<Category Name=\"C{i}\" />"));
        var xml = $"<PhysicsConfig><Categories>{categories}</Categories></PhysicsConfig>";

        var ex = Assert.Throws<FormatException>(() => PhysicsConfig.LoadFromXml(xml));
        Assert.Contains("Too many categories", ex.Message);
    }

    [Fact]
    public void LoadFromXml_MissingName_Throws()
    {
        var xml = "<PhysicsConfig><Categories><Category /></Categories></PhysicsConfig>";

        Assert.Throws<FormatException>(() => PhysicsConfig.LoadFromXml(xml));
    }

    // ===== Defaults =====

    [Fact]
    public void CreateDefault_HasNoCategoriesAndDefaults()
    {
        var config = PhysicsConfig.CreateDefault();

        Assert.Empty(config.Categories);
        Assert.Equal(Vector2.Zero, config.Gravity);
        Assert.Equal(8, config.VelocityIterations);
        Assert.Equal(3, config.PositionIterations);
    }

    [Fact]
    public void LoadFromXml_Max31Categories_Works()
    {
        // Order-based bits: the 31st category is bit 31 (the maximum).
        var categories = string.Concat(
            Enumerable.Range(1, 31).Select(i => $"<Category Name=\"C{i}\" />"));
        var xml = $"<PhysicsConfig><Categories>{categories}</Categories></PhysicsConfig>";
        var config = PhysicsConfig.LoadFromXml(xml);

        Assert.Equal(31, config.Categories.Count);
        Assert.Equal(CollisionCategory.Cat31, config.Resolve("C31"));
    }
}
