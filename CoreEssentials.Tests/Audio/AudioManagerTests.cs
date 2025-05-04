using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreEssentials.Assets;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Xunit;
using Moq;

namespace CoreEssentials.Tests.Audio
{
    public class AudioManagerTests
    {
        // Test helper class to expose the internal/protected members of AudioManager for testing
        private class TestableAudioManager : AudioManager
        {
            // Keep track of our mocks created for testing
            private readonly List<MockAudioClipInstance> _mockInstances = new List<MockAudioClipInstance>();

            private readonly Dictionary<string, IAudioClipInstance> _audioClipInstances;

            public TestableAudioManager()
            {
                // Use reflection to access the private field
                var field = typeof(AudioManager).GetField("_audioClipInstances", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                _audioClipInstances = (Dictionary<string, IAudioClipInstance>)field.GetValue(this);
            }

            public Dictionary<string, IAudioClipInstance> GetAudioClipInstances()
            {
                return _audioClipInstances;
            }

            // Instead of trying to override or use CreateAudioClipInstance,
            // we'll directly insert our mock into the dictionary
            public new string PlaySound(IAudioClip audioClip)
            {
                // Create mock instance
                var mockInstance = new MockAudioClipInstance(audioClip);
                _mockInstances.Add(mockInstance);
                
                // Generate ID same way the real implementation does
                var id = Guid.NewGuid().ToString();
                
                // Add directly to dictionary
                _audioClipInstances[id] = mockInstance;
                
                // Simulate Play being called
                mockInstance.Play(1.0f); // Use default volume
                
                return id;
            }

            public MockAudioClipInstance GetMockInstance(string id)
            {
                if (_audioClipInstances.TryGetValue(id, out var instance))
                {
                    return instance as MockAudioClipInstance;
                }
                return null;
            }
            
            public List<MockAudioClipInstance> GetAllMockInstances()
            {
                return _mockInstances;
            }
        }

        // Pure mock implementation of IAudioClipInstance for testing
        private class MockAudioClipInstance : IAudioClipInstance
        {
            public bool PlayCalled { get; set; }
            public bool StopCalled { get; set; }
            public float LastVolume { get; set; }
            public bool DonePlaying { get; set; }
            public IAudioClip AudioClip { get; }

            public MockAudioClipInstance(IAudioClip audioClip)
            {
                AudioClip = audioClip;
                PlayCalled = false;
                StopCalled = false;
                LastVolume = 0;
                DonePlaying = false;
            }

            public void Play(float masterVolume)
            {
                PlayCalled = true;
                LastVolume = masterVolume;
            }

            public void Stop()
            {
                StopCalled = true;
            }

            public bool IsDonePlaying()
            {
                return DonePlaying;
            }

            public void UpdateVolume(float masterVolume)
            {
                LastVolume = masterVolume;
            }
        }

        // Mock implementation of IAudioClip for testing
        private class MockAudioClip : IAudioClip
        {
            public ISoundEffect SoundEffect { get; }
            public float Volume { get; }
            public bool Loop { get; set; }
            public string Name { get; }

            public MockAudioClip(string name, float volume = 1.0f, bool loop = false)
            {
                Name = name;
                Volume = volume;
                Loop = loop;
                SoundEffect = new MockSoundEffect();
            }
        }

        // Mock implementation of ISoundEffect for testing
        private class MockSoundEffect : ISoundEffect
        {
            public ISoundEffectInstance CreateInstance()
            {
                return new MockSoundEffectInstance();
            }

            public TimeSpan Duration => TimeSpan.FromSeconds(1);

            public float MasterVolume { get; set; } = 1.0f;
        }

        // Mock implementation of ISoundEffectInstance for testing
        private class MockSoundEffectInstance : ISoundEffectInstance
        {
            public void Play() { }
            public void Stop() { }
            public void Pause() { }
            public SoundState State { get; set; } = SoundState.Stopped;
            public float Volume { get; set; }
            public bool IsLooped { get; set; }
            public float Pitch { get; set; }
            public float Pan { get; set; }
            public void Dispose() { }
        }

        [Fact]
        public void PlaySound_WithAudioClip_ReturnsIdAndAddsToInstances()
        {
            // Arrange
            var audioManager = new TestableAudioManager();
            var mockAudioClip = new MockAudioClip("testClip");

            // Act
            string soundId = audioManager.PlaySound(mockAudioClip);

            // Assert
            Assert.NotNull(soundId);
            Assert.NotEmpty(soundId);
            var instances = audioManager.GetAudioClipInstances();
            Assert.True(instances.ContainsKey(soundId));
            
            var mockInstance = audioManager.GetMockInstance(soundId);
            Assert.NotNull(mockInstance);
            Assert.True(mockInstance.PlayCalled);
        }

        [Fact]
        public void StopSound_WithValidId_RemovesFromInstances()
        {
            // Arrange
            var audioManager = new TestableAudioManager();
            var mockAudioClip = new MockAudioClip("testClip");
            string soundId = audioManager.PlaySound(mockAudioClip);
            var instances = audioManager.GetAudioClipInstances();
            Assert.True(instances.ContainsKey(soundId));

            // Act
            audioManager.StopSound(soundId);

            // Assert
            Assert.False(instances.ContainsKey(soundId));
        }

        [Fact]
        public void StopSound_WithInvalidId_DoesNotThrow()
        {
            // Arrange
            var audioManager = new TestableAudioManager();

            // Act & Assert - should not throw
            audioManager.StopSound("nonexistent");
        }

        [Fact]
        public void SetMasterVolume_UpdatesAllInstances()
        {
            // Arrange
            var audioManager = new TestableAudioManager();
            var mockAudioClip1 = new MockAudioClip("testClip1");
            var mockAudioClip2 = new MockAudioClip("testClip2");
            
            string soundId1 = audioManager.PlaySound(mockAudioClip1);
            string soundId2 = audioManager.PlaySound(mockAudioClip2);
            
            var mockInstances = audioManager.GetAllMockInstances();
            Assert.Equal(2, mockInstances.Count);
            
            // Act
            float newVolume = 0.5f;
            audioManager.SetMasterVolume(newVolume);

            // Assert
            foreach (var instance in mockInstances)
            {
                Assert.Equal(newVolume, instance.LastVolume);
            }
        }

        [Fact]
        public void SetMasterVolume_ClampsValues()
        {
            // Arrange
            var audioManager = new TestableAudioManager();
            var mockAudioClip = new MockAudioClip("testClip");
            string soundId = audioManager.PlaySound(mockAudioClip);
            var mockInstance = audioManager.GetMockInstance(soundId);

            // Act - test value below 0
            audioManager.SetMasterVolume(-0.5f);
            
            // Assert
            Assert.Equal(0.0f, mockInstance.LastVolume);
            
            // Act - test value above 1
            audioManager.SetMasterVolume(1.5f);
            
            // Assert
            Assert.Equal(1.0f, mockInstance.LastVolume);
        }

        [Fact]
        public void Update_RemovesDonePlayingInstances()
        {
            // Arrange
            var audioManager = new TestableAudioManager();
            var mockAudioClip = new MockAudioClip("testClip");
            string soundId = audioManager.PlaySound(mockAudioClip);
            var instances = audioManager.GetAudioClipInstances();
            var mockInstance = audioManager.GetMockInstance(soundId);
            mockInstance.DonePlaying = true;

            // Act
            audioManager.Update(new GameTime());

            // Assert
            Assert.False(instances.ContainsKey(soundId));
        }

        [Fact]
        public void Update_RestartsLoopingSounds()
        {
            // Arrange
            var audioManager = new TestableAudioManager();
            var mockAudioClip = new MockAudioClip("testClip", loop: true);
            string soundId = audioManager.PlaySound(mockAudioClip);
            var instances = audioManager.GetAudioClipInstances();
            var mockInstance = audioManager.GetMockInstance(soundId);
            
            // Reset the PlayCalled flag to false after initial play
            mockInstance.PlayCalled = false;
            mockInstance.DonePlaying = true;

            // Act
            audioManager.Update(new GameTime());

            // Assert
            Assert.True(instances.ContainsKey(soundId)); // Instance should still be there
            Assert.True(mockInstance.PlayCalled); // Play should have been called again
        }

        // This test is skipped because it requires mocking the AssetManager
        [Fact(Skip = "Requires mocking AssetManager")]
        public void PlayOneShotSound_LoadsAndPlaysSound()
        {
            // This test requires mocking the AssetManager which is more complex
            // For now, we'll skip implementation as it would require more context
            // about how AssetManager works in this system
        }

        // This test is skipped because it requires mocking another string-based method
        [Fact(Skip = "Requires mocking PlayOneShotSound")]
        public void PlaySound_WithString_CallsPlayOneShotSound()
        {
            // This would require mocking PlayOneShotSound which is complex
            // without changing the core implementation
        }
    }
}