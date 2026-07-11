using System;
using System.IO;
using System.Xml.Serialization;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Assets;

/// <summary>
/// Defines the contract for audio clip assets.
/// </summary>
public interface IAudioClip
{
    /// <summary>Gets the underlying sound effect.</summary>
    /// <summary>Gets the underlying sound effect, or null if unloaded.</summary>
    ISoundEffect? SoundEffect { get; }
    /// <summary>Gets the volume level (0.0 to 1.0).</summary>
    float Volume { get; }
    /// <summary>Indicates whether the clip should loop.</summary>
    bool Loop { get; set; }
    /// <summary>The name of the asset.</summary>
    string Name { get; }
}

/// <summary>A concrete audio clip asset that loads from XML metadata and wraps a <see cref="SoundEffect"/> instance.</summary>
public class AudioClip : Asset
{
    /// <summary>Gets the underlying sound effect.</summary>
    /// <summary>Gets the underlying sound effect, or null if unloaded.</summary>
    public ISoundEffect? SoundEffect { get; internal set; }

    /// <summary>The volume level (0.0 to 1.0).</summary>
    public float Volume { get; internal set; }

    /// <summary>Indicates whether the clip should loop.</summary>
    public bool Loop { get; set; }

    /// <summary>Initializes a new audio clip with the specified asset name.</summary>
    /// <param name="name">The name of the asset to load.</param>
    public AudioClip(string name) : base(name)
    {
        // No additional initialization required.
    }

    /// <summary>Loads audio data from an XML asset.</summary>
    /// <param name="name">The name of the XML asset to load.</param>
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
            if (xml == null || xml.XMLContent == null)
                throw new InvalidOperationException("XML content is missing.");

            using (StringReader reader = new StringReader(xml.XMLContent))
            {
                var xmlObj = serializer.Deserialize(reader);
                if (!(xmlObj is SoundDataXml xmlData))
                    throw new InvalidOperationException("Failed to deserialize XML sound data.");

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
                if (string.IsNullOrWhiteSpace(xmlData?.Source))
                    throw new InvalidOperationException("XML data is missing required 'Source' attribute.");

                var soundEffect = (SoundEffectAsset)AssetManager.LoadAsset<SoundEffectAsset>(xmlData.Source);
                if (soundEffect == null)
                    throw new InvalidOperationException($"Could not load sound effect '{xmlData.Source}'.");
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

    /// <summary>Loads the asset using the provided content manager.</summary>
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

    /// <summary>Unloads the audio asset and frees resources.</summary>
    public override void Unload(IContentManager contentManager)
    {
        if (SoundEffect != null)
        {
            contentManager.Unload(Name);
            // Clear the reference so tests can assert null.
            SoundEffect = null;
        }
    }

    /// <summary>
    /// XML serializable class for sound data
    /// </summary>
    [XmlRoot("SoundData", Namespace = "http://schemas.coreessentials.monogame/2025/audio")]
    public class SoundDataXml
    {
        /// <summary>The source file of the sound effect.</summary>
        public string? Source { get; set; }
        /// <summary>Type of the source (unused currently).</summary>
        public string? SourceType { get; set; }
        /// <summary>Volume value as string from XML, parsed to float later.</summary>
        public string? Volume { get; set; }

        /// <summary>Loop flag from XML. Defaults to "false" if missing.</summary>
        public string? Loop { get; set; } // Added for future use
    }
}
