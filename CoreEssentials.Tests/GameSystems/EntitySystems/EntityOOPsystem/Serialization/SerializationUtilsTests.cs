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

        // ─────────────────────────────── ParseValue (nullable) ───────────────────────────────

        [Fact]
        public void ParseValue_NullableColor_UnwrapsAndParses()
        {
            var value = SerializationUtils.ParseValue(typeof(Color?), "Coral");
            Assert.IsType<Color>(value);
            Assert.Equal(Color.Coral, (Color)value);
        }

        [Fact]
        public void ParseValue_NullableInt_UnwrapsAndParses()
        {
            var value = SerializationUtils.ParseValue(typeof(int?), "42");
            Assert.Equal(42, (int?)value);
        }

        [Fact]
        public void ParseValue_NonNullableColor_StillParses()
        {
            // Regression: non-nullable behavior is unchanged by nullable support.
            var value = SerializationUtils.ParseValue(typeof(Color), "Red");
            Assert.Equal(Color.Red, (Color)value);
        }

        [Fact]
        public void ParseValue_NullableUnsupportedType_Throws()
        {
            // Unwrapping exposes the underlying type, which is itself unsupported.
            Assert.Throws<NotSupportedException>(() => SerializationUtils.ParseValue(typeof(DateTime?), "2026-01-01"));
        }

        [Fact]
        public void ParseValue_NullableVector2_UnwrapsAndParses()
        {
            var value = SerializationUtils.ParseValue(typeof(Vector2?), "1,2");
            Assert.Equal(new Vector2(1f, 2f), (Vector2?)value);
        }

        // ─────────────────────────── ParseVector3FromString ───────────────────────────

        [Fact]
        public void ParseVector3_XYZFormat_ReturnsAllComponents()
        {
            Assert.Equal(new Vector3(1.5f, 2.5f, 3.5f), SerializationUtils.ParseVector3FromString("1.5,2.5,3.5"));
        }

        [Fact]
        public void ParseVector3_NegativeValues_ParsesCorrectly()
        {
            Assert.Equal(new Vector3(-3f, -4.5f, 6f), SerializationUtils.ParseVector3FromString("-3,-4.5,6"));
        }

        [Fact]
        public void ParseVector3_ScalarExpandsToUniformVector()
        {
            // A bare scalar like "2" should become (2, 2, 2)
            Assert.Equal(new Vector3(2f, 2f, 2f), SerializationUtils.ParseVector3FromString("2"));
        }

        [Fact]
        public void ParseVector3_WhitespaceAroundValues_IsTolerated()
        {
            Assert.Equal(new Vector3(1f, 2f, 3f), SerializationUtils.ParseVector3FromString(" 1 , 2 , 3 "));
        }

        [Fact]
        public void ParseVector3_MalformedInput_FallsBackToZero()
        {
            Assert.Equal(Vector3.Zero, SerializationUtils.ParseVector3FromString("abc"));
            Assert.Equal(Vector3.Zero, SerializationUtils.ParseVector3FromString("1.5,2.5")); // too few components
            Assert.Equal(Vector3.Zero, SerializationUtils.ParseVector3FromString(string.Empty));
        }

        // ─────────────────────────── ParseVector4FromString ───────────────────────────

        [Fact]
        public void ParseVector4_XYZWFormat_ReturnsAllComponents()
        {
            Assert.Equal(new Vector4(1f, 2f, 3f, 4f), SerializationUtils.ParseVector4FromString("1,2,3,4"));
        }

        [Fact]
        public void ParseVector4_NegativeValues_ParsesCorrectly()
        {
            Assert.Equal(new Vector4(-1f, -2f, -3f, -4f), SerializationUtils.ParseVector4FromString("-1,-2,-3,-4"));
        }

        [Fact]
        public void ParseVector4_ScalarExpandsToUniformVector()
        {
            Assert.Equal(new Vector4(0.5f, 0.5f, 0.5f, 0.5f), SerializationUtils.ParseVector4FromString("0.5"));
        }

        [Fact]
        public void ParseVector4_MalformedInput_FallsBackToZero()
        {
            Assert.Equal(Vector4.Zero, SerializationUtils.ParseVector4FromString("abc"));
            Assert.Equal(Vector4.Zero, SerializationUtils.ParseVector4FromString("1,2,3")); // too few components
            Assert.Equal(Vector4.Zero, SerializationUtils.ParseVector4FromString(string.Empty));
        }

        // ─────────────────────── ParseValue (Vector3 / Vector4) ───────────────────────

        [Fact]
        public void ParseValue_Vector3_Parses()
        {
            var value = SerializationUtils.ParseValue(typeof(Vector3), "1,2,3");
            Assert.Equal(new Vector3(1f, 2f, 3f), (Vector3)value);
        }

        [Fact]
        public void ParseValue_Vector4_Parses()
        {
            var value = SerializationUtils.ParseValue(typeof(Vector4), "1,2,3,4");
            Assert.Equal(new Vector4(1f, 2f, 3f, 4f), (Vector4)value);
        }

        [Fact]
        public void ParseValue_NullableVector3_UnwrapsAndParses()
        {
            var value = SerializationUtils.ParseValue(typeof(Vector3?), "1,2,3");
            Assert.Equal(new Vector3(1f, 2f, 3f), (Vector3?)value);
        }
    }
}
