// CoreEssentials/src/inputs/MouseEventArgs.cs
using System;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Event arguments for mouse events raised by the CoreEssentials <see cref="Mouse"/> wrapper.
    /// This is a CoreEssentials-owned type, so consumers do not need to reference any
    /// <c>MonoGame.Extended</c> namespaces to handle mouse input.
    /// </summary>
    public class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MouseEventArgs"/> class.
        /// </summary>
        /// <param name="time">The time at which the event occurred.</param>
        /// <param name="previousPosition">The mouse position (in pixels) on the previous frame.</param>
        /// <param name="position">The mouse position (in pixels) on the current frame.</param>
        /// <param name="button">The mouse button associated with this event, if any.</param>
        /// <param name="scrollWheelValue">The current scroll wheel value.</param>
        /// <param name="scrollWheelDelta">The change in scroll wheel value since the previous frame.</param>
        public MouseEventArgs(
            TimeSpan time,
            Vector2 previousPosition,
            Vector2 position,
            MouseButton button = MouseButton.None,
            int scrollWheelValue = 0,
            int scrollWheelDelta = 0)
        {
            Time = time;
            PreviousPosition = previousPosition;
            Position = position;
            Button = button;
            ScrollWheelValue = scrollWheelValue;
            ScrollWheelDelta = scrollWheelDelta;
        }

        /// <summary>
        /// Gets the time at which the event occurred.
        /// </summary>
        public TimeSpan Time { get; }

        /// <summary>
        /// Gets the mouse position (in pixels) on the previous frame.
        /// </summary>
        public Vector2 PreviousPosition { get; }

        /// <summary>
        /// Gets the current mouse position in pixels. Unlike the underlying engine event args,
        /// this is a viewport-independent <see cref="Vector2"/> that does not depend on any
        /// viewport adapter being configured.
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// Gets the mouse button associated with this event, or <see cref="MouseButton.None"/>
        /// for movement and scroll events.
        /// </summary>
        public MouseButton Button { get; }

        /// <summary>
        /// Gets the current scroll wheel value.
        /// </summary>
        public int ScrollWheelValue { get; }

        /// <summary>
        /// Gets the change in scroll wheel value since the previous frame.
        /// Positive values indicate scrolling up, negative values indicate scrolling down.
        /// </summary>
        public int ScrollWheelDelta { get; }

        /// <summary>
        /// Gets the amount the mouse moved since the previous frame.
        /// </summary>
        public Vector2 DeltaMoved => Position - PreviousPosition;

        /// <summary>
        /// Gets a value indicating whether this event was caused by the left (primary) button.
        /// </summary>
        public bool IsLeftButton => Button == MouseButton.Left;

        /// <summary>
        /// Gets a value indicating whether this event was caused by the right (secondary) button.
        /// </summary>
        public bool IsRightButton => Button == MouseButton.Right;

        /// <summary>
        /// Gets a value indicating whether this event was caused by the middle button.
        /// </summary>
        public bool IsMiddleButton => Button == MouseButton.Middle;
    }
}
