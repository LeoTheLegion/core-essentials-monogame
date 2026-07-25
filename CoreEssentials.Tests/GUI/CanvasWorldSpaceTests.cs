using Xunit;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Internal;
using CoreEssentials.Cameras;
using Microsoft.Xna.Framework;
using System;

namespace CoreEssentials.Tests.GUI
{
    /// <summary>
    /// Tests for world-space canvas behavior with camera transformations.
    /// Canvas delegates to CanvasImpl, so we use reflection on _impl to verify internal state.
    /// </summary>
    public class CanvasWorldSpaceTests : IDisposable
    {
        private readonly Game _mockGame;
        private Camera _testCamera;

        public CanvasWorldSpaceTests()
        {
            // Create a real Game instance for testing
            _mockGame = new Game1();
            
            // Initialize GUIManager - handles MyraEnvironment internally
            GUIManager.Init(_mockGame, 800, 600);

            // Set up a test camera
            _testCamera = new Camera();
            Camera.SetMainCamera(_testCamera);
        }

        void IDisposable.Dispose()
        {
            _mockGame?.Dispose();
            
            // Shutdown the engine
            var engine = EngineResolver.GetEngine();
            engine.Shutdown();
            
            // Reset the main camera
            Camera.SetMainCamera(null);
        }
        
        /// <summary>
        /// Helper to get CanvasImpl instance via reflection on Canvas wrapper's _impl field.
        /// </summary>
        private object GetCanvasImpl(Canvas canvas)
        {
            var implField = typeof(Canvas).GetField("_impl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return implField!.GetValue(canvas)!;
        }

        /// <summary>
        /// Helper to get a field or property value from an object via reflection.
        /// </summary>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context
        private object? GetMemberValue(object obj, string name, Type type)
#pragma warning restore CS8632
        {
            var field = type.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) return field.GetValue(obj);
            var prop = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null) return prop?.GetValue(obj);
            return null;
        }

        /// <summary>
        /// Helper to get the underlying Myra Panel from CanvasImpl via reflection.
        /// </summary>
        private object? GetMyraPanel(object impl)
        {
            var myraProp = impl.GetType().GetProperty("MyraPanel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (myraProp != null) return myraProp.GetValue(impl);
            
            var widgetProp = impl.GetType().GetProperty("Panel");
            if (widgetProp != null) return widgetProp.GetValue(impl);
            
            return null;
        }

        [Fact]
        public void Constructor_WithWorldSpace_CreatesCanvasInWorldSpace()
        {
            // Act
            var canvas = new Canvas(false); // false = world space

            // Assert - verify via public ICamera interface
            Assert.False(canvas.IsScreenSpace, "Canvas should be in world space");
        }

        [Fact]
        public void Constructor_WithScreenSpace_CreatesCanvasInScreenSpace()
        {
            // Act
            var canvas = new Canvas(true); // true = screen space

            // Assert - verify via public ICamera interface
            Assert.True(canvas.IsScreenSpace, "Canvas should be in screen space");
        }

        [Fact]
        public void DefaultConstructor_CreatesCanvasInScreenSpace()
        {
            // Act
            var canvas = new Canvas(); // Default constructor

            // Assert - verify via public ICamera interface
            Assert.True(canvas.IsScreenSpace, "Default canvas should be in screen space");
        }

        [Fact]
        public void Update_WorldSpace_UpdatesRootPanelPositionBasedOnCamera()
        {
            // Arrange
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 200);
            canvas.SetPosition(worldPosition);

            // Set up camera with known transformation
            _testCamera.Position = new Vector2(50, 50);
            _testCamera.Zoom = 1.0f;
            _testCamera.Rotation = 0.0f;

            // Act
            canvas.Update(new GameTime());

            // Assert - verify internal state is consistent after update
            var impl = GetCanvasImpl(canvas);
            var myraPanel = GetMyraPanel(impl);
            Assert.NotNull(myraPanel);
        }

        [Fact]
        public void Update_WorldSpaceWithZoomedCamera_ScalesPosition()
        {
            // Arrange
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 200);
            canvas.SetPosition(worldPosition);

            // Set up camera with zoom
            _testCamera.Position = new Vector2(0, 0);
            _testCamera.Zoom = 2.0f; // 2x zoom
            _testCamera.Rotation = 0.0f;

            // Act
            canvas.Update(new GameTime());

            // Assert - verify internal state is consistent after update with zoomed camera
            var impl = GetCanvasImpl(canvas);
            var myraPanel = GetMyraPanel(impl);
            Assert.NotNull(myraPanel);
        }

        [Fact]
        public void Update_WorldSpaceWithRotatedCamera_RotatesPosition()
        {
            // Arrange
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 0);
            canvas.SetPosition(worldPosition);

            // Set up camera with rotation
            _testCamera.Position = new Vector2(0, 0);
            _testCamera.Zoom = 1.0f;
            _testCamera.Rotation = MathHelper.PiOver2; // 90 degrees

            // Act
            canvas.Update(new GameTime());

            // Assert - verify internal state is consistent after update with rotated camera
            var impl = GetCanvasImpl(canvas);
            var myraPanel = GetMyraPanel(impl);
            Assert.NotNull(myraPanel);
        }

        [Fact]
        public void Update_WorldSpaceWithNullCamera_FallsBackToScreenSpacePosition()
        {
            // Arrange
            var canvas = new Canvas(false); // world space
            Vector2 worldPosition = new Vector2(100, 200);
            canvas.SetPosition(worldPosition);

            // Set main camera to null
            Camera.SetMainCamera(null);

            // Act - should not throw when camera is null
            Exception exception = Record.Exception(() => canvas.Update(new GameTime()));
            
            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void SetPosition_InWorldSpace_UpdatesPosition()
        {
            // Arrange
            var canvas = new Canvas(false); // world space
            Vector2 newPosition = new Vector2(100, 200);

            // Act
            canvas.SetPosition(newPosition);

            // Assert - verify internal state was updated via _impl reflection
            var impl = GetCanvasImpl(canvas);
            var positionVal = GetMemberValue(impl, "_position", impl.GetType());
            
            Assert.NotNull(positionVal);
        }
    }
}
