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
        private Camera.Camera camera;
        private Entity targetToFollow;
        private float cameraSpeed = 1f;
        private float zoomSpeed = 1f; // This is more like a sensitivity factor
        private float actualZoomSpeed = 0.1f; // Actual amount to change zoom per input
        private bool followingTarget = false;

        private EventHandler<KeyboardEventArgs> _keyReleaseHandler;
        
        /// <summary>
        /// Gets the wrapped camera instance
        /// </summary>
        public Camera.Camera Camera => camera;
        
        /// <summary>
        /// Gets or sets the speed at which the camera moves
        /// </summary>
        public float CameraSpeed
        {
            get => cameraSpeed;
            set => cameraSpeed = value;
        }
        
        /// <summary>
        /// Gets or sets the speed at which the camera zooms
        /// </summary>
        public float ZoomSpeed
        {
            get => zoomSpeed;
            set => zoomSpeed = value;
        }
        
        /// <summary>
        /// Gets or sets whether the camera is following a target
        /// </summary>
        public bool FollowingTarget
        {
            get => followingTarget;
            set => followingTarget = value;
        }
        
        /// <summary>
        /// Creates a new camera entity
        /// </summary>
        public CameraEntity()
        {
            // Create a camera and set it as the main camera
            camera = new Camera.Camera();
            camera.SetAsMainCamera();
        }
        
        public override void OnStart()
        {
            base.OnStart();
            // _keyPressHandler = HandleCameraKeyPress; // Commented out, will be handled in Update
            _keyReleaseHandler = HandleCameraKeyRelease;

            // Input.Keyboard.KeyPressed += _keyPressHandler; // Commented out
            Input.Keyboard.KeyReleased += _keyReleaseHandler;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            // if (_keyPressHandler != null) // Commented out
            // {
            //     Input.Keyboard.KeyPressed -= _keyPressHandler;
            // }
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
            if (!followingTarget)
            {
                if (Input.Keyboard.IsKeyDown(Keys.A))
                {
                    Move(new Vector2(-1, 0), (float)Time.DeltaTime);
                }
                if (Input.Keyboard.IsKeyDown(Keys.D))
                {
                    Move(new Vector2(1, 0), (float)Time.DeltaTime);
                }
                if (Input.Keyboard.IsKeyDown(Keys.W))
                {
                    Move(new Vector2(0, -1), (float)Time.DeltaTime);
                }
                if (Input.Keyboard.IsKeyDown(Keys.S))
                {
                    Move(new Vector2(0, 1), (float)Time.DeltaTime);
                }
            }

            // Zoom controls with Q and E
            if (Input.Keyboard.IsKeyDown(Keys.Q))
            {
                Zoom(1f * (float)Time.DeltaTime * 0.1f); // Adjusted zoom to be time-based and smoother
            }
            if (Input.Keyboard.IsKeyDown(Keys.E))
            {
                Zoom(-1f * (float)Time.DeltaTime * 0.1f); // Adjusted zoom to be time-based and smoother
            }
            
            // Update position
            camera.Position = Position;
            
            // Follow target if enabled
            if (followingTarget && targetToFollow != null)
            {
                Position = Vector2.Lerp(Position, targetToFollow.Position, 0.1f); // Increased Lerp factor for snappier follow
            }
        }
        
        /// <summary>
        /// Moves the camera in the specified direction
        /// </summary>
        /// <param name="direction">Direction to move the camera</param>
        /// <param name="deltaTime">The time elapsed since last frame in seconds</param>
        public void Move(Vector2 direction, float deltaTime)
        {
            if (!followingTarget) // Only allow manual move if not following
            {
                Position += direction * cameraSpeed * deltaTime;
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
            followingTarget = startFollowingImmediately;
        }
        
        /// <summary>
        /// Zooms the camera by the specified amount
        /// </summary>
        /// <param name="amount">Amount to zoom (positive to zoom in, negative to zoom out)</param>
        public void Zoom(float amount) // Amount is now a factor, actual zoom speed is internal
        {
            camera.Zoom += amount * zoomSpeed * actualZoomSpeed ; // Removed Time.DeltaTime here as it's applied at call site
            camera.Zoom = MathHelper.Clamp(camera.Zoom, 0.1f, 3f);
        }
        
        /// <summary>
        /// Resets the camera position, rotation, and zoom
        /// </summary>
        public void ResetCamera()
        {
            Position = Vector2.Zero;
            camera.Rotation = 0f;
            camera.Zoom = 1f;
            // Optionally stop following
            // SetFollowTarget(null, false); 
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
