using System;
using System.Collections.Generic;
using FontStashSharp;

namespace CoreEssentials.GUI.Fonts;

/// <summary>
/// Loads TrueType/OpenType font files into Myra <see cref="SpriteFontBase"/> instances for use by
/// GUI widgets (labels and buttons). Fonts are read from the game's <c>Content</c> folder as raw
/// bytes — the same location convention used by other file-based assets — and rendered through
/// FontStashSharp, which is the text backend Myra uses. Results are cached per (asset, size) pair.
/// </summary>
public static class GuiFontLoader
{
    private static readonly Dictionary<string, SpriteFontBase> _cache = new();

    /// <summary>
    /// Loads (or returns a cached) Myra font for the given TTF/OTF asset name at the requested size.
    /// </summary>
    /// <param name="assetName">The font file name relative to the game's Content folder (e.g. "Fonts/display.ttf").</param>
    /// <param name="size">The font size in pixels.</param>
    /// <returns>A Myra <see cref="SpriteFontBase"/> that renders the requested typeface.</returns>
    public static SpriteFontBase Load(string assetName, int size)
    {
        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentException("Font asset name cannot be null or empty.", nameof(assetName));

        var key = $"{assetName}:{size}";
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var system = new FontSystem();
        system.AddFont(ReadFontBytes(assetName));
        var font = system.GetFont(size);

        lock (_cache)
        {
            _cache[key] = font;
        }

        return font;
    }

    /// <summary>
    /// Reads the raw bytes of a font file from the game's Content folder.
    /// </summary>
    private static byte[] ReadFontBytes(string assetName)
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Content", assetName);
        if (!System.IO.File.Exists(path))
            throw new FileNotFoundException($"Font file not found: {path}", path);

        return System.IO.File.ReadAllBytes(path);
    }

    /// <summary>
    /// Clears the font cache. Primarily useful for tests.
    /// </summary>
    public static void ClearCache()
    {
        lock (_cache)
        {
            _cache.Clear();
        }
    }
}
