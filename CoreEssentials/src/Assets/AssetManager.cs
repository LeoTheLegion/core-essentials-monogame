using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEssentials.Assets
{
    /// <summary>
    /// Manages game assets with reference counting for efficient resource management.
    /// Provides centralized loading and unloading of game resources.
    /// </summary>
    public static class AssetManager
    {
        /// <summary>
        /// Dictionary of loaded assets, indexed by a combination of asset name and type.
        /// </summary>
        static Dictionary<string, Asset> assetsLoaded = new Dictionary<string, Asset>();
        
        /// <summary>
        /// Dictionary tracking reference counts for each loaded asset.
        /// </summary>
        static Dictionary<string, int> countOfObjectsUsingAsset = new Dictionary<string, int>();

        /// <summary>
        /// Content manager reference used to load assets from files.
        /// </summary>
        static IContentManager? Content;
        
        /// <summary>
        /// Initializes the AssetManager with a ContentManager.
        /// </summary>
        /// <param name="content">The ContentManager to use for loading assets.</param>
        public static void Init(IContentManager content)
        {
            Content = content;
        }

        /// <summary>
        /// Loads an asset and manages its reference count.
        /// If the asset is already loaded, its reference count is incremented.
        /// </summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The name of the asset to load.</param>
        /// <returns>The loaded asset.</returns>
        public static T LoadAsset<T>(string assetName) where T : Asset
        {
            if(Content == null)
            {
                throw new InvalidOperationException("AssetManager has not been initialized with a ContentManager.");
            }

            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentNullException(nameof(assetName), "Asset name cannot be null or empty.");
            }

            if(!typeof(Asset).IsAssignableFrom(typeof(T))) {
                throw new ArgumentException("Asset type must inherit from Asset.", nameof(T));
            }

            var AssetNameType = typeof(T).Name;
            var AssetKey = assetName + "_" + AssetNameType;

            if (assetsLoaded.ContainsKey(AssetKey))
            {
                countOfObjectsUsingAsset[AssetKey]++;
                return (T)assetsLoaded[AssetKey];
            }

            if (assetName == null || assetName == string.Empty)
            {
                throw new ArgumentNullException("assetName", "Asset name cannot be null or empty.");
            }

            Asset asset;

            if (!typeof(Asset).IsAssignableFrom(typeof(T))) {
                throw new ArgumentException("Asset type must inherit from Asset.", nameof(T));
            }
             
            asset = (Asset?)Activator.CreateInstance(typeof(T), new object[] { assetName })
                ?? throw new InvalidOperationException($"Could not create an instance of asset type {typeof(T).Name} with name '{assetName}'.");
            
            asset.Load(Content);
            
            assetsLoaded.Add(AssetKey, asset);
            if (!countOfObjectsUsingAsset.ContainsKey(AssetKey))
                countOfObjectsUsingAsset.Add(AssetKey, 1);
            else
                countOfObjectsUsingAsset[AssetKey]++;

            Console.WriteLine(String.Format("Loaded <{0}> {1}", typeof(T).Name, AssetKey));

            return (T)asset;
        }

        /// <summary>
        /// Decreases the reference count for an asset and unloads it if no longer used.
        /// </summary>
        /// <typeparam name="T">The type of asset to unload.</typeparam>
        /// <param name="assetName">The name of the asset to unload.</param>
        public static void UnloadAsset<T>(string assetName)
        {
            var AssetNameType = typeof(T).Name;
            var AssetKey = assetName + "_" + AssetNameType;
            if (assetsLoaded.ContainsKey(AssetKey))
            {
                countOfObjectsUsingAsset[AssetKey]--;
                if (countOfObjectsUsingAsset[AssetKey] == 0)
                {
                    assetsLoaded.Remove(AssetKey);
                    
                    if (Content != null)
                    {
                        Content.Unload(assetName);
                    }
                    
                    Console.WriteLine(String.Format("Unloaded <{0}> {1}", typeof(T).Name, AssetKey));
                }
            }
        }
    }
}
