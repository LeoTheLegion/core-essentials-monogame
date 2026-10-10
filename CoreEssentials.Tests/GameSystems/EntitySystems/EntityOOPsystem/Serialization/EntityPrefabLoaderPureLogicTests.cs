#nullable enable
using System;
using System.IO;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization;

/// <summary>
/// Device-free tests for the pure XML→model and reflection logic in <see cref="EntityPrefabLoader"/>:
/// the file/parse guards, a full-document parse exercising tags, components (properties + effect
/// parameters), nested children, binds, and scalar attributes, plus entity/component type resolution.
/// None of these paths touch a GraphicsDevice or the content manager.
/// </summary>
public class EntityPrefabLoaderPureLogicTests
{
    // ──────────────────────────── Parse guards ────────────────────────────

    [Fact]
    public void LoadFromFile_MissingFile_ThrowsFileNotFound()
    {
        var ex = Assert.Throws<FileNotFoundException>(() => EntityPrefabLoader.LoadFromFile(@"C:\does\not\exist-{guid}.xml"));

        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadFromXml_RootNotPrefab_Throws()
    {
        var ex = Assert.Throws<FormatException>(() => EntityPrefabLoader.LoadFromXml(@"<NotAPrefab Type=""X"" />"));

        Assert.Contains("must be 'Prefab'", ex.Message);
    }

    [Fact]
    public void LoadFromXml_MissingTypeAttribute_Throws()
    {
        var ex = Assert.Throws<FormatException>(() => EntityPrefabLoader.LoadFromXml(@"<Prefab />"));

        Assert.Contains("missing required 'Type'", ex.Message);
    }

    // ──────────────────────────── Full document parse (tags, components, children, binds, scalars) ────────────────────────────

    [Fact]
    public void LoadFromXml_FullDocument_PopulatesEveryPart()
    {
        const string xml = @"<Prefab Type=""RootEntity"" Rotation=""15"" Sort=""2"" ZLayer=""3"" Active=""false"">
            <Tags><Tag Name=""Player"" /><Tag Name=""Mover"" /></Tags>
            <Components>
                <Component Type=""SomeComponent"">
                    <Properties><Property Name=""Speed"" Value=""42"" /></Properties>
                    <EffectParameter Name=""Intensity"" Value=""0.5"" />
                </Component>
                <Component Type=""NoTypeProps"" />
            </Components>
            <Bind Event=""Clicked"" Command=""DoThing"" />
            <Children>
                <Prefab Type=""ChildEntity"" Sort=""9"" Active=""true"">
                    <Tags><Tag Name=""Kid"" /></Tags>
                    <Bind Event=""Pinged"" Command=""OnPing"" />
                </Prefab>
            </Children>
        </Prefab>";

        var prefab = EntityPrefabLoader.LoadFromXml(xml);

        // Scalars with non-default values.
        Assert.Equal("RootEntity", prefab.Type);
        Assert.Equal(15f, prefab.Rotation);
        Assert.Equal(2, prefab.Sort);
        Assert.Equal(3, prefab.ZLayer);
        Assert.False(prefab.Active);

        // Tags.
        Assert.Contains("Player", prefab.Tags);
        Assert.Contains("Mover", prefab.Tags);

        // Components: properties + effect parameters parsed; a component without props still appears.
        var some = Assert.Single(prefab.Components, c => c.Type == "SomeComponent");
        Assert.Equal("42", some.Properties["Speed"]);
        Assert.Equal("0.5", some.EffectParameters["Intensity"]);
        Assert.Contains(prefab.Components, c => c.Type == "NoTypeProps");

        // Direct bind captured.
        Assert.Single(prefab.Binds);
        Assert.Equal("Clicked", prefab.Binds[0].Attribute("Event")!.Value);

        // Nested child parsed as its own template (rotation/sort/active + tags + bind).
        var child = Assert.Single(prefab.Children);
        Assert.Equal("ChildEntity", child.Type);
        Assert.Equal(9, child.Sort);
        Assert.True(child.Active);
        Assert.Contains("Kid", child.Tags);
        Assert.Single(child.Binds);
    }

    [Fact]
    public void LoadFromXml_OmittedScalars_DefaultSafely()
    {
        // No Rotation/Sort/ZLayer/Active attributes — must fall back to documented defaults.
        var prefab = EntityPrefabLoader.LoadFromXml(@"<Prefab Type=""T"" />");

        Assert.Equal(0f, prefab.Rotation);
        Assert.Equal(0, prefab.Sort);
        Assert.Equal(0, prefab.ZLayer);
        Assert.True(prefab.Active);
    }

    // ──────────────────────────── Type resolution (reflection only) ────────────────────────────

    [Fact]
    public void ResolveEntityType_KnownEntity_ByShortName()
    {
        var resolved = EntityPrefabLoader.ResolveEntityType("EntityPrefabLoaderProbeEntity");

        Assert.Equal(typeof(EntityPrefabLoaderProbeEntity), resolved);
    }

    [Fact]
    public void ResolveComponentType_KnownComponent_ByShortName()
    {
        var resolved = EntityPrefabLoader.ResolveComponentType("EntityPrefabLoaderProbeComponent");

        Assert.Equal(typeof(EntityPrefabLoaderProbeComponent), resolved);
    }

    [Fact]
    public void ResolveEntityType_Unknown_ReturnsNull()
    {
        Assert.Null(EntityPrefabLoader.ResolveEntityType("NoSuchEntityTypeAnywhere"));
    }

    [Fact]
    public void ResolveComponentType_Unknown_ReturnsNull()
    {
        Assert.Null(EntityPrefabLoader.ResolveComponentType("NoSuchComponentTypeAnywhere"));
    }
}

/// <summary>Concrete entity used to exercise type resolution by short name.</summary>
public class EntityPrefabLoaderProbeEntity : Entity
{
    public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch) { }
}

/// <summary>Concrete component used to exercise type resolution by short name.</summary>
public class EntityPrefabLoaderProbeComponent : EntityComponent
{
}
