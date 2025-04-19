namespace CoreEssentials.Debugging
{
    /// <summary>
    /// Central access point for debugging functionality in the CoreEssentials framework.
    /// Provides static access to various debugging tools including logging, performance monitoring, and interactive console.
    /// </summary>
    public class Debug
    {
        /// <summary>
        /// Gets the base game diagnostics instance for tracking performance metrics.
        /// </summary>
        public static BaseGameDiagnostics baseGameDiagnostics { get; private set; } 

        /// <summary>
        /// Gets the primitives instance for drawing basic shapes and objects.
        /// </summary>
        public static Primitives Primitives { get; private set; }

        /// <summary>
        /// Gets the sticky log instance for displaying persistent debug information on screen.
        /// </summary>
        public static StickyLog StickyLog { get; private set; }

        /// <summary>
        /// Gets the console instance for interactive debugging commands and messages.
        /// </summary>
        public static Console Console { get; private set; }

        /// <summary>
        /// Static constructor that initializes the debugging system components.
        /// </summary>
        static Debug()
        {
            Primitives = new Primitives();
            StickyLog = new StickyLog();
            baseGameDiagnostics = new BaseGameDiagnostics(StickyLog);
            Console = new Console();
        }              
    }
}
