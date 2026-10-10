#nullable enable
using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.Asset
{
    /// <summary>
    /// Device-free tests for the guard rails of <see cref="SpriteSheet"/> that reach no graphics device:
    /// the content-manager null check, the unsupported-format throw, every XML metadata validation throw
    /// (missing SourceType / Grid / Origin all fire before a texture is ever fetched), the unknown-source-type
    /// fallthrough, and the frame/origin lookups before metadata is loaded. The Load happy path and
    /// InitializeFrames remain device-bound on the Texture2D dereference and are covered by the playground
    /// smoke-run.
    /// </summary>
    public class SpriteSheetGuardTests
    {
        private static void WriteContentAsset(string fileName, string xml)
        {
            var contentDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Content");
            System.IO.Directory.CreateDirectory(contentDir);
            System.IO.File.WriteAllText(System.IO.Path.Combine(contentDir, fileName), xml);
        }

        [Fact]
        public void Load_NullContentManager_Throws()
        {
            var asset = new SpriteSheet("sheet.xml");

            Assert.Throws<ArgumentNullException>(() => asset.Load(null!));
        }

        [Fact]
        public void Load_NonXmlName_ThrowsUnsupportedFormat()
        {
            // A non-.xml extension is rejected before any asset manager call.
            var asset = new SpriteSheet("sheet.txt");

            var ex = Assert.Throws<InvalidOperationException>(() => asset.Load(new MockContentManager()));
            Assert.Contains("Unsupported sprite sheet data format", ex.Message);
        }

        [Fact]
        public void Load_MissingSourceType_Throws()
        {
            WriteContentAsset("GuardNoType.xml",
                @"<SpriteSheetData xmlns=""http://schemas.coreessentials.monogame/2025/spritesheet"">
                    <Grid><Rows>1</Rows><Columns>2</Columns></Grid>
                    <Origin><X>0</X><Y>0</Y></Origin>
                </SpriteSheetData>");
            AssetManager.Init(new MockContentManager());

            var asset = new SpriteSheet("GuardNoType.xml");

            var ex = Assert.Throws<InvalidOperationException>(() => asset.Load(new MockContentManager()));
            Assert.Contains("source type cannot be null", ex.Message);
        }

        [Fact]
        public void Load_MissingGrid_Throws()
        {
            WriteContentAsset("GuardNoGrid.xml",
                @"<SpriteSheetData xmlns=""http://schemas.coreessentials.monogame/2025/spritesheet"">
                    <SourceType>texture2d</SourceType>
                    <Source>t.png</Source>
                    <Origin><X>0</X><Y>0</Y></Origin>
                </SpriteSheetData>");
            AssetManager.Init(new MockContentManager());

            var asset = new SpriteSheet("GuardNoGrid.xml");

            var ex = Assert.Throws<InvalidOperationException>(() => asset.Load(new MockContentManager()));
            Assert.Contains("grid cannot be null", ex.Message);
        }

        [Fact]
        public void Load_MissingOrigin_Throws()
        {
            WriteContentAsset("GuardNoOrigin.xml",
                @"<SpriteSheetData xmlns=""http://schemas.coreessentials.monogame/2025/spritesheet"">
                    <SourceType>texture2d</SourceType>
                    <Source>t.png</Source>
                    <Grid><Rows>1</Rows><Columns>2</Columns></Grid>
                </SpriteSheetData>");
            AssetManager.Init(new MockContentManager());

            var asset = new SpriteSheet("GuardNoOrigin.xml");

            var ex = Assert.Throws<InvalidOperationException>(() => asset.Load(new MockContentManager()));
            Assert.Contains("origin cannot be null", ex.Message);
        }

        [Fact]
        public void Load_UnknownSourceType_Throws()
        {
            // A non-null but unrecognized SourceType passes validation, then falls through the switch default.
            WriteContentAsset("GuardBadType.xml",
                @"<SpriteSheetData xmlns=""http://schemas.coreessentials.monogame/2025/spritesheet"">
                    <SourceType>spritesheet</SourceType>
                    <Source>t.png</Source>
                    <Grid><Rows>1</Rows><Columns>2</Columns></Grid>
                    <Origin><X>0</X><Y>0</Y></Origin>
                </SpriteSheetData>");
            AssetManager.Init(new MockContentManager());

            var asset = new SpriteSheet("GuardBadType.xml");

            var ex = Assert.Throws<InvalidOperationException>(() => asset.Load(new MockContentManager()));
            Assert.Contains("Unknown source type", ex.Message);
        }

        [Fact]
        public void FrameOrigin_BeforeLoad_Throws()
        {
            var asset = new SpriteSheet("sheet.xml");

            var ex = Assert.Throws<InvalidOperationException>(() => _ = asset.FrameOrigin);
            Assert.Contains("metadata is not loaded", ex.Message);
        }

        [Fact]
        public void Rows_Columns_BeforeLoad_Throw()
        {
            var asset = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => _ = asset.Rows);
            Assert.Throws<InvalidOperationException>(() => _ = asset.Columns);
        }

        [Fact]
        public void GetFrameCount_FramesNotInitialized_Throws()
        {
            var asset = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => asset.GetFrameCount());
        }

        [Fact]
        public void Unload_NullContentManager_Throws()
        {
            var asset = new SpriteSheet("sheet.xml");

            Assert.Throws<ArgumentNullException>(() => asset.Unload(null!));
        }
    }
}
