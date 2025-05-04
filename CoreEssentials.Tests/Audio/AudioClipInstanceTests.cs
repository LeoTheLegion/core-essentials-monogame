using System;
using CoreEssentials.Assets;
using CoreEssentials.Audio;
using Microsoft.Xna.Framework.Audio;
using Xunit;
using System.Reflection;

namespace CoreEssentials.Tests.Audio
{
    public class AudioClipInstanceTests
    {
        [Fact]
        public void Constructor_SetsAudioClip()
        {
            // Arrange
            var audioClip = new MockAudioClip("test.xml", 0.5f);
            
            // Act
            var instance = new AudioClipInstance(audioClip);
            
            // Assert
            Assert.Same(audioClip, instance.AudioClip);
        }
        
        [Fact]
        public void IsDonePlaying_ReturnsTrueWhenNoSoundEffectInstance()
        {
            // Arrange
            var audioClip = new MockAudioClip("test.xml");
            var instance = new AudioClipInstance(audioClip);
            
            // Act & Assert
            Assert.True(instance.IsDonePlaying());
        }
        
        [Fact]
        public void Play_CreatesSoundEffectInstance()
        {
            // Arrange
            var mockSoundEffect = new MockSoundEffect();
            var audioClip = new MockAudioClip("test.xml", 0.75f);
            audioClip.SoundEffect = mockSoundEffect;
            
            var instance = new AudioClipInstance(audioClip);
            
            // Act
            instance.Play(1.0f);
            
            // Assert
            Assert.Equal(1, mockSoundEffect.CreateInstanceCallCount);
            Assert.False(instance.IsDonePlaying());
        }
        
        [Fact]
        public void UpdateVolume_ScalesVolumeCorrectly()
        {
            // Arrange
            var mockSoundEffect = new MockSoundEffect();
            var mockInstance = new MockSoundEffectInstance();
            
            // Setup test so we know what instance is created
            mockSoundEffect.LastCreatedInstance = mockInstance;
            
            var audioClip = new MockAudioClip("test.xml", 0.5f);
            audioClip.SoundEffect = mockSoundEffect;
            
            var instance = new AudioClipInstance(audioClip);
            instance.Play(1.0f);
            
            // Use reflection to access soundEffectInstance field
            var fieldInfo = typeof(AudioClipInstance).GetField("soundEffectInstance", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(instance, mockInstance);
            
            // Act
            instance.UpdateVolume(0.8f);
            
            // Assert - Verify volume is a product of clip volume and master volume
            Assert.Equal(0.4f, mockInstance.Volume); // 0.5f * 0.8f = 0.4f
        }
        
        [Fact]
        public void Stop_CleanupsSoundEffectInstance()
        {
            // Arrange
            var mockSoundEffect = new MockSoundEffect();
            var mockInstance = new MockSoundEffectInstance();
            
            var audioClip = new MockAudioClip("test.xml");
            audioClip.SoundEffect = mockSoundEffect;
            
            var instance = new AudioClipInstance(audioClip);
            instance.Play(1.0f);
            
            // Use reflection to access and set soundEffectInstance field
            var fieldInfo = typeof(AudioClipInstance).GetField("soundEffectInstance", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(instance, mockInstance);
            
            // Act
            instance.Stop();
            
            // Assert
            Assert.Equal(1, mockInstance.StopCallCount);
            Assert.Equal(1, mockInstance.DisposeCallCount);
        }
        
        [Fact]
        public void Play_WhenAlreadyPlaying_ReusesExistingInstance()
        {
            // Arrange
            var mockSoundEffect = new MockSoundEffect();
            var mockInstance = new MockSoundEffectInstance();
            
            var audioClip = new MockAudioClip("test.xml");
            audioClip.SoundEffect = mockSoundEffect;
            
            var instance = new AudioClipInstance(audioClip);
            
            // Simulate already played state
            instance.Play(1.0f);
            
            // Use reflection to access and set soundEffectInstance field
            var fieldInfo = typeof(AudioClipInstance).GetField("soundEffectInstance", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(instance, mockInstance);
            
            // Act 
            instance.Play(0.5f);
            
            // Assert
            Assert.Equal(1, mockInstance.PlayCallCount);
        }
        
        [Fact]
        public void IsDonePlaying_ReturnsTrueWhenStopped()
        {
            // Arrange
            var mockSoundEffect = new MockSoundEffect();
            var mockInstance = new MockSoundEffectInstance();
            mockInstance.SetState(SoundState.Stopped);
            
            var audioClip = new MockAudioClip("test.xml");
            audioClip.SoundEffect = mockSoundEffect;
            
            var instance = new AudioClipInstance(audioClip);
            
            // Use reflection to set the mock instance
            var fieldInfo = typeof(AudioClipInstance).GetField("soundEffectInstance", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(instance, mockInstance);
            
            // Act & Assert
            Assert.True(instance.IsDonePlaying());
        }
        
        [Fact]
        public void IsDonePlaying_ReturnsFalseWhenPlaying()
        {
            // Arrange
            var mockSoundEffect = new MockSoundEffect();
            var mockInstance = new MockSoundEffectInstance();
            mockInstance.SetState(SoundState.Playing);
            
            var audioClip = new MockAudioClip("test.xml");
            audioClip.SoundEffect = mockSoundEffect;
            
            var instance = new AudioClipInstance(audioClip);
            
            // Use reflection to set the mock instance
            var fieldInfo = typeof(AudioClipInstance).GetField("soundEffectInstance", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(instance, mockInstance);
            
            // Act & Assert
            Assert.False(instance.IsDonePlaying());
        }
    }
}