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
            if (assetsLoaded.ContainsKey(assetName))
            {
                countOfObjectsUsingAsset[assetName]++;
                return (T)assetsLoaded[assetName];
            }

            if (assetName == null || assetName == string.Empty)
            {
                throw new ArgumentNullException("assetName", "Asset name cannot be null or empty.");
            }

            T asset;

            // Check if the asset is a .xmmp file
            if (assetName.EndsWith(".xmmp", StringComparison.OrdinalIgnoreCase))
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
            }else {
                asset = Content.Load<T>(assetName);
            }
             
            assetsLoaded.Add(assetName, asset);
            countOfObjectsUsingAsset.Add(assetName, 1);

            Debug.Console.WriteLine(String.Format("Loaded <{0}> {1}", typeof(T).Name, assetName));

            return asset;
        }

        public static void UnloadAsset<T>(string assetName)
        {
            if (assetsLoaded.ContainsKey(assetName))
            {
                countOfObjectsUsingAsset[assetName]--;
                if (countOfObjectsUsingAsset[assetName] == 0)
                {
                    assetsLoaded.Remove(assetName);
                    Content.UnloadAsset(assetName);
                    Debug.Console.WriteLine(String.Format("Unloaded <{0}> {1}", typeof(T).Name, assetName));
                }
            }
        }
    }
}
