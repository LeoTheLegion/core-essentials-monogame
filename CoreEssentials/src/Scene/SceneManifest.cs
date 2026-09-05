using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CoreEssentials.Scenes;

/// <summary>A single entry in the manifest's ordered <c>&lt;GameScenes&gt;</c> list.</summary>
/// <param name="Name">The scene asset name (e.g. "HomeScene.xml").</param>
/// <param name="LoadingScreen">Optional loading screen to show when transitioning into this scene; falls back to the default loading screen when null.</param>
public sealed record SceneEntry(string Name, string? LoadingScreen);

/// <summary>A single entry in the manifest's <c>&lt;LoadingScenes&gt;</c> registry.</summary>
/// <param name="Name">The loading-screen asset name (e.g. "loading.xml").</param>
/// <param name="IsDefault">Whether this is the default loading screen used when a game scene does not name one.</param>
public sealed record LoadingSceneEntry(string Name, bool IsDefault);

/// <summary>
/// The two-list scene manifest — the single source of truth for what a game contains.
/// Root &lt;Scenes&gt; holds an ordered &lt;GameScenes&gt; list (position = navigation order;
/// first entry = startup scene) and an optional &lt;LoadingScenes&gt; registry. Unknown
/// elements or attributes are parse errors that name the offender, so typos fail fast.
/// </summary>
public sealed class SceneManifest
{
    private readonly Dictionary<string, int> _gameSceneIndex = new(StringComparer.Ordinal);

    /// <summary>The ordered game scenes; index 0 is the startup scene.</summary>
    public IReadOnlyList<SceneEntry> GameScenes { get; }

    /// <summary>The registered loading screens (empty when the manifest declares none).</summary>
    public IReadOnlyList<LoadingSceneEntry> LoadingScenes { get; }

    private SceneManifest(IReadOnlyList<SceneEntry> gameScenes, IReadOnlyList<LoadingSceneEntry> loadingScenes)
    {
        GameScenes = gameScenes;
        LoadingScenes = loadingScenes;
        for (var i = 0; i < gameScenes.Count; i++)
            _gameSceneIndex[gameScenes[i].Name] = i;
    }

    /// <summary>The startup scene — the first entry in <c>&lt;GameScenes&gt;</c>.</summary>
    public string StartupScene => GameScenes[0].Name;

    /// <summary>The loading screen marked <c>Default="true"</c>, or null when none is declared.</summary>
    public string? DefaultLoadingScene => LoadingScenes.FirstOrDefault(l => l.IsDefault)?.Name;

    /// <summary>Returns the position of a game scene in the list, or -1 when it is not listed.</summary>
    public int IndexOf(string name) => _gameSceneIndex.TryGetValue(name, out var index) ? index : -1;

    /// <summary>The position to move to from <paramref name="index"/> via "next"; clamped at the last entry.</summary>
    public int NextOf(int index) => Math.Min(index + 1, GameScenes.Count - 1);

    /// <summary>The position to move to from <paramref name="index"/> via "previous"; clamped at the first entry.</summary>
    public int PreviousOf(int index) => Math.Max(index - 1, 0);

    /// <summary>
    /// Resolves the loading screen for a transition into <paramref name="sceneName"/>:
    /// the scene's own <c>LoadingScreen</c> attribute, else the default loading screen, else null.
    /// Returns null for scenes that are not in the manifest.
    /// </summary>
    public string? LoadingScreenFor(string sceneName)
    {
        if (IndexOf(sceneName) < 0) return null;
        var explicitScreen = GameScenes[_gameSceneIndex[sceneName]].LoadingScreen;
        return explicitScreen ?? DefaultLoadingScene;
    }

    /// <summary>Attributes allowed on the manifest's elements.</summary>
    private static readonly HashSet<string> EmptySet = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> SceneAttributes = new(StringComparer.OrdinalIgnoreCase) { "Name", "LoadingScreen" };
    private static readonly HashSet<string> LoadingSceneAttributes = new(StringComparer.OrdinalIgnoreCase) { "Name", "Default" };

    /// <summary>Parses a scene manifest from an XML string.</summary>
    /// <exception cref="FormatException">Thrown when the document violates the manifest schema.</exception>
    public static SceneManifest Parse(string xmlData)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(xmlData);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new FormatException($"Scene manifest XML is malformed: {ex.Message}", ex);
        }

        var root = doc.Root ?? throw new FormatException("Scene manifest XML has no root element.");
        ExpectElementName(root, "Scenes");
        RejectUnknownAttributes(root, EmptySet);

        List<SceneEntry>? gameScenes = null;
        List<LoadingSceneEntry>? loadingScenes = null;

        foreach (var child in root.Elements())
        {
            switch (child.Name.LocalName)
            {
                case "GameScenes":
                    if (gameScenes != null)
                        throw new FormatException("<Scenes> must contain at most one <GameScenes> element.");
                    gameScenes = ParseGameScenes(child);
                    break;
                case "LoadingScenes":
                    if (loadingScenes != null)
                        throw new FormatException("<Scenes> must contain at most one <LoadingScenes> element.");
                    loadingScenes = ParseLoadingScenes(child);
                    break;
                default:
                    throw new FormatException($"Unknown element <{child.Name.LocalName}> under <Scenes>; expected <GameScenes> or <LoadingScenes>.");
            }
        }

        if (gameScenes == null)
            throw new FormatException("<Scenes> must contain a <GameScenes> element with at least one <Scene>.");

        var manifest = new SceneManifest(gameScenes, loadingScenes ?? new List<LoadingSceneEntry>());

        // Cross-list validation: every per-scene LoadingScreen attribute must reference a declared loading screen.
        foreach (var scene in manifest.GameScenes)
        {
            if (scene.LoadingScreen != null && !manifest.LoadingScenes.Any(l => l.Name == scene.LoadingScreen))
                throw new FormatException($"Scene '{scene.Name}' references loading screen '{scene.LoadingScreen}', which is not declared in <LoadingScenes>.");
        }

        return manifest;
    }

    private static List<SceneEntry> ParseGameScenes(XElement element)
    {
        RejectUnknownAttributes(element, EmptySet);
        var scenes = new List<SceneEntry>();
        foreach (var child in element.Elements())
        {
            ExpectElementName(child, "Scene");
            RejectUnknownAttributes(child, SceneAttributes);

            var name = RequireAttribute(child, "Name", "Scene");
            var loadingScreen = (string?)child.Attribute("LoadingScreen")?.Value;

            if (scenes.Any(s => s.Name == name))
                throw new FormatException($"Duplicate <Scene Name=\"{name}\"> in <GameScenes>.");

            scenes.Add(new SceneEntry(name, loadingScreen));
        }

        if (scenes.Count == 0)
            throw new FormatException("<GameScenes> must contain at least one <Scene>; the first entry is the startup scene.");

        return scenes;
    }

    private static List<LoadingSceneEntry> ParseLoadingScenes(XElement element)
    {
        RejectUnknownAttributes(element, EmptySet);
        var screens = new List<LoadingSceneEntry>();
        foreach (var child in element.Elements())
        {
            ExpectElementName(child, "LoadingScene");
            RejectUnknownAttributes(child, LoadingSceneAttributes);

            var name = RequireAttribute(child, "Name", "LoadingScene");
            var isDefault = ParseBoolAttribute((string?)child.Attribute("Default")?.Value, "Default", name);

            if (screens.Any(s => s.Name == name))
                throw new FormatException($"Duplicate <LoadingScene Name=\"{name}\"> in <LoadingScenes>.");
            if (isDefault && screens.Any(s => s.IsDefault))
                throw new FormatException($"More than one <LoadingScene> is marked Default=\"true\"; only '{screens.Single(s => s.IsDefault).Name}' may be the default.");

            screens.Add(new LoadingSceneEntry(name, isDefault));
        }

        return screens;
    }

    private static void ExpectElementName(XElement element, string expected)
    {
        if (element.Name.LocalName != expected)
            throw new FormatException($"Expected <{expected}> but found <{element.Name.LocalName}>.");
    }

    private static string RequireAttribute(XElement element, string attribute, string context)
    {
        var value = (string?)element.Attribute(attribute)?.Value;
        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException($"<{element.Name.LocalName}> in <{context}> is missing its required '{attribute}' attribute.");
        return value;
    }

    private static bool ParseBoolAttribute(string? raw, string attribute, string context)
    {
        if (raw == null) return false;
        if (bool.TryParse(raw, out var parsed)) return parsed;
        throw new FormatException($"<LoadingScene Name=\"{context}\"> has an invalid '{attribute}' value '{raw}'; expected 'true' or 'false'.");
    }

    private static void RejectUnknownAttributes(XElement element, HashSet<string> allowed)
    {
        foreach (var attribute in element.Attributes())
        {
            if (!allowed.Contains(attribute.Name.LocalName))
                throw new FormatException($"Unknown attribute '{attribute.Name.LocalName}' on <{element.Name.LocalName}>.");
        }
    }
}
