using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;

namespace CoreEssentials.Inputs
{
    public static class Input
    {
        public static TouchListener Touch { private set; get; }
        public static KeyboardListener Keyboard { private set; get; }
        public static MouseListener Mouse { private set; get; }

        static Input()
        {
            Touch = new TouchListener();
            Keyboard = new KeyboardListener();
            Mouse = new MouseListener();
        }

        public static void Update(GameTime gameTime)
        {
            Touch.Update(gameTime);
            Keyboard.Update(gameTime);
            Mouse.Update(gameTime);
        }
    }
}
