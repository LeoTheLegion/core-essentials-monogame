// CoreEssentials/src/inputs/DefaultKeyboardStateProvider.cs
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Inputs
{
    public class DefaultKeyboardStateProvider : IKeyboardStateProvider
    {
        public KeyboardState GetState()
        {
            return Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }
    }
}
