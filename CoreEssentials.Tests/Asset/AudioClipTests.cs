using System;
using CoreEssentials.Assets;
using Xunit;
using System.Reflection;

namespace CoreEssentials.Tests
{
    public class AudioClipTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            var asset = new AudioClip("audio.xml");
            Assert.Equal("audio.xml", asset.Name);
        }

        [Fact]
        public void Unload_ClearsSoundEffect()
        {
            var asset = new AudioClip("audio.xml");
            var mockManager = new MockContentManager();
            // Simulate loaded sound effect
            typeof(AudioClip).GetProperty("SoundEffect")!.SetValue(asset, new FakeSoundEffect());
            asset.Unload(mockManager);
            Assert.Null(asset.SoundEffect);
        }
    }
}
