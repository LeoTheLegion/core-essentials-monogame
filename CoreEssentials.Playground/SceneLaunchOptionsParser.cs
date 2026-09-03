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
    /// Gets the name of the scene XML asset to load (e.g., <c>"HomeScene.xml"</c>).
    /// </summary>
    public string Scene { get; }

    /// <summary>
    /// Gets the optional number of seconds to run before auto-exiting. Null means run indefinitely.
    /// </summary>
    public double? RunForSeconds { get; }

    internal SceneLaunchOptions(string scene, double? runForSeconds)
    {
        Scene = scene;
        RunForSeconds = runForSeconds;
    }
}

/// <summary>
/// Parses the playground's command-line arguments. Supports two options:
/// <list type="bullet">
/// <item><c>--scene &lt;file&gt;</c> — the scene XML asset to launch (defaults to <c>"HomeScene.xml"</c>).</item>
/// <item><c>--run-for &lt;seconds&gt;</c> — how long to keep running before auto-exiting (optional; default is to run indefinitely).</item>
/// </list>
/// Unknown arguments are ignored (with a console note) so the parser stays forgiving. A recognized
/// option that is missing its value is an error and throws <see cref="ArgumentException"/>.
/// This is a pure, side-effect-free parse so it can be unit-tested without launching a game window.
/// </summary>
public static class SceneLaunchOptionsParser
{
    /// <summary>The scene launched when no <c>--scene</c> argument is supplied.</summary>
    public const string DefaultScene = "HomeScene.xml";

    private const string SceneFlag = "--scene";
    private const string RunForFlag = "--run-for";

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

        if (args == null || args.Length == 0)
            return new SceneLaunchOptions(scene, runForSeconds);

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
            else
            {
                Console.WriteLine($"[Playground] Ignoring unrecognized argument: '{arg}'");
            }
        }

        return new SceneLaunchOptions(scene, runForSeconds);
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
