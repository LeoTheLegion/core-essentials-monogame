using System;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Tests
{
    // Shared fake for all tests needing ISoundEffect
    public class FakeSoundEffect : ISoundEffect
    {
        public ISoundEffectInstance CreateInstance() => new FakeSoundEffectInstance();
        public TimeSpan Duration => TimeSpan.Zero;
        public float MasterVolume { get; set; }
    }

    public class FakeSoundEffectInstance : ISoundEffectInstance
    {
        public void Play() { }
        public void Stop() { }
        public void Pause() { }
        public SoundState State => SoundState.Stopped;
        public float Volume { get; set; }
        public bool IsLooped { get; set; }
        public float Pitch { get; set; }
        public float Pan { get; set; }
        public void Dispose() { }
    }
}
