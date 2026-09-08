using System;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GUI;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.Coroutines;

namespace CoreEssentials.Tests.SceneManagement
{
    /// <summary>
    /// Regression tests for a data-driven scene bug: nested &lt;Children&gt; entity definitions that
    /// carry GUI widget components (e.g. a LabelComponent) were being instantiated independently —
    /// each child's components attached BEFORE the parent/child link existed — so a child's
    /// LabelComponent could not find its ancestor CanvasComponent and threw "No CanvasComponent found".
    /// The fix makes DataDrivenScene build + link the whole subtree first, then attach components
    /// pre-order (parents before children), matching EntityPrefabLoader.
    /// </summary>
    public class DataDrivenSceneNestedGuiTests : IDisposable
    {
        private readonly Game _mockGame;

        public DataDrivenSceneNestedGuiTests()
        {
            // Canvas/Label components require the GUI engine to be initialized (as in a running game).
            _mockGame = new Game1();
            GUIManager.Init(_mockGame, 800, 600);
        }

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _mockGame?.Dispose();
            CoreEssentials.GUI.Internal.EngineResolver.GetEngine()?.Shutdown();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void NestedCanvasRootAndLabelChild_Loads_WithoutThrowing()
        {
            // Arrange — a canvas root with a nested anchored label child (the idiomatic GUI layout).
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity"" Id=""root"">
          <Position X=""0"" Y=""0"" />
          <Components><Component Type=""CanvasComponent"" /></Components>
          <Children>
            <EntityDefinition Type=""CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity"" Id=""label"">
              <Components>
                <Component Type=""AnchorComponent""><Properties>
                  <Property Name=""Preset"" Value=""MiddleCenter"" />
                </Properties></Component>
                <Component Type=""LabelComponent""><Properties>
                  <Property Name=""Text"" Value=""hi"" />
                </Properties></Component>
              </Components>
            </EntityDefinition>
          </Children>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));

                // Act — drive the load to completion, capturing any exception (the bug threw here).
                scene.Load();
                Assert.Null(Record.Exception(() =>
                {
                    for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                        helper.Tick();
                }));

                // Assert — the scene fully loaded and the nested label child exists with its component.
                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();
                var root = entitySystem.FindById("root");
                Assert.NotNull(root);
                Assert.NotNull(root!.GetComponent<CanvasComponent>());

                var label = entitySystem.FindById("label");
                Assert.NotNull(label);
                // The child's LabelComponent only survives OnAttach if it found the ancestor canvas.
                Assert.NotNull(label!.GetComponent<LabelComponent>());
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void DeeplyNestedGuiChild_Loads_WithoutThrowing()
        {
            // Arrange — three levels deep: canvas -> panel -> label. Ensures the fix is not just
            // one level of nesting.
            var xml = @"<Scene>
  <GameSystems>
    <System Type=""EntitySystem"">
      <Entities>
        <EntityDefinition Type=""CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity"" Id=""root"">
          <Position X=""0"" Y=""0"" />
          <Components><Component Type=""CanvasComponent"" /></Components>
          <Children>
            <EntityDefinition Type=""CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity"" Id=""panel"">
              <Children>
                <EntityDefinition Type=""CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity"" Id=""deepLabel"">
                  <Components>
                    <Component Type=""LabelComponent""><Properties>
                      <Property Name=""Text"" Value=""deep"" />
                    </Properties></Component>
                  </Components>
                </EntityDefinition>
              </Children>
            </EntityDefinition>
          </Children>
        </EntityDefinition>
      </Entities>
    </System>
  </GameSystems>
</Scene>";

            var helper = new CoroutineTestHelper();
            try
            {
                AssetManager.Init(new MockContentManager());
                var scene = new DataDrivenScene(SceneParser.Parse(xml));

                // Act
                scene.Load();
                Assert.Null(Record.Exception(() =>
                {
                    for (int i = 0; i < 30 && !scene.IsLoaded; i++)
                        helper.Tick();
                }));

                // Assert
                Assert.True(scene.IsLoaded);
                var entitySystem = scene.GetGameSystem<EntitySystem>();
                var deepLabel = entitySystem.FindById("deepLabel");
                Assert.NotNull(deepLabel);
                Assert.NotNull(deepLabel!.GetComponent<LabelComponent>());
            }
            finally
            {
                helper.Cleanup();
            }
        }
    }
}
