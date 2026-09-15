using System;
using System.Globalization;
using CoreEssentials.Playground;
using CoreEssentials.Scenes;


using var game = new CoreEssentials.MainGame();

game.Graphics.PreferredBackBufferWidth = 1280;
game.Graphics.PreferredBackBufferHeight = 720;
game.Graphics.ApplyChanges();

// Command-line harness for smoke-running a scene unattended:
//   --scene <file>       which scene XML to launch (default: the manifest's first <GameScenes> entry)
//   --run-for <seconds>  close the game after N seconds of runtime (default: run indefinitely)
//   --no-focus-pause     keep audio playing even when the window is unfocused (for unattended runs)
var options = SceneLaunchOptionsParser.Parse(args);

if (options.RunForSeconds.HasValue)
{
    Console.WriteLine($"[Playground] Auto-exit enabled: closing after {options.RunForSeconds.Value.ToString(CultureInfo.InvariantCulture)}s.");
    game.EnableAutoExit(options.RunForSeconds.Value);
}

if (options.NoFocusPause)
{
    Console.WriteLine("[Playground] Focus-pause disabled: audio will keep playing even if the window loses focus.");
    game.EnableIgnoreFocusForPause();
}

// Boot purely from data files. The loading screen and the first scene are both strict-format XML
// assets staged into Content/ — no C# LoadingScene or scene subclass. Screen size is set once here
// rather than per-scene. With no --scene argument the game boots the manifest's startup scene (the
// first <GameScenes> entry); --scene overrides that with a specific scene.
//
// The core enforces the scene manifest: every name-based load must reference an entry in scenes.xml,
// and the manifest must be configured before any name-based load. Next/Previous navigation walks the
// manifest's ordered <GameScenes> list.
game.SceneManager.SetManifestAsset("scenes.xml");
game.SceneManager.SetLoadingScene("Scenes/loading.xml");
if (options.Scene is { } scene)
    game.SceneManager.LoadScene(scene);
else
    game.SceneManager.LoadStartupScene();

game.Run();
