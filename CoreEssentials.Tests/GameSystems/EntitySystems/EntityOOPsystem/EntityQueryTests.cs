using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class EntityQueryTests
{
    private EntitySystem CreateEntitySystem() => new EntitySystem();

    private TestEntity CreateEntity(EntitySystem system, Vector2? position = null)
    {
        var entity = system.CreateEntity<TestEntity>();
        if (position.HasValue)
            entity.Position = position.Value;
        return entity;
    }

    private SpecialEntity CreateSpecialEntity(EntitySystem system, Vector2? position = null)
    {
        var entity = system.CreateEntity<SpecialEntity>();
        if (position.HasValue)
            entity.Position = position.Value;
        return entity;
    }

    // ===== T1: FindByType<T>() Tests =====

    [Fact]
    public void FindByType_ReturnsAllEntitiesOfType()
    {
        var system = CreateEntitySystem();
        var test1 = CreateEntity(system);
        var test2 = CreateEntity(system);
        CreateSpecialEntity(system);

        var found = system.FindByType<TestEntity>();

        Assert.Contains(test1, found);
        Assert.Contains(test2, found);
        Assert.Equal(2, found.Count);
    }

    [Fact]
    public void FindByType_ExcludesInactiveEntities()
    {
        var system = CreateEntitySystem();
        var active = CreateEntity(system);
        var inactive = CreateEntity(system);
        inactive.SetActive(false);

        var found = system.FindByType<TestEntity>();

        Assert.Contains(active, found);
        Assert.DoesNotContain(inactive, found);
        Assert.Single(found);
    }

    [Fact]
    public void FindByType_ReturnsEmptyListWhenNoEntitiesOfType()
    {
        var system = CreateEntitySystem();
        CreateEntity(system);

        var found = system.FindByType<SpecialEntity>();

        Assert.Empty(found);
    }

    [Fact]
    public void FindByType_ReturnsEmptyListWhenNoEntities()
    {
        var system = CreateEntitySystem();

        var found = system.FindByType<TestEntity>();

        Assert.Empty(found);
    }

    // ===== T2: FindNearby() Tests =====

    [Fact]
    public void FindNearby_ReturnsEntitiesWithinRadius()
    {
        var system = CreateEntitySystem();
        var center = Vector2.Zero;
        CreateEntity(system, new Vector2(0, 0));
        CreateEntity(system, new Vector2(3, 4)); // distance = 5
        CreateEntity(system, new Vector2(10, 0)); // distance = 10

        var found = system.FindNearby(center, 5f);

        Assert.Equal(2, found.Count);
    }

    [Fact]
    public void FindNearby_ExcludesEntitiesOutsideRadius()
    {
        var system = CreateEntitySystem();
        CreateEntity(system, new Vector2(0, 0));
        CreateEntity(system, new Vector2(10, 0));

        var found = system.FindNearby(Vector2.Zero, 5f);

        Assert.Single(found);
    }

    [Fact]
    public void FindNearby_IncludesEntityAtExactBoundary()
    {
        var system = CreateEntitySystem();
        CreateEntity(system, new Vector2(5, 0));

        var found = system.FindNearby(Vector2.Zero, 5f);

        Assert.Single(found);
    }

    [Fact]
    public void FindNearby_ExcludesInactiveEntities()
    {
        var system = CreateEntitySystem();
        var active = CreateEntity(system, new Vector2(1, 0));
        var inactive = CreateEntity(system, new Vector2(2, 0));
        inactive.SetActive(false);

        var found = system.FindNearby(Vector2.Zero, 5f);

        Assert.Contains(active, found);
        Assert.DoesNotContain(inactive, found);
        Assert.Single(found);
    }

    [Fact]
    public void FindNearby_ReturnsEmptyListWhenNoEntities()
    {
        var system = CreateEntitySystem();

        var found = system.FindNearby(Vector2.Zero, 10f);

        Assert.Empty(found);
    }

    [Fact]
    public void FindNearby_ReturnsEmptyListWhenNoEntitiesInRange()
    {
        var system = CreateEntitySystem();
        CreateEntity(system, new Vector2(100, 0));

        var found = system.FindNearby(Vector2.Zero, 5f);

        Assert.Empty(found);
    }

    // ===== T2: FindNearby<T>() Tests =====

    [Fact]
    public void FindNearbyType_CombinesTypeAndSpatialFiltering()
    {
        var system = CreateEntitySystem();
        CreateEntity(system, new Vector2(1, 0));
        CreateEntity(system, new Vector2(3, 0));
        CreateSpecialEntity(system, new Vector2(2, 0));
        CreateSpecialEntity(system, new Vector2(10, 0));

        var found = system.FindNearby<SpecialEntity>(Vector2.Zero, 5f);

        Assert.Single(found);
        Assert.IsType<SpecialEntity>(found[0]);
    }

    [Fact]
    public void FindNearbyType_ExcludesInactiveEntities()
    {
        var system = CreateEntitySystem();
        var active = CreateSpecialEntity(system, new Vector2(1, 0));
        var inactive = CreateSpecialEntity(system, new Vector2(2, 0));
        inactive.SetActive(false);

        var found = system.FindNearby<SpecialEntity>(Vector2.Zero, 5f);

        Assert.Contains(active, found);
        Assert.DoesNotContain(inactive, found);
        Assert.Single(found);
    }

    [Fact]
    public void FindNearbyType_ReturnsEmptyListWhenNoEntitiesOfType()
    {
        var system = CreateEntitySystem();
        CreateEntity(system, new Vector2(1, 0));

        var found = system.FindNearby<SpecialEntity>(Vector2.Zero, 5f);

        Assert.Empty(found);
    }

    // ===== T3: FindByTag() Tests (already implemented in Sprint 0) =====

    [Fact]
    public void FindByTag_ReturnsFirstEntityWithTag()
    {
        var system = CreateEntitySystem();
        var first = CreateEntity(system);
        var second = CreateEntity(system);
        first.SetTag("enemy");
        second.SetTag("enemy");

        var found = system.FindByTag("enemy");

        Assert.Equal(first, found);
    }

    [Fact]
    public void FindByTag_ReturnsNullWhenNoEntitiesHaveTag()
    {
        var system = CreateEntitySystem();
        CreateEntity(system);

        var found = system.FindByTag("enemy");

        Assert.Null(found);
    }

    [Fact]
    public void FindByTag_ReturnsNullForNullTag()
    {
        var system = CreateEntitySystem();

        var found = system.FindByTag(null);

        Assert.Null(found);
    }

    // ===== Test Helpers =====

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }

    private class SpecialEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }
}
