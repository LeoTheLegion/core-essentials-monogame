// CoreEssentials/src/inputs/Mouse.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Wraps the MonoGame.Extended <c>MouseListener</c> to provide a clean, CoreEssentials-owned
    /// mouse API. Consumers do not need to reference any <c>MonoGame.Extended</c> namespaces:
    /// events use the CoreEssentials <see cref="MouseEventArgs"/> (with a viewport-independent
    /// <see cref="Vector2">Position</see>) and polling uses the CoreEssentials
    /// <see cref="MouseButton"/> enum.
    /// </summary>
    public class Mouse
    {
        private readonly IMouseStateProvider _mouseStateProvider;
        private readonly MouseListener _mouseListener;

        private MouseState _previousState;
        private MouseState _currentState;

        /// <summary>
        /// Occurs when a mouse button is pressed down.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseDown;

        /// <summary>
        /// Occurs when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseUp;

        /// <summary>
        /// Occurs when a mouse button is clicked (pressed and released without moving beyond
        /// the drag threshold, and not part of a double click).
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseClicked;

        /// <summary>
        /// Occurs when a mouse button is double-clicked.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseDoubleClicked;

        /// <summary>
        /// Occurs when the mouse moves between frames.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseMoved;

        /// <summary>
        /// Occurs when the mouse scroll wheel value changes.
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseWheelMoved;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mouse"/> class.
        /// </summary>
        /// <param name="stateProvider">The mouse state provider to use for polling.
        /// If null, a <see cref="DefaultMouseStateProvider"/> will be used.</param>
        public Mouse(IMouseStateProvider? stateProvider = null)
        {
            _mouseListener = new MouseListener();
            _mouseStateProvider = stateProvider ?? new DefaultMouseStateProvider();

            _currentState = _mouseStateProvider.GetState();
            _previousState = _currentState;

            // Subscribe once to the underlying listener and re-raise with CoreEssentials args.
            _mouseListener.MouseDown += OnListenerMouseDown;
            _mouseListener.MouseUp += OnListenerMouseUp;
            _mouseListener.MouseClicked += OnListenerMouseClicked;
            _mouseListener.MouseDoubleClicked += OnListenerMouseDoubleClicked;
            _mouseListener.MouseMoved += OnListenerMouseMoved;
            _mouseListener.MouseWheelMoved += OnListenerMouseWheelMoved;
        }

        /// <summary>
        /// Updates the mouse state. This should be called once per frame (e.g. via
        /// <see cref="Input.Update"/>).
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        public virtual void Update(GameTime gameTime)
        {
            _previousState = _currentState;
            _currentState = _mouseStateProvider.GetState();

            // Update the internal MonoGame.Extended MouseListener.
            // This is important for the events to fire.
            _mouseListener.Update(gameTime);
        }

        /// <summary>
        /// Gets the current mouse position in pixels. This is viewport-independent and does not
        /// require any viewport adapter to be configured on the underlying listener.
        /// </summary>
        public Vector2 Position => new Vector2(_currentState.X, _currentState.Y);

        /// <summary>
        /// Checks if a specific mouse button is currently held down.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is down, false otherwise.</returns>
        public bool IsButtonDown(MouseButton button)
        {
            return GetButtonState(_currentState, button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if a specific mouse button is currently up.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is up, false otherwise.</returns>
        public bool IsButtonUp(MouseButton button)
        {
            return GetButtonState(_currentState, button) == ButtonState.Released;
        }

        /// <summary>
        /// Checks if a mouse button was just pressed in the current frame (was up last frame, is down now).
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button was pressed once, false otherwise.</returns>
        public bool IsButtonPressedOnce(MouseButton button)
        {
            return GetButtonState(_currentState, button) == ButtonState.Pressed &&
                   GetButtonState(_previousState, button) == ButtonState.Released;
        }

        /// <summary>
        /// Checks if a mouse button was just released in the current frame (was down last frame, is up now).
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button was released once, false otherwise.</returns>
        public bool IsButtonReleasedOnce(MouseButton button)
        {
            return GetButtonState(_currentState, button) == ButtonState.Released &&
                   GetButtonState(_previousState, button) == ButtonState.Pressed;
        }

        private static ButtonState GetButtonState(MouseState state, MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => state.LeftButton,
                MouseButton.Right => state.RightButton,
                MouseButton.Middle => state.MiddleButton,
                MouseButton.XButton1 => state.XButton1,
                MouseButton.XButton2 => state.XButton2,
                _ => ButtonState.Released
            };
        }

        // ---- Underlying listener event forwarding (converted to CoreEssentials args) ----

        private void OnListenerMouseDown(object? sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
            => MouseDown?.Invoke(this, ToCoreArgs(e));

        private void OnListenerMouseUp(object? sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
            => MouseUp?.Invoke(this, ToCoreArgs(e));

        private void OnListenerMouseClicked(object? sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
            => MouseClicked?.Invoke(this, ToCoreArgs(e));

        private void OnListenerMouseDoubleClicked(object? sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
            => MouseDoubleClicked?.Invoke(this, ToCoreArgs(e));

        private void OnListenerMouseMoved(object? sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
            => MouseMoved?.Invoke(this, ToCoreArgs(e));

        private void OnListenerMouseWheelMoved(object? sender, MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
            => MouseWheelMoved?.Invoke(this, ToCoreArgs(e));

        private static MouseEventArgs ToCoreArgs(MonoGame.Extended.Input.InputListeners.MouseEventArgs e)
        {
            return new MouseEventArgs(
                time: e.Time,
                previousPosition: new Vector2(e.PreviousState.X, e.PreviousState.Y),
                position: new Vector2(e.Position.X, e.Position.Y),
                button: ToCoreButton(e.Button),
                scrollWheelValue: e.ScrollWheelValue,
                scrollWheelDelta: e.ScrollWheelDelta);
        }

        private static MouseButton ToCoreButton(MonoGame.Extended.Input.MouseButton button)
        {
            return button switch
            {
                MonoGame.Extended.Input.MouseButton.Left => CoreEssentials.Inputs.MouseButton.Left,
                MonoGame.Extended.Input.MouseButton.Right => CoreEssentials.Inputs.MouseButton.Right,
                MonoGame.Extended.Input.MouseButton.Middle => CoreEssentials.Inputs.MouseButton.Middle,
                MonoGame.Extended.Input.MouseButton.XButton1 => CoreEssentials.Inputs.MouseButton.XButton1,
                MonoGame.Extended.Input.MouseButton.XButton2 => CoreEssentials.Inputs.MouseButton.XButton2,
                _ => CoreEssentials.Inputs.MouseButton.None
            };
        }
    }
}
