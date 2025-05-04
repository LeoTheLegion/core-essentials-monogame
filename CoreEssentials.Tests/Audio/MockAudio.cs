using System;
using CoreEssentials.Assets;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Tests.Audio
{
    // Mock class for testing AudioClip
    public class MockAudioClip : AudioClip
    {
        private bool _isLoaded;
        
        public MockAudioClip(string name, float volume = 1.0f, bool loop = false) : base(name)
        {
            SoundEffect = new FakeSoundEffect();
            Volume = volume;
            Loop = loop;
        }

        public override void Load(IContentManager contentManager)
        {
            _isLoaded = true;
        }

        public override void Unload(IContentManager contentManager)
        {
            SoundEffect = null;
            _isLoaded = false;
        }
        
        public bool IsLoaded => _isLoaded;
    }

    // Mock class for testing AudioManager
    public class MockAudioManager : AudioManager
    {
        // Expose constructor for testing
        public MockAudioManager() : base()
        {
        }
        
        // Track instances to verify cleanup
        public int InstanceCount { get; set; }

        // Override to use mock instances
        protected override AudioClipInstance CreateAudioClipInstance(AudioClip audioClip)
        {
            InstanceCount++;
            return new MockAudioClipInstance(audioClip);
        }
        
        // Allow direct access to master volume for testing
        public void SetTestMasterVolume(float volume)
        {
            SetMasterVolume(volume);
        }
        
        public float GetMasterVolume()
        {
            // Use reflection to access the private _masterVolume field
            var fieldInfo = typeof(AudioManager).GetField("_masterVolume", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float)fieldInfo.GetValue(this);
        }
        
        // Override PlayOneShotSound to avoid loading a real file
        public new string PlayOneShotSound(string soundName)
        {
            // Instead of loading a real file, create a mock audio clip
            var mockClip = new MockAudioClip(soundName)
            {
                SoundEffect = new MockSoundEffect()
            };
            
            return PlaySound(mockClip);
        }
        
        // Override PlaySound(string) to use our mocked implementation
        public new string PlaySound(string name)
        {
            // Create a mock audio clip instead of trying to load one
            var mockClip = new MockAudioClip(name)
            {
                SoundEffect = new MockSoundEffect()
            };
            
            return PlaySound(mockClip);
        }
    }
    
    // Mock class for testing AudioClipInstance behavior
    public class MockAudioClipInstance : AudioClipInstance
    {
        private bool _stopped = false;
        private bool _disposed = false;
        
        public MockAudioClipInstance(AudioClip audioClip) : base(audioClip)
        {
        }

        public override void Stop()
        {
            base.Stop();
            _stopped = true;
        }
        
        public bool WasStopped => _stopped;
        public bool WasDisposed => _disposed;
        
        // For testing volume updates
        public float LastAppliedVolume { get; private set; } = -1;

        public new void UpdateVolume(float volume)
        {
            LastAppliedVolume = volume;
            base.UpdateVolume(volume);
        }
    }
}