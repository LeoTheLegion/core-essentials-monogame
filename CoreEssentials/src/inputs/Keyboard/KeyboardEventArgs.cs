// CoreEssentials/src/inputs/KeyboardEventArgs.cs
using System;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Inputs
{
    /// <summary>
    /// Event arguments for keyboard events raised by the CoreEssentials <see cref="Keyboard"/> wrapper.
    /// This is a CoreEssentials-owned type, so consumers do not need to reference any
    /// <c>MonoGame.Extended</c> namespaces to handle keyboard input.
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardEventArgs"/> class.
        /// </summary>
        /// <param name="key">The key that was pressed or released.</param>
        /// <param name="modifiers">The modifier keys held down at the time of the event.</param>
        public KeyboardEventArgs(Keys key, KeyboardModifiers modifiers = KeyboardModifiers.None)
        {
            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Gets the key that was pressed or released.
        /// </summary>
        public Keys Key { get; }

        /// <summary>
        /// Gets the modifier keys (Control, Shift, Alt) held down at the time of the event.
        /// </summary>
        public KeyboardModifiers Modifiers { get; }

        /// <summary>
        /// Gets a value indicating whether the Control key was held down.
        /// </summary>
        public bool IsControl => (Modifiers & KeyboardModifiers.Control) == KeyboardModifiers.Control;

        /// <summary>
        /// Gets a value indicating whether the Shift key was held down.
        /// </summary>
        public bool IsShift => (Modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift;

        /// <summary>
        /// Gets a value indicating whether the Alt key was held down.
        /// </summary>
        public bool IsAlt => (Modifiers & KeyboardModifiers.Alt) == KeyboardModifiers.Alt;

        /// <summary>
        /// Gets the printable character corresponding to this key and modifier state, or
        /// <see langword="null"/> if the key does not produce a character (e.g. arrows, F-keys).
        /// </summary>
        public char? Character => ToChar(Key, Modifiers);

        private static char? ToChar(Keys key, KeyboardModifiers modifiers)
        {
            bool shift = (modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift;

            if (key >= Keys.A && key <= Keys.Z)
                return (char)((int)(shift ? 'A' : 'a') + (int)(key - Keys.A));

            switch (key)
            {
                case Keys.D0: return shift ? ')' : '0';
                case Keys.NumPad0: return '0';
                case Keys.D1: return shift ? '!' : '1';
                case Keys.NumPad1: return '1';
                case Keys.D2: return shift ? '@' : '2';
                case Keys.NumPad2: return '2';
                case Keys.D3: return shift ? '#' : '3';
                case Keys.NumPad3: return '3';
                case Keys.D4: return shift ? '$' : '4';
                case Keys.NumPad4: return '4';
                case Keys.D5: return shift ? '%' : '5';
                case Keys.NumPad5: return '5';
                case Keys.D6: return shift ? '^' : '6';
                case Keys.NumPad6: return '6';
                case Keys.D7: return shift ? '&' : '7';
                case Keys.NumPad7: return '7';
                case Keys.D8: return shift ? '*' : '8';
                case Keys.NumPad8: return '8';
                case Keys.D9: return shift ? '(' : '9';
                case Keys.NumPad9: return '9';

                case Keys.Space: return ' ';
                case Keys.Tab: return '\t';
                case Keys.Enter: return (char)13;
                case Keys.Back: return (char)8;

                case Keys.Add: return '+';
                case Keys.Decimal: return '.';
                case Keys.Divide: return '/';
                case Keys.Multiply: return '*';
                case Keys.Subtract: return '-';

                case Keys.OemBackslash: return '\\';
                case Keys.OemComma: return shift ? '<' : ',';
                case Keys.OemOpenBrackets: return shift ? '{' : '[';
                case Keys.OemCloseBrackets: return shift ? '}' : ']';
                case Keys.OemPeriod: return shift ? '>' : '.';
                case Keys.OemPipe: return shift ? '|' : '\\';
                case Keys.OemPlus: return shift ? '+' : '=';
                case Keys.OemMinus: return shift ? '_' : '-';
                case Keys.OemQuestion: return shift ? '?' : '/';
                case Keys.OemQuotes: return shift ? '"' : '\'';
                case Keys.OemSemicolon: return shift ? ':' : ';';
                case Keys.OemTilde: return shift ? '~' : '`';

                default: return null;
            }
        }
    }
}
