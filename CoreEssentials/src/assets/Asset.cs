using System;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

public abstract class Asset
{
    protected string _assetName;

    public Asset(string name)
    {
        _assetName = name;
    }
}
