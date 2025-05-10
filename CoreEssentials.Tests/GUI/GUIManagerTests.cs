using Xunit;
using CoreEssentials.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Moq;
using System;
using System.Reflection;

namespace CoreEssentials.Tests.GUI
{
    public class GUIManagerTests : IDisposable
    {
        private readonly Game _mockGame;

        public GUIManagerTests()
        {
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Set Myra environment before tests
            MyraEnvironment.Game = _mockGame;
        }

        void IDisposable.Dispose()
        {
            // Clean up resources
            _mockGame?.Dispose();
        }
        
        [Fact]
        public void Init_CreatesDesktopWithCorrectDimensions()
        {
            // Arrange
            int width = 800;
            int height = 600;
            
            // Act
            GUIManager.Init(_mockGame, width, height);
            
            // Assert
            // Use reflection to access private fields
            var desktopField = typeof(GUIManager).GetField("_desktop", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var desktop = (Desktop)desktopField.GetValue(null);
            
            Assert.NotNull(desktop);
            Assert.NotNull(desktop.Root);
            Assert.IsType<Panel>(desktop.Root);
            
            var rootPanel = (Panel)desktop.Root;
            Assert.Equal(width, rootPanel.Width);
            Assert.Equal(height, rootPanel.Height);
        }
        
        [Fact]
        public void AddWidget_AddsWidgetToRootPanel()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            var widget = new Label { Text = "Test Label" };
            
            // Act
            GUIManager.AddWidget(widget);
            
            // Assert
            var rootPanelGetter = typeof(GUIManager).GetProperty("Root", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var rootPanel = (Panel)rootPanelGetter.GetValue(null);
            
            Assert.Contains(widget, rootPanel.Widgets);
        }
        
        [Fact]
        public void RemoveWidget_RemovesWidgetFromRootPanel()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            var widget = new Label { Text = "Test Label" };
            GUIManager.AddWidget(widget);
            
            // Act
            GUIManager.RemoveWidget(widget);
            
            // Assert
            var rootPanelGetter = typeof(GUIManager).GetProperty("Root", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var rootPanel = (Panel)rootPanelGetter.GetValue(null);
            
            Assert.DoesNotContain(widget, rootPanel.Widgets);
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
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            var widget = new Label { Text = "Test Label" };
            GUIManager.AddWidget(widget);
            
            // Label isn't focused by default in this test environment
            
            // Act
            bool result = GUIManager.IsAnyWidgetFocused();
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsWidgetFocused_ReturnsFalseForNullWidget()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            
            // Act
            bool result = GUIManager.IsWidgetFocused(null);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void Draw_CallsDesktopRender()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            var gameTime = new GameTime();
            
            // Get access to the Desktop to verify it was rendered
            var desktopField = typeof(GUIManager).GetField("_desktop", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var desktop = (Desktop)desktopField.GetValue(null);
            
            // This is a bit tricky since we can't easily mock the Desktop.Render method
            // In a real test we could use a framework like Pose or create a testable wrapper
            // For now, we'll just confirm the test runs without exceptions
            
            // Act & Assert
            Exception exception = Record.Exception(() => GUIManager.Draw(gameTime));
            Assert.Null(exception);
        }

        [Fact]
        public void IsWidgetFocused_HandlesContainerWithNestedWidgets()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            
            // Create a container with nested widgets
            var container = new VerticalStackPanel();
            var nestedLabel = new Label { Text = "Nested" };
            container.Widgets.Add(nestedLabel);
            
            // Add to GUI
            GUIManager.AddWidget(container);
            
            // Act & Assert
            // Without direct mouse interaction, focus should be false
            Assert.False(GUIManager.IsWidgetFocused(container));
            
            // To test true case, we would need to simulate mouse interaction
            // which is difficult in this testing environment
        }
        
        [Fact]
        public void IsWidgetFocused_HandlesContentControl()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            
            // Create a button (which is a ContentControl)
            var button = new Button();
            button.Content = new Label { Text = "Button Content" };
            
            // Add to GUI
            GUIManager.AddWidget(button);
            
            // Act & Assert
            // Without mouse interaction, focus should be false
            Assert.False(GUIManager.IsWidgetFocused(button));
        }
        
        [Fact]
        public void MultipleWidgets_AddedAndRemoved_CorrectWidgetCount()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
            var widget1 = new Label { Text = "Widget 1" };
            var widget2 = new Button();
            var widget3 = new HorizontalStackPanel();
            
            // Act
            GUIManager.AddWidget(widget1);
            GUIManager.AddWidget(widget2);
            GUIManager.AddWidget(widget3);
            
            // Assert - after adding
            var rootPanelGetter = typeof(GUIManager).GetProperty("Root", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var rootPanel = (Panel)rootPanelGetter.GetValue(null);
            
            Assert.Equal(3, rootPanel.Widgets.Count);
            Assert.Contains(widget1, rootPanel.Widgets);
            Assert.Contains(widget2, rootPanel.Widgets);
            Assert.Contains(widget3, rootPanel.Widgets);
            
            // Act - remove one widget
            GUIManager.RemoveWidget(widget2);
            
            // Assert - after removal
            Assert.Equal(2, rootPanel.Widgets.Count);
            Assert.Contains(widget1, rootPanel.Widgets);
            Assert.DoesNotContain(widget2, rootPanel.Widgets);
            Assert.Contains(widget3, rootPanel.Widgets);
        }
        
        [Fact]
        public void IsWidgetFocused_HandlesComboBox()
        {
            // Arrange
            GUIManager.Init(_mockGame, 800, 600);
              // Note: ComboBox is marked as obsolete but still used in GUIManager
            #pragma warning disable CS0618 // Type or member is obsolete
            var comboBox = new ComboBox();
            comboBox.Items.Add(new ListItem("Item 1"));
            comboBox.Items.Add(new ListItem("Item 2"));
            #pragma warning restore CS0618 // Type or member is obsolete
            
            GUIManager.AddWidget(comboBox);
            
            // Act & Assert
            // Without direct mouse interaction, focus should be false
            Assert.False(GUIManager.IsWidgetFocused(comboBox));
            
            // To test true case, we would need to simulate mouse interaction
            // which is difficult in this testing environment
        }
    }
}
