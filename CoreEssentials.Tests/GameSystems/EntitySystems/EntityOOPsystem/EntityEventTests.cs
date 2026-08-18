using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Events;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;

/// <summary>
/// Test entity that tracks event subscriptions and published events.
/// </summary>
public class TestEventEntity : Entity
{
    public List<string> PublishedEvents { get; } = new();
    public List<string> SubscribedEvents { get; } = new();
    public int OnDestroyCallCount { get; set; }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        OnDestroyCallCount++;
        SubscribedEvents.Clear();
        PublishedEvents.Clear();
    }

    public void TrackPublish(string eventName)
    {
        PublishedEvents.Add(eventName);
    }

    public void TrackSubscribe(string eventName)
    {
        SubscribedEvents.Add(eventName);
    }

    public new void Subscribe(string eventName, EntityEventHandler handler)
    {
        TrackSubscribe(eventName);
        base.Subscribe(eventName, handler);
    }

    public new void Unsubscribe(string eventName, EntityEventHandler handler)
    {
        base.Unsubscribe(eventName, handler);
        SubscribedEvents.RemoveAll(e => e == eventName);
    }
}

/// <summary>
/// Test event data payload.
/// </summary>
public class TestEventData
{
    public string Message { get; set; } = string.Empty;
    public int Value { get; set; }

    public TestEventData(string message, int value)
    {
        Message = message;
        Value = value;
    }
}

public class EntityEventTests
{
    private EntityEventSystem _eventSystem;
    private EntitySystem _entitySystem;

    public EntityEventTests()
    {
        // Initialize the singleton event system
        _eventSystem = new EntityEventSystem();
        EntityEventSystem.Instance = _eventSystem;
        _entitySystem = new EntitySystem();
    }

    // ===== T1: EntityEventArgs Tests =====

    [Fact]
    public void EntityEventArgs_GetsSourceEntity()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        var args = new EntityEventArgs(entity);

        Assert.Equal(entity, args.Source);
    }

    [Fact]
    public void EntityEventArgs_GetsTimestamp()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        var args = new EntityEventArgs(entity);

        Assert.True(args.Timestamp > 0);
    }

    [Fact]
    public void EntityEventArgsT_GetsSourceAndData()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        var data = new TestEventData("test", 42);
        var args = new EntityEventArgs<TestEventData>(entity, data);

        Assert.Equal(entity, args.Source);
        Assert.Equal(data, args.Data);
        Assert.Equal("test", args.Data.Message);
        Assert.Equal(42, args.Data.Value);
    }

    // ===== T2: EntityEventSystem Tests =====

    [Fact]
    public void EventSystem_SubscribeAddsHandler()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        EntityEventHandler handler = (s, e) => { };

        _eventSystem.Subscribe(entity, "TestEvent", handler);

        Assert.Equal(1, _eventSystem.GetSubscriberCount("TestEvent"));
    }

    [Fact]
    public void EventSystem_PublishInvokesHandler()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        bool handlerCalled = false;
        EntityEventHandler handler = (s, e) => handlerCalled = true;

        _eventSystem.Subscribe(entity, "TestEvent", handler);
        _eventSystem.Publish(entity, "TestEvent", new EntityEventArgs(entity));

        Assert.True(handlerCalled);
    }

    [Fact]
    public void EventSystem_PublishWithGenericData()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        TestEventData receivedData = null;
        EntityEventHandler handler = (s, e) =>
        {
            if (e is EntityEventArgs<TestEventData> genericArgs)
            {
                receivedData = genericArgs.Data;
            }
        };

        var eventData = new TestEventData("hello", 123);
        _eventSystem.Subscribe(entity, "TestEvent", handler);
        _eventSystem.Publish(entity, "TestEvent", new EntityEventArgs<TestEventData>(entity, eventData));

        Assert.NotNull(receivedData);
        Assert.Equal("hello", receivedData.Message);
        Assert.Equal(123, receivedData.Value);
    }

    [Fact]
    public void EventSystem_UnsubscribeRemovesHandler()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        EntityEventHandler handler = (s, e) => { };

        _eventSystem.Subscribe(entity, "TestEvent", handler);
        _eventSystem.Unsubscribe(entity, "TestEvent", handler);

        Assert.Equal(0, _eventSystem.GetSubscriberCount("TestEvent"));
    }

    [Fact]
    public void EventSystem_UnsubscribePreventsHandlerInvocation()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        bool handlerCalled = false;
        EntityEventHandler handler = (s, e) => handlerCalled = true;

        _eventSystem.Subscribe(entity, "TestEvent", handler);
        _eventSystem.Unsubscribe(entity, "TestEvent", handler);
        _eventSystem.Publish(entity, "TestEvent", new EntityEventArgs(entity));

        Assert.False(handlerCalled);
    }

    [Fact]
    public void EventSystem_MultipleHandlersForSameEvent()
    {
        var entity1 = _entitySystem.CreateEntity<TestEventEntity>();
        var entity2 = _entitySystem.CreateEntity<TestEventEntity>();
        int callCount = 0;

        EntityEventHandler handler1 = (s, e) => callCount++;
        EntityEventHandler handler2 = (s, e) => callCount++;

        _eventSystem.Subscribe(entity1, "TestEvent", handler1);
        _eventSystem.Subscribe(entity2, "TestEvent", handler2);

        _eventSystem.Publish(entity1, "TestEvent", new EntityEventArgs(entity1));

        Assert.Equal(2, callCount); // Both handlers called
    }

    [Fact]
    public void EventSystem_GetEventNamesReturnsActiveEvents()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        EntityEventHandler handler = (s, e) => { };

        _eventSystem.Subscribe(entity, "EventA", handler);
        _eventSystem.Subscribe(entity, "EventB", handler);

        var eventNames = _eventSystem.GetEventNames();

        Assert.Contains("EventA", eventNames);
        Assert.Contains("EventB", eventNames);
    }

    [Fact]
    public void EventSystem_PublishToNonExistentEventDoesNotThrow()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();

        var ex = Record.Exception(() => _eventSystem.Publish(entity, "NonExistentEvent", new EntityEventArgs(entity)));
        Assert.Null(ex);
    }

    // ===== T3: Entity Subscribe/Publish Convenience Methods Tests =====

    [Fact]
    public void Entity_SubscribeAddsToTrackingList()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        EntityEventHandler handler = (s, e) => { };

        entity.Subscribe("TestEvent", handler);

        Assert.Contains("TestEvent", entity.SubscribedEvents);
    }

    [Fact]
    public void Entity_PublishTracksEvent()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        bool handlerCalled = false;
        EntityEventHandler handler = (s, e) => handlerCalled = true;

        _eventSystem.Subscribe(entity, "TestEvent", handler);
        entity.Publish("TestEvent", new EntityEventArgs(entity));

        Assert.True(handlerCalled);
    }

    [Fact]
    public void Entity_UnsubscribeRemovesFromTrackingList()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        EntityEventHandler handler = (s, e) => { };

        entity.Subscribe("TestEvent", handler);
        entity.Unsubscribe("TestEvent", handler);

        Assert.DoesNotContain("TestEvent", entity.SubscribedEvents);
    }

    [Fact]
    public void Entity_SubscribeBeforeInitializationThrows()
    {
        // Create entity without adding to system
        var entity = new TestEventEntity();

        var ex = Record.Exception(() => entity.Subscribe("Test", (s, a) => { }));
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void Entity_PublishBeforeInitializationThrows()
    {
        // Create entity without adding to system
        var entity = new TestEventEntity();

        var ex = Record.Exception(() => entity.Publish("Test", new EntityEventArgs(entity)));
        Assert.IsType<InvalidOperationException>(ex);
    }

    // ===== T4: Auto-Unsubscribe on Destroy Tests =====

    [Fact]
    public void Entity_DestroyAutoUnsubscribes()
    {
        var entity1 = _entitySystem.CreateEntity<TestEventEntity>();
        var entity2 = _entitySystem.CreateEntity<TestEventEntity>();
        bool handlerCalled = false;
        EntityEventHandler handler = (s, e) => handlerCalled = true;

        entity1.Subscribe("TestEvent", handler);
        entity1.Publish("TestEvent", new EntityEventArgs(entity1));

        Assert.True(handlerCalled);
        handlerCalled = false;

        // Destroy entity - should auto-unsubscribe
        entity1.Destroy();
        _entitySystem.Update(new GameTime());

        // Handler should not be called after destroy
        entity2.Publish("TestEvent", new EntityEventArgs(entity2));
        Assert.False(handlerCalled);
    }

    [Fact]
    public void Entity_DestroyClearsSubscriptionsList()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        EntityEventHandler handler = (s, e) => { };

        entity.Subscribe("Event1", handler);
        entity.Subscribe("Event2", handler);

        Assert.Equal(2, entity.SubscribedEvents.Count);

        entity.Destroy();
        _entitySystem.Update(new GameTime());

        Assert.Empty(entity.SubscribedEvents);
    }

    [Fact]
    public void Entity_OnDestroyCalledAfterDestroy()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();

        entity.Destroy();
        _entitySystem.Update(new GameTime());

        Assert.True(entity.Destroyed);
        Assert.True(entity.OnDestroyCallCount > 0);
    }

    [Fact]
    public void EntityEventSystem_UnsubscribeEntityRemovesAllSubscriptions()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        EntityEventHandler handler1 = (s, e) => { };
        EntityEventHandler handler2 = (s, e) => { };

        _eventSystem.Subscribe(entity, "Event1", handler1);
        _eventSystem.Subscribe(entity, "Event2", handler2);

        Assert.Equal(1, _eventSystem.GetSubscriberCount("Event1"));
        Assert.Equal(1, _eventSystem.GetSubscriberCount("Event2"));

        _eventSystem.UnsubscribeEntity(entity);

        Assert.Equal(0, _eventSystem.GetSubscriberCount("Event1"));
        Assert.Equal(0, _eventSystem.GetSubscriberCount("Event2"));
    }

    [Fact]
    public void EntityEventArgs_ThrowsOnNullSource()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityEventArgs((Entity)null));
    }

    [Fact]
    public void EntityEventArgsT_ThrowsOnNullSource()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityEventArgs<TestEventData>((Entity)null, new TestEventData("", 0)));
    }

    [Fact]
    public void EntityEventArgsT_ThrowsOnNullData()
    {
        var entity = _entitySystem.CreateEntity<TestEventEntity>();
        Assert.Throws<ArgumentNullException>(() => new EntityEventArgs<TestEventData>(entity, null));
    }
}
