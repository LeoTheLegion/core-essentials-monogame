// CoreEssentials/src/inputs/Touch/Touch.cs
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended.Input.InputListeners;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Wraps the MonoGame.Extended <c>TouchListener</c> to provide a clean, CoreEssentials-owned
    /// touch API. Consumers do not need to reference any <c>MonoGame.Extended</c> namespaces:
    /// events use the CoreEssentials <see cref="TouchEventArgs"/> (with a viewport-independent
    /// <see cref="Vector2">Position</see>).
    /// </summary>
    public class Touch
    {
        private readonly ITouchStateProvider _touchStateProvider;
        private readonly TouchListener _touchListener;

        private IReadOnlyList<TouchLocation> _currentState;

        /// <summary>
        /// Occurs when a new touch point is pressed down.
        /// </summary>
        public event EventHandler<TouchEventArgs>? TouchStarted;

        /// <summary>
        /// Occurs when a touch point is released.
        /// </summary>
        public event EventHandler<TouchEventArgs>? TouchEnded;

        /// <summary>
        /// Occurs while an active touch point moves.
        /// </summary>
        public event EventHandler<TouchEventArgs>? TouchMoved;

        /// <summary>
        /// Occurs when a touch point is cancelled by the system (e.g. an incoming call).
        /// </summary>
        public event EventHandler<TouchEventArgs>? TouchCancelled;

        /// <summary>
        /// Initializes a new instance of the <see cref="Touch"/> class.
        /// </summary>
        /// <param name="stateProvider">The touch state provider to use for polling.
        /// If null, a <see cref="DefaultTouchStateProvider"/> will be used.</param>
        public Touch(ITouchStateProvider? stateProvider = null)
        {
            _touchListener = new TouchListener();
            _touchStateProvider = stateProvider ?? new DefaultTouchStateProvider();

            _currentState = _touchStateProvider.GetState();

            // Subscribe once to the underlying listener and re-raise with CoreEssentials args.
            _touchListener.TouchStarted += OnListenerTouchStarted;
            _touchListener.TouchEnded += OnListenerTouchEnded;
            _touchListener.TouchMoved += OnListenerTouchMoved;
            _touchListener.TouchCancelled += OnListenerTouchCancelled;
        }

        /// <summary>
        /// Updates the touch state. This should be called once per frame (e.g. via
        /// <see cref="Input.Update"/>).
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        public virtual void Update(GameTime gameTime)
        {
            _currentState = _touchStateProvider.GetState();

            // Update the internal MonoGame.Extended TouchListener.
            // This is important for the events to fire.
            _touchListener.Update(gameTime);
        }

        /// <summary>
        /// Gets the number of currently active touch points.
        /// </summary>
        public int ActiveTouchCount => _currentState.Count;

        /// <summary>
        /// Gets a value indicating whether any touch points are currently active.
        /// </summary>
        public bool HasActiveTouches => _currentState.Count > 0;

        // ---- Underlying listener event forwarding (converted to CoreEssentials args) ----

        private void OnListenerTouchStarted(object? sender, MonoGame.Extended.Input.InputListeners.TouchEventArgs e)
            => TouchStarted?.Invoke(this, ToCoreArgs(e));

        private void OnListenerTouchEnded(object? sender, MonoGame.Extended.Input.InputListeners.TouchEventArgs e)
            => TouchEnded?.Invoke(this, ToCoreArgs(e));

        private void OnListenerTouchMoved(object? sender, MonoGame.Extended.Input.InputListeners.TouchEventArgs e)
            => TouchMoved?.Invoke(this, ToCoreArgs(e));

        private void OnListenerTouchCancelled(object? sender, MonoGame.Extended.Input.InputListeners.TouchEventArgs e)
            => TouchCancelled?.Invoke(this, ToCoreArgs(e));

        private static TouchEventArgs ToCoreArgs(MonoGame.Extended.Input.InputListeners.TouchEventArgs e)
        {
            return new TouchEventArgs(
                time: e.Time,
                id: e.RawTouchLocation.Id,
                position: e.RawTouchLocation.Position,
                state: e.RawTouchLocation.State);
        }
    }
}
