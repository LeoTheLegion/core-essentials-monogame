using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

public class EntityComponentTests
{
    private class TestComponent : EntityComponent
    {
        public bool OnAttachCalled { get; private set; }
        public bool OnDetachCalled { get; private set; }
        public int UpdateCount { get; private set; }

        public override void OnAttach()
        {
            OnAttachCalled = true;
        }

        public override void OnDetach()
        {
            OnDetachCalled = true;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateCount++;
        }
    }

    private class SecondComponent : EntityComponent
    {
        public bool OnDetachCalled { get; private set; }

        public override void OnDetach()
        {
            OnDetachCalled = true;
        }
    }

    private class TestEntity : Entity
    {
        public override void Update(GameTime gameTime) { }
        public override void Render(SpriteBatch spriteBatch) { }
    }

    [Fact]
    public void AddComponent_AddsComponentToEntity()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        var retrieved = entity.GetComponent<TestComponent>();
        Assert.Same(component, retrieved);
    }

    [Fact]
    public void AddComponent_SetsOwnerReference()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        Assert.Same(entity, component.Owner);
    }

    [Fact]
    public void AddComponent_CallsOnAttach()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        Assert.True(component.OnAttachCalled);
    }

    [Fact]
    public void AddComponent_ReturnsComponent()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        var result = entity.AddComponent(component);

        Assert.Same(component, result);
    }

    [Fact]
    public void GetComponent_ReturnsNullWhenNotPresent()
    {
        var entity = new TestEntity();
        var component = entity.GetComponent<TestComponent>();

        Assert.Null(component);
    }

    [Fact]
    public void TryGetComponent_ReturnsTrueWhenPresent()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        bool result = entity.TryGetComponent<TestComponent>(out var retrieved);

        Assert.True(result);
        Assert.Same(component, retrieved);
    }

    [Fact]
    public void TryGetComponent_ReturnsFalseWhenNotPresent()
    {
        var entity = new TestEntity();
        bool result = entity.TryGetComponent<TestComponent>(out var retrieved);

        Assert.False(result);
        Assert.Null(retrieved);
    }

    [Fact]
    public void HasComponent_ReturnsTrueWhenPresent()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        Assert.True(entity.HasComponent<TestComponent>());
    }

    [Fact]
    public void HasComponent_ReturnsFalseWhenNotPresent()
    {
        var entity = new TestEntity();

        Assert.False(entity.HasComponent<TestComponent>());
    }

    [Fact]
    public void RemoveComponent_RemovesComponentFromEntity()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        var removed = entity.RemoveComponent<TestComponent>();

        Assert.Same(component, removed);
        Assert.Null(entity.GetComponent<TestComponent>());
    }

    [Fact]
    public void RemoveComponent_CallsOnDetach()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        entity.RemoveComponent<TestComponent>();

        Assert.True(component.OnDetachCalled);
    }

    [Fact]
    public void RemoveComponent_ClearsOwnerReference()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        entity.RemoveComponent<TestComponent>();

        Assert.Null(component.Owner);
    }

    [Fact]
    public void RemoveComponent_ReturnsNullWhenNotPresent()
    {
        var entity = new TestEntity();
        var removed = entity.RemoveComponent<TestComponent>();

        Assert.Null(removed);
    }

    [Fact]
    public void AddComponent_DuplicateType_ThrowsInvalidOperationException()
    {
        var entity = new TestEntity();
        var component1 = new TestComponent();
        var component2 = new TestComponent();
        entity.AddComponent(component1);

        var exception = Assert.Throws<InvalidOperationException>(() => entity.AddComponent(component2));

        Assert.Contains("TestComponent", exception.Message);
    }

    [Fact]
    public void AddComponent_NullComponent_ThrowsArgumentNullException()
    {
        var entity = new TestEntity();

        Assert.Throws<ArgumentNullException>(() => entity.AddComponent((TestComponent?)null!));
    }

    [Fact]
    public void Components_ReturnsAllComponents()
    {
        var entity = new TestEntity();
        var component1 = new TestComponent();
        var component2 = new SecondComponent();
        entity.AddComponent(component1);
        entity.AddComponent(component2);

        var components = new List<EntityComponent>(entity.Components);

        Assert.Equal(2, components.Count);
        Assert.Contains(component1, components);
        Assert.Contains(component2, components);
    }

    [Fact]
    public void OnDestroy_DetachesAllComponents()
    {
        var entity = new TestEntity();
        var component1 = new TestComponent();
        var component2 = new SecondComponent();
        entity.AddComponent(component1);
        entity.AddComponent(component2);

        entity.OnDestroy();

        Assert.True(component1.OnDetachCalled);
        Assert.Null(component1.Owner);
        Assert.Null(component2.Owner);
    }

    [Fact]
    public void OnDestroy_ClearsComponentsCollection()
    {
        var entity = new TestEntity();
        var component = new TestComponent();
        entity.AddComponent(component);

        entity.OnDestroy();

        Assert.Empty(entity.Components);
    }
}
