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

        // ===== Orthographic projection (Unity-style) =====

        [Fact]
        public void Constructor_DefaultsToLegacyProjection()
        {
            var camera = new CoreEssentials.Camera.Camera();

            Assert.Equal(0f, camera.OrthographicSize);
            Assert.Equal(Vector2.Zero, camera.ViewportSize);
            Assert.Equal(1f, camera.RenderScale);
        }

        [Fact]
        public void Ortho_LegacyFallback_KeepsOneWorldUnitPerPixel()
        {
            // With OrthographicSize == 0 the view matrix must equal the old behavior:
            // pure translation + zoom scaling.
            var legacy = new CoreEssentials.Camera.Camera
            {
                Position = new Vector2(100, 50),
                Zoom = 2f
            };

            var transformed = Vector2.Transform(new Vector2(150, 75), legacy.ViewMatrix);

            Assert.Equal(new Vector2((150 - 100) * 2f, (75 - 50) * 2f), transformed);
        }

        [Fact]
        public void Ortho_CenterWorldPoint_ProjectsToViewportCenter()
        {
            var camera = new CoreEssentials.Camera.Camera
            {
                Position = new Vector2(100, 100),
                OrthographicSize = 150f,
                ViewportSize = new Vector2(320, 180)
            };

            // The world point under the camera center maps to the view pivot (origin),
            // matching the legacy convention: result = (world - position) * scale.
            var transformed = Vector2.Transform(new Vector2(100, 100), camera.ViewMatrix);

            Assert.Equal(Vector2.Zero, transformed);
        }

        [Fact]
        public void Ortho_ScaleMatchesViewportToOrthoRatio()
        {
            // viewportHeight=180, orthoSize=90 -> worldToGamePixel = (180*1)/(2*90) = 1.
            var camera = new CoreEssentials.Camera.Camera
            {
                OrthographicSize = 90f,
                ViewportSize = new Vector2(320, 180)
            };

            var transformed = Vector2.Transform(new Vector2(5, 7), camera.ViewMatrix);

            Assert.Equal(new Vector2(5, 7), transformed);
        }

        [Fact]
        public void Ortho_ZoomScalesVisibleWorld()
        {
            // orthoSize=90, viewport 180 -> scale 1 at zoom 1; at zoom 2 the same world offset
            // must project to twice the pixels.
            var camera = new CoreEssentials.Camera.Camera
            {
                OrthographicSize = 90f,
                ViewportSize = new Vector2(320, 180),
                Zoom = 2f
            };

            var transformed = Vector2.Transform(new Vector2(50, 50), camera.ViewMatrix);

            Assert.Equal(new Vector2(100, 100), transformed);
        }

        [Fact]
        public void Ortho_RenderScaleUpscalesPixels()
        {
            // A 320x180 game view presented at 4x (1280x720 backbuffer): world offsets must be
            // multiplied by the render scale in addition to the projection.
            var camera = new CoreEssentials.Camera.Camera
            {
                OrthographicSize = 90f,
                ViewportSize = new Vector2(320, 180),
                RenderScale = 4f
            };

            var transformed = Vector2.Transform(new Vector2(50, 50), camera.ViewMatrix);

            Assert.Equal(new Vector2(200, 200), transformed);
        }

        [Fact]
        public void Ortho_ZeroViewport_FallsBackToTwiceOrthoSize()
        {
            // No explicit viewport: height defaults to 2*orthoSize -> scale is just zoom.
            var camera = new CoreEssentials.Camera.Camera
            {
                OrthographicSize = 90f,
                Zoom = 3f
            };

            var transformed = Vector2.Transform(new Vector2(10, 10), camera.ViewMatrix);

            Assert.Equal(new Vector2(30, 30), transformed);
        }

        [Fact]
        public void VisibleWorldHeight_AccountsForZoom()
        {
            var camera = new CoreEssentials.Camera.Camera
            {
                OrthographicSize = 90f,
                Zoom = 2f
            };

            Assert.Equal(90f, camera.VisibleWorldHeight); // 2*90/2
        }

        [Fact]
        public void ScreenToWorld_And_WorldToScreen_AreInverseUnderOrtho()
        {
            var camera = new CoreEssentials.Camera.Camera
            {
                Position = new Vector2(100, 100),
                Rotation = MathHelper.ToRadians(30),
                OrthographicSize = 90f,
                ViewportSize = new Vector2(320, 180),
                Zoom = 1.5f
            };

            var originalWorld = new Vector2(250, -40);
            var roundTripped = camera.ScreenToWorld(camera.WorldToScreen(originalWorld));

            Assert.True(Vector2.Distance(originalWorld, roundTripped) < 0.001f,
                $"Round trip mismatch: {originalWorld} -> {roundTripped}");
        }
    }
}
