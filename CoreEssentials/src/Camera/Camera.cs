using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CoreEssentials.Camera
{
    /// <summary>
    /// Represents an orthographic camera that can be attached to an entity.
    /// Provides functionality for view and projection matrix transformations.
    /// </summary>
    public class Camera : IDisposable
    {
        #region Static Properties and Methods

        /// <summary>
        /// The main camera used for rendering the scene
        /// </summary>
        public static Camera? MainCamera { get; private set; }

        /// <summary>
        /// Sets the specified camera as the main camera
        /// </summary>
        /// <param name="camera">The camera to set as main, or null to clear the main camera.</param>
        public static void SetMainCamera(Camera? camera)
        {
            MainCamera = camera;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the position of the camera in world space
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the camera in radians
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the zoom factor of the camera
        /// </summary>
        public float Zoom { get; set; }

        /// <summary>
        /// Gets or sets the origin point used for rotation
        /// </summary>
        public Vector2 Origin { get; set; }

        /// <summary>
        /// Gets or sets the orthographic half-height of the visible area in world units, mirroring
        /// Unity's <c>Camera.orthographicSize</c>. When greater than zero the camera uses the
        /// orthographic projection model (see <see cref="ComputeProjectionScale"/>); when zero it
        /// falls back to the legacy behavior where one world unit equals one pixel at zoom 1.
        /// </summary>
        public float OrthographicSize { get; set; }

        /// <summary>
        /// Gets or sets the logical (game) resolution in pixels that the camera projects into. For
        /// pixel-art games this is typically smaller than the actual render/backbuffer resolution
        /// (e.g. a 320x180 game view presented on a 1280x720 window). When zero, the viewport height
        /// is derived from <see cref="OrthographicSize"/>.
        /// </summary>
        public Vector2 ViewportSize { get; set; }

        /// <summary>
        /// Gets or sets the ratio of render resolution to game resolution (pixel-art upscaling).
        /// Defaults to 1. A value of 4 means a 320x180 game view is presented on a 1280x720 window.
        /// </summary>
        public float RenderScale { get; set; } = 1f;

        /// <summary>
        /// Gets the height of the visible world area in world units, accounting for zoom.
        /// Mirrors Unity's relationship between orthographic size and zoom.
        /// </summary>
        public float VisibleWorldHeight => OrthographicSize > 0f ? (2f * OrthographicSize) / Zoom : ViewportSize.Y;

        /// <summary>
        /// Gets the view matrix for this camera
        /// </summary>
        public Matrix ViewMatrix
        {
            get
            {
                return CalculateViewMatrix();
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Camera class
        /// </summary>
        public Camera()
        {
            Position = Vector2.Zero;
            Rotation = 0f;
            Zoom = 1f;
            Origin = Vector2.Zero;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets this camera as the main camera
        /// </summary>
        public void SetAsMainCamera()
        {
            SetMainCamera(this);
        }

        /// <summary>
        /// Moves the camera by the specified amount
        /// </summary>
        /// <param name="amount">Amount to move the camera</param>
        public void Move(Vector2 amount)
        {
            Position += amount;
        }

        /// <summary>
        /// Converts a screen position to world position
        /// </summary>
        /// <param name="screenPosition">Position on the screen</param>
        /// <returns>Position in the world</returns>
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Matrix inverseViewMatrix = Matrix.Invert(ViewMatrix);
            return Vector2.Transform(screenPosition, inverseViewMatrix);
        }

        /// <summary>
        /// Converts a world position to screen position
        /// </summary>
        /// <param name="worldPosition">Position in the world</param>
        /// <returns>Position on the screen</returns>
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, ViewMatrix);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates the view matrix for the camera
        /// </summary>
        /// <returns>The calculated view matrix</returns>
        private Matrix CalculateViewMatrix()
        {
            // The camera view matrix is a combination of translation, rotation, and scaling.
            float scale = ComputeProjectionScale();
            return Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateScale(scale, scale, 1.0f) *
                   Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        /// <summary>
        /// Computes the pixels-per-world-unit projection scale.
        /// </summary>
        /// <remarks>
        /// When <see cref="OrthographicSize"/> is zero the legacy model is used: one world unit
        /// equals one pixel, scaled by <see cref="Zoom"/>. This preserves the pre-ortho behavior so
        /// existing scenes render unchanged unless they opt in.
        ///
        /// Otherwise the orthographic model is used: the scale maps world units into the logical
        /// (<see cref="ViewportSize"/>) resolution and is multiplied by <see cref="RenderScale"/> to
        /// account for pixel-art upscaling to the actual backbuffer.
        /// </remarks>
        private float ComputeProjectionScale()
        {
            if (OrthographicSize <= 0f)
                return Zoom;

            float viewportHeight = ViewportSize.Y > 0f ? ViewportSize.Y : 2f * OrthographicSize;
            float worldToGamePixel = (viewportHeight * Zoom) / (2f * OrthographicSize);
            return worldToGamePixel * RenderScale;
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        /// <summary>
        /// Releases all resources used by the <see cref="Camera"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Camera"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Dispose managed state (managed objects).
                if (MainCamera == this)
                {
                    SetMainCamera(null!);
                }


                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Camera"/> class.
        /// </summary>
        ~Camera()
        {
            Dispose(false);
        }

        #endregion
    }
}
