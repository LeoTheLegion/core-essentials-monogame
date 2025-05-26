// CoreEssentials/src/inputs/IKeyboardStateProvider.cs
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Inputs
{
    public interface IKeyboardStateProvider
    {
        KeyboardState GetState();
    }
}
