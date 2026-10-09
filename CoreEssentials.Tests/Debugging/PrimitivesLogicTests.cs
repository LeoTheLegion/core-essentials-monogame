#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.Debugging
{
    /// <summary>
    /// Drives <see cref="Primitives"/> through the <see cref="IPrimitiveDrawer"/> seam with a recording
    /// fake, asserting the geometry each primitive expands into (line distance/angle, rectangle corners,
    /// circle segment count) without a graphics device.
    /// </summary>
    public class PrimitivesLogicTests
    {
        private static Primitives Make() => new Primitives(new RecordingDrawer());

        [Fact]
        public void DrawLine_Horizontal_SetsZeroRotationAndPixelLength()
        {
            var prim = Make();
            var drawer = GetDrawer(prim);

            prim.DrawLine(null!, new Vector2(0, 0), new Vector2(10, 0), Color.Red, thickness: 2f);

            var call = Assert.Single(drawer.Calls);
            Assert.Equal(new Vector2(0, 0), call.Position);
            Assert.Equal(0f, call.Rotation, 4);          // pointing +X
            Assert.Equal(10f, call.Scale.X, 4);          // length
            Assert.Equal(2f, call.Scale.Y, 4);           // thickness
        }

        [Fact]
        public void DrawLine_Vertical_SetsQuarterTurnRotation()
        {
            var prim = Make();
            var drawer = GetDrawer(prim);

            prim.DrawLine(null!, new Vector2(0, 0), new Vector2(0, 5), Color.Red, thickness: 1f);

            var call = Assert.Single(drawer.Calls);
            Assert.Equal(MathHelper.PiOver2, call.Rotation, 4); // pointing +Y
            Assert.Equal(5f, call.Scale.X, 4);
        }

        [Fact]
        public void DrawLine_Diagonal_ComputesLengthAndAngle()
        {
            var prim = Make();
            var drawer = GetDrawer(prim);

            // (0,0) -> (3,4): a 3-4-5 line. length=5, angle=atan2(4,3).
            prim.DrawLine(null!, new Vector2(0, 0), new Vector2(3, 4), Color.Red, thickness: 1f);

            var call = Assert.Single(drawer.Calls);
            Assert.Equal(5f, call.Scale.X, 4);
            Assert.Equal((float)Math.Atan2(4, 3), call.Rotation, 4);
        }

        [Fact]
        public void DrawRectangle_EmitsFourLineSegmentsAtItsCorners()
        {
            var prim = Make();
            var drawer = GetDrawer(prim);

            // Rectangle (0,0) size 10x20 → corners TL(0,0) TR(10,0) BL(0,20) BR(10,20).
            prim.DrawRectangle(null!, new Rectangle(0, 0, 10, 20), Color.Red, thickness: 1f);

            Assert.Equal(4, drawer.Calls.Count);

            var positions = new HashSet<Vector2> { new Vector2(0, 0), new Vector2(10, 0), new Vector2(0, 20), new Vector2(10, 20) };
            foreach (var c in drawer.Calls)
                Assert.Contains(c.Position, positions);

            // Two horizontal segments of length 10 and two vertical of length 20.
            var lengths = drawer.Calls.Select(c => (float)Math.Round(c.Scale.X, 3)).ToList();
            Assert.Equal(2, lengths.Count(l => l == 10f));
            Assert.Equal(2, lengths.Count(l => l == 20f));
        }

        [Fact]
        public void DrawCircle_EmitsExactlySegmentsLineSegments()
        {
            var prim = Make();
            var drawer = GetDrawer(prim);

            prim.DrawCircle(null!, new Vector2(10, 10), radius: 8f, Color.Red, segments: 24, thickness: 1f);

            Assert.Equal(24, drawer.Calls.Count);
            // A circle approximated by a regular polygon has all sides the same length (the chord).
            float expected = drawer.Calls[0].Scale.X;
            Assert.True(expected > 0f);
            Assert.All(drawer.Calls, c => Assert.Equal(expected, c.Scale.X, 3));
        }

        [Fact]
        public void Draw_ForwardsColorAndOriginToDrawer()
        {
            var prim = Make();
            var drawer = GetDrawer(prim);

            prim.DrawLine(null!, new Vector2(1, 2), new Vector2(4, 2), Color.Green, thickness: 3f);

            var call = Assert.Single(drawer.Calls);
            Assert.Equal(Color.Green, call.Color);
            Assert.Equal(new Vector2(0f, 0.5f), call.Origin);
        }

        // ────────────────────────── helpers ──────────────────────────

        private static RecordingDrawer GetDrawer(Primitives prim)
        {
            var field = typeof(Primitives).GetField("_drawer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            return (RecordingDrawer)field.GetValue(prim)!;
        }

        /// <summary>Records each primitive draw so geometry can be asserted device-free.</summary>
        private sealed class RecordingDrawer : IPrimitiveDrawer
        {
            public readonly List<Call> Calls = new();
            public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
                => Calls.Add(new Call(position, color, rotation, origin, scale));

            public sealed record Call(Vector2 Position, Color Color, float Rotation, Vector2 Origin, Vector2 Scale);
        }
    }
}
