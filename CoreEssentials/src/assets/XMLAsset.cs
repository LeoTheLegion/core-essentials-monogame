using System;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents an asset containing XML configuration data for game resources.
/// </summary>
public class XMLAsset : Asset
{
    private string _xmlContent = "";

    /// <summary>
    /// Gets the raw XML content of this asset.
    /// </summary>
    public string XMLContent => _xmlContent;

    /// <summary>
    /// Initializes a new instance of the XMLAsset class with the specified name.
    /// </summary>
    /// <param name="name">The name/identifier for this asset.</param>
    public XMLAsset(string name) : base(name)
    {
    }

    /// <summary>
    /// Loads the XML content from file into this asset instance.
    /// </summary>
    /// <param name="contentManager">The content manager used to locate and load assets.</param>
    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        var exePath = AppContext.BaseDirectory;
        var filePath = System.IO.Path.Combine(exePath, "Content", _assetName);

        _xmlContent = System.IO.File.ReadAllText(filePath);
    }

    /// <summary>
    /// Unloads the XML asset and clears its cached content.
    /// </summary>
    /// <param name="contentManager">The content manager used for unloading.</param>
    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }
        // Reset the XML content to an empty string instead of null.
        // This satisfies the non‑nullable reference type for `_xmlContent`.
        _xmlContent = "";
    }
}
