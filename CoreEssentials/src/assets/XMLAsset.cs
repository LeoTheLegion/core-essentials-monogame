using System;

namespace CoreEssentials.Assets;

public class XMLAsset : Asset
{
    private string _xmlContent;

    public string XMLContent => _xmlContent;

    public XMLAsset(string name) : base(name)
    {
    }

    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        var exePath = AppContext.BaseDirectory;
        var filePath = System.IO.Path.Combine(exePath,"Content",_assetName);

        _xmlContent = System.IO.File.ReadAllText(filePath);
    }

    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _xmlContent = null;
    }
}
