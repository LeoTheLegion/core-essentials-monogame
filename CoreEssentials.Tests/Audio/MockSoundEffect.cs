using System;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Tests.Audio
{
    /// <summary>
    /// Enhanced mock implementation of ISoundEffect for testing
    /// </summary>
    public class MockSoundEffect : ISoundEffect
    {
        // Track method calls for assertions
        public int CreateInstanceCallCount { get; private set; }
        public MockSoundEffectInstance? LastCreatedInstance { get; set; } // Changed to public setter
        
        public ISoundEffectInstance CreateInstance()
        {
            CreateInstanceCallCount++;
            var instance = new MockSoundEffectInstance();
            LastCreatedInstance = instance; // Store the created instance
            return instance;
        }
        
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(3.0);
        
        public float MasterVolume { get; set; } = 1.0f;
    }
    
    /// <summary>
    /// Enhanced mock implementation of ISoundEffectInstance for testing
    /// </summary>
    public class MockSoundEffectInstance : ISoundEffectInstance
    {
        // Track method calls for assertions
        public int PlayCallCount { get; private set; }
        public int StopCallCount { get; private set; }
        public int PauseCallCount { get; private set; }
        public int DisposeCallCount { get; private set; }
        
        // Current state properties
        private SoundState _state = SoundState.Stopped;
        
        public void Play() 
        {
            PlayCallCount++;
            _state = SoundState.Playing;
        }
        
        public void Stop() 
        {
            StopCallCount++;
            _state = SoundState.Stopped;
        }
        
        public void Pause() 
        {
            PauseCallCount++;
            _state = SoundState.Paused;
        }
        
        public SoundState State => _state;
        
        public float Volume { get; set; } = 1.0f;
        
        public bool IsLooped { get; set; }
        
        public float Pitch { get; set; }
        
        public float Pan { get; set; }
        
        public void Dispose()
        {
            DisposeCallCount++;
        }
        
        // For testing purposes only - let tests manually set state
        public void SetState(SoundState state)
        {
            _state = state;
        }
    }
}