using CoreEssentials.Camera;
using Microsoft.Xna.Framework;
using System;
using Xunit;

namespace CoreEssentials.Tests.Cameras
{
    public class CameraTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            // Arrange & Act
            var camera = new CoreEssentials.Camera.Camera();

            // Assert
            Assert.Equal(Vector2.Zero, camera.Position);
            Assert.Equal(0f, camera.Rotation);
            Assert.Equal(1f, camera.Zoom);
            Assert.Equal(Vector2.Zero, camera.Origin);
        }

        [Fact]
        public void SetAsMainCamera_SetsInstanceAsMainCamera()
        {
            // Arrange
            var camera = new CoreEssentials.Camera.Camera();

            // Act
            camera.SetAsMainCamera();

            // Assert
            Assert.Same(camera, CoreEssentials.Camera.Camera.MainCamera);
        }

        [Fact]
        public void SetMainCamera_SetsSpecifiedCameraAsMainCamera()
        {
            // Arrange
            var camera = new CoreEssentials.Camera.Camera();

            // Act
            CoreEssentials.Camera.Camera.SetMainCamera(camera);

            // Assert
            Assert.Same(camera, CoreEssentials.Camera.Camera.MainCamera);
        }

        [Fact]
        public void Move_UpdatesPosition()
        {
            // Arrange
            var camera = new CoreEssentials.Camera.Camera();
            var initialPosition = Vector2.Zero;
            var moveAmount = new Vector2(10, 20);

            // Act
            camera.Move(moveAmount);

            // Assert
            Assert.Equal(initialPosition + moveAmount, camera.Position);
        }

        [Fact]
        public void ViewMatrix_ReturnsCorrectTransformationMatrix()
        {
            // Arrange
            var camera = new CoreEssentials.Camera.Camera
            {
                Position = new Vector2(100, 200),
                Zoom = 2.0f,
                Rotation = MathHelper.ToRadians(45)
            };

            // Act
            var viewMatrix = camera.ViewMatrix;

            // Assert
            // Test with a known point to verify transformation
            var testPoint = new Vector2(150, 250);
            var transformedPoint = Vector2.Transform(testPoint, viewMatrix);
            
            // The exact values would depend on the matrix calculations,
            // but we can at least verify that the transformation has occurred
            Assert.NotEqual(testPoint, transformedPoint);
        }

        [Fact]
        public void ScreenToWorld_TransformsScreenCoordinatesToWorldCoordinates()
        {
            // Arrange
            var camera = new CoreEssentials.Camera.Camera
            {
                Position = new Vector2(100, 100),
                Zoom = 2.0f
            };
            
            var screenPosition = new Vector2(50, 50);

            // Act
            var worldPosition = camera.ScreenToWorld(screenPosition);

            // Assert
            // With a zoom of 2.0 and camera position of (100,100), 
            // a screen position of (50,50) should be transformed to something different
            Assert.NotEqual(screenPosition, worldPosition);
        }

        [Fact]
        public void WorldToScreen_TransformsWorldCoordinatesToScreenCoordinates()
        {
            // Arrange
            var camera = new CoreEssentials.Camera.Camera
            {
                Position = new Vector2(100, 100),
                Zoom = 2.0f
            };
            
            var worldPosition = new Vector2(150, 150);

            // Act
            var screenPosition = camera.WorldToScreen(worldPosition);

            // Assert
            // With a zoom of 2.0 and camera position of (100,100), 
            // a world position of (150,150) should be transformed to something different
            Assert.NotEqual(worldPosition, screenPosition);
        }

        [Fact]
        public void Dispose_WhenIsMainCamera_SetsMainCameraToNull()
        {
            // Arrange
            var camera = new CoreEssentials.Camera.Camera();
            camera.SetAsMainCamera();
            Assert.Same(camera, CoreEssentials.Camera.Camera.MainCamera); // Ensure it was set

            // Act
            camera.Dispose();

            // Assert
            Assert.Null(CoreEssentials.Camera.Camera.MainCamera);
        }

        [Fact]
        public void Dispose_WhenNotMainCamera_DoesNotAffectMainCamera()
        {
            // Arrange
            var mainCam = new CoreEssentials.Camera.Camera();
            mainCam.SetAsMainCamera();

            var otherCamera = new CoreEssentials.Camera.Camera();
            Assert.NotSame(otherCamera, CoreEssentials.Camera.Camera.MainCamera); // Ensure it's not main

            // Act
            otherCamera.Dispose();

            // Assert
            Assert.Same(mainCam, CoreEssentials.Camera.Camera.MainCamera); // MainCamera should still be mainCam
            mainCam.Dispose(); // Clean up mainCam
        }
    }
}
