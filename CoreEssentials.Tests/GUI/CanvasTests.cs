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
{    public class CanvasTests : IDisposable
    {
        private readonly Game _mockGame;
        private readonly Mock<GraphicsDevice> _mockGraphicsDevice;

        public CanvasTests()
        {
            // Set up a mock GraphicsDevice
            _mockGraphicsDevice = new Mock<GraphicsDevice>();
            
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Use the real Game1 class from the test project
            // Set Myra environment before tests
            MyraEnvironment.Game = _mockGame;
        }
          void IDisposable.Dispose()
        {
            // Clean up resources
            _mockGame?.Dispose();
        }
        
        // Helper method to initialize GUIManager before tests
        private void InitializeGUIManager()
        {
            // Initialize GUIManager with real Game instance
            // This works because we've already set MyraEnvironment.Game
            GUIManager.Init(_mockGame, 800, 600);
        }
        
        [Fact]
        public void Constructor_InitializesRootPanelAndPosition()
        {
            // Arrange
            InitializeGUIManager();

            // Act
            var canvas = new Canvas();

            // Assert
            // Check that the position is Vector2.Zero
            // We need to use reflection to access private fields
            var positionProperty = typeof(Canvas).GetProperty("Position", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var position = (Vector2)positionProperty.GetValue(canvas);
            
            Assert.Equal(Vector2.Zero, position);
        }

        [Fact]
        public void SetPosition_UpdatesPosition()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas();
            Vector2 newPosition = new Vector2(100, 200);

            // Act
            canvas.SetPosition(newPosition);

            // Assert
            var positionProperty = typeof(Canvas).GetProperty("Position", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var position = (Vector2)positionProperty.GetValue(canvas);
            
            Assert.Equal(newPosition, position);
        }

        [Fact]
        public void AddWidget_AddsWidgetToRootPanel()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas();
            var widget = new Label();

            // Act
            canvas.AddWidget(widget);

            // Assert
            // Use reflection to access _rootPanel
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            Assert.Contains(widget, rootPanel.Widgets);
        }

        [Fact]
        public void RemoveWidget_RemovesWidgetFromRootPanel()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas();
            var widget = new Label();
            canvas.AddWidget(widget);

            // Act
            canvas.RemoveWidget(widget);

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            Assert.DoesNotContain(widget, rootPanel.Widgets);
        }

        [Fact]
        public void CleanUp_ClearsWidgetsAndRemovesRootPanel()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas();
            var widget = new Label();
            canvas.AddWidget(widget);

            // Act
            canvas.CleanUp();

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            Assert.Empty(rootPanel.Widgets);
            
            // We can't easily verify that GUIManager.RemoveWidget was called
            // because it's a static method and we can't mock it directly
        }

        [Fact]
        public void Update_UpdatesRootPanelPosition()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas();
            Vector2 position = new Vector2(150, 250);
            canvas.SetPosition(position);
            var gameTime = new GameTime();

            // Act
            canvas.Update(gameTime);

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            Assert.Equal((int)position.X, rootPanel.Left);
            Assert.Equal((int)position.Y, rootPanel.Top);
        }        [Fact]
        public void AddMultipleWidgets_AllWidgetsAreAddedToRootPanel()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas();
            var widget1 = new Label { Text = "Label 1" };
            var widget2 = new Button();
            var widget3 = new HorizontalStackPanel();

            // Act
            canvas.AddWidget(widget1);
            canvas.AddWidget(widget2);
            canvas.AddWidget(widget3);

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            Assert.Equal(3, rootPanel.Widgets.Count);
            Assert.Contains(widget1, rootPanel.Widgets);
            Assert.Contains(widget2, rootPanel.Widgets);
            Assert.Contains(widget3, rootPanel.Widgets);
        }
    }
}
