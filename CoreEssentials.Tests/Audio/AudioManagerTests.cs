using System;
using System.Reflection;
using System.Collections.Generic;
using CoreEssentials.Assets;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework;
using Xunit;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Tests.Audio
{
    public class AudioManagerTests
    {
        private MockContentManager contentManager;
        
        public AudioManagerTests()
        {
            // Set up content manager for tests
            contentManager = new MockContentManager();
            
            // Replace AssetManager's internal content manager - requires reflection
            var contentManagerField = typeof(AssetManager).GetField("_contentManager", 
                BindingFlags.NonPublic | BindingFlags.Static);
            contentManagerField?.SetValue(null, contentManager);
        }
        
        [Fact]
        public void Instance_ReturnsNonNullSingleton()
        {
            // Act
            var instance = AudioManager.Instance;
            
            // Assert
            Assert.NotNull(instance);
        }
        
        [Fact]
        public void PlaySound_WithAudioClip_ReturnsValidId()
        {
            // Arrange
            var mockManager = new MockAudioManager();
            var audioClip = new MockAudioClip("test.xml") { 
                SoundEffect = new MockSoundEffect() 
            };
            
            // Act
            var id = mockManager.PlaySound(audioClip);
            
            // Assert
            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }
        
        [Fact]
        public void PlaySound_WithName_CallsPlayOneShotSound()
        {
            // Arrange
            var mockManager = new MockAudioManager();
            
            // Need to setup AssetManager to return a mock asset
            var audioClip = new MockAudioClip("test.xml") {
                SoundEffect = new MockSoundEffect()
            };
            SetupAssetManagerToReturnMock(audioClip);
            
            // Act
            var id = mockManager.PlaySound("test.xml");
            
            // Assert
            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }
        
        [Fact]
        public void StopSound_RemovesAudioClipInstance()
        {
            // Arrange
            var mockManager = new MockAudioManager();
            var audioClip = new MockAudioClip("test.xml") {
                SoundEffect = new MockSoundEffect()
            };
            var id = mockManager.PlaySound(audioClip);
            
            // Get the instances field to verify proper cleanup
            var instancesField = typeof(AudioManager).GetField("_audioClipInstances", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var instances = instancesField.GetValue(mockManager) as Dictionary<string, AudioClipInstance>;
            
            // Act
            mockManager.StopSound(id);
            
            // Assert
            Assert.False(instances.ContainsKey(id), "Audio clip instance should be removed after stopping");
        }
        
        [Fact]
        public void SetMasterVolume_ClampsValuesBetweenZeroAndOne()
        {
            // Arrange
            var mockManager = new MockAudioManager();
            
            // Act - Test upper bound
            mockManager.SetTestMasterVolume(1.5f);  // Too high
            
            // Assert
            Assert.Equal(1.0f, mockManager.GetMasterVolume());
            
            // Act again - Test lower bound
            mockManager.SetTestMasterVolume(-0.5f); // Too low
            
            // Assert
            Assert.Equal(0.0f, mockManager.GetMasterVolume());
        }
        
        [Fact]
        public void SetMasterVolume_UpdatesAllAudioClipInstances()
        {
            // Arrange
            var mockManager = new MockAudioManager();
            var mockSoundEffect = new MockSoundEffect();
            var mockSoundEffectInstance = new MockSoundEffectInstance();
            
            // Setup the relationship between mock objects
            mockSoundEffect.LastCreatedInstance = mockSoundEffectInstance;
            
            var audioClip = new MockAudioClip("test.xml", 0.5f) {
                SoundEffect = mockSoundEffect
            };
            
            var id = mockManager.PlaySound(audioClip);
            
            // Get the audio clip instance
            var instancesField = typeof(AudioManager).GetField("_audioClipInstances", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var instances = instancesField.GetValue(mockManager) as Dictionary<string, AudioClipInstance>;
            var instance = instances[id];
            
            // Use reflection to replace the sound effect instance with our mock
            var soundEffectInstanceField = typeof(AudioClipInstance).GetField("soundEffectInstance",
                BindingFlags.NonPublic | BindingFlags.Instance);
            soundEffectInstanceField.SetValue(instance, mockSoundEffectInstance);
            
            // Act
            mockManager.SetTestMasterVolume(0.7f);
            
            // Assert - The volume should be audioClip.Volume * masterVolume = 0.5 * 0.7 = 0.35
            Assert.Equal(0.35f, mockSoundEffectInstance.Volume, 0.001f);
        }
        
        [Fact]
        public void Update_RemovesDonePlayingSounds()
        {
            // Arrange
            var mockManager = new MockAudioManager();
            var mockSoundEffect = new MockSoundEffect();
            
            var audioClip = new MockAudioClip("test.xml") {
                SoundEffect = mockSoundEffect,
                Loop = false // Important - don't loop
            };
            
            var id = mockManager.PlaySound(audioClip);
            
            // Get instances dictionary to check state
            var instancesField = typeof(AudioManager).GetField("_audioClipInstances", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var instances = instancesField.GetValue(mockManager) as Dictionary<string, AudioClipInstance>;
            
            // Verify initial state
            Assert.True(instances.ContainsKey(id));
            
            // Simulate a sound that's finished playing by making it report as "done"
            var instance = instances[id];
            ForceSoundEffectInstanceToReport(instance, isDonePlaying: true);
            
            // Act
            mockManager.Update(new GameTime());
            
            // Assert
            Assert.False(instances.ContainsKey(id), "Audio clip instance should be removed after it's done playing");
        }
        
        [Fact]
        public void Update_RestartsSoundsWithLoopEnabled()
        {
            // Arrange
            var mockManager = new MockAudioManager();
            var mockSoundEffect = new MockSoundEffect();
            var mockSoundEffectInstance = new MockSoundEffectInstance();
            
            var audioClip = new MockAudioClip("test.xml") {
                SoundEffect = mockSoundEffect,
                Loop = true // Important - enable looping
            };
            
            var id = mockManager.PlaySound(audioClip);
            
            // Get instances dictionary to check state
            var instancesField = typeof(AudioManager).GetField("_audioClipInstances", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var instances = instancesField.GetValue(mockManager) as Dictionary<string, AudioClipInstance>;
            
            // Get the instance that was created
            var instance = instances[id];
            
            // Replace with our mock instance that we can control
            var soundEffectInstanceField = typeof(AudioClipInstance).GetField("soundEffectInstance",
                BindingFlags.NonPublic | BindingFlags.Instance);
            soundEffectInstanceField.SetValue(instance, mockSoundEffectInstance);
            
            // Use reflection to make the instance report it's done playing
            ForceSoundEffectInstanceToReport(instance, isDonePlaying: true);
            
            // Act
            mockManager.Update(new GameTime());
            
            // Assert
            Assert.True(instances.ContainsKey(id), "Looping audio clip instance should not be removed after update");
            Assert.Equal(1, mockSoundEffectInstance.PlayCallCount);
        }
        
        #region Helper Methods
        
        private void SetupAssetManagerToReturnMock(MockAudioClip audioClip)
        {
            // Add mock to the asset dictionary - requires reflection
            var assetsField = typeof(AssetManager).GetField("_assets", BindingFlags.NonPublic | BindingFlags.Static);
            var assets = assetsField?.GetValue(null) as Dictionary<string, CoreEssentials.Assets.Asset>; // Explicitly use CoreEssentials.Assets.Asset
            
            if (assets != null)
            {
                if (assets.ContainsKey(audioClip.Name))
                {
                    assets[audioClip.Name] = audioClip;
                }
                else
                {
                    assets.Add(audioClip.Name, audioClip);
                }
            }
        }
        
        private void ForceSoundEffectInstanceToReport(AudioClipInstance instance, bool isDonePlaying)
        {
            // This method affects how IsDonePlaying() behaves
            if (isDonePlaying)
            {
                var field = typeof(AudioClipInstance).GetField("soundEffectInstance", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                var soundEffectInstance = field?.GetValue(instance) as MockSoundEffectInstance;
                if (soundEffectInstance != null)
                {
                    // Set the state to Stopped which will make IsDonePlaying return true
                    soundEffectInstance.SetState(SoundState.Stopped);
                }
                else
                {
                    // Fall back to original behavior if the instance isn't a MockSoundEffectInstance
                    field?.SetValue(instance, null);
                }
            }
        }
        
        #endregion
    }
}