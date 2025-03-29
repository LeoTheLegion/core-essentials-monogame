

namespace CoreEssentials.Debugging
{
    public class Debug
    {
        public static BaseGameDiagnostics baseGameDiagnostics { get; private set; } 
        public static Primitives Primitives { get; private set; }

        public static StickyLog StickyLog { get; private set; }

        public static Console Console { get; private set; }

        static Debug()
        {
            Primitives = new Primitives();
            StickyLog = new StickyLog();
            baseGameDiagnostics = new BaseGameDiagnostics(StickyLog);
            Console = new Console();
        }              
    }
}
