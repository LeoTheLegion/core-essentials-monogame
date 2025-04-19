using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Provides a centralized system for handling various input methods including touch, keyboard, and mouse.
    /// This class manages input state tracking and exposes events for input changes.
    /// </summary>
    public static class Input
    {
        /// <summary>
        /// Gets the touch input handler with events for touch gestures and interactions.
        /// </summary>
        public static TouchListener Touch { private set; get; }

        /// <summary>
        /// Gets the keyboard input handler with events for key presses and releases.
        /// </summary>
        public static KeyboardListener Keyboard { private set; get; }

        /// <summary>
        /// Gets the mouse input handler with events for mouse movement and button presses.
        /// </summary>
        public static MouseListener Mouse { private set; get; }

        static Input()
        {
            Touch = new TouchListener();
            Keyboard = new KeyboardListener();
            Mouse = new MouseListener();
        }

        /// <summary>
        /// Updates all input handlers with the latest input states.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Update(GameTime gameTime)
        {
            Touch.Update(gameTime);
            Keyboard.Update(gameTime);
            Mouse.Update(gameTime);
        }
    }
}
