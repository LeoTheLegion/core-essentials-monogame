// CoreEssentials/src/inputs/MouseButton.cs
namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Represents a mouse button in a CoreEssentials-owned type, so consumers do not need to
    /// reference the underlying <c>MonoGame.Extended</c> namespaces.
    /// </summary>
    public enum MouseButton
    {
        /// <summary>No button.</summary>
        None = 0,

        /// <summary>The primary (left) button.</summary>
        Left = 1,

        /// <summary>The secondary (right) button.</summary>
        Right = 2,

        /// <summary>The middle (auxiliary/wheel) button.</summary>
        Middle = 3,

        /// <summary>The first extra side button.</summary>
        XButton1 = 4,

        /// <summary>The second extra side button.</summary>
        XButton2 = 5
    }
}
