using System;
using System.IO;
using System.Xml.Serialization;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Assets;

// Concrete implementation
public class AudioClip : Asset
{
    public ISoundEffect SoundEffect { get; internal set; }
    public float Volume { get; internal set; }

    public bool Loop { get; set; }

    public AudioClip(string name) : base(name)
    {
        string extension = Path.GetExtension(name);
        if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            LoadFromXml(name);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported audio data format: {extension}. Use .xml format");
        }
    }

    private void LoadFromXml(string name)
    {
        var xml = AssetManager.LoadAsset<string>(name);
        if (xml == null)
        {
            throw new ArgumentNullException("xml", "XML data cannot be null.");
        }

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SoundDataXml), "http://schemas.coreessentials.monogame/2025/audio");
            using (StringReader reader = new StringReader(xml))
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
                var soundEffect = AssetManager.LoadAsset<SoundEffect>(xmlData.Source);
                SoundEffect = new SoundEffectAdapter(soundEffect);

                Loop = xmlData.Loop?.ToLower() == "true" || xmlData.Loop?.ToLower() == "yes";

                Volume = volume;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize XML sound data: {ex.Message}", ex);
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
