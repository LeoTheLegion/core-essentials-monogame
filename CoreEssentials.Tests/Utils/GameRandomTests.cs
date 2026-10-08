using System;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Utils;

namespace CoreEssentials.Tests.Utils;

public class GameRandomTests
{
    [Fact]
    public void Next_ReturnsNonNegative()
    {
        for (var i = 0; i < 100; i++)
            Assert.True(GameRandom.Next() >= 0);
    }

    [Fact]
    public void Next_MaxValue_ReturnsWithinRange()
    {
        for (var i = 0; i < 500; i++)
        {
            var v = GameRandom.Next(10);
            Assert.True(v >= 0 && v < 10);
        }
    }

    [Fact]
    public void Next_MinMax_ReturnsWithinRange()
    {
        for (var i = 0; i < 500; i++)
        {
            var v = GameRandom.Next(3, 7);
            Assert.True(v >= 3 && v < 7);
        }
    }

    [Fact]
    public void NextFloat_ReturnsInUnitRange()
    {
        for (var i = 0; i < 500; i++)
        {
            var v = GameRandom.NextFloat();
            Assert.True(v >= 0f && v < 1f);
        }
    }

    [Fact]
    public void NextFloat_MinMax_ReturnsWithinRange()
    {
        for (var i = 0; i < 500; i++)
        {
            var v = GameRandom.NextFloat(2f, 5f);
            Assert.True(v >= 2f && v < 5f);
        }
    }

    [Fact]
    public void NextBool_ProducesBothOutcomesOverManySamples()
    {
        var sawTrue = false;
        var sawFalse = false;
        for (var i = 0; i < 200; i++)
        {
            if (GameRandom.NextBool()) sawTrue = true;
            else sawFalse = true;
        }
        Assert.True(sawTrue);
        Assert.True(sawFalse);
    }

    [Fact]
    public void NextBool_ProbabilityZero_IsAlwaysFalse()
    {
        for (var i = 0; i < 100; i++)
            Assert.False(GameRandom.NextBool(0f));
    }

    [Fact]
    public void NextBool_ProbabilityOne_IsAlwaysTrue()
    {
        for (var i = 0; i < 100; i++)
            Assert.True(GameRandom.NextBool(1f));
    }

    [Fact]
    public void Pick_ReturnsElementFromArray()
    {
        var items = new[] { "a", "b", "c" };
        for (var i = 0; i < 200; i++)
            Assert.Contains(GameRandom.Pick(items), items);
    }

    [Fact]
    public void Pick_EmptyArray_ReturnsDefault()
    {
        Assert.Null(GameRandom.Pick(new string[0]));
    }

    [Fact]
    public void Pick_NullArray_ReturnsDefault()
    {
        Assert.Null(GameRandom.Pick((string[]?)null));
    }

    [Fact]
    public void RandomDirection_HasRequestedMagnitude()
    {
        var dir = GameRandom.RandomDirection(3f);
        Assert.InRange(dir.Length(), 2.999f, 3.001f);
    }

    [Fact]
    public void NextSignedFloat_ReturnsWithinUnitRange()
    {
        for (var i = 0; i < 500; i++)
        {
            var v = GameRandom.NextSignedFloat();
            Assert.True(v >= -1f && v <= 1f);
        }
    }

    [Fact]
    public void RandomVector2_ComponentsWithinUnitRange()
    {
        for (var i = 0; i < 500; i++)
        {
            var v = GameRandom.RandomVector2();
            Assert.True(v.X >= -1f && v.X <= 1f);
            Assert.True(v.Y >= -1f && v.Y <= 1f);
        }
    }
}
