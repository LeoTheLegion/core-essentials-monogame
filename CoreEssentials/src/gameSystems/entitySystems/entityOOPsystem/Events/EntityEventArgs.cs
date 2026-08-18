using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Events;

/// <summary>
/// Base event arguments for entity events. Provides the source entity reference.
/// </summary>
public class EntityEventArgs : EventArgs
{
    /// <summary>
    /// Gets the entity that raised this event.
    /// </summary>
    public Entity Source { get; }

    /// <summary>
    /// Gets the timestamp (total milliseconds) when this event was raised.
    /// </summary>
    public long Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityEventArgs"/> class.
    /// </summary>
    /// <param name="source">The entity that raised this event.</param>
    public EntityEventArgs(Entity source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }
}

/// <summary>
/// Generic event arguments for entity events with typed data payload.
/// </summary>
/// <typeparam name="T">The type of the event data payload.</typeparam>
public class EntityEventArgs<T> : EntityEventArgs
{
    /// <summary>
    /// Gets the data payload of this event.
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityEventArgs{T}"/> class.
    /// </summary>
    /// <param name="source">The entity that raised this event.</param>
    /// <param name="data">The data payload of this event.</param>
    public EntityEventArgs(Entity source, T data) : base(source)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}
