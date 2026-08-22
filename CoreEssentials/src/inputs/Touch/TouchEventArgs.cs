// CoreEssentials/src/inputs/Touch/TouchEventArgs.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Event arguments for touch events raised by the CoreEssentials <see cref="Touch"/> wrapper.
    /// This is a CoreEssentials-owned type, so consumers do not need to reference any
    /// <c>MonoGame.Extended</c> namespaces to handle touch input.
    /// </summary>
    public class TouchEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TouchEventArgs"/> class.
        /// </summary>
        /// <param name="time">The time at which the event occurred.</param>
        /// <param name="id">The unique identifier of the touch point.</param>
        /// <param name="position">The touch position (in pixels) on the current frame.</param>
        /// <param name="state">The state of the touch location at the time of the event.</param>
        public TouchEventArgs(
            TimeSpan time,
            int id,
            Vector2 position,
            TouchLocationState state)
        {
            Time = time;
            Id = id;
            Position = position;
            State = state;
        }

        /// <summary>
        /// Gets the time at which the event occurred.
        /// </summary>
        public TimeSpan Time { get; }

        /// <summary>
        /// Gets the unique identifier of the touch point. Use this to track a specific finger
        /// across multiple events (e.g. from <see cref="Touch.TouchStarted"/> to
        /// <see cref="Touch.TouchMoved"/>).
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the current touch position in pixels. This is viewport-independent and does not
        /// require any viewport adapter to be configured on the underlying listener.
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// Gets the state of the touch location at the time of the event.
        /// </summary>
        public TouchLocationState State { get; }
    }
}
