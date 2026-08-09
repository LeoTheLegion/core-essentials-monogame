using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Spatial;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class SpatialGridTests : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    // ===== T1: SpatialGrid Insert/Remove Tests =====

    [Fact]
    public void Constructor_ValidCellSize_Succeeds()
    {
        var grid = new SpatialGrid(100f);
        Assert.Equal(100f, grid.CellSize);
        Assert.Equal(0, grid.Count);
    }

    [Fact]
    public void Constructor_ZeroCellSize_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SpatialGrid(0));
    }

    [Fact]
    public void Constructor_NegativeCellSize_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new SpatialGrid(-50f));
    }

    [Fact]
    public void Insert_NullEntity_ThrowsArgumentNullException()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();

        Assert.Throws<ArgumentNullException>(() => grid.Insert(null!));
    }

    [Fact]
    public void Insert_Entity_IncreasesCount()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();

        grid.Insert(entity);

        Assert.Equal(1, grid.Count);
    }

    [Fact]
    public void Remove_Entity_DecreasesCount()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        grid.Insert(entity);

        grid.Remove(entity);

        Assert.Equal(0, grid.Count);
    }

    [Fact]
    public void Remove_NonExistentEntity_DoesNotThrow()
    {
        var grid = new SpatialGrid(100f);

        grid.Remove(new TestEntity());
    }

    [Fact]
    public void Insert_SameEntityTwice_UpdatesPosition()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();

        grid.Insert(entity);
        grid.Insert(entity);

        Assert.Equal(1, grid.Count);
    }

    // ===== Rectangle Query Tests =====

    [Fact]
    public void Query_Rectangle_ReturnsEntitiesInBounds()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();
        var entity1 = system.CreateEntity<TestEntity>();
        entity1.Position = new Vector2(50, 50);

        var entity2 = system.CreateEntity<TestEntity>();
        entity2.Position = new Vector2(400, 400);

        grid.Insert(entity1);
        grid.Insert(entity2);

        var results = grid.Query(new Rectangle(0, 0, 100, 100));

        Assert.Contains(entity1, results);
        Assert.DoesNotContain(entity2, results);
    }

    [Fact]
    public void Query_Rectangle_ReturnsMultipleEntities()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();

        for (int i = 0; i < 5; i++)
        {
            var entity = system.CreateEntity<TestEntity>();
            entity.Position = new Vector2(i * 10, i * 10);
            grid.Insert(entity);
        }

        var results = grid.Query(new Rectangle(0, 0, 100, 100));

        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void Query_Rectangle_EmptyRange_ReturnsNoEntities()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = Vector2.Zero;

        grid.Insert(entity);

        var results = grid.Query(new Rectangle(1000, 1000, 100, 100));

        Assert.Empty(results);
    }

    // ===== Radius Query Tests =====

    [Fact]
    public void Query_Radius_ReturnsEntitiesWithinRadius()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();

        var closeEntity = system.CreateEntity<TestEntity>();
        closeEntity.Position = new Vector2(50, 50);

        var farEntity = system.CreateEntity<TestEntity>();
        farEntity.Position = new Vector2(300, 300);

        grid.Insert(closeEntity);
        grid.Insert(farEntity);

        var results = grid.Query(new Vector2(0, 0), 100f);

        Assert.Contains(closeEntity, results);
        Assert.DoesNotContain(farEntity, results);
    }

    [Fact]
    public void Query_Radius_ExcludesEntitiesOutsideRadius()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();

        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(150, 0);

        grid.Insert(entity);

        var results = grid.Query(new Vector2(0, 0), 100f);

        Assert.DoesNotContain(entity, results);
    }

    [Fact]
    public void Query_Radius_OnBoundary_IncludesEntity()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();

        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(100, 0);

        grid.Insert(entity);

        var results = grid.Query(new Vector2(0, 0), 100f);

        Assert.Contains(entity, results);
    }

    // ===== Entity Movement Tests =====

    [Fact]
    public void UpdatePosition_EntityMovesToNewCell()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();
        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(50, 50);

        grid.Insert(entity);

        entity.Position = new Vector2(250, 250);
        grid.UpdatePosition(entity);

        var results = grid.Query(new Rectangle(200, 200, 100, 100));
        Assert.Contains(entity, results);
    }

    [Fact]
    public void Clear_RemovesAllEntities()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();

        for (int i = 0; i < 5; i++)
        {
            var entity = system.CreateEntity<TestEntity>();
            grid.Insert(entity);
        }

        grid.Clear();

        Assert.Equal(0, grid.Count);
    }

    // ===== T2: EntitySystem Integration Tests =====

    [Fact]
    public void FindInBounds_ReturnsEntitiesInRectangle()
    {
        var system = new EntitySystem();

        var entity1 = system.CreateEntity<TestEntity>();
        entity1.Position = new Vector2(50, 50);

        var entity2 = system.CreateEntity<TestEntity>();
        entity2.Position = new Vector2(400, 400);

        var results = system.FindInBounds(new Rectangle(0, 0, 100, 100));

        Assert.Contains(entity1, results);
        Assert.DoesNotContain(entity2, results);
    }

    [Fact]
    public void FindInBounds_ExcludesInactiveEntities()
    {
        var system = new EntitySystem();

        var activeEntity = system.CreateEntity<TestEntity>();
        activeEntity.Position = new Vector2(50, 50);

        var inactiveEntity = system.CreateEntity<TestEntity>();
        inactiveEntity.Position = new Vector2(50, 50);
        inactiveEntity.SetActive(false);

        var results = system.FindInBounds(new Rectangle(0, 0, 100, 100));

        Assert.Contains(activeEntity, results);
        Assert.DoesNotContain(inactiveEntity, results);
    }

    [Fact]
    public void FindClosest_ReturnsNearestEntity()
    {
        var system = new EntitySystem();

        var farEntity = system.CreateEntity<TestEntity>();
        farEntity.Position = new Vector2(200, 200);

        var closeEntity = system.CreateEntity<TestEntity>();
        closeEntity.Position = new Vector2(50, 50);

        var closest = system.FindClosest(new Vector2(0, 0), 300f);

        Assert.Same(closeEntity, closest);
    }

    [Fact]
    public void FindClosest_ReturnsNullWhenNoEntitiesInRange()
    {
        var system = new EntitySystem();

        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(500, 500);

        var closest = system.FindClosest(new Vector2(0, 0), 100f);

        Assert.Null(closest);
    }

    [Fact]
    public void FindClosest_ExcludesInactiveEntities()
    {
        var system = new EntitySystem();

        var activeEntity = system.CreateEntity<TestEntity>();
        activeEntity.Position = new Vector2(200, 0);

        var inactiveEntity = system.CreateEntity<TestEntity>();
        inactiveEntity.Position = new Vector2(50, 0);
        inactiveEntity.SetActive(false);

        var closest = system.FindClosest(new Vector2(0, 0), 300f);

        Assert.Same(activeEntity, closest);
    }

    [Fact]
    public void SpatialPartitioning_Disabled_FallsBackToLinearSearch()
    {
        var system = new EntitySystem();
        system.SpatialPartitioningEnabled = false;

        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(50, 50);

        var results = system.FindInBounds(new Rectangle(0, 0, 100, 100));

        Assert.Contains(entity, results);
    }

    [Fact]
    public void Entity_Destroyed_RemovedFromSpatialGrid()
    {
        var system = new EntitySystem();

        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(50, 50);
        entity.Destroy();

        system.Update(new GameTime());

        var results = system.FindInBounds(new Rectangle(0, 0, 100, 100));
        Assert.DoesNotContain(entity, results);
    }

    // ===== Entities Spanning Multiple Cells Tests =====

    [Fact]
    public void Query_Rectangle_CrossesCellBoundaries()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();

        var entity = system.CreateEntity<TestEntity>();
        entity.Position = new Vector2(95, 95); // Near cell boundary

        grid.Insert(entity);

        // Query that crosses cell boundaries
        var results = grid.Query(new Rectangle(80, 80, 40, 40));

        Assert.Contains(entity, results);
    }

    // ===== Performance Test =====

    [Fact]
    public void Query_SpatialGrid_FasterThanLinearSearch()
    {
        var grid = new SpatialGrid(100f);
        var system = new EntitySystem();

        const int entityCount = 1000;

        // Create entities spread across the world
        for (int i = 0; i < entityCount; i++)
        {
            var entity = system.CreateEntity<TestEntity>();
            entity.Position = new Vector2(i * 10, i * 10);
            grid.Insert(entity);
        }

        // Time spatial grid query
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            grid.Query(new Rectangle(5000, 5000, 200, 200));
        }
        var spatialTime = sw.Elapsed;

        // Time linear search
        sw.Restart();
        for (int i = 0; i < 100; i++)
        {
            foreach (var entity in system.GetEntities())
            {
                var pos = entity.Position;
                if (new Rectangle(5000, 5000, 200, 200).Contains((int)pos.X, (int)pos.Y))
                    _ = entity;
            }
        }
        var linearTime = sw.Elapsed;

        Assert.True(spatialTime < linearTime * 1.5,
            $"Spatial grid should be reasonably faster: spatial={spatialTime.TotalMilliseconds}ms, linear={linearTime.TotalMilliseconds}ms");
    }
}

/// <summary>
/// Test entity implementation for spatial tests.
/// </summary>
public class TestEntity : Entity
{
    public override void Update(GameTime gameTime) { }
    public override void Render(SpriteBatch spriteBatch) { }
}
