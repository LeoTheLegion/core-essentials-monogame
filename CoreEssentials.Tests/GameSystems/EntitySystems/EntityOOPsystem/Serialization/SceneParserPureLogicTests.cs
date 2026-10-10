#nullable enable
using System;
using System.IO;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization;

/// <summary>
/// Device-free tests for the parts of <see cref="SceneParser"/> that are pure string/XML/reflection
/// logic: game-system type resolution (built-in table, short-name reflection fallback, unknown throw)
/// and the &lt;Prefab&gt; registration guards that fail before any asset is loaded. None of these paths
/// touch a GraphicsDevice or the content manager, so no fixtures beyond a bare <c>GameSystem</c> are needed.
/// </summary>
public class SceneParserPureLogicTests
{
    // ──────────────────────────── ResolveSystemType ────────────────────────────

    [Fact]
    public void ResolveSystemType_BuiltInShortName_ResolvesWithoutScanning()
    {
        Assert.Equal(typeof(EntitySystem), SceneParser.ResolveSystemType("EntitySystem"));
        Assert.Equal(
            typeof(CoreEssentials.GameSystems.Physics.Engines.Aether.PhysicsEngine),
            SceneParser.ResolveSystemType("PhysicsEngine"));
    }

    [Fact]
    public void ResolveSystemType_FullyQualifiedCustomSystem_ResolvesViaGetType()
    {
        // A fully-qualified name is found by Type.GetType on one of the loaded assemblies.
        var resolved = SceneParser.ResolveSystemType(typeof(SceneParserFallbackSystem).FullName!);

        Assert.Equal(typeof(SceneParserFallbackSystem), resolved);
    }

    [Fact]
    public void ResolveSystemType_ShortNameCustomSystem_ResolvesViaReflectionFallback()
    {
        // Not a built-in, and a bare simple name is not resolvable by Type.GetType — so the
        // OrdinalIgnoreCase assembly scan must locate it by type name.
        var resolved = SceneParser.ResolveSystemType("SceneParserFallbackSystem");

        Assert.Equal(typeof(SceneParserFallbackSystem), resolved);
    }

    [Fact]
    public void ResolveSystemType_UnknownName_ThrowsFormat()
    {
        var ex = Assert.Throws<FormatException>(() => SceneParser.ResolveSystemType("NoSuchGameSystemAtAll"));

        Assert.Contains("Could not resolve game system type", ex.Message);
    }

    // ──────────────────────────── <Prefab> registration guards (throw before any load) ────────────────────────────

    [Fact]
    public void Parse_PrefabMissingName_ThrowsBeforeLoadingAsset()
    {
        const string xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
            <Prefabs><Prefab Asset=""whatever.xml"" /></Prefabs>
        </System></GameSystems></Scene>";

        var ex = Assert.Throws<FormatException>(() => SceneParser.Parse(xml));

        Assert.Contains("missing its required 'Name'", ex.Message);
    }

    [Fact]
    public void Parse_PrefabMissingAsset_ThrowsBeforeLoadingAsset()
    {
        const string xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
            <Prefabs><Prefab Name=""p"" /></Prefabs>
        </System></GameSystems></Scene>";

        var ex = Assert.Throws<FormatException>(() => SceneParser.Parse(xml));

        Assert.Contains("missing its required 'Asset'", ex.Message);
    }

    [Fact]
    public void Parse_DuplicatePrefab_Throws()
    {
        // The duplicate check runs after the first prefab loads, so both assets must exist.
        WriteContentAsset("DupA.xml", @"<Prefab Type=""SceneParserProbeEntity"" />");
        WriteContentAsset("DupB.xml", @"<Prefab Type=""SceneParserProbeEntity"" />");
        AssetManager.Init(new MockContentManager());

        const string xml = @"<Scene><GameSystems><System Type=""EntitySystem"">
            <Prefabs>
                <Prefab Name=""p"" Asset=""DupA.xml"" />
                <Prefab Name=""P"" Asset=""DupB.xml"" />
            </Prefabs>
        </System></GameSystems></Scene>";

        var ex = Assert.Throws<FormatException>(() => SceneParser.Parse(xml));

        Assert.Contains("Duplicate prefab registration", ex.Message);
    }

    private static void WriteContentAsset(string fileName, string xml)
    {
        var contentDir = Path.Combine(AppContext.BaseDirectory, "Content");
        Directory.CreateDirectory(contentDir);
        File.WriteAllText(Path.Combine(contentDir, fileName), xml);
    }
}

/// <summary>Concrete, non-abstract game system used to exercise the short-name reflection fallback in type resolution.</summary>
public class SceneParserFallbackSystem : GameSystem
{
}

/// <summary>Entity referenced by the duplicate-prefab test's prefab assets so they resolve during load.</summary>
public class SceneParserProbeEntity : Entity
{
    public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
}
