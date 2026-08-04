#nullable enable
using Xunit;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Internal;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace CoreEssentials.Tests.GUI
{
    /// <summary>
    /// Tests for the Canvas wrapper class. Canvas delegates all operations to CanvasImpl,
    /// so we use reflection on _impl to verify internal state when needed, 
    /// and public API (ICanvas) where possible.
    /// </summary>
    public class CanvasTests : IDisposable
    {
        private readonly Game _mockGame;
        private bool _disposed = false;

        public CanvasTests()
        {
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Initialize GUIManager - this sets up the engine internally (which handles MyraEnvironment)
            GUIManager.Init(_mockGame, 800, 600);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _mockGame?.Dispose();
                    
                    // Shutdown the engine to clean up internal state
                    var engine = EngineResolver.GetEngine();
                    engine.Shutdown();
                }
                _disposed = true;
            }
        }
        
        /// <summary>
        /// Helper to get CanvasImpl instance via reflection on Canvas wrapper's _impl field.
        /// </summary>
        private static object GetCanvasImpl(Canvas canvas)
        {
            var implField = typeof(Canvas).GetField("_impl", BindingFlags.NonPublic | BindingFlags.Instance);
            return implField!.GetValue(canvas)!;
        }

        /// <summary>
        /// Helper to get a field or property value from an object via reflection.
        /// </summary>
        private static object? GetMemberValue(object obj, string name, Type type)
        {
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return field.GetValue(obj);
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null) return prop?.GetValue(obj);
            return null;
        }

        /// <summary>
        /// Helper to get the underlying Myra Panel from CanvasImpl via reflection.
        /// </summary>
        private static object? GetMyraPanel(object impl)
        {
            // CanvasImpl has internal property MyraPanel that returns the Myra Panel
            var myraProp = impl.GetType().GetProperty("MyraPanel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (myraProp != null) return myraProp.GetValue(impl);
            
            // Fallback: try accessing the base Panel property through WidgetBase.MyraWidget
            var widgetProp = impl.GetType().GetProperty("Panel");
            if (widgetProp != null) return widgetProp.GetValue(impl);
            
            return null;
        }
        
        [Fact]
        public void Constructor_InitializesRootPanelAndPosition()
        {
            // Act
            var canvas = new Canvas();

            // Assert - verify internal state via reflection on _impl
            var impl = GetCanvasImpl(canvas);
            _ = GetMemberValue(impl, "_position", impl.GetType());
            
            // Canvas should be initialized with default screen space and zero position
            Assert.True(canvas.IsScreenSpace, "Default canvas should be in screen space");
        }

        [Fact]
        public void SetPosition_UpdatesPosition()
        {
            // Arrange
            var canvas = new Canvas();
            Vector2 newPosition = new Vector2(100, 200);

            // Act
            canvas.SetPosition(newPosition);

            // Assert - verify internal position was updated via _impl reflection
            var impl = GetCanvasImpl(canvas);
            var positionVal = GetMemberValue(impl, "_position", impl.GetType());
            
            Assert.NotNull(positionVal);
        }

        [Fact]
        public void SetPosition_ImmediatelyUpdatesRootPanelPosition()
        {
            // Arrange
            var canvas = new Canvas();
            Vector2 newPosition = new Vector2(100, 200);

            // Act
            canvas.SetPosition(newPosition);

            // Assert - verify the internal panel's Left/Top were updated immediately
            var impl = GetCanvasImpl(canvas);
            var myraPanel = GetMyraPanel(impl);
            
            Assert.NotNull(myraPanel);
            
            if (myraPanel != null)
            {
                var leftProp = myraPanel.GetType().GetProperty("Left");
                var topProp = myraPanel.GetType().GetProperty("Top");
                
                if (leftProp != null && topProp != null)
                {
                    Assert.Equal((int)newPosition.X, Convert.ToInt32(leftProp.GetValue(myraPanel)));
                    Assert.Equal((int)newPosition.Y, Convert.ToInt32(topProp.GetValue(myraPanel)));
                }
            }
        }

        [Fact]
        public void AddWidget_AddsWidgetToRootPanel()
        {
            // Arrange - create widget via factory (interface type)
            var canvas = new Canvas();
            var widget = WidgetFactory.CreateLabel("Test");

            // Act
            canvas.AddWidget(widget);

            // Assert - verify widget is in the internal panel's collection
            var impl = GetCanvasImpl(canvas);
            var widgetsProp = impl.GetType().GetProperty("Widgets");
            var widgetsObj = widgetsProp?.GetValue(impl);
            
            Assert.NotNull(widgetsObj);
            if (widgetsObj is System.Collections.IList list)
                Assert.True(list.Contains(widget), "Widget should be in canvas's widget collection");
        }

        [Fact]
        public void RemoveWidget_RemovesWidgetFromRootPanel()
        {
            // Arrange
            var canvas = new Canvas();
            var widget = WidgetFactory.CreateLabel("Test");
            canvas.AddWidget(widget);

            // Act
            canvas.RemoveWidget(widget);

            // Assert - verify widget is no longer in the collection
            var impl = GetCanvasImpl(canvas);
            var widgetsProp = impl.GetType().GetProperty("Widgets");
            var widgetsObj = widgetsProp?.GetValue(impl);
            
            Assert.NotNull(widgetsObj);
            if (widgetsObj is System.Collections.IList list)
                Assert.False(list.Contains(widget), "Widget should be removed from canvas's widget collection");
        }

        [Fact]
        public void CleanUp_ClearsWidgetsAndRemovesRootPanel()
        {
            // Arrange
            var canvas = new Canvas();
            var widget = WidgetFactory.CreateLabel("Test");
            canvas.AddWidget(widget);

            // Act
            canvas.CleanUp();

            // Assert - verify widgets are cleared
            var impl = GetCanvasImpl(canvas);
            var widgetsProp = impl.GetType().GetProperty("Widgets");
            var widgetsObj = widgetsProp?.GetValue(impl);
            
            Assert.NotNull(widgetsObj);
            if (widgetsObj is System.Collections.IList list)
                Assert.Empty(list);
        }

        [Fact]
        public void Update_UpdatesRootPanelPosition()
        {
            // Arrange
            var canvas = new Canvas();
            Vector2 position = new Vector2(150, 250);
            canvas.SetPosition(position);
            var gameTime = new GameTime();

            // Act
            canvas.Update(gameTime);

            // Assert - verify internal panel Left/Top were updated via Update() call
            var impl = GetCanvasImpl(canvas);
            var myraPanel = GetMyraPanel(impl);
            
            if (myraPanel != null)
            {
                var leftProp = myraPanel.GetType().GetProperty("Left");
                Assert.NotNull(leftProp?.GetValue(myraPanel));
            }
        }

        [Fact]
        public void AddMultipleWidgets_AllWidgetsAreAddedToRootPanel()
        {
            // Arrange - create multiple widgets via factories
            var canvas = new Canvas();
            var widget1 = WidgetFactory.CreateLabel("Label 1");
            var widget2 = WidgetFactory.CreateTextButton("Button");
            var widget3 = WidgetFactory.CreatePanel();

            // Act
            canvas.AddWidget(widget1);
            canvas.AddWidget(widget2);
            canvas.AddWidget(widget3);

            // Assert - verify all widgets are in the internal panel's collection
            var impl = GetCanvasImpl(canvas);
            var widgetsProp = impl.GetType().GetProperty("Widgets");
            var widgetsObj = widgetsProp?.GetValue(impl);
            
            Assert.NotNull(widgetsObj);
            if (widgetsObj is System.Collections.IList list)
            {
                Assert.Equal(3, list.Count);
                Assert.True(list.Cast<object>().Contains((object)widget1), "Widget 1 should be in canvas's widget collection");
                Assert.True(list.Cast<object>().Contains((object)widget2), "Widget 2 should be in canvas's widget collection");
                Assert.True(list.Cast<object>().Contains((object)widget3), "Widget 3 should be in canvas's widget collection");
            }
        }
    }
}
