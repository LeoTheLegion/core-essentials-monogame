using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System.Collections.Generic;
using CoreEssentials.Inputs;
using MonoGame.Extended.Input.InputListeners;
using CoreEssentials.GUI;
using System;

namespace CoreEssentials.Debugging
{
    /// <summary>
    /// Provides an interactive in-game console for debugging and runtime commands.
    /// The console allows developers to view logs and execute commands during gameplay.
    /// </summary>
    [Obsolete("Use System.Console instead.")]
    /// <remarks>This class is marked as obsolete and should not be used in new code.</remarks>
    public class Console
    {

        /// <summary>
        /// Initializes a new instance of the Console class.
        /// Sets up key handlers for toggling visibility.
        /// </summary>
        public Console() { 
        }

        /// <summary>
        /// Finalizes an instance of the Console class.
        /// Removes key handlers to prevent memory leaks.
        /// </summary>
        ~Console() {
        }
        /// <summary>
        /// Writes a message to the console log.
        /// </summary>
        /// <param name="line">The message to write.</param>
        [Obsolete("Use System.Console.WriteLine instead.")]
        /// <remarks>This method is marked as obsolete and should not be used in new code.</remarks>
        public void WriteLine(string line)
        {
            System.Console.WriteLine(line);
        }
    }
}
