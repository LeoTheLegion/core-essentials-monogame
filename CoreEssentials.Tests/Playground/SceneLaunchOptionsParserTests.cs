using System;
using Xunit;
using CoreEssentials.Playground;

namespace CoreEssentials.Tests.Playground
{
    public class SceneLaunchOptionsParserTests
    {
        [Fact]
        public void Parse_NoArgs_UsesDefaults()
        {
            var options = SceneLaunchOptionsParser.Parse(Array.Empty<string>());

            Assert.Equal(SceneLaunchOptionsParser.DefaultScene, options.Scene);
            Assert.Null(options.RunForSeconds);
            Assert.False(options.NoFocusPause);
        }

        [Fact]
        public void Parse_NullArgs_UsesDefaults()
        {
            var options = SceneLaunchOptionsParser.Parse(null);

            Assert.Equal(SceneLaunchOptionsParser.DefaultScene, options.Scene);
            Assert.Null(options.RunForSeconds);
        }

        [Fact]
        public void Parse_ExplicitScene_SetsScene()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--scene", "CharacterScene.xml" });

            Assert.Equal("CharacterScene.xml", options.Scene);
            Assert.Null(options.RunForSeconds);
        }

        [Fact]
        public void Parse_ExplicitRunFor_SetsRunForSeconds()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--run-for", "5" });

            Assert.Equal(SceneLaunchOptionsParser.DefaultScene, options.Scene);
            Assert.Equal(5.0, options.RunForSeconds);
        }

        [Fact]
        public void Parse_BothOptions_SetsBoth()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--scene", "PhysicsEntityScene.xml", "--run-for", "7.5" });

            Assert.Equal("PhysicsEntityScene.xml", options.Scene);
            Assert.Equal(7.5, options.RunForSeconds);
        }

        [Fact]
        public void Parse_RunForFloatingPoint_ParsesAsDouble()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--run-for", "2.5" });

            Assert.Equal(2.5, options.RunForSeconds);
        }

        [Fact]
        public void Parse_NoFocusPauseFlag_SetsNoFocusPause()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--no-focus-pause" });

            Assert.True(options.NoFocusPause);
            Assert.Equal(SceneLaunchOptionsParser.DefaultScene, options.Scene);
            Assert.Null(options.RunForSeconds);
        }

        [Fact]
        public void Parse_NoFocusPauseFlag_WithOtherOptions_SetsAll()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--scene", "CharacterScene.xml", "--run-for", "6", "--no-focus-pause" });

            Assert.Equal("CharacterScene.xml", options.Scene);
            Assert.Equal(6.0, options.RunForSeconds);
            Assert.True(options.NoFocusPause);
        }

        [Fact]
        public void Parse_NoFocusPauseFlag_AppearsMultipleTimes_StillTrue()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--no-focus-pause", "--no-focus-pause" });

            Assert.True(options.NoFocusPause);
        }

        [Fact]
        public void Parse_UnknownFlag_IsIgnoredAndDefaultsRemain()
        {
            var options = SceneLaunchOptionsParser.Parse(new[] { "--verbose", "--scene", "BallScene.xml" });

            Assert.Equal("BallScene.xml", options.Scene);
            Assert.Null(options.RunForSeconds);
            Assert.False(options.NoFocusPause);
        }

        [Fact]
        public void Parse_SceneMissingValue_Throws()
        {
            Assert.Throws<ArgumentException>(() => SceneLaunchOptionsParser.Parse(new[] { "--scene" }));
        }

        [Fact]
        public void Parse_RunForMissingValue_Throws()
        {
            Assert.Throws<ArgumentException>(() => SceneLaunchOptionsParser.Parse(new[] { "--run-for" }));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-3")]
        [InlineData("abc")]
        public void Parse_RunForNonPositiveOrInvalid_Throws(string value)
        {
            Assert.Throws<ArgumentException>(() => SceneLaunchOptionsParser.Parse(new[] { "--run-for", value }));
        }
    }
}
