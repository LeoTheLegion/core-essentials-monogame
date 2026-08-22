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
        /// Gets the keyboard input handler with events for key presses and releases, and polling methods.
        /// </summary>
        public static Keyboard Keyboard { private set; get; } // Changed type to CoreEssentials.Inputs.Keyboard

        /// <summary>
        /// Gets the mouse input handler with events for mouse movement and button presses,
        /// as well as polling methods. This is a CoreEssentials-owned wrapper that does not
        /// require consumers to reference MonoGame.Extended namespaces.
        /// </summary>
        public static Mouse Mouse { private set; get; }

        static Input()
        {
            Touch = new TouchListener();
            Keyboard = new Keyboard(); // Changed to instantiate our new Keyboard class
            Mouse = new Mouse(); // Changed to instantiate our new Mouse wrapper class
        }

        /// <summary>
        /// Updates all input handlers with the latest input states.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Update(GameTime gameTime)
        {
            Touch.Update(gameTime);
            Keyboard.Update(gameTime); // This will now call Keyboard.Update()
            Mouse.Update(gameTime);
        }
    }
}
