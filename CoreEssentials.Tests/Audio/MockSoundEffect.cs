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
        private float _masterVolume = 1.0f;
        
        // Track method calls for assertions
        public int CreateInstanceCallCount { get; private set; }
        public MockSoundEffectInstance LastCreatedInstance { get; set; } // Changed to public setter
        
        public ISoundEffectInstance CreateInstance()
        {
            CreateInstanceCallCount++;
            var instance = new MockSoundEffectInstance();
            LastCreatedInstance = instance; // Store the created instance
            return instance;
        }
        
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(3.0);
        
        public float MasterVolume 
        { 
            get => _masterVolume;
            set => _masterVolume = value; 
        }
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
        private float _volume = 1.0f;
        private bool _isLooped = false;
        private float _pitch = 0.0f;
        private float _pan = 0.0f;
        
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
        
        public float Volume
        {
            get => _volume;
            set => _volume = value;
        }
        
        public bool IsLooped
        {
            get => _isLooped;
            set => _isLooped = value;
        }
        
        public float Pitch
        {
            get => _pitch;
            set => _pitch = value;
        }
        
        public float Pan
        {
            get => _pan;
            set => _pan = value;
        }
        
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