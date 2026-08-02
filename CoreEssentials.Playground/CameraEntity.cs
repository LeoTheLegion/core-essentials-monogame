using CoreEssentials.Camera;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System; // Added for MathHelper if not already present, and for event handlers
using CoreEssentials.Inputs; // Added for Input class
using MonoGame.Extended.Input.InputListeners; // Added for KeyboardEventArgs
using CoreEssentials.Timing; // Added for Time.DeltaTime

namespace CoreEssentials.Playground
{
    /// <summary>
    /// An entity that manages a camera with movement, zoom, and follow functionality.
    /// </summary>
    public class CameraEntity : Entity
    {
        private readonly Camera.Camera camera;
        private Entity targetToFollow;
        private const float ActualZoomSpeed = 0.1f; // Actual amount to change zoom per input

        private EventHandler<KeyboardEventArgs> _keyReleaseHandler;
        
        /// <summary>
        /// Gets the wrapped camera instance.
        /// </summary>
        public Camera.Camera Camera => camera;
        
        /// <summary>
        /// Gets or sets the speed at which the camera moves.
        /// </summary>
        public float CameraSpeed { get; set; }

        /// <summary>
        /// Gets or sets the sensitivity factor for zooming.
        /// </summary>
        public float ZoomSpeed { get; set; }
        
        /// <summary>
        /// Gets or sets whether the camera is following a target.
        /// </summary>
        public bool FollowingTarget { get; set; }
        
        /// <summary>
        /// Creates a new camera entity
        /// </summary>
        public CameraEntity()
        {
            // Create a camera and set it as the main camera
            CameraSpeed = 1f;
            ZoomSpeed = 1f;
            FollowingTarget = false;

            camera = new Camera.Camera();
            camera.SetAsMainCamera();
        }
        
        public override void OnStart()
        {
            base.OnStart();
            _keyReleaseHandler = HandleCameraKeyRelease;
            Input.Keyboard.KeyReleased += _keyReleaseHandler;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_keyReleaseHandler != null)
            {
                Input.Keyboard.KeyReleased -= _keyReleaseHandler;
            }
            camera.Dispose();
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Handle manual camera movement with WASD if not following a target
            if (!FollowingTarget)
            {
                if (Input.Keyboard.IsKeyDown(Keys.A))
                {
                    Position += new Vector2(-CameraSpeed, 0) * (float)Time.DeltaTime;
                }
                if (Input.Keyboard.IsKeyDown(Keys.D))
                {
                    Position += new Vector2(CameraSpeed, 0) * (float)Time.DeltaTime;
                }
                if (Input.Keyboard.IsKeyDown(Keys.W))
                {
                    Position += new Vector2(0, -CameraSpeed) * (float)Time.DeltaTime;
                }
                if (Input.Keyboard.IsKeyDown(Keys.S))
                {
                    Position += new Vector2(0, CameraSpeed) * (float)Time.DeltaTime;
                }
            }

            // Zoom controls with Q and E
            if (Input.Keyboard.IsKeyDown(Keys.Q))
            {
                camera.Zoom += 1f * (float)Time.DeltaTime * ActualZoomSpeed * ZoomSpeed;
            }
            if (Input.Keyboard.IsKeyDown(Keys.E))
            {
                camera.Zoom -= 1f * (float)Time.DeltaTime * ActualZoomSpeed * ZoomSpeed;
            }
            camera.Zoom = MathHelper.Clamp(camera.Zoom, 0.1f, 3f);
            
            // Update position
            camera.Position = Position;
            
            // Follow target if enabled
            if (FollowingTarget && targetToFollow != null)
            {
                Position = Vector2.Lerp(Position, targetToFollow.Position, 0.1f);
            }
        }
        
        /// <summary>
        /// Moves the camera in the specified direction
        /// </summary>
        /// <param name="direction">Direction to move the camera</param>
        /// <param name="deltaTime">The time elapsed since last frame in seconds</param>
        public void Move(Vector2 direction, float deltaTime)
        {
            if (!FollowingTarget) // Only allow manual move if not following
            {
                Position += direction * CameraSpeed * deltaTime;
            }
        }
        
        /// <summary>
        /// Sets the entity that the camera should follow
        /// </summary>
        /// <param name="target">The entity to follow</param>
        /// <param name="startFollowingImmediately">Whether to start following immediately</param>
        public void SetFollowTarget(Entity target, bool startFollowingImmediately = true)
        {
            targetToFollow = target;
            FollowingTarget = startFollowingImmediately;
        }
        
        /// <summary>
        /// Zooms the camera by the specified amount
        /// </summary>
        /// <param name="amount">Amount to zoom (positive to zoom in, negative to zoom out)</param>
        public void Zoom(float amount) // Amount is now a factor, actual zoom speed is internal
        {
            camera.Zoom += amount * ZoomSpeed * ActualZoomSpeed;
            camera.Zoom = MathHelper.Clamp(camera.Zoom, 0.1f, 3f);
        }
        
        /// <summary>
        /// Resets the camera position, rotation, and zoom.
        /// </summary>
        public void ResetCamera()
        {
            Position = Vector2.Zero;
            camera.Rotation = 0f;
            camera.Zoom = 1f;
        }

        public void ToggleFollow(Entity targetToToggle)
        {
            if (FollowingTarget && targetToFollow == targetToToggle)
            {
                SetFollowTarget(null, false);
            }
            else
            {
                SetFollowTarget(targetToToggle, true);
            }
        }

        private void HandleCameraKeyRelease(object sender, KeyboardEventArgs args)
        {
            // Reset camera with R key
            if (args.Key == Keys.R)
            {
                ResetCamera();
            }
            // Note: 'F' key for follow toggle is handled by CameraScene 
            // as it needs to know about the 'player' entity.
        }
    }
}
