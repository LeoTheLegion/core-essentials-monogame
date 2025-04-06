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
    public static class AssetManager
    {
        
        static Dictionary<string, object> assetsLoaded = new Dictionary<string, object>();
        static Dictionary<string, int> countOfObjectsUsingAsset = new Dictionary<string, int>();

        static ContentManager Content;
        public static void Init(ContentManager content)
        {
            Content = content;
        }

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
            countOfObjectsUsingAsset.Add(AssetKey, 1);

            Debug.Console.WriteLine(String.Format("Loaded <{0}> {1}", typeof(T).Name, AssetKey));

            return asset;
        }

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
                    Content.UnloadAsset(assetName);
                    Debug.Console.WriteLine(String.Format("Unloaded <{0}> {1}", typeof(T).Name, AssetKey));
                }
            }
        }
    }
}
