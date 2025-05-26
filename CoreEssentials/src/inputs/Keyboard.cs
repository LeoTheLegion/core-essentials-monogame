// CoreEssentials/src/inputs/Keyboard.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using System;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Wraps the MonoGame.Extended KeyboardListener to provide enhanced polling capabilities
    /// and a testable interface via IKeyboardStateProvider.
    /// </summary>
    public class Keyboard
    {
        private readonly IKeyboardStateProvider _keyboardStateProvider;
        private readonly KeyboardListener _keyboardListener;

        private KeyboardState _previousState;
        private KeyboardState _currentState;

        /// <summary>
        /// Occurs when a key is pressed.
        /// This event is forwarded from the underlying MonoGame.Extended.Input.InputListeners.KeyboardListener.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyPressed
        {
            add => _keyboardListener.KeyPressed += value;
            remove => _keyboardListener.KeyPressed -= value;
        }

        /// <summary>
        /// Occurs when a key is released.
        /// This event is forwarded from the underlying MonoGame.Extended.Input.InputListeners.KeyboardListener.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyReleased
        {
            add => _keyboardListener.KeyReleased += value;
            remove => _keyboardListener.KeyReleased -= value;
        }

        // If KeyboardListener has other events like KeyTyped, they can be exposed similarly.
        // public event EventHandler<KeyTypedEventArgs> KeyTyped ...

        /// <summary>
        /// Initializes a new instance of the <see cref="Keyboard"/> class.
        /// </summary>
        /// <param name="stateProvider">The keyboard state provider to use for polling. 
        /// If null, a <see cref="DefaultKeyboardStateProvider"/> will be used.</param>
        public Keyboard(IKeyboardStateProvider stateProvider = null) 
        {
            _keyboardListener = new KeyboardListener(); 
            _keyboardStateProvider = stateProvider ?? new DefaultKeyboardStateProvider();
            
            _currentState = _keyboardStateProvider.GetState();
            _previousState = _currentState;
        }

        /// <summary>
        /// Updates the keyboard state. This should be called once per frame.
        /// </summary>
        /// <param name="gameTime">The current game time.</param>
        public virtual void Update(GameTime gameTime)
        {
            _previousState = _currentState;
            _currentState = _keyboardStateProvider.GetState();

            // Update the internal MonoGame.Extended KeyboardListener.
            // This is important for the KeyPressed/KeyReleased events to fire.
            _keyboardListener.Update(gameTime);
        }

        /// <summary>
        /// Checks if a specific key is currently held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is down, false otherwise.</returns>
        public bool IsKeyDown(Keys key)
        {
            return _currentState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a specific key is currently up.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is up, false otherwise.</returns>
        public bool IsKeyUp(Keys key)
        {
            return _currentState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a key was just pressed in the current frame (was up last frame, is down now).
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key was pressed once, false otherwise.</returns>
        public bool IsKeyPressedOnce(Keys key)
        {
            return _currentState.IsKeyDown(key) && _previousState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a key was just released in the current frame (was down last frame, is up now).
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key was released once, false otherwise.</returns>
        public bool IsKeyReleasedOnce(Keys key)
        {
            return _currentState.IsKeyUp(key) && _previousState.IsKeyDown(key);
        }
    }
}
