# Event System Documentation

The Event System provides a decoupled publish/subscribe mechanism for entity communication. Entities can publish events and other entities can subscribe to them without needing direct references to each other.

## Overview

- **EntityEventArgs**: Base and generic event data containers
- **EntityEventSystem**: Global event registry managing subscriptions and publishing
- **Entity**: Convenience methods for subscribing, publishing, and unsubscribing

## EntityEventArgs

### Base Class
```csharp
var args = new EntityEventArgs(sourceEntity);
var source = args.Source;      // The entity that raised the event
var timestamp = args.Timestamp; // Timestamp in milliseconds
```

### Generic Class
```csharp
var data = new MyEventData("message", 42);
var args = new EntityEventArgs<MyEventData>(sourceEntity, data);
var payload = args.Data; // The typed data payload
```

## EntityEventSystem

### Subscribing
```csharp
// Subscribe to an event
entity.Subscribe("OnDamage", (sender, args) => {
    var damage = ((EntityEventArgs<float>)args).Data;
    Health -= damage;
});
```

### Publishing
```csharp
// Publish an event
entity.Publish("OnDamage", new EntityEventArgs<float>(entity, 10f));
```

### Unsubscribing
```csharp
// Unsubscribe from an event
entity.Unsubscribe("OnDamage", handler);
```

## Auto-Cleanup

When an entity is destroyed, it automatically unsubscribes from all events it was subscribed to. This prevents memory leaks and null reference exceptions.

## Thread Safety

The event system uses locks to ensure thread-safe subscription modifications.

## Testing

The event system is covered by 23 unit tests in `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/EntityEventTests.cs`.
