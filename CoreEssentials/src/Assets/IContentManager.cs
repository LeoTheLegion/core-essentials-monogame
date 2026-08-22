using System;

namespace CoreEssentials.Assets;

/// <summary>
/// Defines an interface for a content manager that can load and unload assets.
/// </summary>
public interface IContentManager
{
    // This interface is a placeholder for the ContentManager class in XNA/MonoGame.
    // It should be implemented by any class that needs to manage content loading and unloading.
    // The actual implementation would depend on the specific requirements of the game engine.

    // Example methods that might be included in a content manager interface:

    /// <summary>
    /// Loads an asset of the specified type and name.
    /// </summary>
    /// <typeparam name="T">The type of asset to load.</typeparam>
    /// <param name="assetName">The name of the asset to load.</param>
    /// <returns>The loaded asset.</returns>
    public T Load<T>(string assetName);

    /// <summary>
    /// Unloads the asset with the specified name.
    /// </summary>
    /// <param name="assetName">The name of the asset to unload.</param>
    public void Unload(string assetName);
}
