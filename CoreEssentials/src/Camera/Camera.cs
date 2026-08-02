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
            // The camera view matrix is a combination of translation, rotation, and scaling
            return Matrix.CreateTranslation(new Vector3(-Position, 0.0f)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateScale(new Vector3(Zoom, Zoom, 1.0f)) *
                   Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
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
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    if (MainCamera == this)
                    {
                        SetMainCamera(null!);
                    }
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
