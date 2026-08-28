#nullable enable
using System;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    /// <summary>
    /// Tests for SerializationUtils string parsing (Vector2 and Color property values).
    /// </summary>
    public class SerializationUtilsTests
    {
        // ─────────────────────────── ParseVector2FromString ───────────────────────────

        [Fact]
        public void ParseVector2_XYFormat_ReturnsBothComponents()
        {
            Assert.Equal(new Vector2(1.5f, 2.5f), SerializationUtils.ParseVector2FromString("1.5,2.5"));
        }

        [Fact]
        public void ParseVector2_NegativeValues_ParsesCorrectly()
        {
            Assert.Equal(new Vector2(-3f, -4.5f), SerializationUtils.ParseVector2FromString("-3,-4.5"));
        }

        [Fact]
        public void ParseVector2_ScalarExpandsToUniformVector()
        {
            // A bare scalar like Scale="1.5" should become (1.5, 1.5)
            Assert.Equal(new Vector2(1.5f, 1.5f), SerializationUtils.ParseVector2FromString("1.5"));
        }

        [Fact]
        public void ParseVector2_ScalarZero_ReturnsUniformZero()
        {
            Assert.Equal(Vector2.Zero, SerializationUtils.ParseVector2FromString("0"));
        }

        [Fact]
        public void ParseVector2_WhitespaceAroundValues_IsTolerated()
        {
            Assert.Equal(new Vector2(1f, 2f), SerializationUtils.ParseVector2FromString(" 1 , 2 "));
        }

        [Fact]
        public void ParseVector2_MalformedInput_FallsBackToZero()
        {
            Assert.Equal(Vector2.Zero, SerializationUtils.ParseVector2FromString("abc"));
            Assert.Equal(Vector2.Zero, SerializationUtils.ParseVector2FromString("1.5,xyz"));
            Assert.Equal(Vector2.Zero, SerializationUtils.ParseVector2FromString(string.Empty));
        }

        // ─────────────────────────────── ParseColor ───────────────────────────────

        [Fact]
        public void ParseColor_NamedColor_ResolvesFromPalette()
        {
            Assert.Equal(Color.LightGreen, SerializationUtils.ParseColor("LightGreen"));
            Assert.Equal(Color.Red, SerializationUtils.ParseColor("Red"));
        }

        [Fact]
        public void ParseColor_RGBString_ParsesNumericComponents()
        {
            // "100,255,100" should parse instead of falling back to White
            var color = SerializationUtils.ParseColor("100,255,100");
            Assert.Equal(100, color.R);
            Assert.Equal(255, color.G);
            Assert.Equal(100, color.B);
            Assert.Equal(255, color.A); // defaults to opaque
        }

        [Fact]
        public void ParseColor_RGBAString_ParsesAlpha()
        {
            var color = SerializationUtils.ParseColor("10,20,30,40");
            Assert.Equal(new Color(10, 20, 30, 40), color);
        }

        [Fact]
        public void ParseColor_OutOfRangeComponents_AreClamped()
        {
            var color = SerializationUtils.ParseColor("300,-5,999");
            Assert.Equal(255, color.R);
            Assert.Equal(0, color.G);
            Assert.Equal(255, color.B);
        }

        [Fact]
        public void ParseColor_WhitespaceAroundComponents_IsTolerated()
        {
            var color = SerializationUtils.ParseColor(" 100 , 255 , 100 ");
            Assert.Equal(new Color(100, 255, 100), color);
        }

        [Fact]
        public void ParseColor_TwoPartString_FallsBackToWhite()
        {
            // Not a valid R,G,B or R,G,B,A — must not be misparsed
            Assert.Equal(Color.White, SerializationUtils.ParseColor("100,255"));
        }

        [Fact]
        public void ParseColor_NonNumericString_FallsBackToWhite()
        {
            Assert.Equal(Color.White, SerializationUtils.ParseColor("not-a-color"));
            Assert.Equal(Color.White, SerializationUtils.ParseColor(string.Empty));
        }
    }
}
