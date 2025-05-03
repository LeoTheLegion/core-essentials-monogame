using CoreEssentials.Debugging;
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
        static Dictionary<string, object> assetsLoaded = new Dictionary<string, object>();
        
        /// <summary>
        /// Dictionary tracking reference counts for each loaded asset.
        /// </summary>
        static Dictionary<string, int> countOfObjectsUsingAsset = new Dictionary<string, int>();

        /// <summary>
        /// Content manager reference used to load assets from files.
        /// </summary>
        static ContentManager Content;
        
        /// <summary>
        /// Initializes the AssetManager with a ContentManager.
        /// </summary>
        /// <param name="content">The ContentManager to use for loading assets.</param>
        public static void Init(ContentManager content)
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
        public static T LoadAsset<T>(string assetName)
        {
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

            T asset;

            if (typeof(Asset).IsAssignableFrom(typeof(T))) {
                asset = (T)Activator.CreateInstance(typeof(T), new object[] { assetName });
            }
            else if (typeof(String).IsAssignableFrom(typeof(T)))
            {
                var extention = Path.GetExtension(assetName);

                if(extention != null && extention != string.Empty)
                {
                    string exePath = AppContext.BaseDirectory;
                    string filePath = Path.Combine(exePath, "Content", assetName);
                    if (typeof(T) == typeof(string))
                    {
                        asset = (T)(object)File.ReadAllText(filePath);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Cannot load asset of type {typeof(T).Name} from a text file.");
                    }
                }
                else
                {
                    asset = Content.Load<T>(assetName);
                }
            }
            else {
                var extention = Path.GetExtension(assetName);
                if (extention != null && extention != string.Empty)
                {
                    throw new InvalidOperationException($"Cannot load asset of type {typeof(T).Name} using an extention. Please remove it.");
                }
                asset = Content.Load<T>(assetName);
            }
             
            assetsLoaded.Add(AssetKey, asset);
            if (!countOfObjectsUsingAsset.ContainsKey(AssetKey))
                countOfObjectsUsingAsset.Add(AssetKey, 1);
            else
                countOfObjectsUsingAsset[AssetKey]++;

            Debug.Console.WriteLine(String.Format("Loaded <{0}> {1}", typeof(T).Name, AssetKey));

            return asset;
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
                        Content.UnloadAsset(assetName);
                    }
                    
                    Debug.Console.WriteLine(String.Format("Unloaded <{0}> {1}", typeof(T).Name, AssetKey));
                }
            }
        }
    }
}
