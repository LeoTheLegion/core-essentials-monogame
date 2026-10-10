using System;
using System.Reflection;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework;
using Xunit;

namespace CoreEssentials.Tests.Asset
{
    /// <summary>
    /// Exercises the device-free query and validation logic of <see cref="SpriteSheet"/> by
    /// injecting its private frame/metadata state via reflection (a pattern already used by other
    /// sprite tests). This covers the grid/frame accessors and their guard branches, which are
    /// otherwise only reached through a real texture load.
    /// </summary>
    public class SpriteSheetLogicTests
    {
        [Fact]
        public void GetFrameCount_NotInitialized_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => sheet.GetFrameCount());
        }

        [Fact]
        public void GetFrameSize_NotInitialized_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => sheet.GetFrameSize());
        }

        [Fact]
        public void GetFrame_NotInitialized_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => sheet.GetFrame(0));
        }

        [Fact]
        public void FrameOrigin_NotInitialized_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => _ = sheet.FrameOrigin);
        }

        [Fact]
        public void Rows_NotInitialized_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => _ = sheet.Rows);
        }

        [Fact]
        public void Columns_NotInitialized_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => _ = sheet.Columns);
        }

        [Fact]
        public void GetFrameSize_ReturnsFirstFrameDimensions()
        {
            var sheet = SetFrames(new SpriteSheet("sheet.xml"),
                new[] { new Rectangle(0, 0, 16, 24), new Rectangle(16, 0, 16, 24) });

            var size = sheet.GetFrameSize();

            Assert.Equal(16f, size.X);
            Assert.Equal(24f, size.Y);
        }

        [Fact]
        public void GetFrameCount_ReturnsInjectedLength()
        {
            var sheet = SetFrames(new SpriteSheet("sheet.xml"), new Rectangle[5]);

            Assert.Equal(5, sheet.GetFrameCount());
        }

        [Fact]
        public void GetFrame_ReturnsRequestedRectangle()
        {
            var sheet = SetFrames(new SpriteSheet("sheet.xml"),
                new[] { new Rectangle(0, 0, 8, 8), new Rectangle(8, 0, 8, 8) });

            Assert.Equal(new Rectangle(8, 0, 8, 8), sheet.GetFrame(1));
        }

        [Fact]
        public void GetFrameAt_ReturnsRectangleByGridCoordinates()
        {
            // 2x2 grid; index = row * Columns + column
            var sheet = new SpriteSheet("sheet.xml");
            SetFrames(sheet, new[]
            {
                new Rectangle(0, 0, 10, 10),
                new Rectangle(10, 0, 10, 10),
                new Rectangle(0, 10, 10, 10),
                new Rectangle(10, 10, 10, 10)
            });
            SetMetadata(sheet, rows: 2, columns: 2);

            // row=1, col=0 -> index 1*2+0 = 2
            Assert.Equal(new Rectangle(0, 10, 10, 10), sheet.GetFrameAt(1, 0));
            // row=1, col=1 -> index 3
            Assert.Equal(new Rectangle(10, 10, 10, 10), sheet.GetFrameAt(1, 1));
        }

        [Fact]
        public void GetFrameAt_OutOfRange_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");
            SetFrames(sheet, new Rectangle[4]);
            SetMetadata(sheet, rows: 2, columns: 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => sheet.GetFrameAt(2, 0)); // row too high
            Assert.Throws<ArgumentOutOfRangeException>(() => sheet.GetFrameAt(0, -1)); // col negative
        }

        [Fact]
        public void GetFrameAt_NotInitialized_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");

            Assert.Throws<InvalidOperationException>(() => sheet.GetFrameAt(0, 0));
        }

        [Fact]
        public void RowsColumns_FrameOrigin_ReadInjectedMetadata()
        {
            var sheet = new SpriteSheet("sheet.xml");
            SetMetadata(sheet, rows: 3, columns: 4);
            SetOrigin(sheet, x: 5f, y: 6f);

            Assert.Equal(3, sheet.Rows);
            Assert.Equal(4, sheet.Columns);
            var origin = sheet.FrameOrigin;
            Assert.Equal(5f, origin.X);
            Assert.Equal(6f, origin.Y);
        }

        [Fact]
        public void Rows_GridMissing_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");
            // Metadata present but Grid is null.
            SetMetadata(sheet, rows: 0, columns: 0, withGrid: false);

            Assert.Throws<InvalidOperationException>(() => _ = sheet.Rows);
        }

        [Fact]
        public void FrameOrigin_OriginMissing_Throws()
        {
            var sheet = new SpriteSheet("sheet.xml");
            SetMetadata(sheet, rows: 1, columns: 1, withGrid: false); // no origin set -> null

            Assert.Throws<InvalidOperationException>(() => _ = sheet.FrameOrigin);
        }

        // ── Reflection helpers (match the existing codebase convention of driving private state) ──

        private static SpriteSheet SetFrames(SpriteSheet sheet, Rectangle[]? frames)
        {
            typeof(SpriteSheet).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(sheet, frames);
            return sheet;
        }

        private static void SetMetadata(SpriteSheet sheet, int rows, int columns, bool withGrid = true)
        {
            var metaType = typeof(SpriteSheet).GetNestedType("SpriteSheetMetadata", BindingFlags.NonPublic)!;
            var meta = Activator.CreateInstance(metaType);

            if (withGrid)
            {
                var gridType = typeof(SpriteSheet).GetNestedType("Grid", BindingFlags.NonPublic)!;
                var grid = Activator.CreateInstance(gridType);
                gridType.GetProperty("Rows")!.SetValue(grid, rows);
                gridType.GetProperty("Columns")!.SetValue(grid, columns);
                metaType.GetProperty("Grid")!.SetValue(meta, grid);
            }

            typeof(SpriteSheet).GetField("_metaData", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(sheet, meta);
        }

        private static void SetOrigin(SpriteSheet sheet, float x, float y)
        {
            // Ensure a metadata object exists first, then attach an Origin to it.
            var metaType = typeof(SpriteSheet).GetNestedType("SpriteSheetMetadata", BindingFlags.NonPublic)!;
            var field = typeof(SpriteSheet).GetField("_metaData", BindingFlags.NonPublic | BindingFlags.Instance)!;

            var existing = (metaType.IsValueType ? null : (object?)field.GetValue(sheet)) ?? Activator.CreateInstance(metaType);
            var originType = typeof(SpriteSheet).GetNestedType("Origin", BindingFlags.NonPublic)!;
            var origin = Activator.CreateInstance(originType);
            originType.GetProperty("X")!.SetValue(origin, x);
            originType.GetProperty("Y")!.SetValue(origin, y);
            metaType.GetProperty("Origin")!.SetValue(existing, origin);

            field.SetValue(sheet, existing);
        }
    }
}
