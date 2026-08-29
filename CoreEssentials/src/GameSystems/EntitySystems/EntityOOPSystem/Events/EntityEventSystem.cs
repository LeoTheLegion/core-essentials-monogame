using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Events;

/// <summary>
/// Delegate for handling entity events.
/// </summary>
/// <param name="sender">The entity that raised the event.</param>
/// <param name="args">The event arguments containing event data.</param>
public delegate void EntityEventHandler(Entity sender, EntityEventArgs args);

/// <summary>
/// Manages a global event registry for decoupled entity communication.
/// Supports both generic and non-generic events with entity-scoped subscription tracking.
/// </summary>
/// <remarks>
/// Events are identified by string names. Subscriptions can be scoped to entities
/// and are automatically cleaned up when the entity is destroyed.
/// 
/// Usage:
/// <code>
/// // Subscribe to an event
/// entity.Subscribe("OnDamage", (sender, args) =&gt; {
///     var damage = ((EntityEventArgs&lt;float&gt;)args).Data;
///     Health -= damage;
/// });
/// 
/// // Publish an event
/// entity.Publish("OnDamage", new EntityEventArgs&lt;float&gt;(entity, 10f));
/// </code>
/// </remarks>
[Obsolete("Use EntitySystem.SendMessage for scene-wide messages or declarative <Bind> wiring in XML scenes. The legacy entity event system is being removed.")]
public class EntityEventSystem : GameSystem
{
    /// <summary>
    /// Gets the singleton instance of the EntityEventSystem.
    /// </summary>
    public static EntityEventSystem? Instance { get; set; }

    /// <summary>
    /// Dictionary mapping event names to their handlers.
    /// </summary>
    private readonly Dictionary<string, List<EntityEventHandler>> _eventHandlers = new Dictionary<string, List<EntityEventHandler>>();

    /// <summary>
    /// Dictionary mapping entities to their subscribed event name/handler pairs for auto-cleanup.
    /// </summary>
    private readonly Dictionary<Entity, List<(string EventName, EntityEventHandler Handler)>> _entitySubscriptions = new Dictionary<Entity, List<(string, EntityEventHandler)>>();

    /// <summary>
    /// Queue of events to publish, processed at the end of Update to avoid reentrancy issues.
    /// </summary>
    private readonly Queue<(Entity Source, EntityEventArgs Args)> _pendingEvents = new Queue<(Entity, EntityEventArgs)>();

    /// <summary>
    /// Lock for thread-safe subscription modifications.
    /// </summary>
    private readonly object _lock = new object();

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    public override void OnStart()
    {
        Instance = this;
    }

    /// <summary>
    /// Subscribes an entity to an event with a handler.
    /// The subscription is automatically removed when the entity is destroyed.
    /// </summary>
    /// <param name="entity">The entity subscribing to the event.</param>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="handler">The handler to invoke when the event is raised.</param>
    public void Subscribe(Entity entity, string eventName, EntityEventHandler handler)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (eventName == null) throw new ArgumentNullException(nameof(eventName));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            if (!_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers = new List<EntityEventHandler>();
                _eventHandlers[eventName] = handlers;
            }

            handlers.Add(handler);

            // Track subscription for auto-cleanup
            if (!_entitySubscriptions.TryGetValue(entity, out var subscribedEvents))
            {
                subscribedEvents = new List<(string, EntityEventHandler)>();
                _entitySubscriptions[entity] = subscribedEvents;
            }
            subscribedEvents.Add((eventName, handler));
        }
    }

    /// <summary>
    /// Unsubscribes an entity from an event.
    /// </summary>
    /// <param name="entity">The entity unsubscribing from the event.</param>
    /// <param name="eventName">The name of the event to unsubscribe from.</param>
    /// <param name="handler">The handler to remove.</param>
    public void Unsubscribe(Entity entity, string eventName, EntityEventHandler handler)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (eventName == null) throw new ArgumentNullException(nameof(eventName));
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        lock (_lock)
        {
            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(eventName);
                }
            }

            // Remove from entity subscriptions tracking
            if (_entitySubscriptions.TryGetValue(entity, out var subscribedEvents))
            {
                subscribedEvents.RemoveAll(pair => pair.EventName == eventName && pair.Handler == handler);
                if (subscribedEvents.Count == 0)
                {
                    _entitySubscriptions.Remove(entity);
                }
            }
        }
    }

    /// <summary>
    /// Publishes an event immediately, invoking all handlers synchronously.
    /// </summary>
    /// <param name="source">The entity raising the event.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="args">The event arguments.</param>
    public void Publish(Entity source, string eventName, EntityEventArgs args)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (eventName == null) throw new ArgumentNullException(nameof(eventName));
        if (args == null) throw new ArgumentNullException(nameof(args));

        lock (_lock)
        {
            if (_eventHandlers.TryGetValue(eventName, out var handlers))
            {
                // Copy handlers to avoid modification during iteration
                var handlersCopy = new List<EntityEventHandler>(handlers);
                foreach (var handler in handlersCopy)
                {
                    handler(source, args);
                }
            }
        }
    }

    /// <summary>
    /// Cleans up all subscriptions for a destroyed entity.
    /// Called automatically when an entity is destroyed.
    /// </summary>
    /// <param name="entity">The entity being destroyed.</param>
    public void UnsubscribeEntity(Entity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        lock (_lock)
        {
            if (_entitySubscriptions.TryGetValue(entity, out var subscribedEvents))
            {
                foreach (var (eventName, handler) in subscribedEvents)
                {
                    if (_eventHandlers.TryGetValue(eventName, out var handlers))
                    {
                        handlers.Remove(handler);
                        if (handlers.Count == 0)
                        {
                            _eventHandlers.Remove(eventName);
                        }
                    }
                }

                _entitySubscriptions.Remove(entity);
            }
        }
    }

    /// <summary>
    /// Gets the number of handlers subscribed to an event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The number of handlers, or 0 if no handlers are subscribed.</returns>
    public int GetSubscriberCount(string eventName)
    {
        lock (_lock)
        {
            return _eventHandlers.TryGetValue(eventName, out var handlers) ? handlers.Count : 0;
        }
    }

    /// <summary>
    /// Gets all event names that have active subscriptions.
    /// </summary>
    /// <returns>An array of event name strings.</returns>
    public string[] GetEventNames()
    {
        lock (_lock)
        {
            return _eventHandlers.Keys.ToArray();
        }
    }
}
