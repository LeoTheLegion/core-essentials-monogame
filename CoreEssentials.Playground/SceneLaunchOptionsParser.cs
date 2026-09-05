using System;
using System.Globalization;

namespace CoreEssentials.Playground;

/// <summary>
/// The result of parsing the playground's command-line arguments: which scene to launch and,
/// optionally, how long (in seconds) to keep running before auto-exiting.
/// </summary>
public sealed class SceneLaunchOptions
{
    /// <summary>
    /// Gets the name of the scene XML asset to load (e.g., <c>"Scenes/HomeScene.xml"</c>).
    /// </summary>
    public string Scene { get; }

    /// <summary>
    /// Gets the optional number of seconds to run before auto-exiting. Null means run indefinitely.
    /// </summary>
    public double? RunForSeconds { get; }

    /// <summary>
    /// Gets whether focus changes should be ignored for pausing purposes (true) or handled normally
    /// (false, the default). When true, the game does not pause audio/systems when the window loses
    /// focus — useful for unattended smoke-runs where the window may never hold foreground.
    /// </summary>
    public bool NoFocusPause { get; }

    internal SceneLaunchOptions(string scene, double? runForSeconds, bool noFocusPause)
    {
        Scene = scene;
        RunForSeconds = runForSeconds;
        NoFocusPause = noFocusPause;
    }
}

/// <summary>
/// Parses the playground's command-line arguments. Supports three options:
/// <list type="bullet">
/// <item><c>--scene &lt;file&gt;</c> — the scene XML asset to launch (defaults to <c>"Scenes/HomeScene.xml"</c>).</item>
/// <item><c>--run-for &lt;seconds&gt;</c> — how long to keep running before auto-exiting (optional; default is to run indefinitely).</item>
/// <item><c>--no-focus-pause</c> — ignore window focus changes for pausing, so audio keeps playing even when the window is unfocused (a flag with no value; useful for unattended smoke-runs).</item>
/// </list>
/// Unknown arguments are ignored (with a console note) so the parser stays forgiving. A recognized
/// option that is missing its value is an error and throws <see cref="ArgumentException"/>.
/// This is a pure, side-effect-free parse so it can be unit-tested without launching a game window.
/// </summary>
public static class SceneLaunchOptionsParser
{
    /// <summary>The scene launched when no <c>--scene</c> argument is supplied.</summary>
    public const string DefaultScene = "Scenes/HomeScene.xml";

    private const string SceneFlag = "--scene";
    private const string RunForFlag = "--run-for";
    private const string NoFocusPauseFlag = "--no-focus-pause";

    /// <summary>
    /// Parses the given command-line arguments into launch options.
    /// </summary>
    /// <param name="args">The raw command-line arguments (may be null or empty).</param>
    /// <returns>The parsed launch options.</returns>
    /// <exception cref="ArgumentException">Thrown when a recognized option is missing its value, or when <c>--run-for</c> is not a positive number.</exception>
    public static SceneLaunchOptions Parse(string[]? args)
    {
        string scene = DefaultScene;
        double? runForSeconds = null;
        bool noFocusPause = false;

        if (args == null || args.Length == 0)
            return new SceneLaunchOptions(scene, runForSeconds, noFocusPause);

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg == SceneFlag)
            {
                scene = ReadValue(args, ref i, SceneFlag);
            }
            else if (arg == RunForFlag)
            {
                runForSeconds = ParseRunFor(ReadValue(args, ref i, RunForFlag));
            }
            else if (arg == NoFocusPauseFlag)
            {
                noFocusPause = true;
            }
            else
            {
                Console.WriteLine($"[Playground] Ignoring unrecognized argument: '{arg}'");
            }
        }

        return new SceneLaunchOptions(scene, runForSeconds, noFocusPause);
    }

    private static string ReadValue(string[] args, ref int i, string flag)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"The '{flag}' option requires a value.");

        i++;
        return args[i];
    }

    private static double ParseRunFor(string raw)
    {
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds) || seconds <= 0)
            throw new ArgumentException($"The '--run-for' option must be a positive number of seconds. Got: '{raw}'.");

        return seconds;
    }
}
