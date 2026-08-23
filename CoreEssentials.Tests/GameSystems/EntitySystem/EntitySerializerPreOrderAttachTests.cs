using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Tests that EntitySerializer attaches components in pre-order (parents before children)
/// when a scene contains &lt;Children&gt; hierarchies, so hierarchy-dependent components —
/// e.g. a LabelComponent resolving its CanvasComponent through the parent chain — can load
/// from XML without throwing "No CanvasComponent found".
/// </summary>
public class EntitySerializerPreOrderAttachTests : IDisposable
{
    private readonly Game _mockGame = null!;
    private bool _disposed;

    public EntitySerializerPreOrderAttachTests()
    {
        // Canvas/Label components create real Myra widgets on attach, so the GUI engine must be up.
        _mockGame = new Game1();
        GUIManager.Init(_mockGame, 800, 600);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _mockGame?.Dispose();
            EngineResolver.GetEngine().Shutdown();
        }
        _disposed = true;
    }

    private class CanvasRootEntity : Entity
    {
        public override void Render(SpriteBatch _spriteBatch) { }
    }

    private static string FqName(Type t) => t.FullName!;

    [Fact]
    public void LoadSceneFromXml_ChildLabelWithAncestorCanvas_SucceedsAndAttaches()
    {
        var system = new EntitySystem();
        var rootType = FqName(typeof(CanvasRootEntity));
        var xml = $@"
            <Scene>
                <EntityDefinition Type=""{rootType}"" Id=""hud"">
                    <Position X=""0"" Y=""0"" />
                    <Components>
                        <Component Type=""CanvasComponent"" />
                    </Components>
                    <Children>
                        <EntityDefinition Type=""{rootType}"" Id=""title"">
                            <Components>
                                <Component Type=""LabelComponent"">
                                    <Properties>
                                        <Property Name=""Text"" Value=""Hello XML"" />
                                    </Properties>
                                </Component>
                            </Components>
                        </EntityDefinition>
                    </Children>
                </EntityDefinition>
            </Scene>";

        var entities = EntitySerializer.LoadSceneFromXml(xml, system);

        Assert.Single(entities);
        var root = entities[0];
        var canvas = Assert.IsType<CanvasComponent>(root.GetComponent(typeof(CanvasComponent)));
        var child = Assert.Single(root.Children);
        var label = Assert.IsType<LabelComponent>(child.GetComponent(typeof(LabelComponent)));

        // The child's label widget must have been added to the ancestor's canvas.
        Assert.Single(canvas.Canvas.Children);
        Assert.Equal("Hello XML", label.Text);
    }

    [Fact]
    public void LoadSceneFromXml_DeepHierarchy_AllLevelsAttached()
    {
        var system = new EntitySystem();
        var t = FqName(typeof(CanvasRootEntity));
        var xml = $@"
            <Scene>
                <EntityDefinition Type=""{t}"" Id=""root"">
                    <Components>
                        <Component Type=""CanvasComponent"" />
                    </Components>
                    <Children>
                        <EntityDefinition Type=""{t}"" Id=""mid"">
                            <Components>
                                <Component Type=""LabelComponent"">
                                    <Properties><Property Name=""Text"" Value=""Mid"" /></Properties>
                                </Component>
                            </Components>
                            <Children>
                                <EntityDefinition Type=""{t}"" Id=""leaf"">
                                    <Components>
                                        <Component Type=""LabelComponent"">
                                            <Properties><Property Name=""Text"" Value=""Leaf"" /></Properties>
                                        </Component>
                                    </Components>
                                </EntityDefinition>
                            </Children>
                        </EntityDefinition>
                    </Children>
                </EntityDefinition>
            </Scene>";

        var entities = EntitySerializer.LoadSceneFromXml(xml, system);

        Assert.Single(entities);
        var canvas = (CanvasComponent)entities[0].GetComponent(typeof(CanvasComponent))!;
        // Both the mid and leaf labels resolve to the same ancestor canvas.
        Assert.Equal(2, canvas.Canvas.Children.Count);
    }

    [Fact]
    public void LoadSceneFromXml_FlatEntity_StillAttachesComponents()
    {
        // Regression guard: entities without children keep the original immediate-attach behavior.
        var system = new EntitySystem();
        var t = FqName(typeof(CanvasRootEntity));
        var xml = $@"
            <Scene>
                <EntityDefinition Type=""{t}"" Id=""solo"">
                    <Components>
                        <Component Type=""CanvasComponent"" />
                        <Component Type=""LabelComponent"">
                            <Properties><Property Name=""Text"" Value=""Solo"" /></Properties>
                        </Component>
                    </Components>
                </EntityDefinition>
            </Scene>";

        var entities = EntitySerializer.LoadSceneFromXml(xml, system);

        Assert.Single(entities);
        var canvas = (CanvasComponent)entities[0].GetComponent(typeof(CanvasComponent))!;
        Assert.Single(canvas.Canvas.Children);
    }

    [Fact]
    public void LoadSceneFromXml_ChildWithoutAncestorCanvas_StillThrows()
    {
        // The pre-order change must not hide genuine configuration errors:
        // a label with no canvas anywhere in its hierarchy still fails loudly.
        var system = new EntitySystem();
        var t = FqName(typeof(CanvasRootEntity));
        var xml = $@"
            <Scene>
                <EntityDefinition Type=""{t}"" Id=""orphan"">
                    <Children>
                        <EntityDefinition Type=""{t}"" Id=""labelless"">
                            <Components>
                                <Component Type=""LabelComponent"" />
                            </Components>
                        </EntityDefinition>
                    </Children>
                </EntityDefinition>
            </Scene>";

        Assert.Throws<InvalidOperationException>(() => EntitySerializer.LoadSceneFromXml(xml, system));
    }
}
