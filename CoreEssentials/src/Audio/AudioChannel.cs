namespace CoreEssentials.Audio;

/// <summary>
/// A logical audio channel (bus) for independent volume control — a deliberately minimal stand-in
/// for a full mixer graph. Each playing instance is tagged with the channel it belongs to, and a
/// per-channel volume multiplies into that instance's effective volume alongside the clip's own
/// volume and the global master volume:
/// <code>effective = clipVolume × instanceVolume × channelVolume × masterVolume</code>
/// This lets games duck or mute music independently of sound effects (and vice versa) without a
/// per-sound API.
/// </summary>
public enum AudioChannel
{
    /// <summary>The default channel. One-shots and untagged playback land here.</summary>
    Master,

    /// <summary>Background / music track channel.</summary>
    Music,

    /// <summary>Sound effects channel.</summary>
    Sfx
}
