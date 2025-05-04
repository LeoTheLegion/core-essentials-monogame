using System;

namespace CoreEssentials.Assets;

public interface IContentManager
{
    // This interface is a placeholder for the ContentManager class in XNA/MonoGame.
    // It should be implemented by any class that needs to manage content loading and unloading.
    // The actual implementation would depend on the specific requirements of the game engine.

    // Example methods that might be included in a content manager interface:
    public T Load<T>(string assetName);
    public void Unload(string assetName);
}
