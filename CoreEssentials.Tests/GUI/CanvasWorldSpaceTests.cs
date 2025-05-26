using Xunit;
using CoreEssentials.GUI;
using CoreEssentials.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Moq;
using System;
using System.Reflection;

namespace CoreEssentials.Tests.GUI
{
    public class CanvasWorldSpaceTests : IDisposable
    {
        private readonly Game _mockGame;
        private readonly Mock<GraphicsDevice> _mockGraphicsDevice;
        private CoreEssentials.Cameras.Camera _testCamera;

        public CanvasWorldSpaceTests()
        {
            // Set up a mock GraphicsDevice
            _mockGraphicsDevice = new Mock<GraphicsDevice>();
            
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Set Myra environment before tests
            MyraEnvironment.Game = _mockGame;

            // Set up a test camera
            _testCamera = new Camera();
            Camera.SetMainCamera(_testCamera);
        }

        void IDisposable.Dispose()
        {
            // Clean up resources
            _mockGame?.Dispose();
            // Reset the main camera
            Camera.SetMainCamera(null);
        }
        
        // Helper method to initialize GUIManager before tests
        private void InitializeGUIManager()
        {
            // Initialize GUIManager with real Game instance
            GUIManager.Init(_mockGame, 800, 600);
        }

        [Fact]
        public void Constructor_WithWorldSpace_CreatesCanvasInWorldSpace()
        {
            // Arrange
            InitializeGUIManager();

            // Act
            var canvas = new Canvas(false); // false = world space

            // Assert
            // Use reflection to verify the _isScreenSpace field is set to false
            var isScreenSpaceField = typeof(Canvas).GetField("_isScreenSpace", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var isScreenSpace = (bool)isScreenSpaceField.GetValue(canvas);
            
            Assert.False(isScreenSpace);
        }

        [Fact]
        public void Constructor_WithScreenSpace_CreatesCanvasInScreenSpace()
        {
            // Arrange
            InitializeGUIManager();

            // Act
            var canvas = new Canvas(true); // true = screen space

            // Assert
            var isScreenSpaceField = typeof(Canvas).GetField("_isScreenSpace", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var isScreenSpace = (bool)isScreenSpaceField.GetValue(canvas);
            
            Assert.True(isScreenSpace);
        }

        [Fact]
        public void DefaultConstructor_CreatesCanvasInScreenSpace()
        {
            // Arrange
            InitializeGUIManager();

            // Act
            var canvas = new Canvas(); // Default constructor

            // Assert
            var isScreenSpaceField = typeof(Canvas).GetField("_isScreenSpace", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var isScreenSpace = (bool)isScreenSpaceField.GetValue(canvas);
            
            Assert.True(isScreenSpace);
        }

        [Fact]
        public void Update_WorldSpace_UpdatesRootPanelPositionBasedOnCamera()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 200);
            canvas.SetPosition(worldPosition);

            // Set up camera with known transformation
            _testCamera.Position = new Vector2(50, 50);
            _testCamera.Zoom = 1.0f;
            _testCamera.Rotation = 0.0f;

            // Act
            canvas.Update(new GameTime());

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            // Calculate expected screen position based on camera's WorldToScreen
            Vector2 expectedScreenPos = _testCamera.WorldToScreen(worldPosition);
            
            Assert.Equal((int)expectedScreenPos.X, rootPanel.Left);
            Assert.Equal((int)expectedScreenPos.Y, rootPanel.Top);
        }

        [Fact]
        public void Update_WorldSpaceWithZoomedCamera_ScalesPosition()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 200);
            canvas.SetPosition(worldPosition);

            // Set up camera with zoom
            _testCamera.Position = new Vector2(0, 0);
            _testCamera.Zoom = 2.0f; // 2x zoom
            _testCamera.Rotation = 0.0f;

            // Act
            canvas.Update(new GameTime());

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            // Calculate expected screen position with zoom
            Vector2 expectedScreenPos = _testCamera.WorldToScreen(worldPosition);
            
            Assert.Equal((int)expectedScreenPos.X, rootPanel.Left);
            Assert.Equal((int)expectedScreenPos.Y, rootPanel.Top);
        }

        [Fact]
        public void Update_WorldSpaceWithRotatedCamera_RotatesPosition()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 0);
            canvas.SetPosition(worldPosition);

            // Set up camera with rotation
            _testCamera.Position = new Vector2(0, 0);
            _testCamera.Zoom = 1.0f;
            _testCamera.Rotation = MathHelper.PiOver2; // 90 degrees

            // Act
            canvas.Update(new GameTime());

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            // Calculate expected screen position with rotation
            Vector2 expectedScreenPos = _testCamera.WorldToScreen(worldPosition);
            
            // Due to floating point precision, we use Assert.InRange
            Assert.InRange(rootPanel.Left, (int)expectedScreenPos.X - 1, (int)expectedScreenPos.X + 1);
            Assert.InRange(rootPanel.Top, (int)expectedScreenPos.Y - 1, (int)expectedScreenPos.Y + 1);
        }

        [Fact]
        public void Update_WorldSpaceWithNullCamera_FallsBackToScreenSpacePosition()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 200);
            canvas.SetPosition(worldPosition);

            // Set main camera to null
            Camera.SetMainCamera(null);

            // Act
            canvas.Update(new GameTime());

            // Assert
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            // Should use the position directly as screen position when no camera is available
            Assert.Equal((int)worldPosition.X, rootPanel.Left);
            Assert.Equal((int)worldPosition.Y, rootPanel.Top);
        }

        [Fact]
        public void SetPosition_InWorldSpace_UpdatesPosition()
        {
            // Arrange
            InitializeGUIManager();
            var canvas = new Canvas(false); // world space
            Vector2 newPosition = new Vector2(100, 200);

            // Act
            canvas.SetPosition(newPosition);

            // Assert
            var positionProperty = typeof(Canvas).GetProperty("Position", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var position = (Vector2)positionProperty.GetValue(canvas);
            
            Assert.Equal(newPosition, position);
            
            // Also verify the panel position was updated directly too
            var rootPanelField = typeof(Canvas).GetField("_rootPanel", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var rootPanel = (Panel)rootPanelField.GetValue(canvas);
            
            Assert.Equal((int)newPosition.X, rootPanel.Left);
            Assert.Equal((int)newPosition.Y, rootPanel.Top);
        }
    }
}
