using System;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    /// <summary>
    /// Exercises the strict, device-free schema-validation branches of <see cref="SceneParser"/>.
    /// Each case feeds a small XML string and asserts the specific parse error is surfaced as a
    /// <see cref="FormatException"/>. None of these reach the content manager, so no mocks are needed.
    /// </summary>
    public class SceneParserErrorTests
    {
        [Fact]
        public void Parse_MalformedXml_Throws()
        {
            Assert.Throws<FormatException>(() => SceneParser.Parse("<Scene><GameSystems></Scene>"));
        }

        [Fact]
        public void Parse_WrongRootElement_Throws()
        {
            Assert.Throws<FormatException>(() => SceneParser.Parse("<Root><GameSystems /></Root>"));
        }

        [Fact]
        public void Parse_UnknownAttributeOnScene_Throws()
        {
            Assert.Throws<FormatException>(
                () => SceneParser.Parse(@"<Scene Name=""x""><GameSystems /></Scene>"));
        }

        [Fact]
        public void Parse_MissingGameSystems_Throws()
        {
            Assert.Throws<FormatException>(() => SceneParser.Parse("<Scene />"));
        }

        [Theory]
        [InlineData("<Scene></Scene>")]                                // zero children
        [InlineData("<Scene><A /><B /></Scene>")]                      // two non-GameSystems children
        [InlineData("<Scene><GameSystems /><GameSystems /></Scene>")]  // two GameSystems
        public void Parse_NotExactlyOneGameSystems_Throws(string xml)
        {
            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_UnknownAttributeOnGameSystems_Throws()
        {
            Assert.Throws<FormatException>(
                () => SceneParser.Parse(@"<Scene><GameSystems Name=""x"" /></Scene>"));
        }

        [Theory]
        [InlineData("<Scene><GameSystems><System /></GameSystems></Scene>")]                              // missing Type (must be FormatException, not NRE)
        [InlineData("<Scene><GameSystems><System Type=\"\" /></GameSystems></Scene>")]                    // empty Type
        [InlineData("<Scene><GameSystems><System Type=\"EntitySystem\" Config=\"\" /></GameSystems></Scene>")] // empty Config
        public void Parse_SystemAttributeErrors_Throw(string xml)
        {
            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_UnknownElementInsideSystem_Throws()
        {
            Assert.Throws<FormatException>(
                () => SceneParser.Parse(@"<Scene><GameSystems><System Type=""EntitySystem""><Bogus /></System></GameSystems></Scene>"));
        }

        [Fact]
        public void ResolveSystemType_Unknown_Throws()
        {
            Assert.Throws<FormatException>(() => SceneParser.ResolveSystemType("NoSuchSystem"));
        }

        [Theory]
        [InlineData(@"<EntityDefinition Type=""ProbeEntity"" Source=""p"" Id=""a"" />")] // both set
        [InlineData(@"<EntityDefinition Id=""a"" />")]                                   // neither set
        public void Parse_EntityDefinitionTypeSourceConflict_Throws(string entityDef)
        {
            var xml = $@"<Scene><GameSystems><System Type=""EntitySystem""><Entities>{entityDef}</Entities></System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_SourceNotRegistered_Throws()
        {
            // No <Prefabs> registered, so a Source reference cannot resolve and fails fast.
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
                          <Entities><EntityDefinition Source=""missing"" Id=""a"" /></Entities>
                        </System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_UnknownElementInsideEntityDefinition_Throws()
        {
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
                          <Entities><EntityDefinition Type=""ProbeEntity"" Id=""a""><Bogus /></EntityDefinition></Entities>
                        </System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_TagMissingName_Throws()
        {
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
                          <Entities><EntityDefinition Type=""ProbeEntity"" Id=""a""><Tags><Tag /></Tags></EntityDefinition></Entities>
                        </System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_ComponentMissingType_Throws()
        {
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
                          <Entities><EntityDefinition Type=""ProbeEntity"" Id=""a""><Components><Component /></Components></EntityDefinition></Entities>
                        </System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_PropertyMissingName_Throws()
        {
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
                          <Entities>
                            <EntityDefinition Type=""ProbeEntity"" Id=""a"">
                              <Components><Component Type=""SingleMatchComponent""><Properties><Property /></Properties></Component></Components>
                            </EntityDefinition>
                          </Entities>
                        </System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_PositionUnknownAttribute_Throws()
        {
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
                          <Entities>
                            <EntityDefinition Type=""ProbeEntity"" Id=""a""><Position X=""1"" Y=""2"" Z=""3"" /></EntityDefinition>
                          </Entities>
                        </System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

        [Fact]
        public void Parse_DuplicateEntityId_Throws()
        {
            var xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
                          <Entities>
                            <EntityDefinition Type=""ProbeEntity"" Id=""dup"" />
                            <EntityDefinition Type=""ProbeEntity"" Id=""dup"" />
                          </Entities>
                        </System></GameSystems></Scene>";

            Assert.Throws<FormatException>(() => SceneParser.Parse(xml));
        }

    }
}
