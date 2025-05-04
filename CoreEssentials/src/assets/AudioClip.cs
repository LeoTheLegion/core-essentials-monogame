using System;
using System.IO;
using System.Xml.Serialization;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Assets;

public interface IAudioClip{
    ISoundEffect SoundEffect { get; }
    float Volume { get; }
    bool Loop { get; set; }
    string Name { get; }
}

// Concrete implementation
public class AudioClip : Asset
{
    public ISoundEffect SoundEffect { get; internal set; }
    public float Volume { get; internal set; }

    public bool Loop { get; set; }

    public AudioClip(string name) : base(name)
    {
        
    }

    protected virtual void LoadFromXml(string name)
    {
        var xml = (XMLAsset)AssetManager.LoadAsset<XMLAsset>(name);
        if (xml == null)
        {
            throw new ArgumentNullException("xml", "XML data cannot be null.");
        }

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SoundDataXml), "http://schemas.coreessentials.monogame/2025/audio");
            using (StringReader reader = new StringReader(xml.XMLContent))
            {
                var xmlData = (SoundDataXml)serializer.Deserialize(reader);
                
                // Parse volume (default to 1 if not specified)
                float volume = 1.0f;
                if (!string.IsNullOrEmpty(xmlData.Volume))
                {
                    float.TryParse(xmlData.Volume, out volume);
                    
                    // Make sure the volume is valid
                    if (volume < 0f) volume = 0f;
                    if (volume > 1f) volume = 1f;
                }
                
                // Load the sound effect and wrap it with our adapter
                var soundEffect = (SoundEffectAsset)AssetManager.LoadAsset<SoundEffectAsset>(xmlData.Source);
                SoundEffect = new SoundEffectAdapter(soundEffect.SoundEffect);

                Loop = xmlData.Loop?.ToLower() == "true" || xmlData.Loop?.ToLower() == "yes";

                Volume = volume;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize XML sound data: {ex.Message}", ex);
        }
    }

    public override void Load(IContentManager contentManager)
    {
        string extension = Path.GetExtension(Name);
        if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            LoadFromXml(Name);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported audio data format: {extension}. Use .xml format");
        }
    }

    public override void Unload(IContentManager contentManager)
    {
        if (SoundEffect != null)
        {
            contentManager.Unload(Name);
            SoundEffect = null;
        }
    }

    /// <summary>
    /// XML serializable class for sound data
    /// </summary>
    [XmlRoot("SoundData", Namespace = "http://schemas.coreessentials.monogame/2025/audio")]
    public class SoundDataXml
    {
        public string Source { get; set; }
        public string SourceType { get; set; }
        public string Volume { get; set; }

        public string Loop { get; set; } // Added for future use
    }
}
