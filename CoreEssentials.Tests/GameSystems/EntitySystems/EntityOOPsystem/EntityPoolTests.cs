using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Pooling;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class EntityPoolTests
{
    private static EntitySystem CreateEntitySystem() => new EntitySystem();

    // ===== T1: IPooledEntity Interface Tests =====

    [Fact]
    public void PooledEntity_ImplementsIPooledEntity()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        Assert.IsAssignableFrom<IPooledEntity>(entity);
    }

    [Fact]
    public void PooledEntity_IsActiveAfterAcquire()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        Assert.True(entity.GetActive());
    }

    [Fact]
    public void PooledEntity_ResetSetsPositionToZero()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(new Vector2(10, 20));

        entity.Reset();

        Assert.Equal(Vector2.Zero, entity.Position);
    }

    [Fact]
    public void PooledEntity_ActivateSetsPosition()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(Vector2.Zero);
        entity.Reset();

        entity.Activate(new Vector2(5, 10));

        Assert.Equal(new Vector2(5, 10), entity.Position);
    }

    [Fact]
    public void PooledEntity_ResetSetsInactive()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        entity.Reset();

        Assert.False(entity.GetActive());
    }

    // ===== T2: EntityPool<T> Tests =====

    [Fact]
    public void Pool_PreciatesInitialCapacity()
    {
        var pool = new EntityPool<TestPooledEntity>(initialCapacity: 5, maxSize: 10);

        Assert.Equal(5, pool.TotalCount);
        Assert.Equal(5, pool.AvailableCount);
        Assert.Equal(0, pool.ActiveCount);
    }

    [Fact]
    public void Pool_AcquireReturnsActiveEntity()
    {
        var pool = new EntityPool<TestPooledEntity>(initialCapacity: 3, maxSize: 10);
        var entity = pool.Acquire(new Vector2(1, 2));

        Assert.True(entity.GetActive());
        Assert.Equal(new Vector2(1, 2), entity.Position);
    }

    [Fact]
    public void Pool_AcquireDecreasesAvailableCount()
    {
        var pool = new EntityPool<TestPooledEntity>(initialCapacity: 3, maxSize: 10);
        pool.Acquire(Vector2.Zero);

        Assert.Equal(2, pool.AvailableCount);
        Assert.Equal(1, pool.ActiveCount);
    }

    [Fact]
    public void Pool_ReleaseReturnsEntityToPool()
    {
        var pool = new EntityPool<TestPooledEntity>(initialCapacity: 3, maxSize: 10);
        var entity = pool.Acquire(Vector2.Zero);

        pool.Release(entity);

        Assert.Equal(3, pool.AvailableCount);
        Assert.Equal(0, pool.ActiveCount);
        Assert.False(entity.GetActive());
    }

    [Fact]
    public void Pool_ThrowsOnNullRelease()
    {
        var pool = new EntityPool<TestPooledEntity>(initialCapacity: 1, maxSize: 10);

        Assert.Throws<ArgumentNullException>(() => pool.Release(null));
    }

    [Fact]
    public void Pool_CreatesNewInstanceWhenPoolExhausted()
    {
        var pool = new EntityPool<TestPooledEntity>(initialCapacity: 1, maxSize: 5);
        pool.Acquire(Vector2.Zero);
        pool.Acquire(Vector2.Zero);

        Assert.Equal(2, pool.TotalCount);
        Assert.Equal(2, pool.ActiveCount);
    }

    // ===== T3: EntitySystem Pool-Aware Methods Tests =====

    [Fact]
    public void CreatePooled_ReturnsActiveEntity()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(new Vector2(3, 4));

        Assert.True(entity.GetActive());
        Assert.Equal(new Vector2(3, 4), entity.Position);
    }

    [Fact]
    public void CreatePooled_AddsEntityToSystem()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        var entities = system.GetEntities();
        Assert.Contains(entity, entities);
    }

    [Fact]
    public void ReleasePooled_RemovesEntityFromSystem()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        system.ReleasePooled(entity);

        var entities = system.GetEntities();
        Assert.DoesNotContain(entity, entities);
    }

    [Fact]
    public void ReleasePooled_MakesEntityInactive()
    {
        var system = CreateEntitySystem();
        var entity = system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        system.ReleasePooled(entity);

        Assert.False(entity.GetActive());
    }

    [Fact]
    public void ReleasePooled_AllowsEntityToBeReacquired()
    {
        var system = CreateEntitySystem();
        var entity1 = system.CreatePooled<TestPooledEntity>(Vector2.Zero);
        system.ReleasePooled(entity1);

        var entity2 = system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        // Should be the same recycled instance
        Assert.Same(entity1, entity2);
    }

    [Fact]
    public void ReleasePooled_AcceptsNull()
    {
        var system = CreateEntitySystem();

        // Should not throw on null
        system.ReleasePooled<TestPooledEntity>(null);
        Assert.True(true); // No exception thrown
    }

    [Fact]
    public void CreatePooled_RespectsPoolCapacity()
    {
        var system = CreateEntitySystem();
        var pool = system.GetOrCreatePool<TestPooledEntity>(initialCapacity: 2, maxSize: 5);

        system.CreatePooled<TestPooledEntity>(Vector2.Zero);
        system.CreatePooled<TestPooledEntity>(Vector2.Zero);
        system.CreatePooled<TestPooledEntity>(Vector2.Zero);

        Assert.Equal(3, pool.TotalCount);
        Assert.Equal(3, pool.ActiveCount);
    }

    [Fact]
    public void GetOrCreatePool_ReturnsSamePoolInstance()
    {
        var system = CreateEntitySystem();

        var pool1 = system.GetOrCreatePool<TestPooledEntity>();
        var pool2 = system.GetOrCreatePool<TestPooledEntity>();

        Assert.Same(pool1, pool2);
    }

    // ===== Test Helpers =====

    private class TestPooledEntity : Entity, IPooledEntity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch _spriteBatch) { }

        public void Reset()
        {
            SetActive(false);
            Position = Vector2.Zero;
            Rotation = 0f;
        }

        public void Activate(Vector2 position)
        {
            SetActive(true);
            Position = position;
        }
    }
}
