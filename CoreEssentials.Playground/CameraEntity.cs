using CoreEssentials.Camera;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System; // Added for event handlers
using CoreEssentials.Inputs; // Added for Input class + CoreEssentials-owned KeyboardEventArgs

namespace CoreEssentials.Playground
{
    /// <summary>
    /// A playground entity that demonstrates camera control. Camera ownership lives in a built-in
    /// <see cref="CameraComponent"/> (which keeps the camera anchored to this entity every late
    /// update); this entity only adds the input layer — WASD pans, Q/E zoom, R resets, and an
    /// optional follow target.
    /// </summary>
    public class CameraEntity : Entity
    {
        private readonly CameraComponent _cameraComponent;
        private Entity? _followTarget;

        /// <summary>
        /// Gets the wrapped camera instance (owned by the attached <see cref="CameraComponent"/>).
        /// </summary>
        public Camera.Camera Camera => _cameraComponent.Camera;

        /// <summary>
        /// Gets or sets the speed at which the camera moves.
        /// </summary>
        public float CameraSpeed { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the sensitivity factor for zooming.
        /// </summary>
        public float ZoomSpeed { get; set; } = 1f;

        /// <summary>
        /// Gets whether the camera is following a target.
        /// </summary>
        public bool FollowingTarget => _followTarget != null;

        private EventHandler<KeyboardEventArgs>? _keyReleaseHandler;

        /// <summary>
        /// Creates a new camera entity. The camera itself is owned by an attached
        /// <see cref="CameraComponent"/> and registered as the main camera on attach.
        /// </summary>
        public CameraEntity()
        {
            _cameraComponent = AddComponent(new CameraComponent());
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
                _keyReleaseHandler = null;
            }
            // The CameraComponent disposes the camera and clears MainCamera when the entity is destroyed.
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Handle manual camera movement with WASD if not following a target
            if (!FollowingTarget)
            {
                if (Input.Keyboard.IsKeyDown(Keys.A))
                {
                    Position += new Vector2(-CameraSpeed, 0) * deltaTime;
                }
                if (Input.Keyboard.IsKeyDown(Keys.D))
                {
                    Position += new Vector2(CameraSpeed, 0) * deltaTime;
                }
                if (Input.Keyboard.IsKeyDown(Keys.W))
                {
                    Position += new Vector2(0, -CameraSpeed) * deltaTime;
                }
                if (Input.Keyboard.IsKeyDown(Keys.S))
                {
                    Position += new Vector2(0, CameraSpeed) * deltaTime;
                }
            }

            // Zoom controls with Q and E
            if (Input.Keyboard.IsKeyDown(Keys.Q))
            {
                _cameraComponent.Camera.Zoom += 1f * deltaTime * ActualZoomSpeed * ZoomSpeed;
            }
            if (Input.Keyboard.IsKeyDown(Keys.E))
            {
                _cameraComponent.Camera.Zoom -= 1f * deltaTime * ActualZoomSpeed * ZoomSpeed;
            }
            _cameraComponent.Camera.Zoom = MathHelper.Clamp(_cameraComponent.Camera.Zoom, 0.1f, 3f);

            // Follow target if enabled (movement stays here; the component only anchors the camera)
            if (FollowingTarget && _followTarget != null)
            {
                Position = Vector2.Lerp(Position, _followTarget.Position, 0.1f);
            }
        }

        /// <summary>
        /// Moves the entity (and therefore the anchored camera) in the specified direction.
        /// </summary>
        /// <param name="direction">Direction to move in.</param>
        /// <param name="deltaTime">The time elapsed since last frame in seconds.</param>
        public void Move(Vector2 direction, float deltaTime)
        {
            if (!FollowingTarget) // Only allow manual move when not following
            {
                Position += direction * CameraSpeed * deltaTime;
            }
        }

        /// <summary>
        /// Sets the entity to follow.
        /// </summary>
        /// <param name="target">The entity to follow, or null to stop.</param>
        /// <param name="startFollowingImmediately">Whether to start following right away.</param>
        public void SetFollowTarget(Entity? target, bool startFollowingImmediately = true)
        {
            _followTarget = startFollowingImmediately ? target : null;
        }

        /// <summary>
        /// Zooms the camera by the specified amount.
        /// </summary>
        /// <param name="amount">Amount to zoom (positive to zoom in, negative to zoom out)</param>
        public void Zoom(float amount) // Amount is now a factor, actual zoom speed is internal
        {
            _cameraComponent.Camera.Zoom += amount * ZoomSpeed * ActualZoomSpeed;
            _cameraComponent.Camera.Zoom = MathHelper.Clamp(_cameraComponent.Camera.Zoom, 0.1f, 3f);
        }

        /// <summary>
        /// Resets the camera position, rotation, and zoom.
        /// </summary>
        public void ResetCamera()
        {
            Position = Vector2.Zero;
            _cameraComponent.Camera.Rotation = 0f;
            _cameraComponent.Camera.Zoom = 1f;
        }

        public void ToggleFollow(Entity targetToToggle)
        {
            if (FollowingTarget && _followTarget == targetToToggle)
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

        private const float ActualZoomSpeed = 0.1f; // Actual amount to change zoom per input
    }
}
