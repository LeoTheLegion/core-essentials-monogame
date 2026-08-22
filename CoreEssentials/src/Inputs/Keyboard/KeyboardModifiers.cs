// CoreEssentials/src/inputs/KeyboardModifiers.cs
namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Bit flags indicating which modifier keys (Control, Shift, Alt) are held down at the time
    /// of a keyboard event. This is a CoreEssentials-owned type, so consumers do not need to
    /// reference any <c>MonoGame.Extended</c> namespaces.
    /// </summary>
    [System.Flags]
    public enum KeyboardModifiers
    {
        /// <summary>No modifier keys.</summary>
        None = 0,

        /// <summary>The Control key is held down.</summary>
        Control = 1,

        /// <summary>The Shift key is held down.</summary>
        Shift = 2,

        /// <summary>The Alt key is held down.</summary>
        Alt = 4
    }
}
