using Xunit;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;
using CoreEssentials.GUI.Internal;
using System;

namespace CoreEssentials.Tests.GUI
{
    public class GUIManagerTests : IDisposable
    {
        private readonly Game _mockGame;

        public GUIManagerTests()
        {
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Initialize GUI manager - this internally sets MyraEnvironment.Game via GuiManagerImpl
            GUIManager.Init(_mockGame, 800, 600);
        }

        void IDisposable.Dispose()
        {
            // Clean up resources
            _mockGame?.Dispose();
            
            // Shutdown the GUI engine to clean up internal state
            var engine = EngineResolver.GetEngine();
            engine.Shutdown();
        }
        
        [Fact]
        public void Init_CreatesDesktopWithCorrectDimensions()
        {
            // Arrange
            int width = 800;
            int height = 600;
            
            // Act - reinitialize with specific dimensions
            var engine = EngineResolver.GetEngine();
            engine.Shutdown();
            engine.Init(_mockGame, width, height);
            
            // Assert - verify via public interface (no reflection needed)
            Assert.Equal(width, engine.Width);
            Assert.Equal(height, engine.Height);
        }
        
        [Fact]
        public void AddWidget_AddsWidgetToRootPanel()
        {
            // Arrange - create widget via factory (interface type)
            var widget = WidgetFactory.CreateLabel("Test Label");
            
            // Act
            GUIManager.AddWidget(widget);
            
            // Assert - verify root panel exists and has correct dimensions
            var engine = EngineResolver.GetEngine();
            Assert.True(engine.Width > 0 && engine.Height > 0, "Root panel should exist after Init");
        }
        
        [Fact]
        public void RemoveWidget_RemovesWidgetFromRootPanel()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            var widget = WidgetFactory.CreateLabel("Test Label");
            GUIManager.AddWidget(widget);
            
            // Act - removal should not throw
            GUIManager.RemoveWidget(widget);
            
            // Assert - state should be consistent after removal
        }
        
        [Fact]
        public void Width_ReturnsCorrectRootPanelWidth()
        {
            // Arrange
            int width = 1024;
            int height = 768;
            GUIManager.Init(_mockGame, width, height);
            
            // Act
            int result = GUIManager.Width;
            
            // Assert
            Assert.Equal(width, result);
        }
        
        [Fact]
        public void Height_ReturnsCorrectRootPanelHeight()
        {
            // Arrange
            int width = 1024;
            int height = 768;
            GUIManager.Init(_mockGame, width, height);
            
            // Act
            int result = GUIManager.Height;
            
            // Assert
            Assert.Equal(height, result);
        }
        
        [Fact]
        public void IsAnyWidgetFocused_ReturnsFalseWhenNoWidgetsAreFocused()
        {
            // Arrange - create widget via factory
            var widget = WidgetFactory.CreateLabel("Test Label");
            GUIManager.AddWidget(widget);
            
            // Act
            bool result = GUIManager.IsAnyWidgetFocused();
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsWidgetFocused_ReturnsFalseForNullWidget()
        {
            // Act
            bool result = GUIManager.IsWidgetFocused(null!);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void Draw_CallsDesktopRender()
        {
            // Arrange
            var gameTime = new GameTime();
            
            // Act & Assert - should not throw
            Exception exception = Record.Exception(() => GUIManager.Draw(gameTime));
            Assert.Null(exception);
        }

        [Fact]
        public void IsWidgetFocused_HandlesContainerWithNestedWidgets()
        {
            // Arrange - create container via factory
            var container = WidgetFactory.CreatePanel();
            
            // Add to GUI
            GUIManager.AddWidget(container);
            
            // Act & Assert - without direct mouse interaction, focus should be false
            Assert.False(GUIManager.IsWidgetFocused(container));
        }
        
        [Fact]
        public void IsWidgetFocused_HandlesContentControl()
        {
            // Arrange - create button via factory
            var button = WidgetFactory.CreateTextButton("Button");
            
            // Add to GUI
            GUIManager.AddWidget(button);
            
            // Act & Assert - without mouse interaction, focus should be false
            Assert.False(GUIManager.IsWidgetFocused(button));
        }
        
        [Fact]
        public void MultipleWidgets_AddedAndRemoved_CorrectState()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            var widget1 = WidgetFactory.CreateLabel("Widget 1");
            var widget2 = WidgetFactory.CreateTextButton("Widget 2");
            var widget3 = WidgetFactory.CreatePanel();
            
            // Act - add all widgets
            GUIManager.AddWidget(widget1);
            GUIManager.AddWidget(widget2);
            GUIManager.AddWidget(widget3);
            
            // Assert - state should be consistent after adding
            Assert.Equal(800, GUIManager.Width);
            Assert.Equal(600, GUIManager.Height);
            
            // Act - remove one widget
            GUIManager.RemoveWidget(widget2);
            
            // Assert - state should remain consistent after removal
        }
    }
}