using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreEssentials.Scenes;

namespace CoreEssentials.Tests.SceneManagement;

/// <summary>
/// Builds a <see cref="SceneManifest"/> for test fixtures in one line, so migrated tests can supply
/// their scene list without hand-writing XML. Game scenes are added in the given order (index 0 = startup);
/// each may name its own loading screen via <c>LoadingScreen</c>. A single default loading screen and any
/// number of extra (non-default) loading screens complete the registry.
/// </summary>
public static class SceneManifestFixture
{
    /// <summary>A game-scene entry: its asset name plus an optional per-scene loading screen.</summary>
    public readonly record struct GameScene(string Name, string? LoadingScreen = null);

    /// <summary>
    /// Builds a manifest from the given game scenes (in order) and loading screens.
    /// </summary>
    /// <param name="gameScenes">The ordered game scenes; index 0 becomes the startup scene.</param>
    /// <param name="defaultLoadingScreen">Optional default loading screen used by any game scene that does not name one.</param>
    /// <param name="extraLoadingScreens">Optional additional (non-default) loading screens.</param>
    public static SceneManifest Build(
        IEnumerable<GameScene> gameScenes,
        string? defaultLoadingScreen = null,
        IEnumerable<string>? extraLoadingScreens = null)
    {
        var scenes = gameScenes.ToList();
        if (scenes.Count == 0)
            throw new System.ArgumentException("At least one game scene is required.", nameof(gameScenes));

        var sb = new StringBuilder();
        sb.Append("<Scenes><GameScenes>");
        foreach (var g in scenes)
        {
            sb.Append($"<Scene Name=\"{g.Name}\"");
            if (!string.IsNullOrEmpty(g.LoadingScreen))
                sb.Append($" LoadingScreen=\"{g.LoadingScreen}\"");
            sb.Append(" />");
        }
        sb.Append("</GameScenes>");

        var extras = extraLoadingScreens?.ToList() ?? new List<string>();
        if (defaultLoadingScreen != null || extras.Count > 0)
        {
            sb.Append("<LoadingScenes>");
            if (defaultLoadingScreen != null)
                sb.Append($"<LoadingScene Name=\"{defaultLoadingScreen}\" Default=\"true\" />");
            foreach (var l in extras.Where(l => l != defaultLoadingScreen))
                sb.Append($"<LoadingScene Name=\"{l}\" />");
            sb.Append("</LoadingScenes>");
        }

        sb.Append("</Scenes>");
        return SceneManifest.Parse(sb.ToString());
    }
}
